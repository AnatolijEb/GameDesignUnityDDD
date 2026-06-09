using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Forward Movement")]
    public float moveSpeed = 10f;

    [Header("Balance Settings")]
    public float balanceDriftSpeed = 0.6f;
    public float counterForce = 2.5f;
    public float steerStrength = 4f;
    public float maxTiltAngle = 30f;

    private float balanceAngle = 0f;
    private float driftDirection = 1f;
    private float nextDriftChange = 0f;

    void Start()
    {
        // Initial drift change time
        nextDriftChange = Time.time + Random.Range(1.5f, 3.5f);
    }

    void Update()
    {
        // 1. Forward movement (Automatic) - Move in World Space to ignore tilt
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime, Space.World);

        // 2. Random Balance Drift
        if (Time.time > nextDriftChange)
        {
            driftDirection = (Random.value > 0.5f) ? 1f : -1f;
            nextDriftChange = Time.time + Random.Range(1.5f, 3.5f);
        }

        // Apply drift
        balanceAngle += driftDirection * balanceDriftSpeed * Time.deltaTime;

        // 3. Player Input (Counter-force)
        float input = Input.GetAxis("Horizontal");
        balanceAngle += input * counterForce * Time.deltaTime;

        // Clamp balanceAngle between -1 and 1
        balanceAngle = Mathf.Clamp(balanceAngle, -1f, 1f);

        // 4. Steering (Lean translates to sideways movement) - Move in World Space to ignore tilt
        transform.Translate(Vector3.right * balanceAngle * steerStrength * Time.deltaTime, Space.World);

        // 5. Visual Tilt (Rotation around Z)
        transform.rotation = Quaternion.Euler(0f, 0f, -balanceAngle * maxTiltAngle);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wall"))
        {
            RestartLevel();
        }
    }

    private void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
