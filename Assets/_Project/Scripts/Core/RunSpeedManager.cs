using UnityEngine;

public class RunSpeedManager : MonoBehaviour
{
    public static RunSpeedManager Instance { get; private set; }

    [Header("Speed Settings")]
    public float baseSpeed = 10f;
    public float speedIncreasePerSecond = 0f;
    public float maxSpeed = 20f;

    private float currentSpeed;
    private float distanceTravelled;

    public float CurrentSpeed => currentSpeed;
    public float DistanceTravelled => distanceTravelled;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        currentSpeed = baseSpeed;
    }

    private void Update()
    {
        // Ensure currentSpeed is at least the baseSpeed (which might be increased by DifficultyManager)
        if (currentSpeed < baseSpeed)
        {
            currentSpeed = baseSpeed;
        }

        // Increase speed over time (if configured)
        currentSpeed += speedIncreasePerSecond * Time.deltaTime;
        currentSpeed = Mathf.Min(currentSpeed, maxSpeed);

        // Track distance
        distanceTravelled += currentSpeed * Time.deltaTime;
    }
}
