using UnityEngine;

/// <summary>
/// Rampe/Schanze: Das Mofa springt in einem Bogen hoch und fällt wieder auf den Boden
/// zurück, während der Spieler eine Belohnung erhält (Pizza und/oder kurzer Speed-Boost).
/// Typisch ausgelöst durch eine PlayerEffectTriggerZone am Rampen-Prefab, oder über
/// ObstacleTypeSO.contactEffect an einem als "Obstacle" getaggten Rampen-Hindernis.
///
/// Der Spieler-Root bleibt dabei auf der Straße (X-Bewegung/Kollision unverändert) –
/// nur das Mofa hebt sichtbar ab. So "fährt" der Spieler die Rampe hoch und weiter.
/// </summary>
[CreateAssetMenu(fileName = "SO_Effect_Ramp", menuName = "DDD/Effects/Ramp Jump")]
public class RampEffectSO : PlayerEffectSO
{
    [Header("Sprung (visuell)")]
    [Tooltip("Sprunghöhe des Mofas in Unity-Einheiten.")]
    public float jumpHeight = 2.5f;
    [Tooltip("Dauer des Sprungbogens (hoch UND wieder runter) in Sekunden.")]
    public float jumpDuration = 0.9f;
    [Tooltip("Während des Sprungs werden Hindernisse überflogen (kein Schaden). Wände bleiben tödlich.")]
    public bool jumpClearsObstacles = true;
    [Tooltip("Zusätzliche Zeit (Sekunden), in der nach dem Sprung noch Hindernisse ignoriert werden – als Puffer, falls das Hindernis knapp hinter der Rampe steht.")]
    public float extraImmunityBuffer = 0.15f;

    [Header("Belohnung: Pizza")]
    [Tooltip("Gibt beim Sprung eine Pizza (Leben) dazu.")]
    public bool grantsPizza = true;

    [Header("Belohnung: Geschwindigkeit")]
    [Tooltip("Zusätzliche Geschwindigkeit direkt nach dem Sprung (0 = kein Boost).")]
    public float speedBoost = 5f;
    [Tooltip("Über wie viele Sekunden der Geschwindigkeits-Boost ausklingt.")]
    public float speedBoostDuration = 2.5f;

    public override PlayerEffectRuntime CreateRuntime() => new RampEffectRuntime(this);
}

public class RampEffectRuntime : PlayerEffectRuntime
{
    private readonly RampEffectSO data;

    public RampEffectRuntime(RampEffectSO data)
    {
        this.data = data;
        // Effekt lebt so lange, bis Sprungbogen UND Speed-Boost fertig sind.
        duration = Mathf.Max(data.jumpDuration, data.speedBoostDuration);
    }

    public override void OnApply(PlayerEffectContext ctx)
    {
        // Belohnung sofort gutschreiben ("sammelt beim Drüberfahren was ein").
        if (data.grantsPizza && ctx.Life != null)
        {
            ctx.Life.AddLife();
        }

        // Für die Dauer des Sprungs Hindernisse überfliegen (kein Schaden).
        if (data.jumpClearsObstacles && ctx.CollisionHandler != null)
        {
            ctx.CollisionHandler.GrantObstacleImmunity(data.jumpDuration + data.extraImmunityBuffer);
        }
    }

    protected override void OnTick(PlayerEffectContext ctx, float dt)
    {
        // Sprungbogen auf das Mofa – zurück auf 0 (Boden) am Ende von jumpDuration.
        if (ctx.Controller != null && data.jumpDuration > 0f)
        {
            float jt = Mathf.Clamp01(elapsed / data.jumpDuration);
            ctx.Controller.AddVisualHeight(PlayerEffectUtil.JumpArc(jt, data.jumpHeight));
        }

        // Geschwindigkeits-Boost, der über die Boost-Dauer linear ausklingt.
        if (data.speedBoost > 0f && data.speedBoostDuration > 0f
            && elapsed < data.speedBoostDuration && RunSpeedManager.Instance != null)
        {
            float fade = 1f - (elapsed / data.speedBoostDuration);
            RunSpeedManager.Instance.AddSpeedBonus(data.speedBoost * fade);
        }
    }
}
