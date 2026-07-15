using UnityEngine;

/// <summary>
/// Steuerungs-Twist: Kehrt für eine feste Dauer die Steuerung um (Links/Rechts und/oder
/// Vor/Zurück). Typisch ausgelöst durch eine PlayerEffectTriggerZone (Pfütze, Schanze).
///
/// Die Umkehr ist MULTIPLIKATIV (Vorzeichen · -1): Fährt der Spieler in zwei Pfützen
/// kurz nacheinander, heben sich die Twists korrekt auf/verrechnen sich, ohne dass
/// Sonderfälle programmiert werden müssen.
/// </summary>
[CreateAssetMenu(fileName = "SO_Effect_ControlTwist", menuName = "DDD/Effects/Control Twist")]
public class ControlTwistEffectSO : PlayerEffectSO
{
    [Header("Steuerungs-Twist")]
    [Tooltip("Wie lange die Steuerung verdreht bleibt (Sekunden).")]
    public float duration = 5f;
    [Tooltip("Links/Rechts (Lenkung) umkehren.")]
    public bool invertSteering = true;
    [Tooltip("Vor/Zurück (Throttle/Gas) umkehren.")]
    public bool invertThrottle = true;

    public override PlayerEffectRuntime CreateRuntime() => new ControlTwistEffectRuntime(this);
}

public class ControlTwistEffectRuntime : PlayerEffectRuntime
{
    private readonly ControlTwistEffectSO data;

    public ControlTwistEffectRuntime(ControlTwistEffectSO data)
    {
        this.data = data;
        duration = data.duration;
    }

    public override void OnApply(PlayerEffectContext ctx)
    {
        if (data.invertSteering && ctx.Balance != null) ctx.Balance.steeringSign *= -1f;
        if (data.invertThrottle && ctx.Throttle != null) ctx.Throttle.throttleSign *= -1f;
    }

    public override void OnRemove(PlayerEffectContext ctx)
    {
        // ·-1 ist selbstinvers -> gleiche Operation nimmt die Umkehr wieder zurück.
        if (data.invertSteering && ctx.Balance != null) ctx.Balance.steeringSign *= -1f;
        if (data.invertThrottle && ctx.Throttle != null) ctx.Throttle.throttleSign *= -1f;
    }
}
