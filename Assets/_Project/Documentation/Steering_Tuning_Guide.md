# Steuerungs-Tuning – Guide

Stand: 2026-07-17

Ziel: Das Fahrgefühl (Lenken, Neigung, Eindrehen, Kurven-Vortrieb) an **einer** Stelle und
möglichst **visuell** (über Kurven) einstellen – ohne im Code herumzustochern.

Das Grundprinzip bleibt: seitlicher „Slider" auf scrollender Welt + betrunkenes Schwanken.
Es wurde **nichts** am bestehenden Prinzip zerstört – bei linearen Standard-Kurven verhält sich
alles wie vorher, nur Yaw (Eindrehen) und Cornering-Slowdown kommen als dezente Zusätze dazu.

---

## 1. Wo stelle ich was ein? (3 Komponenten)

### A) `PlayerBalanceController` (auf dem Player-Root) – das „Lenk-Hirn"
Im Inspector in **zwei Blöcke** getrennt:

**Block „SPIELER-STEUERUNG (Lenken)"** – was der Spieler aktiv beeinflusst:
| Feld | Wirkung |
|---|---|
| `counterForce` | **Wie schnell** sich das Gewicht verlagert (Reaktion auf die Taste). Höher = direkter. |
| `scaleWithSpeed` | Lenkung an Fahrgeschwindigkeit koppeln (schneller = schärfer). |
| **`steerResponseCurve`** ⭐ | **Die Lenk-Kurve.** Formt Lenkeinschlag → Wirkung. Gilt für Neigung, Eindrehen UND Bewegung gemeinsam. |
| `maxTiltAngle` | Maximaler **Neigungswinkel** (Z-Roll) bei vollem Lenkeinschlag. |
| **`maxTurnAngle`** ⭐ | **Eindrehen (Yaw):** wie weit die Nase sich in die Kurve dreht (~10° dezent). 0 = aus. |

**Block „BETRUNKENES SCHWANKEN (automatisch)"** – läuft ohne Spieler-Input:
| Feld | Wirkung |
|---|---|
| `balanceDriftSpeed` | Stärke des betrunkenen Schwankens (zieht von allein zur Seite). |
| `driftChangeMinTime` / `driftChangeMaxTime` | Wie oft die Schwank-Richtung neu gewürfelt wird (Sek.). |

### B) `PlayerMovementController` (auf dem Player-Root) – der „Muskel"
| Feld | Wirkung |
|---|---|
| `steerStrength` | **Wie schnell** man sich tatsächlich seitlich (X) bewegt. |
| `scaleWithSpeed` | Seitwärts-Tempo an Fahrgeschwindigkeit koppeln. |
| `maxX` | Straßenrand (= Wand), an dem gestoppt wird. |

### C) `RunSpeedManager` – Speed & Kurven-Vortrieb
| Feld | Wirkung |
|---|---|
| `steerMultiplierAtMinSpeed` / `steerMultiplierAtMaxSpeed` | Lenkschärfe bei langsam / schnell (Speed-Kopplung). |
| **`enableCorneringSlowdown`** ⭐ | Diagonale Fahrt bremst den Welt-Vortrieb (an/aus). |
| **`corneringSlowdownAtFullSteer`** ⭐ | Max. Vortriebs-Reduktion bei vollem Lenken (0.12 = 12 %). |
| **`corneringSlowdownCurve`** ⭐ | Wie stark die Verlangsamung mit dem Lenkeinschlag zunimmt. |
| `baseSpeed / maxSpeed / minSpeed` | Grund-, Höchst-, Mindest-Tempo der Welt. |
| **`enableDistanceSpeedRamp`** ⭐ | Grundtempo steigt mit der Strecke (je weiter, desto schneller). An/aus. |
| **`baseSpeedGainPer100Units`** ⭐ | +Tempo pro 100 Strecke-Einheiten (0.4 = Default). Durch `maxSpeed` gedeckelt. |

### D) `PlayerThrottleController` (auf dem Player-Root) – Gas/Bremse mit Zeit-Limit
| Feld | Wirkung |
|---|---|
| `throttleResponsiveness` | Wie direkt/weich das Gas dem Tastendruck folgt. |
| **`enableThrottleTimeLimit`** ⭐ | Zeit-Limit an/aus. Aus = Gas/Bremse unbegrenzt (wie früher). |
| **`maxThrottleDuration`** ⭐ | Wie lange man am Stück schneller/langsamer fahren darf, dann automatisch zurück auf Basis (Default 3 s). |
| **`throttleCooldown`** ⭐ | Puffer/Pause danach, bevor man wieder Gas/Bremse geben kann (Default 3 s). |

Ablauf: erster Gas/Bremse-Impuls → **Active** (`maxThrottleDuration`) → automatisch Basis-Geschwindigkeit
→ **Cooldown/Puffer** (`throttleCooldown`, gesperrt) → wieder frei. Gilt für Beschleunigen und Bremsen
gemeinsam. Rampen-/Effekt-Boosts sind davon **nicht** betroffen.

⭐ = neu / die visuell tunebaren Regler.

---

## 2. Die Kurven visuell einstellen (der Kern)

Alle `…Curve`-Felder öffnen im Inspector den **grafischen Kurven-Editor**. Immer gilt:
**x = Lenkeinschlag 0…1** (0 = Mitte/geradeaus, 1 = voll eingeschlagen), **y = Wirkung 0…1**.

### `steerResponseCurve` (PlayerBalanceController) – das wichtigste Feld
Formt, wie sich der Lenkeinschlag „anfühlt". Neigung, Eindrehen und Seitwärtsbewegung nutzen
denselben geformten Wert → **Optik und Bewegung passen immer zusammen** (löst das „voll geneigt,
fährt aber gemütlich"-Problem).

| Kurvenform | Fahrgefühl |
|---|---|
| Gerade Linie (0,0)→(1,1) | Linear = wie bisher (neutral). |
| Flach → steil (Ease-In) | Sanfte, präzise Mitte, giftiges Ende. Gut für „ruhig cruisen, hart einlenken". |
| Steil → flach (Ease-Out) | Sehr direkt aus der Mitte, oben abflachend. Sehr agil/arcadig. |
| S-Kurve | Tote Mitte + kräftiges Ende – ruhig, aber mit klarem „Biss". |

> Tipp: Rechtsklick auf einen Keyframe → *Right Tangent / Left Tangent* für weiche Übergänge.

### `corneringSlowdownCurve` (RunSpeedManager)
Wie schnell der Vortrieb bei zunehmendem Lenken nachgibt. Linear = gleichmäßig; Ease-In =
erst spät (nur bei hartem Lenken) spürbar.

---

## 3. Schnell-Rezepte

- **„Dynamischer & direkter"**: `steerResponseCurve` als Ease-Out (steil aus der Mitte),
  `counterForce` etwas höher, `maxTurnAngle` ~10.
- **„Präzise, weniger zappelig"**: `steerResponseCurve` als Ease-In oder leichte S-Kurve,
  `balanceDriftSpeed` etwas runter.
- **„Realistischeres Kurvenfahren"**: `maxTurnAngle` 8–12, `enableCorneringSlowdown` an,
  `corneringSlowdownAtFullSteer` 0.10–0.15.
- **„Alles wie früher"**: beide Kurven auf gerade Linie, `maxTurnAngle` = 0,
  `enableCorneringSlowdown` = aus.

---

## 4. Was steckt technisch dahinter (Kurz-Referenz)

- **`SteerOutput`** (`PlayerBalanceController`): Vorzeichen des Lenkzustands × `steerResponseCurve`.
  Der zentrale geformte Lenkwert, den **alle** nutzen (Neigung, Yaw, X-Bewegung, Cornering).
  Bei linearer Kurve = alter `BalanceAngle`.
- **Yaw/Eindrehen**: additiv zum Effekt-Yaw (`VisualYaw`), damit Öl-Dreher, Purzelbaum etc.
  unangetastet bleiben.
- **Cornering-Slowdown**: multipliziert nur `CurrentSpeed` (Welt-Scroll & Distanz).
  Die Lenkschärfe (`SteerMultiplier`) nutzt weiterhin die ungebremste Geschwindigkeit →
  Ausweichen bleibt reaktionsschnell, es gibt keinen „zähen" Dodge.

## 5. Test-Checkliste
- [ ] Standard-Kurven (linear) + `maxTurnAngle` 0 + Cornering aus → fährt exakt wie vorher.
- [ ] `maxTurnAngle` ~10 → Mofa dreht sich beim Lenken dezent in die Fahrtrichtung ein (beide Seiten).
- [ ] `steerResponseCurve` verbiegen → Neigung UND Seitwärts-Tempo ändern sich gemeinsam (passen zusammen).
- [ ] Cornering an → bei hartem Lenken zieht die Welt spürbar, aber dezent langsamer nach vorn;
      Lenkung bleibt trotzdem direkt.
- [ ] Effekte weiterhin ok: Öl-Dreher (Rückwärts-Yaw), Sekundenschlaf (Lenk-Sperre), Wheelie, Hiccup.
