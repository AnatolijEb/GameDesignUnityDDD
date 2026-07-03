# Strukturierte Zusammenfassung: Spielersteuerung im GameDesignUnityDDD Projekt

Ich habe das gesamte Repository analysiert und die Balance-basierte Spielersteuerung vollständig dokumentiert. Hier ist deine Übersicht:

---

# 1. KERN-ARCHITEKTUR: Spielersteuerung

Das Projekt nutzt eine **Balance-basierte Steering-Mechanik** mit einer **World-Scrolling-Architektur**:

- Der Spieler bleibt auf **Z ≈ 0** fest und bewegt sich nur auf der **X-Achse** (links/rechts)
- Die Welt bewegt sich auf den Spieler zu (negative Z-Achse)
- Die Lenkung basiert auf Gleichgewicht/Balance, nicht direkter Eingabe

---

# 2. HAUPTSKRIPTE FÜR SPIELERSTEUERUNG

## A. PlayerBalanceController.cs

**Pfad:**  
`/Assets/_Project/Scripts/Player/PlayerBalanceController.cs`

### Funktion

Verwaltet den Balance-Winkel (-1 bis +1) und reagiert auf Spielereingaben.

### Public/Serialized Fields (Inspector-sichtbar)

| Feld | Beschreibung |
|------|--------------|
| `float balanceDriftSpeed = 0.6f` | Geschwindigkeit des zufälligen Abdrifts (wie schnell sich das Gleichgewicht von selbst verschiebt) |
| `float counterForce = 2.5f` | Wie stark der Spieler das Gleichgewicht korrigieren kann (Input-Reaktivität) |
| `float maxTiltAngle = 30f` | Maximaler Neigungswinkel in Grad (visuelle Rotation) |
| `float driftChangeMinTime = 1.5f` | Minimale Zeit zwischen Abdrift-Richtungswechseln |
| `float driftChangeMaxTime = 3.5f` | Maximale Zeit zwischen Abdrift-Richtungswechseln |
| `Transform visualTarget` | Das Objekt, das rotiert wird (normalerweise `PlayerVisual`) |

### Wichtige Methode

```csharp
public float BalanceAngle => balanceAngle;
```

Gibt den aktuellen Balance-Winkel (-1 bis +1) zurück.

### Logik (Update-Loop, Zeilen 29–50)

1. Zufälliger Abdrift:
   - `balanceAngle` wird kontinuierlich um `driftDirection * balanceDriftSpeed * deltaTime` verschoben.

2. Player Input:
   - `Input.GetAxis("Horizontal") * counterForce` wird addiert.

3. Clamping:
   - `balanceAngle` bleibt zwischen `-1` und `+1`.

4. Visuelle Rotation:

```csharp
visualTarget.rotation =
Quaternion.Euler(0, 0, -balanceAngle * maxTiltAngle);
```

### Input-System

```csharp
Input.GetAxis("Horizontal")
```

(Altes Unity Input System, Achse `"Horizontal"`)

---

## B. PlayerMovementController.cs

**Pfad:**  
`/Assets/_Project/Scripts/Player/PlayerMovementController.cs`

### Funktion

Setzt den Balance-Winkel in tatsächliche Seitwärtsbewegung um.

### Public Fields

| Feld | Beschreibung |
|------|--------------|
| `float steerStrength = 4f` | Multiplikator für die Seitwärtsbewegungs-Geschwindigkeit |
| `PlayerBalanceController balanceController` | Referenz zum Balance-Controller |

### Logik (Update-Loop, Zeilen 21–34)

1. Seitwärtsbewegung:

```csharp
transform.Translate(
    Vector3.right *
    balanceController.BalanceAngle *
    steerStrength *
    deltaTime,
    Space.World
);
```

2. X-Position clampen
   - Position.x wird auf `[-7.25f, 7.25f]` begrenzt.

3. Z-Position sperren
   - Position.z wird auf `initialZ` gesetzt.

---

## C. PlayerController.cs (ALTE IMPLEMENTIERUNG)

**Pfad:**  
`/Assets/_Project/Scripts/PlayerController.cs`

### Warnung

Dies ist eine ältere monolithische Implementierung, welche Balance- und Bewegungslogik kombiniert. Sie wird vermutlich bald vollständig durch die neuen Player-Skripte ersetzt.

### Fields (Zeilen 6–12)

- `float moveSpeed = 10f`
- `float balanceDriftSpeed = 0.6f`
- `float counterForce = 2.5f`
- `float steerStrength = 4f`
- `float maxTiltAngle = 30f`

---

# 3. GESCHWINDIGKEIT & SPEED-MANAGEMENT

## RunSpeedManager.cs

**Pfad:**  
`/Assets/_Project/Scripts/Core/RunSpeedManager.cs`

### Funktion

Zentrale Verwaltung der globalen Vorwärts-Geschwindigkeit.

### Public Properties

| Property | Beschreibung |
|----------|--------------|
| `CurrentSpeed` | Aktuelle Scrolling-Geschwindigkeit |
| `DistanceTravelled` | Gesamte zurückgelegte Distanz |

### Public Fields (Inspector)

| Feld | Beschreibung |
|------|--------------|
| `baseSpeed = 10f` | Basis-Geschwindigkeit |
| `speedIncreasePerSecond = 0f` | Kontinuierliche Beschleunigung |
| `maxSpeed = 20f` | Oberes Speed-Limit |

### Architektur

- Singleton Pattern (`RunSpeedManager.Instance`)
- Wird von `WorldScrollMover` genutzt.
- Wird von `RoadChunkManager` genutzt.

### Beziehung zur Balance

Die Systeme sind vollständig getrennt.

- **Speed** bestimmt die Vorwärtsbewegung der Welt.
- **Balance** bestimmt ausschließlich die Seitwärtsposition des Spielers.

---

# 4. INPUT-SYSTEM

## Eingabeachse

```csharp
Input.GetAxis("Horizontal")
```

Wird gelesen in:

- `PlayerBalanceController.cs`
- `PlayerController.cs` (alt)

### Steuerung

- A / D
- Pfeiltasten links / rechts

Der Wert wird mit `counterForce` multipliziert und verändert dadurch die Balance.

---

# 5. KOLLISIONS- & PHYSIK-KOMPONENTEN

## Physik-Setup

- Rigidbody: **Continuous Speculative**
- Interpolation: **Enabled**
- Collider: Trigger-Collider mit Tags:
  - `"Wall"`
  - `"Obstacle"`

### Player

- Scale: **1.8**
- Y-Position: **0.9**

Es werden **keine WheelCollider oder CharacterController** verwendet.

Die Bewegung erfolgt vollständig über eigene Scripts.

---

# 6. KOLLISIONS-HANDLING

## PlayerCollisionHandler.cs

**Pfad:**  
`/Assets/_Project/Scripts/Player/PlayerCollisionHandler.cs`

### Funktion

Erkennt Treffer mit Wänden und Hindernissen.

### Methoden

```csharp
OnTriggerEnter(Collider other)
OnCollisionEnter(Collision collision)
```

Beide rufen intern auf:

```csharp
HandleHit()
```

### HandleHit()

Prüft auf Tags:

- `"Wall"` → Spiel vorbei
- `"Obstacle"` → Leben verlieren (`PlayerLifeSystem`)

Zusätzlich wird ein zufälliger Treffer-Sound abgespielt.

---

# 7. LEBEN-SYSTEM

## PlayerLifeSystem.cs

**Pfad:**  
`/Assets/_Project/Scripts/Player/PlayerLifeSystem.cs`

### Public Fields

| Feld | Beschreibung |
|------|--------------|
| `maxLives = 4` | Startleben |
| `invulnerabilityDuration = 1.5f` | Schutzzeit nach Treffer |

### Mechanik

- Start mit 4 Leben
- Jeder Treffer kostet 1 Leben
- 0 Leben → Game Over
- Event: `OnLivesChanged`

---

# 8. GAME-MANAGEMENT

## GameManager.cs

**Pfad:**  
`/Assets/_Project/Scripts/Core/GameManager.cs`

### Funktion

Globale Spielverwaltung.

- Singleton
- Startet Musik
- Öffnet Game-Over-Menü
- Lädt Szene neu

---

# 9. WELT-SCROLLING

## WorldScrollMover.cs

**Pfad:**  
`/Assets/_Project/Scripts/World/WorldScrollMover.cs`

### Logik

```csharp
transform.Translate(
    Vector3.back *
    runSpeedManager.CurrentSpeed *
    Time.deltaTime,
    Space.World
);
```

Alle Objekte bewegen sich rückwärts.

---

## RoadChunkManager.cs

**Pfad:**  
`/Assets/_Project/Scripts/World/RoadChunkManager.cs`

### Wichtige Parameter

| Parameter | Beschreibung |
|-----------|--------------|
| `chunkLength = 90f` | Länge eines Chunks |
| `initialChunks = 10` | Start-Chunks |
| `chunksAhead = 10` | Voraus gespawnte Chunks |
| `despawnZ = -80f` | Löschgrenze |
| `enableDeliveryGates = false` | Optionales Delivery-System |

---

# 10. STEUERUNGS-PARAMETER (TUNING)

Alle Werte können direkt im Unity Inspector angepasst werden.

| Parameter | Beschreibung | Typ | Standard |
|------------|--------------|------|----------|
| balanceDriftSpeed | Zufälliger Abdrift | float | 0.6 |
| counterForce | Input-Reaktivität | float | 2.5 |
| steerStrength | Seitwärtsbewegung | float | 4.0 |
| maxTiltAngle | Maximale Neigung | float | 30° |
| baseSpeed | Scroll-Geschwindigkeit | float | 10 |
| speedIncreasePerSecond | Beschleunigung | float | 0 |
| maxSpeed | Oberes Speed-Limit | float | 20 |

---

# 11. DOKUMENTATION

Relevante Dateien:

- `/Assets/_Project/Documentation/DDD_Projektguide.md`
- `/Assets/_Project/Documentation/2026-06-10_18-45_current_state_summary.md`
- `/Assets/_Project/Documentation/2026-06-11_15-53_Gameplay_Tuning_and_Fixes.md`

---

# 12. ERWEITERUNGSPLAN FÜR DEINE ANFORDERUNGEN

## A. Dynamischere Geschwindigkeitssteuerung basierend auf Balance

### Aktuell

Balance beeinflusst ausschließlich die horizontale Position.

### Erweiterung

Im `RunSpeedManager`:

```csharp
currentSpeed =
baseSpeed *
(1 + balanceAngle * speedModifier);
```

Dadurch entsteht eine Beschleunigung durch Neigung.

---

## B. Vorwärts-/Rückwärts-Beschleunigung

### Aktuell

Der Spieler fährt immer mit konstanter Geschwindigkeit.

### Erweiterung

Neue Eingabe:

```csharp
Input.GetAxis("Vertical")
```

Möglichkeiten:

- `baseSpeed` erhöhen/verringern
- eigene Velocity-Variable einführen

---

## C. Realistischere Balance-Physik

### Aktuell

Lineares Clamping zwischen -1 und +1.

### Erweiterung

- Trägheit
- Aerodynamische Gegenkraft
- Abhängigkeit von Straßensteigung

---

# 13. PLAYER PREFAB STRUKTUR

```text
Player (Root)
├── PlayerVisual
│   └── 3D-Modell
├── Collider
├── Rigidbody
├── PlayerBalanceController
├── PlayerMovementController
├── PlayerLifeSystem
└── PlayerCollisionHandler
```

---

# ZUSAMMENFASSUNG FÜR DEN EXTENSION-PLAN

Die Architektur besteht aktuell aus drei sauber getrennten Verantwortlichkeiten:

- **PlayerBalanceController**
  - berechnet das Gleichgewicht

- **PlayerMovementController**
  - übersetzt Balance in Seitwärtsbewegung

- **RunSpeedManager**
  - verwaltet die Vorwärtsgeschwindigkeit

Für die geplanten Erweiterungen (Geschwindigkeitskopplung, Vorwärts-/Rückwärtssteuerung) müssen hauptsächlich `RunSpeedManager` sowie `PlayerBalanceController` erweitert werden.

---

# Verifizierter IST-Zustand

Die drei zentralen Skripte wurden direkt geprüft.

## Aktuelle Architektur

### PlayerBalanceController.cs

Berechnet `balanceAngle` (-1 bis 1) aus:

- zufälligem Drift
- `Input.GetAxis("Horizontal")`

Kein Bezug zur Geschwindigkeit.

### PlayerMovementController.cs

Übersetzt

```text
balanceAngle * steerStrength
```

in eine X-Bewegung.

`steerStrength` ist konstant.

### RunSpeedManager.cs

Steuert ausschließlich die Scroll-Geschwindigkeit der Welt.

Kennt lediglich:

- baseSpeed
- speedIncreasePerSecond
- maxSpeed

Keine Spieler-Eingabe.

---

# Ziel 1: Lenkung realistischer machen

Betroffen:

- `PlayerBalanceController.cs`

### Ideen

- SmoothDamp statt direkter Änderung
- Lerp zum Zielwert
- Neue Inspector-Felder:
  - `balanceSmoothing`
  - `inputResponsiveness`
- Rückstell-Trägheit

---

# Ziel 2: Vor-/Zurück-Steuerung

Aktuell existiert keine Geschwindigkeitssteuerung durch den Spieler.

## Neuer Controller

```text
PlayerThrottleController.cs
```

Liest:

```csharp
Input.GetAxis("Vertical")
```

und liefert:

```text
Throttle (-1 bis +1)
```

## RunSpeedManager erweitern

Neue Parameter:

- defaultSpeed
- minSpeed
- maxSpeed
- accelerationRate
- decelerationRate

Zusätzlich Referenz auf:

```text
PlayerThrottleController
```

---

# Ziel 3: Lenkung abhängig von Geschwindigkeit

Kopplung zwischen beiden bisher getrennten Systemen.

Beispiel:

```text
speedRatio =
CurrentSpeed /
defaultSpeed
```

Anschließend:

```text
effectiveSteer =
steerStrength *
speedRatio
```

Optional begrenzt über:

- Clamp
- AnimationCurve

---

# Zentrale Inspector-Steuerung

## Option 1 (empfohlen)

RunSpeedManager wird zentraler Hub.

Neue Parameter:

- defaultSpeed
- minSpeed
- maxSpeed
- accelerationRate
- decelerationRate
- steerSpeedMultiplier

Außerdem Referenzen auf:

- PlayerBalanceController
- PlayerThrottleController

---

## Option 2

ScriptableObject

```text
PlayerControlSettings
```

enthält sämtliche Tuning-Werte.

Vorteil:

- mehrere Fahrzeug-Presets
- keine Szenen-Anpassungen

Für das aktuelle Projekt wird Option 1 empfohlen.

---

# Betroffene Dateien

| Datei | Änderung |
|--------|----------|
| PlayerBalanceController.cs | Trägheit, Smoothing, Speed-Faktor |
| PlayerMovementController.cs | Optional Speed-Faktor auf steerStrength |
| RunSpeedManager.cs | defaultSpeed, min/max, Beschleunigung, Throttle-Controller |
| PlayerThrottleController.cs | **Neu** |
| PlayerController.cs | Alte Version ignorieren bzw. entfernen |

---

Wenn die Umsetzung erfolgen soll, wäre die sinnvolle Reihenfolge:

1. `PlayerThrottleController`
2. Erweiterung des `RunSpeedManager`
3. Kopplung von Geschwindigkeit und Balance-/Movement-System