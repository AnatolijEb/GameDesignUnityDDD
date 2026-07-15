# Player Effect System – Guide

Ein generisches, erweiterbares System für kurzzeitige Effekte auf den Spieler
(Hickup, Steuerungs-Twist, Rampen-Sprung, ...). Ziel: neue Effekte mit **minimalen
Änderungen** ergänzen – meist nur eine neue Datei + ein Asset, **ohne** bestehende
Skripte anzufassen.

---

## 1. Architektur in einem Satz

Ein Effekt = **Daten (ScriptableObject)** + **Verhalten (Runtime-Klasse)**. Beliebige
**Auslöser** rufen nur `PlayerEffectController.Instance.Apply(effect)` auf; der Controller
tickt aktive Effekte und räumt sie nach Ablauf wieder auf.

```
Auslöser ──► PlayerEffectController.Apply(PlayerEffectSO)
                     │  effect.CreateRuntime()
                     ▼
              PlayerEffectRuntime   (OnApply → Tick* → OnRemove)
                     │  nutzt
                     ▼
              PlayerEffectContext   (Referenzen: Balance, Movement, Throttle, Life, CollisionHandler, Visual)
```

### Dateien (`Assets/_Project/Scripts/Effects/`)
| Datei | Rolle |
|---|---|
| `PlayerEffectSO.cs` | abstrakte Basis für Effekt-Daten-Assets |
| `PlayerEffectRuntime.cs` | abstrakte Basis für das Laufzeit-Verhalten |
| `PlayerEffectContext.cs` | Referenz-Bündel, das jeder Runtime bekommt |
| `PlayerEffectController.cs` | zentrale Verwaltung, **einziger Eintrittspunkt** `Apply()` |
| `PlayerEffectUtil.cs` | gemeinsame Helfer (z.B. `JumpArc`) |
| `Effects/HiccupEffectSO.cs` | Hickup (seitlicher Stoß + Mofa-Hop) |
| `Effects/ControlTwistEffectSO.cs` | Steuerung umkehren (Links/Rechts, Vor/Zurück) |
| `Effects/RampEffectSO.cs` | Sprungbogen + Belohnung (Pizza / Speed) + Hindernis-Immunität → siehe `Effect_Ramp_Guide.md` |
| `Effects/OilSpinEffectSO.cs` | Öl-Dreher (1,5× drehen, rückwärts, Steuerung invertiert) → siehe `Effect_OilPuddle_Guide.md` |
| `Triggers/RandomEffectSpawner.cs` | Auslöser: zufällige Zeitabstände (optional an Drunkenness gekoppelt) |
| `Triggers/PlayerEffectTriggerZone.cs` | Auslöser: beim Durchfahren (Trigger-Collider) |

### Kopplungs-Hooks in bestehenden Skripten
Diese kleinen, generischen Andockpunkte werden von Effekten von außen genutzt und
müssen bei neuen Effekten i.d.R. **nicht** erweitert werden:
- `PlayerBalanceController.steeringSign` (·-1 kehrt Lenkung um)
- `PlayerThrottleController.throttleSign` (·-1 kehrt Vor/Zurück um)
- `PlayerMovementController.AddPush(velocityX)` (seitlicher Stoß, additiv pro Frame)
- `RunSpeedManager.AddSpeedBonus(amount)` (Speed-Boost, additiv pro Frame, durch `maxSpeed` gedeckelt)
- `RunSpeedManager.SetScrollMultiplier(m)` (Scroll-Faktor pro Frame; `1`=normal, `0`=Stillstand, negativ=Welt rückwärts)
- `PlayerEffectController.AddVisualHeight(y)` (Mofa anheben – Hop/Sprung, additiv pro Frame)
- `PlayerEffectController.VisualYaw` (Extra-Drehwinkel des Mofas um die Hochachse – wird vom `PlayerBalanceController` in die Rotation eingerechnet)
- `PlayerCollisionHandler.GrantObstacleImmunity(seconds)` (Hindernistreffer für X Sekunden ignorieren; Wände bleiben tödlich)

### Sound pro Effekt
Jedes `PlayerEffectSO` hat ein Feld **Sounds** (`AudioClip[]`, zufällige Auswahl) + **Sound Volume**.
Der `PlayerEffectController` spielt beim `Apply()` automatisch einen Clip über eine eigene
2D-`AudioSource`. Kein Code nötig – Clip am Asset eintragen genügt. Leer = kein Sound.

---

## 2. Warum das robust ist

- **Steuerungs-Umkehr multiplikativ**: Zwei gleichzeitige Twists heben sich korrekt auf
  (`-1 · -1 = 1`). Kein „ist-invertiert ja/nein"-Sonderfall, kein Stacking-Bug.
- **Additive Frame-Hooks** (`AddPush`, `AddSpeedBonus`, `AddVisualHeight`): mehrere Effekte
  können gleichzeitig wirken und summieren sich sauber; ohne aktiven Effekt sind sie 0.
- **Visual vs. Rotation getrennt**: Der Controller schreibt nur `PlayerVisual.localPosition.y` (Hop/Sprung),
  der `PlayerBalanceController` schreibt die Rotation und rechnet dabei `VisualYaw` (Effekt-Drehung)
  mit ein – ein einziger Schreiber pro Kanal, kein Konflikt.
- **Spieler-Root bleibt auf der Straße**: Sprung/Drehung sind rein visuell (nur `PlayerVisual`);
  X-Bewegung, Z-Sperre und Kollision bleiben unverändert. „Über Hindernisse springen" bzw.
  „beim Drehen nicht getroffen werden" wird über `GrantObstacleImmunity` gelöst, nicht über echte Physik.
- **Welt statt Spieler bewegt sich**: Der Öl-Dreher lässt die Welt bewusst normal weiterscrollen –
  „rückwärts fahren" ist nur die visuelle Mofa-Drehung (`VisualYaw`) + invertierte Steuerung.
  Wer die Welt-Geschwindigkeit doch verändern will (Slow-mo, Freeze, echtes Rückwärts), hat mit
  `SetScrollMultiplier` einen generischen Hook (`1`=normal, `0`=Stillstand, negativ=rückwärts);
  echtes Rückwärts nur kurz einsetzen (Puffer hinter dem Spieler ~80 Einheiten).

---

## 3. Einen NEUEN Effekt hinzufügen

### Fall A: nur andere Werte eines bestehenden Effekts
Kein Code. Neues Asset anlegen: **Rechtsklick › Create › DDD › Effects › ...** und Werte
im Inspector einstellen. In den passenden Auslöser eintragen (siehe §4).

### Fall B: neues Verhalten
Eine Datei nach dem Muster von `HiccupEffectSO.cs` anlegen:

```csharp
[CreateAssetMenu(fileName = "SO_Effect_Foo", menuName = "DDD/Effects/Foo")]
public class FooEffectSO : PlayerEffectSO
{
    public float duration = 1f;
    // ... Tuning-Felder ...
    public override PlayerEffectRuntime CreateRuntime() => new FooEffectRuntime(this);
}

public class FooEffectRuntime : PlayerEffectRuntime
{
    private readonly FooEffectSO data;
    public FooEffectRuntime(FooEffectSO data) { this.data = data; duration = data.duration; }

    public override void OnApply(PlayerEffectContext ctx)  { /* einmalig */ }
    protected override void OnTick(PlayerEffectContext ctx, float dt) { /* jeden Frame */ }
    public override void OnRemove(PlayerEffectContext ctx) { /* aufräumen */ }
}
```

Braucht der Effekt eine neue Referenz (z.B. Kamera)? Einmal in `PlayerEffectContext`
ein Feld ergänzen und im `PlayerEffectController.Awake()` befüllen – danach steht sie
allen Effekten zur Verfügung.

`duration <= 0` ⇒ Sofort-Effekt (nur `OnApply`, dann direkt `OnRemove`).

---

## 4. Effekte auslösen (drei Wege)

1. **Zufällig (Timer)** – `RandomEffectSpawner` (auf dem Player). Effekt-Asset mit Gewicht
   in die Liste eintragen. Nutzt für das Hickup.
2. **Beim Durchfahren (Zone)** – `PlayerEffectTriggerZone` auf ein Prefab mit Trigger-Collider
   (Pfütze, Schanze, Rampe). Kostet kein Leben, spielt keinen Crash-Sound. Objekt muss **nicht**
   den Tag „Obstacle" haben.
3. **Als Hindernis** – `ObstacleTypeSO.contactEffect` setzen. Das Hindernis (Tag „Obstacle")
   löst dann beim Kontakt den Effekt aus statt Schaden zu machen. Über den bestehenden
   Obstacle-Spawner mischbar.

Alle drei rufen intern nur `PlayerEffectController.Instance.Apply(effect)` auf.

---

## 5. Unity-Setup (einmalig)

1. **Editor kompilieren lassen**: nach dem Anlegen der Skripte den Unity-Editor fokussieren,
   bis der Compile durch ist (erzeugt auch die `.meta`-Dateien).
2. **Controller**: Auf `Assets/_Project/Prefabs/Player/Player.prefab` die Komponente
   **`PlayerEffectController`** hinzufügen. `Player Visual` bleibt leer (wird automatisch aus
   dem `PlayerBalanceController.visualTarget` = `PlayerVisual` gezogen) oder wird manuell gesetzt.
3. **Hickup-Asset**: Create › DDD › Effects › Hiccup → `SO_Effect_Hiccup`.
4. **RandomEffectSpawner**: Auf `Player.prefab` hinzufügen, in der Effekt-Liste `SO_Effect_Hiccup`
   mit Gewicht 1 eintragen. Intervalle/Grace nach Geschmack.
5. **Steuerungs-Twist**: Create › DDD › Effects › Control Twist → `SO_Effect_ControlTwist`.
   Ein Pfützen-/Schanzen-Prefab mit **Box Collider (Is Trigger)** und
   **`PlayerEffectTriggerZone`** anlegen, das Asset zuweisen, ins Chunk platzieren.
6. **Rampe**: Create › DDD › Effects › Ramp Jump → `SO_Effect_Ramp`. Rampen-Prefab mit
   Trigger-Collider + `PlayerEffectTriggerZone` (empfohlen) **oder** als Obstacle mit
   gesetztem `contactEffect`. Ins Chunk platzieren.

> Wenn der Speed-Boost der Rampe nicht spürbar ist: `maxSpeed` im `RunSpeedManager` erhöhen –
> der Boost wird weiterhin durch `maxSpeed` gedeckelt.

---

## 6. Test-Checkliste
- [ ] Hickup: kein Effekt in der Grace-Period; danach zufällige Richtung, kurzer Ruck (~0.3s),
      Mofa hüpft, Gegenlenken bleibt möglich, X-Clamp verhindert Wand-Durchdringung.
- [ ] Steuerungs-Twist: durch die Zone fahren → Steuerung umgekehrt für die Dauer, danach normal.
- [ ] Rampe: Mofa springt hoch und landet wieder, Pizza/Speed-Belohnung kommt an, Fahrt läuft weiter.
- [ ] Normale Hindernisse (ohne `contactEffect`) verhalten sich exakt wie vorher (Leben − 1, Crash-Sound).
