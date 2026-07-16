using UnityEngine;

public class RunSpeedManager : MonoBehaviour
{
    public static RunSpeedManager Instance { get; private set; }

    [Header("Speed Settings")]
    [Tooltip("Standardgeschwindigkeit, die ohne Vor-/Zurück-Eingabe gehalten wird.")]
    public float baseSpeed = 10f;
    [Tooltip("Zeit-basierter Anstieg (Einheiten pro Sekunde). Alternative zur Strecken-Rampe unten; " +
             "läuft auch beim Trödeln hoch. Standard 0 = aus.")]
    public float speedIncreasePerSecond = 0f;
    [Tooltip("Absolute Obergrenze für die Geschwindigkeit. Deckelt auch beide Fortschritts-Rampen.")]
    public float maxSpeed = 20f;
    [Tooltip("Absolute Untergrenze für die Geschwindigkeit (greift z.B. beim Bremsen per Throttle).")]
    public float minSpeed = 4f;

    [Header("Fortschritt: schneller je weiter man kommt")]
    [Tooltip("Wenn aktiv, steigt die Basis-Geschwindigkeit mit der zurückgelegten Strecke " +
             "(je weiter, desto schneller). Nach oben durch maxSpeed gedeckelt.")]
    public bool enableDistanceSpeedRamp = true;
    [Tooltip("Um wie viel die Basis-Geschwindigkeit pro 100 zurückgelegten Strecke-Einheiten steigt. " +
             "0.4 = +0.4 alle 100 Einheiten (erreicht bei base 10 / max 20 das Maximum nach ~2500 Einheiten). " +
             "Gedeckelt durch maxSpeed.")]
    public float baseSpeedGainPer100Units = 0.4f;

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

    [Header("Cornering (Kurven-Verlangsamung)")]
    [Tooltip("Wenn aktiv: hartes Lenken bremst den Vortrieb der Welt (diagonale Fahrt = weniger Strecke nach vorn). " +
             "Betrifft NUR den Welt-Scroll/Distanz – die Lenkschärfe (SteerMultiplier) bleibt unberührt, " +
             "Ausweichen bleibt also reaktionsschnell.")]
    public bool enableCorneringSlowdown = true;
    [Range(0f, 0.6f)]
    [Tooltip("Maximale Vortriebs-Reduktion bei vollem Lenkeinschlag. 0.12 = bis zu 12 % langsamer. 0 = aus.")]
    public float corneringSlowdownAtFullSteer = 0.12f;
    [Tooltip("Formt, wie stark die Verlangsamung mit dem Lenkeinschlag zunimmt.\n" +
             "x = Lenkeinschlag (0..1), y = Anteil der max. Verlangsamung (0..1). Gerade Linie = proportional.")]
    public AnimationCurve corneringSlowdownCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    [Tooltip("Spieler-Balance/Lenkung (liefert den aktuellen Lenkeinschlag). Leer = wird automatisch gesucht.")]
    public PlayerBalanceController balanceController;

    private float currentSpeed;
    private float distanceTravelled;
    private float timeSpeedBonus;
    private float effectSpeedBonus; // von Effekten (z.B. Rampe) gesetzter Boost, pro Frame

    private float scrollMultiplier = 1f;          // effektiv angewandter Faktor auf die Scroll-Geschwindigkeit
    private float requestedScrollMultiplier = 1f; // pro Frame vom Effekt gesetzt (z.B. Rückwärtsfahren)
    private float corneringFactor = 1f;           // <1 wenn stark gelenkt wird (nur Vortrieb wird gebremst)

    // Effektive Scroll-Geschwindigkeit der Welt. Die Faktoren erlauben Spezialzustände wie
    // Rückwärtsfahren (negativer scrollMultiplier) und Kurven-Verlangsamung (corneringFactor < 1).
    public float CurrentSpeed => currentSpeed * scrollMultiplier * corneringFactor;
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

        if (balanceController == null)
        {
            balanceController = FindFirstObjectByType<PlayerBalanceController>();
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

        // Fortschritt: je weiter man gekommen ist, desto höher das Grundtempo (durch maxSpeed gedeckelt).
        float distanceSpeedBonus = enableDistanceSpeedRamp
            ? (distanceTravelled / 100f) * baseSpeedGainPer100Units
            : 0f;

        float targetSpeed = Mathf.Clamp(baseSpeed + timeSpeedBonus + distanceSpeedBonus + throttleOffset + effectSpeedBonus, minSpeed, maxSpeed);

        // Sanft an die Zielgeschwindigkeit annähern statt sie hart zu setzen (fühlt sich nach Beschleunigen/Bremsen an)
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, throttleAcceleration * Time.deltaTime);

        // Cornering-Verlangsamung: hartes Lenken bremst NUR den Welt-Vortrieb (diagonale Fahrt kostet
        // etwas Strecke nach vorn). Die Lenkschärfe (SteerMultiplier) nutzt weiterhin die ungebremste
        // currentSpeed -> Ausweichen bleibt reaktionsschnell.
        corneringFactor = 1f;
        if (enableCorneringSlowdown && corneringSlowdownAtFullSteer > 0f && balanceController != null)
        {
            float steerAmount = Mathf.Clamp01(Mathf.Abs(balanceController.SteerOutput));
            float reduction = corneringSlowdownAtFullSteer * Mathf.Clamp01(corneringSlowdownCurve.Evaluate(steerAmount));
            corneringFactor = 1f - Mathf.Clamp01(reduction);
        }

        // Scroll-Faktor dieses Frames anwenden, dann für den nächsten Frame zurücksetzen.
        scrollMultiplier = requestedScrollMultiplier;
        requestedScrollMultiplier = 1f;

        // Track distance (nutzt die effektive Geschwindigkeit inkl. Faktor)
        distanceTravelled += CurrentSpeed * Time.deltaTime;

        // Effekt-Boost pro Frame zurücksetzen (Effekte speisen ihn jeden Frame neu ein).
        effectSpeedBonus = 0f;
    }
}
