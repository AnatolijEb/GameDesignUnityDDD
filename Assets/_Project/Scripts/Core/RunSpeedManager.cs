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

    [Tooltip("Wie schnell sich die aktuelle Geschwindigkeit an die Zielgeschwindigkeit annähert (auch für das Hoch-/Runterlaufen beim Boost).")]
    public float speedSmoothing = 12f;

    [Header("Boost (vom PlayerMovementController gesetzt)")]
    [Tooltip("Faktor auf das Welt-/Vorwärtstempo bei vollem Boost. Bewusst kleiner als der seitliche boostMultiplier, damit seitlich im Verhältnis stärker schneller wird.")]
    public float worldBoostMultiplier = 1.5f;

    [Header("Speed → Lenk-Kopplung")]
    [Tooltip("Lenk-Empfindlichkeits-Multiplikator bei minSpeed (langsamer = träger lenken).")]
    public float steerMultiplierAtMinSpeed = 0.6f;
    [Tooltip("Lenk-Empfindlichkeits-Multiplikator bei maxSpeed (schneller = schärfer lenken).")]
    public float steerMultiplierAtMaxSpeed = 1.6f;

    private float currentSpeed;
    private float distanceTravelled;
    private float timeSpeedBonus;
    private float boostFactor01;

    // Vom PlayerMovementController pro Frame gesetzt: 0 = kein Boost, 1 = voller Boost.
    public void SetBoost(float factor01)
    {
        boostFactor01 = Mathf.Clamp01(factor01);
    }

    public float CurrentSpeed => currentSpeed;
    public float DistanceTravelled => distanceTravelled;

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

        currentSpeed = baseSpeed;
    }

    private void Update()
    {
        // Konstantes Grundtempo. Vor/Zurück steuert kein Tempo mehr (siehe Dash im PlayerMovementController).
        // Optionaler kontinuierlicher Anstieg über die Nacht als Schwierigkeitskurve (speedIncreasePerSecond).
        timeSpeedBonus += speedIncreasePerSecond * Time.deltaTime;

        // Boost zieht das Welt-/Vorwärtstempo mit hoch (visualisiert das Rasen). boostFactor01 ist
        // schon im Player sanft gefadet, speedSmoothing glättet zusätzlich -> kein harter Ruck.
        float boostMul = Mathf.Lerp(1f, worldBoostMultiplier, boostFactor01);
        float targetSpeed = Mathf.Clamp((baseSpeed + timeSpeedBonus) * boostMul, minSpeed, maxSpeed);
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, speedSmoothing * Time.deltaTime);

        // Track distance
        distanceTravelled += currentSpeed * Time.deltaTime;
    }
}
