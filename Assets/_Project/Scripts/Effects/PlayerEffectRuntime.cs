/// <summary>
/// Laufzeit-Verhalten eines Effekts. Reines C#-Objekt (KEIN MonoBehaviour):
/// wird vom <see cref="PlayerEffectController"/> erzeugt, jeden Frame getickt und
/// beim Ablaufen sauber wieder entfernt.
///
/// Ableiten und OnApply / OnTick / OnRemove nach Bedarf überschreiben. In der
/// Unterklasse im Konstruktor <see cref="duration"/> setzen:
///   - duration &gt; 0  =&gt; zeitlich begrenzter Effekt (wird getickt),
///   - duration &lt;= 0 =&gt; Sofort-Effekt (nur OnApply, danach direkt OnRemove).
/// </summary>
public abstract class PlayerEffectRuntime
{
    protected float elapsed;
    protected float duration; // <= 0 => Sofort-Effekt ohne Laufzeit

    /// <summary>
    /// Das SO, aus dem diese Runtime erzeugt wurde (von <see cref="PlayerEffectController.Apply"/>
    /// gesetzt). Null bei per Code gebauten Runtimes (z.B. Kollisions-Knockback). Dient dem HUD, um
    /// beim Ende des Effekts zu wissen, welches Icon gemeint ist.
    /// </summary>
    public PlayerEffectSO Source { get; set; }

    public bool HasDuration => duration > 0f;
    public bool IsFinished => HasDuration && elapsed >= duration;

    /// <summary>Einmalig beim Anwenden: Signs umdrehen, Belohnung gutschreiben, Richtung würfeln ...</summary>
    public virtual void OnApply(PlayerEffectContext ctx) { }

    /// <summary>Vom Controller jeden Frame aufgerufen, solange der Effekt läuft.</summary>
    public void Tick(PlayerEffectContext ctx, float dt)
    {
        elapsed += dt;
        OnTick(ctx, dt);
    }

    protected virtual void OnTick(PlayerEffectContext ctx, float dt) { }

    /// <summary>Einmalig beim Ablaufen/Entfernen: alle Änderungen aus OnApply zurücknehmen.</summary>
    public virtual void OnRemove(PlayerEffectContext ctx) { }

    /// <summary>
    /// Wird aufgerufen, wenn ein bereits aktiver Effekt (bei <see cref="PlayerEffectSO.cancelIfActive"/>)
    /// erneut ausgelöst wird. Rückgabe true = die Runtime beendet sich selbst SANFT (bleibt vorerst
    /// aktiv, z.B. Öl-Dreher dreht zurück und läuft dann normal aus); false = der Controller entfernt
    /// die Runtime sofort hart. Standard: hart entfernen.
    /// </summary>
    public virtual bool CancelGracefully() => false;
}
