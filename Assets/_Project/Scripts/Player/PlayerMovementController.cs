using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{
    [Header("Steering")]
    public float steerStrength = 6f;
    public PlayerBalanceController balanceController;

    [Header("Speed Coupling")]
    [Tooltip("Wenn aktiv, wird steerStrength mit RunSpeedManager.SteerMultiplier skaliert. Bei konstantem Tempo ist das ein fester Faktor.")]
    public bool scaleWithSpeed = false;

    [Header("Boost / Rasen (halten)")]
    [Tooltip("Taste (Vor/Hoch) zum Boosten. Gedrueckt halten = schneller seitlich unterwegs.")]
    public string boostAxis = "Vertical";
    [Tooltip("Wie lange man am Stueck boosten kann (Sekunden), wenn der Tank voll ist.")]
    public float maxBoostSeconds = 6f;
    [Tooltip("Faktor, um den die seitliche Geschwindigkeit bei vollem Boost steigt.")]
    public float boostMultiplier = 2.5f;
    [Tooltip("Wie schnell sich der Boost-Tank wieder auffuellt (Sekunden pro echter Sekunde). <1 = laedt langsamer als er sich leert.")]
    public float boostRechargeRate = 0.5f;
    [Tooltip("Wie schnell der Boost ein-/ausfadet. Hoeher = direkter, niedriger = weicher (gegen Ruckeln).")]
    public float boostRampSpeed = 5f;

    private float initialZ;

    // Boost-State
    private float boostRemaining;   // verbleibender Tank in Sekunden
    private float boostFactor;      // 0..1, sanft gefadete Boost-Staerke
    private bool isBoosting;

    // Fuer HUD/UI auslesbar
    public float BoostRemaining => boostRemaining;
    public float MaxBoostSeconds => maxBoostSeconds;
    public float Boost01 => maxBoostSeconds > 0f ? boostRemaining / maxBoostSeconds : 0f;
    public bool IsBoosting => isBoosting;

    private void Awake()
    {
        if (balanceController == null)
        {
            balanceController = GetComponent<PlayerBalanceController>();
        }

        initialZ = transform.position.z;
        boostRemaining = maxBoostSeconds;
    }

    private void Update()
    {
        UpdateBoost();

        // Seitliche Geschwindigkeit aus Neigung ...
        float lateralVelocity = 0f;
        if (balanceController != null)
        {
            float speedFactor = (scaleWithSpeed && RunSpeedManager.Instance != null) ? RunSpeedManager.Instance.SteerMultiplier : 1f;
            lateralVelocity = balanceController.BalanceAngle * steerStrength * speedFactor;
        }

        // ... waehrend des Boosts verstaerkt (sanft gefadet -> kein Ruckeln).
        lateralVelocity *= Mathf.Lerp(1f, boostMultiplier, boostFactor);

        // Move in World Space, damit die visuelle Neigung die Bewegung nicht verdreht
        transform.Translate(Vector3.right * lateralVelocity * Time.deltaTime, Space.World);

        // Z sperren und X begrenzen (nicht durch Waende fahren)
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -7.25f, 7.25f);
        pos.z = initialZ;
        transform.position = pos;
    }

    private void UpdateBoost()
    {
        // Boost aktiv, solange die Taste gehalten wird UND noch Tank uebrig ist
        bool wantsBoost = Input.GetAxisRaw(boostAxis) > 0.5f;
        isBoosting = wantsBoost && boostRemaining > 0f;

        if (isBoosting)
        {
            // Tank leert sich in Echtzeit (1 Sekunde Boost = 1 Sekunde Tank)
            boostRemaining = Mathf.Max(0f, boostRemaining - Time.deltaTime);
        }
        else if (boostRemaining < maxBoostSeconds)
        {
            // Tank laedt wieder auf, wenn nicht geboostet wird
            boostRemaining = Mathf.Min(maxBoostSeconds, boostRemaining + boostRechargeRate * Time.deltaTime);
        }

        // Boost-Staerke sanft Richtung Ziel faden (verhindert das harte Ein-/Ausschalten)
        boostFactor = Mathf.MoveTowards(boostFactor, isBoosting ? 1f : 0f, boostRampSpeed * Time.deltaTime);

        // Der Welt/Vorwaerts-Speed wird im RunSpeedManager anhand dieses Faktors mit hochgezogen
        if (RunSpeedManager.Instance != null)
        {
            RunSpeedManager.Instance.SetBoost(boostFactor);
        }
    }
}
