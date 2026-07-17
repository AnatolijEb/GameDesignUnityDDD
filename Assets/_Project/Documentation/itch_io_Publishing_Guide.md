# itch.io Publishing & WebGL-Optimierung – Vorgehensplan

**Ziel:** "Don't Drink and Drive" als **im Browser spielbares** WebGL-Game auf itch.io veröffentlichen.
**Projekt-Stand geprüft am:** 2026-07-17 · Unity **6000.4.10f1** · **URP 17.4.0** · Input System 1.19 · Linear Color Space.

---

## 0. Bewertung: Ist das Projekt für Online-Browser-Play optimiert?

**Browser-*fähig* ja, Browser-*optimiert* nein.** Die harten Blocker sind alle grün, es fehlt Build-Konfiguration, Größen-Aufräumen und Rendering-Tuning.

| Prüfpunkt | Status | Kommentar |
|---|---|---|
| Render Pipeline | ✅ URP (aktiv) | WebGL-tauglich. Das HDRP-Paket im Projekt ist **nicht** aktiv – nur totes Gewicht. |
| Unity-Version | ✅ Unity 6 | Bester WebGL-Support (WebGL 2.0 / WASM). |
| Eigener Code | ✅ keine Blocker | Kein `System.IO`/Threads/Sockets. `PlayerPrefs` läuft im Browser (IndexedDB). |
| WebGL-Build-Modul | ❓ prüfen | Muss im Unity Hub installiert sein (siehe Schritt 1). |
| Kompression/Loading | 🔴 Fix nötig | Brotli **ohne** Decompression Fallback → hängt auf itch.io beim Laden. |
| Build-/Projektgröße | 🟠 aufräumen | 388 MB Assets, ~106 MB davon Leichen. |
| Rendering-Kosten | 🟠 tunen | Auf Desktop-PC getunt (2048px-Schatten, 4 Cascades, HDR). |

> **Wichtig – Build ≠ Projektgröße:** Unity packt nur Assets in den Build, die in den Build-Szenen
> (`MainMenu.unity`, `SampleScene.unity`) oder in `Resources/` referenziert sind. Die 388 MB landen also
> **nicht** komplett im Build. Aufräumen bringt trotzdem viel: schnellere Builds, kleineres Git, weniger
> versehentlich referenzierter Ballast. Der echte Build-Größen-Hebel sind **Texture-/Audio-Import-Settings**.

---

## Teil A – Kritische Fixes (ohne die lädt/läuft es auf itch.io schlecht)

### A1. 🔴 Kompression & Decompression Fallback (der #1 itch.io-Bug)

**Aktuell:** `webGLCompressionFormat: 0` (Brotli), `webGLDecompressionFallback: 0` (aus).
itch.io liefert Dateien ohne den passenden `Content-Encoding`-Header → der Browser kann die
Brotli-Dateien nicht auspacken → das Spiel hängt bei „Loading…".

**Fix in Unity:** `Edit > Project Settings > Player > Web-Tab > Publishing Settings`
- **Compression Format:** `Gzip` (zuverlässig auf itch) — Brotli geht auch, aber nur mit Fallback.
- **Decompression Fallback:** ✅ **einschalten** (Pflicht für itch.io).
- **Data Caching:** an lassen (schnelleres 2. Laden per IndexedDB).

### A2. 🔴 WebGL-Build-Modul installieren
`Unity Hub > Installs > 6000.4.10f1 > ⚙ > Add Modules > "WebGL Build Support"`. Ohne dieses Modul
fehlt WebGL im Build-Target komplett.

### A3. 🟠 Threads AUS lassen
`webGLThreadsSupport` ist bereits **aus** – so lassen. itch.io setzt keine COOP/COEP-Header,
mit Threads würde das Spiel gar nicht starten.

### A4. 🟡 Audio-Autoplay-Gate
Browser blockieren Sound bis zur ersten Nutzer-Interaktion. Da das Spiel im **MainMenu** startet
und der Spieler „Play" klickt, ist die Interaktion vor dem Gameplay gegeben – In-Game-Audio ist ok.
Nur die **Menü-Musik** (PlayOnAwake im MainMenu) könnte bis zum ersten Klick stumm sein.
→ Unkritisch; optional Menü-Musik erst beim ersten Klick/Hover starten.

---

## Teil B – Optimierung (Ladezeit + Framerate im Browser)

### B1. Tote Dateien löschen (~110+ MB, reiner Ballast)

| Löschen | Größe | Warum |
|---|---|---|
| `Assets/MarpaStudio/HDRP/StylizedStreetHDRP.unitypackage` | 77 MB | HDRP-Installer – Projekt ist URP. Nie benutzt. |
| `Assets/MarpaStudio/URP/URPStreet.unitypackage` | 29 MB | Installer-Datei; die Assets sind bereits nach `MarpaStudio/Textures` + `/Mesh` entpackt. |
| `Assets/MarpaStudio/Built-In/…unitypackage` | <1 MB | Built-In-Variante, nicht benutzt. |
| `Assets/Recordings/Movie_001.mp4` | 5 MB | Screen-Recording, kein Spielinhalt. |
| `Assets/TutorialInfo/` | 80 KB | Unitys Default-„Readme"-Ballast. |
| `Assets/_Recovery/` | prüfen | Auto-Recovery-Reste, falls nicht mehr gebraucht. |

> `.unitypackage`-Dateien landen ohnehin **nie** im Build, aber sie blähen Git und Editor auf. Raus damit.

### B2. Ungenutzte Asset-Packs prüfen & entfernen
Vor dem Löschen mit Rechtsklick auf ein Prefab/Asset im Ordner → **„Find References In Scene"** bzw.
über die Build-Szenen prüfen, ob wirklich verwendet:
- `Assets/Gley/DeliveryVehiclesPack` (**78 MB** – riesige PSD-Texturen) – nur behalten, wenn die
  Fahrzeuge tatsächlich als Hindernisse spawnen.
- `Assets/URP_Flares_Pack` (14 MB), `Assets/BOXOPHOBIC/Skybox Cubemap Extended` Demo-Inhalte (18 MB),
  `Assets/Adrift Team`, `Assets/FastMesh`, `Assets/AI Toolkit` – jeweils prüfen, ob referenziert.

### B3. Texture-Import-Settings (größter *Build*-Hebel)
Die Gley-Fahrzeug-PSDs sind je 23–27 MB. Pro verwendeter Textur im Inspector:
- **Max Size:** 2048 → für Hintergrund/Straße oft **1024** genug.
- **Compression:** `Normal` / für WebGL **ASTC** oder **DXT/BC** + ggf. **Crunch Compression** (kleinere Downloads).
- Nicht als „Sprite" oder unkomprimiert importieren.

### B4. Audio-Import
`Assets/_Project/Audio/Tarantella_on_the_Rocks.mp3` (4 MB) & SFX:
- **Musik (lang):** Load Type `Streaming`, Compression `Vorbis`, Quality ~50–70 %.
- **Kurze SFX:** Load Type `Decompress On Load`, Vorbis.

### B5. Rendering-Kosten senken (URP-Asset)
Geprüft in `Assets/Settings/PC_RPAsset.asset` – für einen Browser-Endless-Runner zu teuer:

| Setting | Aktuell | Empfehlung Browser | Warum |
|---|---|---|---|
| Additional Lights Shadow Resolution | 2048 | 512 oder Zusatzlicht-Schatten **aus** | Nur das Directional-Light braucht i.d.R. Schatten. |
| Additional Lights Cookie Resolution | 2048 | 256/keine | Selten genutzt, teuer. |
| Shadow Cascades | 4 | **1–2** | Man sieht nur einen schmalen Straßen-Streifen. |
| Shadow Distance | 50 | 30–40 | Runner-Kamera sieht nicht weit. |
| HDR | an | **aus**, außer ihr braucht Bloom | Spart Bandbreite & Fill-Rate. URP-Bloom geht auch ohne HDR. |
| Render Scale | 1.0 | 0.85–1.0 | Falls FPS knapp: leicht runter. |

> **Zwei Quality-Tiers vorhanden** (`Mobile` + `PC`). In `Project Settings > Quality` prüfen,
> welches Level für **Web** als Default gesetzt ist – für WebGL das **schlankere (Mobile-)Tier**
> wählen bzw. die obigen Werte dort setzen.

### B6. WorldBend & lesbare Meshes (Memory-Hinweis)
`WorldBendController` fügt nur Meshes mit `isReadable == true` den CPU-Bend hinzu. **Read/Write Enabled**
verdoppelt den Mesh-Speicher – im Browser knapp. Nur bei den Meshes aktivieren, die den Bend wirklich
brauchen (Straße/Gebäude im Sichtfeld); Rest auf Read/Write **aus**.

### B7. Managed Stripping & Code-Optimierung
`Player > Other Settings`:
- **Managed Stripping Level:** `High` (kleinerer Build).
- **Code Optimization:** `Disk Size with LTO` (kleiner) für die finale Version.
- Falls nach Stripping Runtime-Fehler auftauchen (Reflection/ScriptableObjects): eine
  `Assets/link.xml` anlegen, um betroffene Assemblies zu erhalten.

---

## Teil C – Build & Upload (Schritt für Schritt)

### C1. Plattform wechseln
`File > Build Profiles` (bzw. Build Settings) → **Web / WebGL** auswählen → **Switch Platform**
(dauert beim ersten Mal lange – Re-Import aller Assets für WebGL).

### C2. Szenen im Build prüfen
`MainMenu.unity` (Index 0) und `SampleScene.unity` (Index 1) sind bereits aktiv – so lassen,
MainMenu **muss** Index 0 sein (Startszene).

### C3. Player Settings final setzen
- **Product Name / Company:** sauberer Titel (erscheint im Tab & Loading).
- **Publishing Settings:** Gzip + Decompression Fallback (siehe A1).
- **Resolution:** Default Canvas z. B. 1280×720; „Run In Background" an.
- **Splash Screen:** Unity-Logo bleibt (Personal-Lizenz) – ok.

### C4. Bauen
`Build` → in einen leeren Ordner, z. B. `Builds/WebGL/`. Ergebnis: `index.html`, `Build/`, `TemplateData/`.

### C5. Lokal testen (Pflicht – nicht per Doppelklick!)
WebGL läuft nicht über `file://`. Lokalen Server starten:
```bash
cd Builds/WebGL
python3 -m http.server 8000
# dann im Browser: http://localhost:8000
```
Oder direkt Unity **„Build And Run"**. Prüfen: lädt durch, kein Endlos-„Loading", Steuerung & Sound ok,
Konsole (F12) ohne rote Fehler.

### C6. Zippen (häufigster Upload-Fehler!)
Die **Inhalte** des Build-Ordners zippen, sodass **`index.html` direkt im Zip-Root** liegt –
**nicht** den Ordner selbst zippen (sonst findet itch.io die `index.html` nicht).
```bash
cd Builds/WebGL
zip -r ../ddd-webgl.zip .
```

### C7. Auf itch.io hochladen
1. itch.io → **Dashboard > Create new project**.
2. **Kind of project:** `HTML`.
3. Zip hochladen → beim File **„This file will be played in the browser"** anhaken.
4. **Embed options:** Viewport z. B. `1280 × 720`; **„Click to launch in fullscreen"** + Fullscreen-Button an;
   „Mobile friendly" **aus** (Tastatur-Spiel).
5. In der Beschreibung vermerken: **Desktop only, Tastatur (A/D bzw. Pfeiltasten)**.
6. Sichtbarkeit auf **Draft/Restricted** stellen, testen, dann **Public**.

---

## Teil D – Test-Checkliste (nach Upload)

- [ ] Spiel lädt komplett durch (kein Hängen bei „Loading…") → sonst A1 prüfen.
- [ ] MainMenu erscheint, „Play" startet die Runde.
- [ ] Steuerung A/D + Pfeiltasten reagiert (Fokus-Klick ins Canvas nötig?).
- [ ] Musik & SFX spielen nach dem ersten Klick.
- [ ] Highscore bleibt nach Reload erhalten (PlayerPrefs/IndexedDB).
- [ ] Fullscreen-Button funktioniert.
- [ ] Browser-Konsole (F12) ohne rote Fehler.
- [ ] FPS flüssig (Effekte MicroSleep-Vignette/Fog/Bloom nicht zu teuer).
- [ ] Test in **Chrome + Firefox** (Safari ist bei WebGL wählerischer).

---

## Anhang – Geprüfte Ist-Werte (Referenz)

- `webGLCompressionFormat: 0` (Brotli) · `webGLDecompressionFallback: 0` (aus) · `webGLThreadsSupport: 0`
- `webGLLinkerTarget: 1` (WASM) · `webGLMemorySize: 32` (Legacy; Unity 6 wächst dynamisch)
- `m_ActiveColorSpace: 1` (Linear – ok, WebGL 2.0)
- URP PC-Asset: `SupportsHDR: 1`, `MSAA: 1` (=aus), `ShadowDistance: 50`, 4 Cascades,
  `AdditionalLightsShadowmapResolution: 2048`, `AdditionalLightsCookieResolution: 2048`
- Build-Szenen: `MainMenu.unity`, `SampleScene.unity`
- Assets gesamt: ~388 MB (MarpaStudio 141 · _Project 118 · Gley 78 · BOXOPHOBIC 18 · URP_Flares 14 · TMP 10)

---

## Prioritäten-Kurzfassung (TL;DR)

1. **WebGL-Modul installieren** (A2) + **Gzip & Decompression Fallback** (A1) ← ohne das läuft nichts auf itch.
2. `.unitypackage`-Leichen + `.mp4` löschen (B1).
3. URP-Schatten/Cascades/HDR runter (B5), Texturen komprimieren (B3), Audio Vorbis/Streaming (B4).
4. Build → **lokal per HTTP-Server testen** (C5) → **Inhalte** zippen (C6) → itch.io HTML-Projekt (C7).
5. Checkliste Teil D durchgehen.
