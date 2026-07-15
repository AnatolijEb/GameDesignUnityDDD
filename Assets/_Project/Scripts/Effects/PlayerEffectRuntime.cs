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
}
