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

        // Visual Tilt (Rotation around Z)
        visualTarget.rotation = Quaternion.Euler(0f, 0f, -balanceAngle * maxTiltAngle);
    }
}
