# Effekt-Guide: Rampe / Schanze

Stand: 2026-07-15
Teil des generischen Effekt-Systems → siehe `PlayerEffectSystem_Guide.md`.

## 1. Verhalten
Der Spieler berührt eine Rampe → das **Mofa springt in einem Bogen hoch und landet wieder**
(hoch → runter → weiter), erhält eine **Belohnung** (Pizza und/oder kurzer Speed-Boost) und
**überfliegt währenddessen Hindernisse** (kein Schaden). Wände bleiben tödlich.

Wichtig: Der Sprung ist **rein visuell**. Nur `PlayerVisual` (das Mofa) wird angehoben; der
Spieler-Root mit Collider bleibt auf der Straße. Damit man trotzdem „über" ein Hindernis
kommt, wird für die Sprungdauer eine **Hindernis-Immunität** gesetzt (kein echter Physiksprung).

## 2. Beteiligte Skripte / Assets
- Verhalten: `Assets/_Project/Scripts/Effects/Effects/RampEffectSO.cs`
  (`RampEffectSO` = Daten, `RampEffectRuntime` = Ablauf)
- Immunität: `PlayerCollisionHandler.GrantObstacleImmunity(seconds)` (siehe Code-Änderungen §6)
- Belohnung: `PlayerLifeSystem.AddLife()` (Pizza), `RunSpeedManager.AddSpeedBonus()` (Speed)
- Auslöser: `PlayerEffectTriggerZone` (empfohlen) **oder** `ObstacleTypeSO.contactEffect`

## 3. Voraussetzung (einmalig)
Auf dem `Player.prefab` muss die Komponente **`PlayerEffectController`** liegen
(siehe `PlayerEffectSystem_Guide.md` §5). Ohne sie passiert nichts.

## 4. Unity-Setup Schritt für Schritt
1. **Asset:** Rechtsklick › Create › DDD › Effects › **Ramp Jump** → `SO_Effect_Ramp`.
   - **Jump Height** `2.5`, **Jump Duration** `0.9`
   - **Jump Clears Obstacles** ✔, **Extra Immunity Buffer** `0.15`
   - **Grants Pizza** ✔
   - **Speed Boost** `5`, **Speed Boost Duration** `2.5`
   - *(optional)* **Sounds** = Sprung-Clip
2. **Prefab:** Cube › umbenennen `Ramp`, als Rampe formen (Scale z.B. `3 / 0.6 / 3`, Rotation X `-25`),
   distinktes Material.
3. **Add Component › `Player Effect Trigger Zone`** (setzt den Collider automatisch auf *Is Trigger*).
   - **Effect** = `SO_Effect_Ramp`, **Trigger Once** ✔
4. **Tag** = `Untagged` lassen (NICHT „Obstacle").
5. In `Assets/_Project/Prefabs/Obstacles/` als Prefab ablegen.
6. In eine Chunk-Variante unter `AuthoredContent/Obstacles` platzieren (Spur x = −5 / 0 / +5,
   Y so, dass er auf der Straße liegt). Chunk muss im `RoadChunkManager` eingetragen sein.

## 5. Tuning & Reichweite
- Immunitäts-Fenster = `Jump Duration + Extra Immunity Buffer` (Standard 0,9 + 0,15 = **1,05 s**).
  Bei Tempo 10 ≈ **10 Einheiten** Fahrstrecke. Das zu überspringende Hindernis muss also
  innerhalb dieser Strecke **hinter** der Rampe stehen (Rampe kleineres Z, Hindernis knapp größeres Z).
- Reicht die Weite nicht: `Jump Duration` erhöhen (verlängert auch den optischen Sprung) oder
  nur `Extra Immunity Buffer` (Puffer ohne längeren Sprung).
- Speed-Boost nicht spürbar: `Speed Boost` erhöhen **und** ggf. `maxSpeed` im `RunSpeedManager`
  anheben (der Boost wird durch `maxSpeed` gedeckelt).
- Eine Rampe soll ausnahmsweise NICHT überfliegen: **Jump Clears Obstacles** aus.

## 6. Code-Änderungen (Touchpoints)
Diese Änderungen wurden für die Rampe gemacht (verhaltensneutral für bestehende Objekte):
- **`PlayerCollisionHandler.cs`**: Feld `obstacleImmunityTimer`, Methode `GrantObstacleImmunity(float)`,
  `Update()` zählt den Timer runter, `HandleHit()` ignoriert Hindernisse (kein Leben, kein Crash-Sound)
  solange der Timer läuft. Wände sind ausgenommen.
- **`ObstacleTypeSO.cs`**: Feld `contactEffect` (optional). Wenn gesetzt, löst das Hindernis den Effekt
  aus statt Schaden zu machen → so kann eine Rampe auch als „Obstacle" gebaut werden.
- **`PlayerEffectContext.cs`**: Referenz `CollisionHandler`; **`PlayerEffectController.cs`** befüllt sie.
- **`RunSpeedManager.cs`**: `AddSpeedBonus(float)` (Boost, pre-`maxSpeed`-Clamp addiert).

## 7. Test-Checkliste
- [ ] Über die Rampe fahren → Mofa springt im Bogen und landet, Fahrt läuft weiter.
- [ ] Pizza/Leben kommt dazu (HUD), kurzer Speed-Schub spürbar.
- [ ] Ein Hindernis knapp hinter der Rampe wird überflogen → **kein** Lebensverlust,
      Console: `[Collision] Ignored (airborne): …`.
- [ ] Hindernis zu weit hinter der Rampe (außerhalb ~10 Einheiten) → wird wieder normal getroffen.
- [ ] Normale Hindernisse ohne `contactEffect` verhalten sich unverändert (Leben −1, Crash-Sound).

## 8. Fehlerbehebung
- **„Ich verliere trotzdem ein Leben beim Sprung":** Hindernis steht zu weit hinter der Rampe
  (Immunität schon abgelaufen) → Abstand verkleinern oder `Jump Duration` / `Extra Immunity Buffer` erhöhen.
- **„Nichts passiert beim Drüberfahren":** `PlayerEffectController` fehlt am Player, oder der
  Collider der Rampe ist nicht *Is Trigger*, oder der Player ist nicht mit Tag „Player" getaggt.
