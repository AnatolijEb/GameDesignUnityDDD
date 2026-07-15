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

    private float scrollMultiplier = 1f;          // effektiv angewandter Faktor auf die Scroll-Geschwindigkeit
    private float requestedScrollMultiplier = 1f; // pro Frame vom Effekt gesetzt (z.B. Rückwärtsfahren)

    // Effektive Scroll-Geschwindigkeit der Welt. Der Faktor erlaubt Spezialzustände wie
    // Rückwärtsfahren (negativer Faktor -> Welt läuft rückwärts).
    public float CurrentSpeed => currentSpeed * scrollMultiplier;
    public float DistanceTravelled => distanceTravelled;

    /// <summary>
    /// Von Effekten aufgerufen, um kurzzeitig zusätzliche Geschwindigkeit zu geben (Rampen-Boost).
    /// Additiv pro Frame; wird nach oben weiterhin durch maxSpeed begrenzt.
    /// </summary>
    public void AddSpeedBonus(float amount) => effectSpeedBonus += amount;

    /// <summary>
    /// Von Effekten aufgerufen, um die Scroll-Geschwindigkeit dieses Frames zu skalieren.
    /// 1 = normal, 0 = Stillstand, negativ = Welt läuft rückwärts (Rückwärtsfahren).
    /// Muss jeden Frame neu gesetzt werden; ohne Aufruf steht der Faktor auf 1.
    /// </summary>
    public void SetScrollMultiplier(float multiplier) => requestedScrollMultiplier = multiplier;

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

        // Scroll-Faktor dieses Frames anwenden, dann für den nächsten Frame zurücksetzen.
        scrollMultiplier = requestedScrollMultiplier;
        requestedScrollMultiplier = 1f;

        // Track distance (nutzt die effektive Geschwindigkeit inkl. Faktor)
        distanceTravelled += CurrentSpeed * Time.deltaTime;

        // Effekt-Boost pro Frame zurücksetzen (Effekte speisen ihn jeden Frame neu ein).
        effectSpeedBonus = 0f;
    }
}
