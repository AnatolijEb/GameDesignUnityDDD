using UnityEngine;

/// <summary>
/// Hickup-Episode: Statt eines einzelnen Stoßes wird der Spieler jetzt für eine gewisse Dauer
/// (<see cref="episodeDuration"/>) von mehreren Hickups "befallen": in zufälligen Abständen gibt es
/// je einen kurzen seitlichen Stoß + Mofa-Hop, danach endet die Episode wieder von selbst.
///
/// Wird typischerweise über den RandomEffectSpawner in zufälligen Abständen ausgelöst. Das HUD-Icon
/// ist während der Episode sichtbar und blendet am Ende aus (dafür am Asset hudPersistsAfterEnd = false).
/// </summary>
[CreateAssetMenu(fileName = "SO_Effect_Hiccup", menuName = "DDD/Effects/Hiccup")]
public class HiccupEffectSO : PlayerEffectSO
{
    [Header("Episode")]
    [Tooltip("Wie lange die Hickup-Episode insgesamt dauert (Sekunden). Danach endet sie automatisch.")]
    public float episodeDuration = 6f;
    [Tooltip("Minimaler Abstand zwischen zwei einzelnen Hickups innerhalb der Episode (Sekunden).")]
    public float minHiccupInterval = 0.9f;
    [Tooltip("Maximaler Abstand zwischen zwei einzelnen Hickups innerhalb der Episode (Sekunden).")]
    public float maxHiccupInterval = 1.9f;

    [Header("Einzelner Stoß")]
    [Tooltip("Dauer eines einzelnen Stoßes in Sekunden.")]
    public float hiccupDuration = 0.3f;
    [Tooltip("Maximale seitliche Stoß-Geschwindigkeit (gleiche Einheit wie steerStrength am PlayerMovementController).")]
    public float pushForce = 14f;
    [Tooltip("Verlauf der Stoßkraft über die Dauer eines Stoßes. x = 0..1 (Zeit), y = 0..1 (Kraftanteil). Standard: harter Ruck, der ausklingt.")]
    public AnimationCurve pushCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));

    [Header("Mofa-Hop (visuell)")]
    [Tooltip("Wie hoch das Mofa bei jedem Hickup kurz springt (Unity-Einheiten, nur ein paar Zentimeter).")]
    public float hopHeight = 0.25f;

    [Header("Richtung")]
    [Tooltip("true = Richtung wird pro Hickup zufällig (links/rechts) gewürfelt. false = immer nach rechts.")]
    public bool randomDirection = true;

    public override PlayerEffectRuntime CreateRuntime() => new HiccupEffectRuntime(this);
}

public class HiccupEffectRuntime : PlayerEffectRuntime
{
    private readonly HiccupEffectSO data;

    private bool hiccupActive;   // läuft gerade ein einzelner Stoß?
    private float hiccupTimer;   // Zeit seit Start des aktuellen Stoßes
    private float sinceLastHiccup;
    private float nextHiccupDelay;
    private float direction;

    public HiccupEffectRuntime(HiccupEffectSO data)
    {
        this.data = data;
        duration = Mathf.Max(0.01f, data.episodeDuration);
    }

    public override void OnApply(PlayerEffectContext ctx)
    {
        // Sofort mit dem ersten Hickup beginnen.
        StartHiccup();
    }

    private void StartHiccup()
    {
        hiccupActive = true;
        hiccupTimer = 0f;
        direction = data.randomDirection ? (Random.value > 0.5f ? 1f : -1f) : 1f;
    }

    protected override void OnTick(PlayerEffectContext ctx, float dt)
    {
        if (hiccupActive)
        {
            float t = data.hiccupDuration > 0f ? Mathf.Clamp01(hiccupTimer / data.hiccupDuration) : 1f;

            // Seitlicher Stoß – überlagert die normale Lenkung (blockiert sie nicht).
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

            hiccupTimer += dt;
            if (hiccupTimer >= data.hiccupDuration)
            {
                hiccupActive = false;
                sinceLastHiccup = 0f;
                nextHiccupDelay = Random.Range(data.minHiccupInterval, data.maxHiccupInterval);
            }
        }
        else
        {
            // Pause zwischen zwei Hickups. Keinen neuen mehr starten, wenn die Episode gleich endet.
            sinceLastHiccup += dt;
            if (sinceLastHiccup >= nextHiccupDelay && elapsed < duration)
            {
                StartHiccup();
            }
        }
    }
}
