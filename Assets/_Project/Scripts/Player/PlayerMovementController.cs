using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{
    [Header("Steering")]
    public float steerStrength = 4f;
    public PlayerBalanceController balanceController;

    [Header("Speed Coupling")]
    [Tooltip("Wenn aktiv, wird steerStrength mit RunSpeedManager.SteerMultiplier skaliert: schneller fahren = stärkere/weitere Seitwärtsbewegung bei gleicher Neigung, langsamer fahren = schwächere.")]
    public bool scaleWithSpeed = true;

    private float initialZ;
    private float externalPushX; // von Effekten (z.B. Hickup) gesetzter seitlicher Stoß, pro Frame

    private void Awake()
    {
        if (balanceController == null)
        {
            balanceController = GetComponent<PlayerBalanceController>();
        }

        initialZ = transform.position.z;
    }

    /// <summary>
    /// Von Effekten aufgerufen, um dem Spieler zusätzlich einen seitlichen Stoß zu geben
    /// (Einheit: Geschwindigkeit in X, wird mit deltaTime verrechnet). Additiv pro Frame.
    /// </summary>
    public void AddPush(float velocityX) => externalPushX += velocityX;

    private void Update()
    {
        // 1. Steering (Lean translates to sideways movement) - Move in World Space to ignore tilt
        if (balanceController != null)
        {
            float speedFactor = (scaleWithSpeed && RunSpeedManager.Instance != null) ? RunSpeedManager.Instance.SteerMultiplier : 1f;
            transform.Translate(Vector3.right * balanceController.BalanceAngle * steerStrength * speedFactor * Time.deltaTime, Space.World);
        }

        // 1b. Externer Stoß (z.B. Hickup) – überlagert die normale Lenkung.
        if (externalPushX != 0f)
        {
            transform.Translate(Vector3.right * externalPushX * Time.deltaTime, Space.World);
            externalPushX = 0f;
        }

        // 2. Lock Z position and clamp X position to prevent passing through walls
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -7.25f, 7.25f);
        pos.z = initialZ;
        transform.position = pos;
    }
}
