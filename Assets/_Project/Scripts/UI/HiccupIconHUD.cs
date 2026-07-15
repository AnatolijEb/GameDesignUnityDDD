using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Zeigt das Hickup-Icon unten links an, sobald der Spieler den ersten Hickup bekommen hat.
/// Die Größe ist konstant (immer die volle <see cref="displayScale"/>, kein Wachsen über die Zeit).
/// Das Icon wabert im Leerlauf (Rotation + Puls) und bekommt bei JEDEM Hickup einen deutlichen,
/// abklingenden Skalen-/Rotations-Schlag ("Punch"), damit der Effekt gut sichtbar ist.
/// Hört auf <see cref="PlayerEffectController.OnEffectApplied"/> und reagiert nur auf <see cref="HiccupEffectSO"/>.
/// </summary>
public class HiccupIconHUD : MonoBehaviour
{
    [Header("Referenzen (leer lassen = automatisch auf diesem GameObject suchen)")]
    [SerializeField] private Image icon;

    [Header("Größe")]
    [Tooltip("Konstante Anzeigegröße, sobald das Icon sichtbar ist (kein Wachstum über die Zeit mehr).")]
    [SerializeField] private float displayScale = 1.3f;
    [Tooltip("Dauer der Einblend-Animation beim allerersten Hickup (0 -> displayScale).")]
    [SerializeField] private float appearDuration = 0.3f;

    [Header("Idle-Wabern")]
    [SerializeField] private float wobbleRotationSpeed = 2f;
    [SerializeField] private float wobbleRotationAmplitude = 8f;
    [SerializeField] private float wobbleScaleSpeed = 1.6f;
    [SerializeField] private float wobbleScaleAmplitude = 0.05f;

    [Header("Hickup-Punch (bei JEDEM Hickup)")]
    [Tooltip("Wie stark das Icon bei einem Hickup kurz aufbläht/einsackt (0.5 = bis zu +/-50%).")]
    [SerializeField] private float punchScaleAmount = 0.6f;
    [Tooltip("Zusätzlicher Rotations-Ausschlag in Grad beim Punch (Richtung wechselt zufällig pro Hickup).")]
    [SerializeField] private float punchRotationAmount = 30f;
    [Tooltip("Wie lange der Punch ausschwingt, bis er wieder bei 0 ankommt.")]
    [SerializeField] private float punchDuration = 0.5f;
    [Tooltip("Wie schnell der Ausschlag abklingt (höher = kürzeres Nachwippen).")]
    [SerializeField] private float punchDamping = 4f;
    [Tooltip("Wie viele Schwinger der Punch innerhalb von punchDuration macht.")]
    [SerializeField] private float punchFrequency = 2.5f;

    private PlayerEffectController effectController;
    private bool hasHiccupped;

    private bool appearing;
    private float appearTimer;
    private float appearFactor;

    private bool punching;
    private float punchTimer;
    private float punchDirection = 1f;

    private void Awake()
    {
        if (icon == null) icon = GetComponent<Image>();

        appearFactor = 0f;
        SetVisible(false);
    }

    private void OnEnable()
    {
        TryBindController();
    }

    private void OnDisable()
    {
        if (effectController != null)
        {
            effectController.OnEffectApplied -= HandleEffectApplied;
            effectController = null;
        }
    }

    private void Update()
    {
        if (effectController == null)
        {
            TryBindController();
        }

        if (!hasHiccupped) return;

        if (appearing)
        {
            appearTimer += Time.deltaTime;
            float t = appearDuration > 0f ? Mathf.Clamp01(appearTimer / appearDuration) : 1f;
            appearFactor = EaseOutBack(t);
            if (t >= 1f) appearing = false;
        }

        float punchEnvelope = 0f;
        if (punching)
        {
            punchTimer += Time.deltaTime;
            float t = Mathf.Clamp01(punchTimer / punchDuration);
            punchEnvelope = PunchEnvelope(t);
            if (t >= 1f) punching = false;
        }

        // Leichtes Wabern (Rotation + Puls) im Leerlauf, plus der abklingende Punch-Schlag obendrauf.
        float wobbleAngle = Mathf.Sin(Time.time * wobbleRotationSpeed) * wobbleRotationAmplitude;
        float wobblePulse = 1f + Mathf.Sin(Time.time * wobbleScaleSpeed) * wobbleScaleAmplitude;

        float scaleMultiplier = Mathf.Max(0.1f, wobblePulse + punchScaleAmount * punchEnvelope);
        float rotation = wobbleAngle + punchDirection * punchRotationAmount * punchEnvelope;

        transform.localRotation = Quaternion.Euler(0f, 0f, rotation);
        transform.localScale = Vector3.one * (displayScale * appearFactor * scaleMultiplier);
    }

    private void TryBindController()
    {
        effectController = PlayerEffectController.Instance;
        if (effectController != null)
        {
            effectController.OnEffectApplied += HandleEffectApplied;
        }
    }

    private void HandleEffectApplied(PlayerEffectSO effect)
    {
        if (!(effect is HiccupEffectSO)) return;

        if (!hasHiccupped)
        {
            hasHiccupped = true;
            appearing = true;
            appearTimer = 0f;
            SetVisible(true);
        }

        punching = true;
        punchTimer = 0f;
        punchDirection = Random.value > 0.5f ? 1f : -1f;
    }

    private void SetVisible(bool visible)
    {
        if (icon != null) icon.enabled = visible;
    }

    /// <summary>Abklingender Ausschlag: startet bei 1 (voller Schlag) und schwingt/klingt bis 0 aus.</summary>
    private float PunchEnvelope(float t)
    {
        return Mathf.Exp(-punchDamping * t) * Mathf.Cos(t * punchFrequency * Mathf.PI * 2f);
    }

    private static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}
