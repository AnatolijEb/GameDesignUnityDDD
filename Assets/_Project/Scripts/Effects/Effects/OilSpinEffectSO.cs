using UnityEngine;

/// <summary>
/// Öl-Dreher: Beim Überfahren einer Öl-Pfütze dreht sich das Mofa 1,5×, sodass es
/// rückwärts schaut, hat für eine einstellbare Zeit die Steuerung invertiert und dreht
/// sich danach wieder nach vorne. Die Welt scrollt die GANZE Zeit normal weiter – es sieht
/// nur so aus, als würde der Spieler rückwärts fahren (rein visuell, kein echtes Zurück).
/// Gedacht als Hindernis-Effekt: an einem als "Obstacle" getaggten Öl-Prefab über
/// ObstacleTypeSO.contactEffect zuweisen (kostet dann kein Leben, sondern dreht nur).
///
/// Phasen (nacheinander):
///   1. Eindrehen  (spinInDuration):  0° -> 1,5 Drehungen (endet rückwärts schauend)
///   2. Rückwärts  (reverseDuration): rückwärts schauend + Steuerung invertiert (Welt scrollt normal weiter)
///   3. Ausdrehen  (spinOutDuration): dreht weiter, bis wieder vorwärts schauend, dann normal weiter
/// </summary>
[CreateAssetMenu(fileName = "SO_Effect_OilSpin", menuName = "DDD/Effects/Oil Spin")]
public class OilSpinEffectSO : PlayerEffectSO
{
    [Header("Dreher (visuell)")]
    [Tooltip("Anzahl der Umdrehungen, bis der Spieler rückwärts schaut. 1.5 = klassischer Öl-Dreher (endet rückwärts).")]
    public float spins = 1.5f;
    [Tooltip("Dauer des Eindrehens in Sekunden.")]
    public float spinInDuration = 0.5f;
    [Tooltip("Dauer des Zurückdrehens nach vorne in Sekunden.")]
    public float spinOutDuration = 0.5f;

    [Header("Rückwärts-Phase (nur visuell)")]
    [Tooltip("Wie lange der Spieler rückwärts SCHAUT und die Steuerung invertiert bleibt (Sekunden). Die Welt scrollt dabei ganz normal weiter.")]
    public float reverseDuration = 2f;

    [Header("Steuerung")]
    [Tooltip("Steuerung während des gesamten Effekts umkehren (links/rechts und vor/zurück) – passend zum Rückwärtsfahren.")]
    public bool invertControls = true;

    [Header("Schutz")]
    [Tooltip("Während des gesamten Effekts keine Hindernistreffer, damit man beim Rückwärtsdrehen nicht unfair getroffen wird.")]
    public bool immuneWhileActive = true;

    public override PlayerEffectRuntime CreateRuntime() => new OilSpinEffectRuntime(this);
}

public class OilSpinEffectRuntime : PlayerEffectRuntime
{
    private readonly OilSpinEffectSO data;
    private readonly float spinInEnd;   // Zeitpunkt Ende Eindrehen
    private readonly float reverseEnd;  // Zeitpunkt Ende Rückwärtsphase
    private readonly float yawToBackward; // Winkel bis "rückwärts" (z.B. 540°)
    private readonly float yawToForward;  // Endwinkel wieder vorwärts (nächstes Vielfaches von 360°)

    public OilSpinEffectRuntime(OilSpinEffectSO data)
    {
        this.data = data;
        spinInEnd = data.spinInDuration;
        reverseEnd = data.spinInDuration + data.reverseDuration;
        duration = data.spinInDuration + data.reverseDuration + data.spinOutDuration;

        yawToBackward = data.spins * 360f;                    // 1.5 -> 540° (schaut rückwärts)
        yawToForward = Mathf.Ceil(data.spins) * 360f;         // -> 720° (schaut wieder vorwärts)
    }

    public override void OnApply(PlayerEffectContext ctx)
    {
        // Steuerung invertieren (multiplikativ, wie beim Control-Twist -> stapelt sich sauber).
        if (data.invertControls)
        {
            if (ctx.Balance != null) ctx.Balance.steeringSign *= -1f;
            if (ctx.Throttle != null) ctx.Throttle.throttleSign *= -1f;
        }

        // Für die gesamte Effektdauer keine Hindernistreffer.
        if (data.immuneWhileActive && ctx.CollisionHandler != null)
        {
            ctx.CollisionHandler.GrantObstacleImmunity(duration);
        }
    }

    protected override void OnTick(PlayerEffectContext ctx, float dt)
    {
        // --- Dreh-Winkel (Yaw) je nach Phase ---
        float yaw;
        if (elapsed < spinInEnd)
        {
            float t = data.spinInDuration > 0f ? elapsed / data.spinInDuration : 1f;
            yaw = Mathf.Lerp(0f, yawToBackward, t);
        }
        else if (elapsed < reverseEnd)
        {
            yaw = yawToBackward; // rückwärts schauend halten
        }
        else
        {
            float t = data.spinOutDuration > 0f ? (elapsed - reverseEnd) / data.spinOutDuration : 1f;
            yaw = Mathf.Lerp(yawToBackward, yawToForward, t);
        }

        if (ctx.Controller != null) ctx.Controller.VisualYaw = yaw;

        // Die Welt-Scroll-Geschwindigkeit wird bewusst NICHT verändert: die Map läuft normal
        // weiter auf den Spieler zu, nur das Mofa schaut rückwärts (rein visuell).
    }

    public override void OnRemove(PlayerEffectContext ctx)
    {
        // Steuerung zurückdrehen (·-1 ist selbstinvers).
        if (data.invertControls)
        {
            if (ctx.Balance != null) ctx.Balance.steeringSign *= -1f;
            if (ctx.Throttle != null) ctx.Throttle.throttleSign *= -1f;
        }

        // Mofa wieder gerade ausrichten. Der Scroll-Faktor setzt sich pro Frame selbst auf 1 zurück.
        if (ctx.Controller != null) ctx.Controller.VisualYaw = 0f;
    }
}
