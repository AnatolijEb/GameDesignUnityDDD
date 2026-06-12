using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{
    [Header("Steering")]
    public float steerStrength = 4f;
    public PlayerBalanceController balanceController;

    private float initialZ;

    private void Awake()
    {
        if (balanceController == null)
        {
            balanceController = GetComponent<PlayerBalanceController>();
        }
        
        initialZ = transform.position.z;
    }

    private void Update()
    {
        // 1. Steering (Lean translates to sideways movement) - Move in World Space to ignore tilt
        if (balanceController != null)
        {
            transform.Translate(Vector3.right * balanceController.BalanceAngle * steerStrength * Time.deltaTime, Space.World);
        }

        // 2. Lock Z position and clamp X position to prevent passing through walls
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -7.25f, 7.25f);
        pos.z = initialZ;
        transform.position = pos;
    }
}
