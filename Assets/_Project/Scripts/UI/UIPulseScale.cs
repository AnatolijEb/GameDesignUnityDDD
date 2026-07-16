using UnityEngine;

/// <summary>
/// Lässt ein UI-Element ab und zu kurz größer werden ("Pop"), z.B. den "Seriously Don't!"-Text.
/// Rein visuell: skaliert nur die lokale Scale relativ zur Ausgangsgröße. Einfach auf das Objekt legen.
/// </summary>
public class UIPulseScale : MonoBehaviour
{
    [Tooltip("Wie stark der Pop skaliert (1.3 = 30% größer als normal).")]
    public float pulseScale = 1.3f;
    [Tooltip("Dauer eines Pops (hoch UND wieder zurück) in Sekunden.")]
    public float pulseDuration = 0.5f;
    [Tooltip("Minimale Pause zwischen zwei Pops in Sekunden.")]
    public float minInterval = 2f;
    [Tooltip("Maximale Pause zwischen zwei Pops in Sekunden.")]
    public float maxInterval = 5f;

    private Vector3 baseScale;
    private float nextPulseTime;
    private float pulseStartTime = -1f;

    private void Awake()
    {
        baseScale = transform.localScale;
    }

    private void OnEnable()
    {
        ScheduleNext();
    }

    private void ScheduleNext()
    {
        pulseStartTime = -1f;
        nextPulseTime = Time.unscaledTime + Random.Range(minInterval, maxInterval);
    }

    private void Update()
    {
        float now = Time.unscaledTime;

        // Neuen Pop starten, wenn gerade keiner läuft und die Pause vorbei ist.
        if (pulseStartTime < 0f && now >= nextPulseTime)
        {
            pulseStartTime = now;
        }

        if (pulseStartTime >= 0f)
        {
            float t = Mathf.Clamp01((now - pulseStartTime) / pulseDuration); // 0..1 über den Pop
            // Weiche Hüllkurve 0 -> 1 -> 0 (rauf und wieder runter).
            float envelope = Mathf.Sin(t * Mathf.PI);
            transform.localScale = baseScale * (1f + (pulseScale - 1f) * envelope);

            if (t >= 1f)
            {
                transform.localScale = baseScale;
                ScheduleNext();
            }
        }
    }

    private void OnDisable()
    {
        transform.localScale = baseScale;
        pulseStartTime = -1f;
    }
}
