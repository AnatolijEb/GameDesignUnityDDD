using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{
    [Header("Forward Movement")]
    public float moveSpeed = 10f;

    [Header("Steering")]
    public float steerStrength = 4f;
    public PlayerBalanceController balanceController;

    private void Awake()
    {
        if (balanceController == null)
        {
            balanceController = GetComponent<PlayerBalanceController>();
        }
    }

    private void Update()
    {
        // 1. Forward movement (Automatic) - Move in World Space to ignore tilt
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime, Space.World);

        // 2. Steering (Lean translates to sideways movement) - Move in World Space to ignore tilt
        if (balanceController != null)
        {
            transform.Translate(Vector3.right * balanceController.BalanceAngle * steerStrength * Time.deltaTime, Space.World);
        }
    }
}
