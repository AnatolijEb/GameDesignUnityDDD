# Effekt-Guide: Öl-Pfütze (Öl-Dreher)

Stand: 2026-07-15
Teil des generischen Effekt-Systems → siehe `PlayerEffectSystem_Guide.md`.

## 1. Verhalten
Die Öl-Pfütze ist **Hindernis und Effekt in einem**. Beim Überfahren:
1. **Eindrehen:** Mofa dreht sich 1,5× → schaut rückwärts.
2. **Rückwärts (einstellbare Zeit):** schaut rückwärts, **Lenkung invertiert** (nur links/rechts;
   Gas/Bremse hoch/runter bleibt normal), **immun gegen Hindernisse**. Die **Welt scrollt normal weiter**.
3. **Ausdrehen:** dreht sich wieder nach vorne und fährt normal weiter.

Kostet **kein Leben** – die „Strafe" ist der Kontrollverlust, nicht Schaden.

## 2. Architektur-Hinweis (wichtig)
Der Spieler bewegt sich in diesem Spiel nicht selbst vorwärts – die **Welt scrollt** auf ihn zu
(`WorldScrollMover` × `RunSpeedManager.CurrentSpeed`). Der Öl-Dreher **verändert die Welt-
Geschwindigkeit bewusst NICHT**: die Map läuft die ganze Zeit normal auf den Spieler zu.
„Rückwärtsfahren" ist hier **rein visuell** – nur das Mofa dreht sich um (`VisualYaw`) und die
Steuerung wird invertiert. Es sieht dadurch aus, als würde der Spieler rückwärts fahren, während
die Welt ganz normal in −Z an ihm vorbeizieht.

## 3. Beteiligte Skripte / Assets
- Verhalten: `Assets/_Project/Scripts/Effects/Effects/OilSpinEffectSO.cs`
  (`OilSpinEffectSO` = Daten, `OilSpinEffectRuntime` = Ablauf)
- Drehung: `PlayerEffectController.VisualYaw` (vom `PlayerBalanceController` in die Rotation eingerechnet)
- Steuerung: `PlayerBalanceController.steeringSign` / `PlayerThrottleController.throttleSign`
- Immunität: `PlayerCollisionHandler.GrantObstacleImmunity(float)`
- Auslöser: `ObstacleTypeSO.contactEffect` (die Pfütze ist ein „Obstacle")

## 4. Voraussetzung (einmalig)
`PlayerEffectController` liegt auf dem `Player.prefab` (siehe `PlayerEffectSystem_Guide.md` §5).

## 5. Unity-Setup Schritt für Schritt
1. **Effekt-Asset:** Rechtsklick › Create › DDD › Effects › **Oil Spin** → `SO_Effect_OilSpin`.
   - **Spins** `1.5`, **Spin In Duration** `0.5`, **Spin Out Duration** `0.5`
   - **Reverse Duration** `2` (wie lange rückwärts geschaut wird + Lenkung invertiert bleibt)
   - **Invert Steering** ✔ (links/rechts tauschen), **Invert Throttle** aus (hoch/runter = Gas/Bremse bleibt normal)
   - **Immune While Active** ✔
   - *(optional)* **Sounds** = Reifenquietsch-Clip
2. **Pfützen-Prefab:** Plane oder flacher Cube › umbenennen `OilPuddle`, flach auf den Boden
   (Y ≈ `0.02`), dunkles/glänzendes Material.
   - **Tag** = **`Obstacle`**
   - **Box Collider** hinzufügen, **Is Trigger** ✔ (flach über die Pfütze)
   - **`Obstacle Base`** hinzufügen
   - als Prefab in `Assets/_Project/Prefabs/Obstacles/` ablegen
3. **Obstacle-Daten:** Rechtsklick › Create › DDD › Obstacle Type → `SO_Obstacle_OilPuddle`.
   - **Prefab** = `OilPuddle`
   - **Pizzas Lost On Contact** = `0`
   - **Contact Effect** = **`SO_Effect_OilSpin`**  ← entscheidende Verknüpfung
   - Am `OilPuddle`-Prefab in `Obstacle Base` das Feld **Obstacle Data** = `SO_Obstacle_OilPuddle`
4. **Platzieren:** das **Prefab** `OilPuddle` (nie das SO!) manuell in eine Chunk-Variante
   (`AuthoredContent/Obstacles`) ziehen **oder** `SO_Obstacle_OilPuddle` in die `Obstacle Types`-Liste
   eines `ObstacleSpawner` eintragen (zufälliges Spawnen). Für einen Szenen-Schnelltest: Prefab in die
   Szene ziehen und ihm einen `WorldScrollMover` geben.

## 6. Tuning
- **Reverse Duration**: wie lange das Mofa rückwärts schaut und die Steuerung vertauscht bleibt.
  Beliebig lang möglich, da die Welt normal weiterläuft (kein Puffer-Limit).
- **Spins**: sollte auf `x.5` enden (1.5, 2.5 …), damit das Mofa in der Rückwärtsphase auch
  wirklich rückwärts schaut. Ganze Zahlen würden vorwärts enden.
- Die Welt-Scroll-Geschwindigkeit wird vom Effekt **nicht** verändert.

## 7. Code-Änderungen (Touchpoints)
- **`PlayerEffectController.cs`**: Property `VisualYaw` (Extra-Drehwinkel um die Hochachse).
- **`PlayerBalanceController.cs`**: rechnet `VisualYaw` in die Visual-Rotation ein
  (`Euler(0, effectYaw, -tilt)`), statt nur den Neigungswinkel zu setzen.
- (bereits für andere Effekte vorhanden, hier wiederverwendet:) `steeringSign`/`throttleSign`
  (Steuerung invertieren), `GrantObstacleImmunity`, `ObstacleTypeSO.contactEffect`.
- **Hinweis:** `RunSpeedManager.SetScrollMultiplier(float)` existiert als generischer Hook, um die
  Welt-Scroll-Geschwindigkeit zu verändern (Slow-mo / Freeze / rückwärts). Der Öl-Dreher nutzt ihn
  **bewusst nicht** – die Welt bleibt normal.

## 8. Test-Checkliste
- [ ] Über die Pfütze fahren → Mofa dreht 1,5× und schaut rückwärts.
- [ ] In der Rückwärtsphase: Welt läuft **normal weiter** (zieht in −Z vorbei), Mofa schaut
      rückwärts. Lenkung ist vertauscht (links↔rechts), aber Pfeil-hoch = schneller /
      Pfeil-runter = langsamer bleibt normal.
- [ ] Links drücken → Spieler fährt nach rechts UND neigt sich sichtbar nach rechts
      (Neigung und Bewegung zeigen in dieselbe Richtung, dank cos(Yaw)-Kopplung im PlayerBalanceController).
- [ ] Danach dreht das Mofa wieder nach vorne und fährt normal weiter.
- [ ] Kein Lebensverlust; Console: `[Effects] Applied: Oil Spin`.
- [ ] Nach dem Effekt ist die Steuerung wieder normal (steeringSign/throttleSign zurück auf 1).
