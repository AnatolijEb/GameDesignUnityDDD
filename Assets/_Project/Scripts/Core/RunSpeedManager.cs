using UnityEngine;

public class RunSpeedManager : MonoBehaviour
{
    public static RunSpeedManager Instance { get; private set; }

    [Header("Speed Settings")]
    [Tooltip("Standardgeschwindigkeit, die ohne Vor-/Zurück-Eingabe gehalten wird.")]
    public float baseSpeed = 10f;
    public float speedIncreasePerSecond = 0f;
    [Tooltip("Absolute Obergrenze für die Geschwindigkeit.")]
    public float maxSpeed = 20f;
    [Tooltip("Absolute Untergrenze für die Geschwindigkeit (greift z.B. beim Bremsen per Throttle).")]
    public float minSpeed = 4f;

    [Header("Throttle Settings (Vor-/Zurücklehnen)")]
    [Tooltip("Referenz auf den Throttle-Controller des Spielers. Wird sonst automatisch gesucht.")]
    public PlayerThrottleController throttleController;
    [Tooltip("Wie viel schneller als baseSpeed man bei vollem Vorwärts-Throttle maximal fährt.")]
    public float maxThrottleBoost = 6f;
    [Tooltip("Wie viel langsamer als baseSpeed man bei vollem Rückwärts-Throttle (Bremsen) minimal fährt.")]
    public float maxThrottleBrake = 6f;
    [Tooltip("Wie schnell sich die aktuelle Geschwindigkeit an die durch Throttle vorgegebene Zielgeschwindigkeit annähert (Beschleunigung/Bremsen).")]
    public float throttleAcceleration = 6f;

    [Header("Speed → Lenk-Kopplung")]
    [Tooltip("Lenk-Empfindlichkeits-Multiplikator bei minSpeed (langsamer = träger lenken).")]
    public float steerMultiplierAtMinSpeed = 0.6f;
    [Tooltip("Lenk-Empfindlichkeits-Multiplikator bei maxSpeed (schneller = schärfer lenken).")]
    public float steerMultiplierAtMaxSpeed = 1.6f;

    private float currentSpeed;
    private float distanceTravelled;
    private float timeSpeedBonus;
    private float effectSpeedBonus; // von Effekten (z.B. Rampe) gesetzter Boost, pro Frame

    public float CurrentSpeed => currentSpeed;
    public float DistanceTravelled => distanceTravelled;

    /// <summary>
    /// Von Effekten aufgerufen, um kurzzeitig zusätzliche Geschwindigkeit zu geben (Rampen-Boost).
    /// Additiv pro Frame; wird nach oben weiterhin durch maxSpeed begrenzt.
    /// </summary>
    public void AddSpeedBonus(float amount) => effectSpeedBonus += amount;

    // Von PlayerBalanceController genutzt, um Lenk-Empfindlichkeit an die aktuelle Geschwindigkeit zu koppeln.
    public float SteerMultiplier
    {
        get
        {
            float t = Mathf.InverseLerp(minSpeed, maxSpeed, currentSpeed);
            return Mathf.Lerp(steerMultiplierAtMinSpeed, steerMultiplierAtMaxSpeed, t);
        }
    }

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

        if (throttleController == null)
        {
            throttleController = FindFirstObjectByType<PlayerThrottleController>();
        }

        currentSpeed = baseSpeed;
    }

    private void Update()
    {
        // Kontinuierlicher Anstieg der Standardgeschwindigkeit über Zeit (z.B. Schwierigkeitskurve)
        timeSpeedBonus += speedIncreasePerSecond * Time.deltaTime;

        // Throttle (-1 = Bremsen, 0 = neutral, 1 = Beschleunigen) verschiebt die Zielgeschwindigkeit um baseSpeed herum
        float throttle = throttleController != null ? throttleController.Throttle : 0f;
        float throttleOffset = throttle >= 0f ? throttle * maxThrottleBoost : throttle * maxThrottleBrake;

        float targetSpeed = Mathf.Clamp(baseSpeed + timeSpeedBonus + throttleOffset + effectSpeedBonus, minSpeed, maxSpeed);

        // Sanft an die Zielgeschwindigkeit annähern statt sie hart zu setzen (fühlt sich nach Beschleunigen/Bremsen an)
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, throttleAcceleration * Time.deltaTime);

        // Track distance
        distanceTravelled += currentSpeed * Time.deltaTime;

        // Effekt-Boost pro Frame zurücksetzen (Effekte speisen ihn jeden Frame neu ein).
        effectSpeedBonus = 0f;
    }
}
