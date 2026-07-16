# Effekt-Guide: Sekundenschlaf (Micro Sleep)

Stand: 2026-07-16
Teil des generischen Effekt-Systems → siehe `PlayerEffectSystem_Guide.md`.

## 1. Verhalten
Der Charakter „nickt" für eine einstellbare Zeit kurz ein:
1. **Steuerung gesperrt:** Der Spieler kann für die Schlaf-Dauer **nicht lenken** (optional auch
   kein Gas/Bremse). Eingaben werden ignoriert.
2. **ZZZ-Anzeige:** Über dem Kopf erscheint eine **ZZZ**-Anzeige (wippt/pulsiert), solange geschlafen wird.
3. **Drift bleibt aktiv:** Der normale Zufalls-Drift des Mofas läuft weiter – das Mofa **driftet also
   unkontrolliert**, weil nicht gegengelenkt werden kann. Das ist die eigentliche „Gefahr".

Kostet **kein Leben** direkt – gefährlich wird nur das unkontrollierte Driften (in Wand/Hindernis).

**Häufiger, wenn man NÜCHTERN ist:** Der Effekt tritt öfter auf, wenn der Drunkenness-Multiplikator
(Score) **unter einem Schwellwert** liegt (Standard `< 2`, also nur bei 1×). Idee: müde statt betrunken.

## 2. Beteiligte Skripte / Assets
- Verhalten: `Assets/_Project/Scripts/Effects/Effects/MicroSleepEffectSO.cs`
  (`MicroSleepEffectSO` = Daten, `MicroSleepEffectRuntime` = Ablauf)
- Auslöser: `Assets/_Project/Scripts/Effects/Triggers/MicroSleepSpawner.cs`
  (Zufalls-Timer + „häufiger im nüchternen Zustand")
- ZZZ-Anzeige: `Assets/_Project/Scripts/Player/SleepIndicator.cs`
  (toggelt/animiert ein zugewiesenes ZZZ-Kind-Objekt; wird vom Effekt automatisch gefunden)
- Steuerungs-Sperre: `PlayerBalanceController.controlLockCount` / `PlayerThrottleController.controlLockCount`
  (Zähler: `>0` = gesperrt; stapelbar, selbst-aufhebend)

## 3. Voraussetzung (einmalig)
`PlayerEffectController` liegt auf dem `Player.prefab` (siehe `PlayerEffectSystem_Guide.md` §5).

## 4. Unity-Setup Schritt für Schritt
1. **Editor kompilieren lassen** (Fokus auf Unity), bis alle neuen Skripte durch sind
   (erzeugt auch die `.meta`-Dateien).

2. **Effekt-Asset:** Rechtsklick › Create › DDD › Effects › **Micro Sleep** → `SO_Effect_MicroSleep`.
   - **Sleep Duration** = z.B. `1.5` (wie lange geschlafen wird)
   - **Lock Steering** ✔ (Lenkung sperren)
   - **Lock Throttle** ✔ (auch Gas/Bremse sperren; aus = nur Lenkung)
   - *(optional)* **Sounds** = leises Schnarch-/„Wegnick"-Geräusch

3. **ZZZ-Anzeige bauen** (einmalig am Player):
   - Am `Player.prefab` ein Kind-Objekt anlegen, z.B. **`ZZZ`**.
     Empfehlung: als Kind des **Player-Roots** (bleibt aufrecht) und über dem Kopf positionieren
     (z.B. Position `Y ≈ 2`). Alternativ als Kind des `PlayerVisual`, dann kippt/dreht es aber mit dem Mofa mit.
   - Auf `ZZZ` einen Text setzen: **GameObject › 3D Object › Text – TextMeshPro** (oder Legacy „3D Text"),
     Inhalt `Z z Z`. So drehen, dass es zur Kamera zeigt. (Alternativ ein Sprite mit „ZZZ".)
   - **`ZZZ` deaktivieren** (Häkchen oben links im Inspector aus) – es soll nur beim Schlafen erscheinen.
   - Auf den **Player-Root** die Komponente **`SleepIndicator`** legen und **Indicator Root = `ZZZ`** zuweisen.
     (Der Effekt findet den `SleepIndicator` automatisch – keine weitere Verdrahtung nötig.)

4. **Auslöser:** Auf den `Player.prefab` die Komponente **`MicroSleepSpawner`** legen.
   - **Micro Sleep Effect** = `SO_Effect_MicroSleep`
   - **Min / Max Interval** = z.B. `10` / `25` (Abstand zwischen zwei Einschlaf-Effekten)
   - **Start Grace Period** = z.B. `8`
   - **More Frequent When Sober** ✔
   - **Sober Threshold** = `2` (unter diesem Score häufiger)
   - **Sober Frequency Multiplier** = `3` (dreimal so oft im nüchternen Zustand)

## 5. Die drei gewünschten Stellschrauben
| Wunsch | Wo einstellen |
|---|---|
| Wie lange geschlafen wird | `SO_Effect_MicroSleep` → **Sleep Duration** |
| Wie häufig / wie weit auseinander | `MicroSleepSpawner` → **Min Interval** / **Max Interval** |
| „Häufiger wenn nüchtern" | `MicroSleepSpawner` → **Sober Threshold** / **Sober Frequency Multiplier** |

## 6. Tuning-Hinweise
- **Drift zu stark/zu schwach:** Wie heftig das Mofa im Schlaf abdriftet, hängt am
  `PlayerBalanceController` (`balanceDriftSpeed`). Kleiner = das Mofa läuft im Schlaf gerader.
- **Nur Lenkung sperren:** Am Effekt **Lock Throttle** ausschalten – dann kann man im Schlaf noch
  beschleunigen/bremsen, aber nicht lenken.
- **Schwellwert:** `Sober Threshold = 2` heißt „nur bei Multiplikator 1×". Höher setzen (z.B. `4`),
  wenn der Sekundenschlaf über einen größeren nüchtern-Bereich häufiger sein soll.

## 7. Code-Änderungen (Touchpoints)
- **`PlayerBalanceController.cs`**: Feld `controlLockCount` (int). Bei `>0` wird die horizontale
  Eingabe auf 0 gesetzt (Drift läuft weiter).
- **`PlayerThrottleController.cs`**: Feld `controlLockCount` (int). Bei `>0` wird die vertikale
  Eingabe auf 0 gesetzt (Throttle ebbt weich ab).
- **Neu:** `SleepIndicator.cs` (ZZZ ein-/ausblenden + wippen/pulsieren),
  `MicroSleepEffectSO.cs` (Effekt), `MicroSleepSpawner.cs` (Auslöser).

## 8. Test-Checkliste
- [ ] Nach der Grace-Period schläft der Charakter in Zufalls-Abständen kurz ein.
- [ ] Während des Schlafs: **ZZZ** über dem Kopf sichtbar, Lenkung reagiert nicht, Mofa driftet.
- [ ] Nach `Sleep Duration` verschwindet ZZZ und die Steuerung ist wieder normal.
- [ ] Kein Lebensverlust durch den Effekt selbst; Console: `[Effects] Applied: <displayName>`.
- [ ] Im **nüchternen** Zustand (Multiplikator 1×) tritt der Effekt spürbar häufiger auf als im Rausch.
- [ ] Bei ausgeschaltetem **Lock Throttle**: Beschleunigen/Bremsen geht im Schlaf noch, Lenken nicht.
