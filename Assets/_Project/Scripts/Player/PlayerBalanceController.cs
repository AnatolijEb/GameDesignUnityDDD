using UnityEngine;

public class PlayerBalanceController : MonoBehaviour
{
    [Header("Balance Settings")]
    public float balanceDriftSpeed = 0.6f;
    public float counterForce = 2.5f;
    public float maxTiltAngle = 30f;
    public float driftChangeMinTime = 1.5f;
    public float driftChangeMaxTime = 3.5f;

    [Header("Speed Coupling")]
    [Tooltip("Wenn aktiv, wird counterForce mit RunSpeedManager.SteerMultiplier skaliert: schneller fahren = schärfer lenken, langsamer fahren = träger lenken.")]
    public bool scaleWithSpeed = true;

    [Header("Visuals")]
    public Transform visualTarget;

    [Header("Effekte")]
    [Tooltip("Vorzeichen der Lenkung. Effekte (z.B. Steuerungs-Twist) setzen dies auf -1, um links/rechts umzukehren. Nicht im Inspector ändern.")]
    public float steeringSign = 1f;

    private float balanceAngle = 0f;
    private float driftDirection = 1f;
    private float nextDriftChange = 0f;

    public float BalanceAngle => balanceAngle;

    private void Start()
    {
        if (visualTarget == null) visualTarget = transform;
        
        // Initial drift change time
        nextDriftChange = Time.time + Random.Range(driftChangeMinTime, driftChangeMaxTime);
    }

    private void Update()
    {
        // Random Balance Drift
        if (Time.time > nextDriftChange)
        {
            driftDirection = (Random.value > 0.5f) ? 1f : -1f;
            nextDriftChange = Time.time + Random.Range(driftChangeMinTime, driftChangeMaxTime);
        }

        // Apply drift
        balanceAngle += driftDirection * balanceDriftSpeed * Time.deltaTime;

        // Player Input (Counter-force), an aktuelle Geschwindigkeit gekoppelt: schneller = schärfer, langsamer = träger
        float speedFactor = (scaleWithSpeed && RunSpeedManager.Instance != null) ? RunSpeedManager.Instance.SteerMultiplier : 1f;
        float input = Input.GetAxis("Horizontal");
        balanceAngle += input * counterForce * speedFactor * steeringSign * Time.deltaTime;

        // Clamp balanceAngle between -1 and 1
        balanceAngle = Mathf.Clamp(balanceAngle, -1f, 1f);

        // Visual Tilt (Rotation um Z) + optionale Effekt-Drehungen (Yaw um Y z.B. Öl-Dreher,
        // Pitch um X z.B. Purzelbaum bei Frontal-Kollision).
        float effectYaw = (PlayerEffectController.Instance != null) ? PlayerEffectController.Instance.VisualYaw : 0f;
        float effectPitch = (PlayerEffectController.Instance != null) ? PlayerEffectController.Instance.VisualPitch : 0f;

        // Neigung an die Blickrichtung koppeln: bei rückwärts gedrehtem Mofa (Yaw ~180°) würde die
        // lokal gesetzte Neigung aus Kamerasicht gespiegelt erscheinen. cos(Yaw) dreht sie zurück,
        // sodass die Neigung IMMER zur (weltbasierten) Seitwärtsbewegung passt. Vorwärts (0°): cos=1 -> unverändert.
        float tiltDirection = Mathf.Cos(effectYaw * Mathf.Deg2Rad);
        visualTarget.rotation = Quaternion.Euler(effectPitch, effectYaw, -balanceAngle * maxTiltAngle * tiltDirection);
    }
}
