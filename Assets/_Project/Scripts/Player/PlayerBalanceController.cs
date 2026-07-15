using UnityEngine;

public class PlayerBalanceController : MonoBehaviour
{
    [Header("Balance Settings")]
    public float balanceDriftSpeed = 0.6f;
    public float counterForce = 7f;
    public float maxTiltAngle = 30f;
    public float driftChangeMinTime = 1.5f;
    public float driftChangeMaxTime = 3.5f;

    [Tooltip("Wie schnell die Neigung ohne Gegen-Eingabe Richtung Mitte zurückläuft (Selbstzentrierung). Höher = knackigeres Loslassen, aber weniger Suff-Schwanken. 0 = aus.")]
    public float selfCenterSpeed = 1.5f;

    [Header("Visuals")]
    public Transform visualTarget;

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

        // Apply drift (das "Suff-Schwanken")
        balanceAngle += driftDirection * balanceDriftSpeed * Time.deltaTime;

        // Player Input (Counter-force). GetAxisRaw = ohne Unitys eingebaute Input-Rampe -> direkter.
        // Speed-Kopplung sitzt jetzt nur noch im PlayerMovementController (verhindert doppelte/quadrierte Skalierung).
        float input = Input.GetAxisRaw("Horizontal");
        balanceAngle += input * counterForce * Time.deltaTime;

        // Selbstzentrierung: die Neigung strebt sanft zurueck zur Mitte. Unter konstantem Drift
        // pendelt sie sich bei ~drift/selfCenterSpeed ein, kehrt beim Loslassen aber schnell zurueck.
        balanceAngle -= balanceAngle * selfCenterSpeed * Time.deltaTime;

        // Clamp balanceAngle between -1 and 1
        balanceAngle = Mathf.Clamp(balanceAngle, -1f, 1f);

        // Visual Tilt (Rotation around Z)
        visualTarget.rotation = Quaternion.Euler(0f, 0f, -balanceAngle * maxTiltAngle);
    }
}
