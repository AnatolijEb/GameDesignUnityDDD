using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Einzelnes Effekt-Icon im HUD. Wabert dauerhaft (Rotation + Puls, wie das alte Hickup-Icon) und
/// bekommt bei Interaktion einen abklingenden "Punch" (kurzes Aufblähen + Rotation). Die Ziel-
/// Position im Stapel wird weich angefahren, damit Icons beim Umstapeln sauber nachrücken.
///
/// Wird vom <see cref="EffectHudManager"/> zur Laufzeit erzeugt und gesteuert – nicht von Hand
/// in die Szene legen.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class EffectIcon : MonoBehaviour
{
    /// <summary>Aussehen/Timing, zentral vom Manager vorgegeben (damit alle Icons gleich wirken).</summary>
    public struct Style
    {
        public float displayScale;
        public float iconHeight;
        public float appearDuration;
        public float wobbleRotationSpeed, wobbleRotationAmplitude;
        public float wobbleScaleSpeed, wobbleScaleAmplitude;
        public float punchScaleAmount, punchRotationAmount, punchDuration, punchDamping, punchFrequency;
        public float repositionSpeed;
    }

    private RectTransform rect;
    private Image image;
    private Style style;

    private Vector2 targetPos;

    // Eigene Phasen pro Icon -> alle wabern asynchron zueinander.
    private float rotPhase, scalePhase;

    private bool appearing;
    private float appearTimer, appearFactor;

    private bool punching;
    private float punchTimer, punchDirection = 1f;

    private bool vanishing;
    private float vanishTimer;
    private System.Action onVanished;

    public void Setup(Sprite sprite, Vector2 startPos, Style style)
    {
        rect = (RectTransform)transform;
        image = GetComponent<Image>();
        this.style = style;

        image.sprite = sprite;
        image.preserveAspect = true;
        image.raycastTarget = false;

        // Einheitliche Höhe für alle Icons -> gleiche wahrgenommene Größe, unabhängig vom
        // Seitenverhältnis des Sprites. Breite folgt aus dem Seitenverhältnis.
        float aspect = (sprite != null && sprite.rect.height > 0f) ? sprite.rect.width / sprite.rect.height : 1f;
        rect.sizeDelta = new Vector2(style.iconHeight * aspect, style.iconHeight);

        // Zufällige Startphasen -> asynchrones Wabern.
        rotPhase = Random.value * 1000f;
        scalePhase = Random.value * 1000f;

        rect.anchoredPosition = startPos;
        targetPos = startPos;

        appearing = true;
        appearTimer = 0f;
        appearFactor = 0f;
        transform.localScale = Vector3.zero;
    }

    public void SetTargetPosition(Vector2 pos) => targetPos = pos;

    /// <summary>Kurzer, abklingender Ausschlag – bei jeder Interaktion mit dem Effekt.</summary>
    public void Punch()
    {
        punching = true;
        punchTimer = 0f;
        punchDirection = Random.value > 0.5f ? 1f : -1f;
    }

    /// <summary>Abschluss: einmal punchen und dann ausblenden + Objekt zerstören.</summary>
    public void VanishAfterPunch(System.Action onDone)
    {
        Punch();
        vanishing = true;
        vanishTimer = 0f;
        onVanished = onDone;
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        // Ziel-Position weich anfahren (frame-rate-unabhängig).
        rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, targetPos,
            1f - Mathf.Exp(-style.repositionSpeed * dt));

        if (appearing)
        {
            appearTimer += dt;
            float t = style.appearDuration > 0f ? Mathf.Clamp01(appearTimer / style.appearDuration) : 1f;
            appearFactor = EaseOutBack(t);
            if (t >= 1f) { appearing = false; appearFactor = 1f; }
        }
        else if (!vanishing)
        {
            appearFactor = 1f;
        }

        float punchEnvelope = 0f;
        if (punching)
        {
            punchTimer += dt;
            float t = Mathf.Clamp01(punchTimer / style.punchDuration);
            punchEnvelope = PunchEnvelope(t);
            if (t >= 1f) punching = false;
        }

        // Ausblenden: erst volle Größe halten (damit der Abschluss-Punch sichtbar wächst),
        // dann in der zweiten Hälfte wegschrumpfen.
        float vanishFactor = 1f;
        if (vanishing)
        {
            vanishTimer += dt;
            float dur = Mathf.Max(0.0001f, style.punchDuration);
            float t = Mathf.Clamp01(vanishTimer / dur);
            vanishFactor = t < 0.5f ? 1f : 1f - (t - 0.5f) / 0.5f;
            if (t >= 1f)
            {
                onVanished?.Invoke();
                Destroy(gameObject);
                return;
            }
        }

        float wobbleAngle = Mathf.Sin((Time.time + rotPhase) * style.wobbleRotationSpeed) * style.wobbleRotationAmplitude;
        float wobblePulse = 1f + Mathf.Sin((Time.time + scalePhase) * style.wobbleScaleSpeed) * style.wobbleScaleAmplitude;

        float scaleMul = Mathf.Max(0.05f, wobblePulse + style.punchScaleAmount * punchEnvelope);
        float rotation = wobbleAngle + punchDirection * style.punchRotationAmount * punchEnvelope;

        transform.localRotation = Quaternion.Euler(0f, 0f, rotation);
        transform.localScale = Vector3.one * (style.displayScale * appearFactor * vanishFactor * scaleMul);
    }

    /// <summary>Abklingender Ausschlag: startet bei 1 (voller Schlag) und schwingt bis 0 aus.</summary>
    private float PunchEnvelope(float t) =>
        Mathf.Exp(-style.punchDamping * t) * Mathf.Cos(t * style.punchFrequency * Mathf.PI * 2f);

    private static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}
