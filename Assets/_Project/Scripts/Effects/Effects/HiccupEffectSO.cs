using UnityEngine;

/// <summary>
/// Hickup: Der Spieler wird kurz stark in eine (zufällige) Richtung geschoben, während
/// das Mofa kurz nach oben hüpft. Wird typischerweise über den RandomEffectSpawner
/// in zufälligen Abständen ausgelöst.
/// </summary>
[CreateAssetMenu(fileName = "SO_Effect_Hiccup", menuName = "DDD/Effects/Hiccup")]
public class HiccupEffectSO : PlayerEffectSO
{
    [Header("Seitlicher Stoß")]
    [Tooltip("Dauer des Stoßes in Sekunden.")]
    public float duration = 0.3f;
    [Tooltip("Maximale seitliche Stoß-Geschwindigkeit (gleiche Einheit wie steerStrength am PlayerMovementController).")]
    public float pushForce = 14f;
    [Tooltip("Verlauf der Stoßkraft über die Dauer. x = 0..1 (Zeit), y = 0..1 (Kraftanteil). Standard: harter Ruck, der ausklingt.")]
    public AnimationCurve pushCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));

    [Header("Mofa-Hop (visuell)")]
    [Tooltip("Wie hoch das Mofa kurz springt (Unity-Einheiten, nur ein paar Zentimeter).")]
    public float hopHeight = 0.25f;

    [Header("Richtung")]
    [Tooltip("true = Richtung wird zufällig (links/rechts) gewürfelt. false = immer nach rechts.")]
    public bool randomDirection = true;

    public override PlayerEffectRuntime CreateRuntime() => new HiccupEffectRuntime(this);
}

public class HiccupEffectRuntime : PlayerEffectRuntime
{
    private readonly HiccupEffectSO data;
    private float direction;

    public HiccupEffectRuntime(HiccupEffectSO data)
    {
        this.data = data;
        duration = data.duration;
    }

    public override void OnApply(PlayerEffectContext ctx)
    {
        direction = data.randomDirection ? (Random.value > 0.5f ? 1f : -1f) : 1f;
    }

    protected override void OnTick(PlayerEffectContext ctx, float dt)
    {
        float t = duration > 0f ? elapsed / duration : 1f;

        // Seitlicher Stoß – überlagert die normale Lenkung (blockiert sie nicht, Gegenlenken bleibt möglich).
        if (ctx.Movement != null)
        {
            float force = data.pushForce * data.pushCurve.Evaluate(t);
            ctx.Movement.AddPush(direction * force);
        }

        // Kurzer Mofa-Hop nach oben und zurück.
        if (ctx.Controller != null)
        {
            ctx.Controller.AddVisualHeight(PlayerEffectUtil.JumpArc(t, data.hopHeight));
        }
    }
}
