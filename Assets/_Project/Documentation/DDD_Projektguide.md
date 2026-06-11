# Don't Drink and Drive – Projektguide

## 1. Projekt-Kontext

**Projekt-Name:** Don't Drink and Drive
**Genre:** 3D Endless Runner (PC)
**Render Pipeline:** URP (Universal Render Pipeline)
**Kern-Mechanik:** "Drunk Steering" (Balance-basiert). Der Spieler steuert nicht direkt, sondern beeinflusst die Schräglage (Balance). Die Schräglage erzeugt die Seitwärtsbewegung.

### Technische Architektur: World-Scrolling

- **Spieler:** Bleibt fix auf der Z-Achse (Z ≈ 0). Er bewegt sich nur auf der X-Achse (links/rechts).
- **Welt:** Bewegt sich auf den Spieler zu (entlang der negativen Z-Achse).
- **Generierung:** Der `RoadChunkManager` spawnt Straßen-Abschnitte (`RoadChunk_Basic`) vor dem Spieler und löscht sie hinter ihm.
- **Geschwindigkeit:** Wird zentral vom `RunSpeedManager` (baseSpeed) gesteuert.

### Wichtige Skripte & Pfade

| Skript | Funktion |
|--------|----------|
| `RoadChunkManager.cs` | Steuert das Spawnen der Welt |
| `DifficultyManager.cs` | Regelt Tiers (Stufen), Geschwindigkeit und Hindernis-Dichte |
| `PlayerBalanceController.cs` | Berechnet den Neigungswinkel und reagiert auf A/D / Pfeiltasten |
| `PlayerMovementController.cs` | Setzt den Winkel in Seitwärtsbewegung um |
| `ObstacleSpawner.cs` | Platziert Hindernisse zufällig in einer `ObstacleSpawnArea` |

---

## 2. Anleitung: Integration eigener Assets

### A. Den Spieler (Motorrad/Fahrer) ersetzen

**Ziel:** Das aktuelle graue Quadrat durch ein eigenes Modell ersetzen.

1. **Ordner:** Lege deine Modelle unter `Assets/_Project/Models/Player/` ab.
2. Öffne das Prefab `Assets/_Project/Prefabs/Player/Player.prefab`.
3. Suche das Kind-Objekt **`PlayerVisual`**.
4. Lösche das Platzhalter-Modell (Cube) darunter und ziehe dein neues 3D-Modell als Kind hinein.
5. Dein Modell sollte nach vorne (positive Z-Achse) schauen.
6. Passe die Skalierung an, sodass es auf die Straße passt (Referenz: Straßenbreite ist 15 Einheiten).

> Das Skript `PlayerBalanceController` rotiert automatisch das Objekt `PlayerVisual` – dein Modell neigt sich also sofort mit.

---

### B. Die Straße & Umgebung (Häuser/Straßenrand)

**Ziel:** Die Straße hübscher machen und Gebäude hinzufügen, die vorbeiziehen.

1. **Ordner:** `Assets/_Project/Prefabs/World/`
2. Öffne das Prefab **`RoadChunk_Basic.prefab`**.
3. **Straße:** Ersetze das Kind-Objekt "Road" durch dein eigenes Straßen-Mesh. Achte darauf, dass es wieder **30 Einheiten lang** ist (oder passe `chunkLength` im `RoadChunk`-Skript an).
4. **Straßenrand:** Ziehe deine Gebäude-Modelle oder Mauern links und rechts neben die Straße **innerhalb des Prefabs**.

> Da alles im Prefab liegt, bewegen sich die Häuser automatisch mit der Straße nach hinten – das erzeugt den Effekt der Vorbeifahrt.

> **Wichtig:** Randbegrenzungen (Mauern) müssen den Tag **"Wall"** haben und einen **BoxCollider (Is Trigger)** besitzen, damit der Spieler bei Berührung stirbt.

---

### C. Eigene Hindernisse (Autos, Tonnen, etc.)

**Ziel:** Eigene Modelle als ausweichbare Objekte nutzen.

1. **Ordner:** `Assets/_Project/Prefabs/Obstacles/`
2. Erstelle für jedes Modell ein neues Prefab.
3. Füge einen **Collider (Is Trigger)** hinzu und setze den Tag auf **"Obstacle"**.
4. Füge die Komponente **`ObstacleBase`** hinzu.
5. Erstelle ein ScriptableObject via **Right Click > Create > DDD > Obstacle Type** und verknüpfe dein neues Prefab.
6. Trage dieses Daten-Objekt im `Obstacle Spawner` des `RoadChunk_Basic`-Prefabs ein.

---

### D. Skybox & Atmosphäre

**Ziel:** Die Umgebung (Himmel, Licht) anpassen.

**Skybox:**
- Gehe zu **Window > Rendering > Lighting**.
- Im Reiter **Environment** kannst du bei **Skybox Material** dein eigenes Material einfügen (z.B. ein Nacht-Himmel oder HDRi).

**Nacht-Look:**
- Reduziere die Intensität des "Directional Light" in der Szene.
- Ändere die Farbe des Lichts auf ein dunkles Blau.
- Nutze das **Global Volume** (unter dem Objekt `Lighting` in der Hierarchy), um Bloom oder Color Grading hinzuzufügen.

---

## 3. Empfohlene Ordnerstruktur

```
Assets/_Project/
├── Models/          # 3D-Modelle (FBX/OBJ)
├── Textures/        # Texturen
├── Materials/       # Unity Materialien
└── Prefabs/
    ├── Player/
    ├── World/
    └── Obstacles/
```

> **Wichtiger Hinweis:** Alle Objekte, die "vorbeiziehen" sollen, müssen Kinder des `RoadChunk_Basic`-Prefabs sein oder vom `RoadChunkManager` gespawnt werden. Nur so greift die World-Scrolling-Logik.
