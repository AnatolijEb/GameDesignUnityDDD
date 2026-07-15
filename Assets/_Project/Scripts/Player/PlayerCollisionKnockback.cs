using UnityEngine;

/// <summary>
/// Tuning-Werte für die physische Reaktion des Spielers auf einen Hindernis-Kontakt.
/// Wird direkt im PlayerCollisionHandler im Inspector eingestellt (kein eigenes Asset).
/// </summary>
[System.Serializable]
public class CollisionKnockbackSettings
{
    [Header("Seitlicher Treffer (glancing)")]
    [Tooltip("Stärke des seitlichen Rückstoßes weg vom Hindernis (Einheiten/Sek.). Bewusst schwach, " +
             "damit der Spieler nicht ins nächste Auto geschleudert wird.")]
    public float sideKnockbackSpeed = 6f;
    [Tooltip("Wie lange der seitliche Rückstoß wirkt (Sek.). Er klingt über diese Zeit aus.")]
    public float sideKnockbackDuration = 0.35f;

    [Header("Frontaler Treffer (head-on)")]
    [Tooltip("Höhe, in der das Mofa über das Hindernis fliegt (Unity-Einheiten).")]
    public float flyOverHeight = 3f;
    [Tooltip("Dauer des Über-das-Hindernis-Flugs inkl. Purzelbaum (Sek.).")]
    public float flyOverDuration = 0.75f;
    [Tooltip("Anzahl der vollen Überschläge (Purzelbaum) während des Flugs. 1 = eine ganze Drehung.")]
    public float somersaultTurns = 1f;
    [Tooltip("Leichter seitlicher Versatz beim Frontal-Flug, damit man nicht exakt auf dem " +
             "Hindernis wieder aufsetzt. 0 = kein Versatz.")]
    public float headOnSideDrift = 2f;

    [Header("Klassifizierung & Schutz")]
    [Range(0f, 1f)]
    [Tooltip("Anteil der halben Hindernis-Breite, innerhalb dessen ein Treffer als 'frontal' zählt. " +
             "Ist der Spieler seitlich weiter außen (Kante erwischt), gilt es als seitlicher Treffer.")]
    public float headOnWidthFraction = 0.55f;
    [Tooltip("Zusätzliche Immunitätszeit nach der Reaktion (Sek.), damit ein Nachrutschen nicht " +
             "sofort das nächste Hindernis auslöst (Anti-Stuck / kein Ketten-Treffer).")]
    public float immunityBuffer = 0.1f;
    [Tooltip("Kostet ein seitlicher Treffer ein Leben? (Frontal kostet immer.) " +
             "Aus = seitliches Streifen ist folgenlos außer dem Rückstoß.")]
    public bool sideHitCostsLife = true;
}

/// <summary>
/// Physische Reaktion auf einen Hindernis-Kontakt. Wird NICHT als Asset erstellt, sondern
/// vom <see cref="PlayerCollisionHandler"/> bei jeder Kollision frisch mit der berechneten
/// Trefferrichtung gebaut und über <see cref="PlayerEffectController.ApplyRuntime"/> eingespeist.
///
/// Zwei Ausprägungen:
///   - <see cref="HitKind.HeadOn"/>: Der Spieler fliegt in einem Bogen mit Purzelbaum (Pitch)
///     über das Hindernis. Während des Bogens ist er gegen Hindernisse immun – so bleibt er
///     nie hängen und kassiert keine Kettentreffer.
///   - <see cref="HitKind.Side"/>: Kurzer, sanfter seitlicher Rückstoß weg vom Hindernis.
///     Bewusst schwach, damit er nicht ins nächste Auto geschleudert wird.
/// </summary>
public class CollisionKnockbackRuntime : PlayerEffectRuntime
{
    public enum HitKind { HeadOn, Side }

    private readonly HitKind kind;
    private readonly float pushDirX; // -1 / +1: Richtung des seitlichen Rückstoßes
    private readonly CollisionKnockbackSettings s;

    public CollisionKnockbackRuntime(HitKind kind, float dxPlayerMinusObstacle, CollisionKnockbackSettings settings)
    {
        this.kind = kind;
        this.s = settings;
        // Weg vom Hindernis: der Spieler steht auf der Seite von dx, dorthin schieben wir ihn weiter.
        this.pushDirX = (Mathf.Abs(dxPlayerMinusObstacle) < 0.0001f) ? 1f : Mathf.Sign(dxPlayerMinusObstacle);
        duration = (kind == HitKind.HeadOn) ? s.flyOverDuration : s.sideKnockbackDuration;
    }

    public override void OnApply(PlayerEffectContext ctx)
    {
        // Während der gesamten Reaktion Hindernisse ignorieren: verhindert Hängenbleiben
        // am Hindernis und einen sofortigen Folgetreffer (z.B. direkt ins nächste Auto).
        if (ctx.CollisionHandler != null)
        {
            ctx.CollisionHandler.GrantObstacleImmunity(duration + s.immunityBuffer);
        }
    }

    protected override void OnTick(PlayerEffectContext ctx, float dt)
    {
        float t = (duration > 0f) ? Mathf.Clamp01(elapsed / duration) : 1f;

        if (kind == HitKind.HeadOn)
        {
            // Sprungbogen über das Hindernis (0 -> Höhe -> 0).
            if (ctx.Controller != null)
            {
                ctx.Controller.AddVisualHeight(PlayerEffectUtil.JumpArc(t, s.flyOverHeight));
                // Purzelbaum: gleichmäßiger Überschlag über die volle Flugdauer.
                ctx.Controller.VisualPitch = 360f * s.somersaultTurns * t;
            }

            // Sehr leichter seitlicher Versatz (klingt aus), damit man nicht mittig aufs Hindernis fällt.
            if (ctx.Movement != null && s.headOnSideDrift > 0f)
            {
                ctx.Movement.AddPush(pushDirX * s.headOnSideDrift * (1f - t));
            }
        }
        else // Side
        {
            // Sanfter seitlicher Rückstoß, linear ausklingend (stark am Anfang, dann weg).
            if (ctx.Movement != null)
            {
                float fade = 1f - t;
                ctx.Movement.AddPush(pushDirX * s.sideKnockbackSpeed * fade);
            }
        }
    }

    public override void OnRemove(PlayerEffectContext ctx)
    {
        // Überschlag sauber zurücksetzen, damit das Mofa wieder gerade steht.
        if (ctx.Controller != null) ctx.Controller.VisualPitch = 0f;
    }
}
