using UnityEngine;

public class DrunkennessSystem : MonoBehaviour
{
    public static DrunkennessSystem Instance { get; private set; }

    [Header("Drunkenness Settings")]
    [SerializeField] private float startingDrunkenness = 100f;
    [SerializeField] private float maxDrunkenness = 600f;
    [SerializeField] private float decayDelayAfterIncrease = 5f;
    [SerializeField] private float normalDecayPerSecond = 10f;
    [SerializeField] private float lowDrunkennessDecayPerSecond = 5f;
    [SerializeField] private float lowDrunkennessThreshold = 100f;
    [SerializeField] private float startGracePeriod = 10f;

    private float currentDrunkenness;
    private float decayDelayTimer;
    private float gracePeriodTimer;
    private int currentMultiplier;
    private bool isGameOverTriggered = false;

    // Public read-only accessors
    public float CurrentDrunkenness => currentDrunkenness;
    public float MaxDrunkenness => maxDrunkenness;
    public int CurrentMultiplier => currentMultiplier;

    // Events for HUD and score integration
    public event System.Action<float, float> OnDrunkennessChanged;
    public event System.Action<int> OnMultiplierChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        currentDrunkenness = startingDrunkenness;
        currentMultiplier = CalculateMultiplier(currentDrunkenness);
        gracePeriodTimer = startGracePeriod;
    }

    private void Start()
    {
        OnDrunkennessChanged?.Invoke(currentDrunkenness, maxDrunkenness);
        OnMultiplierChanged?.Invoke(currentMultiplier);
    }

    private void Update()
    {
        if (isGameOverTriggered) return;

        // Process starting grace period timer if active
        if (gracePeriodTimer > 0f)
        {
            gracePeriodTimer -= Time.deltaTime;
        }

        // Process post-shot decay delay timer if active
        if (decayDelayTimer > 0f)
        {
            decayDelayTimer -= Time.deltaTime;
        }

        // If starting grace period is still active, do not decay
        if (gracePeriodTimer > 0f)
        {
            return;
        }

        // If post-increase delay is still active, do not decay
        if (decayDelayTimer > 0f)
        {
            return;
        }

        // If drunkenness is already at or below the 1x baseline (100), do not decay further
        if (currentDrunkenness <= 100f)
        {
            return;
        }

        // Decay at normal rate down to the 100 baseline
        float decayRate = normalDecayPerSecond;
        float previousDrunkenness = currentDrunkenness;
        
        currentDrunkenness = Mathf.Max(100f, currentDrunkenness - decayRate * Time.deltaTime);

        if (currentDrunkenness != previousDrunkenness)
        {
            OnDrunkennessChanged?.Invoke(currentDrunkenness, maxDrunkenness);
            UpdateMultiplier();
        }
    }

    public void AddDrunkenness(float amount)
    {
        if (amount <= 0f) return;

        currentDrunkenness = Mathf.Clamp(currentDrunkenness + amount, 100f, maxDrunkenness);
        decayDelayTimer = decayDelayAfterIncrease;

        Debug.Log($"[DrunkennessSystem] Drunkenness increased by {amount}. Current: {currentDrunkenness}");

        OnDrunkennessChanged?.Invoke(currentDrunkenness, maxDrunkenness);
        UpdateMultiplier();
    }

    private void UpdateMultiplier()
    {
        int newMultiplier = CalculateMultiplier(currentDrunkenness);
        if (newMultiplier != currentMultiplier)
        {
            currentMultiplier = newMultiplier;
            Debug.Log($"[DrunkennessSystem] Multiplier changed to {currentMultiplier}x");
            OnMultiplierChanged?.Invoke(currentMultiplier);
        }
    }

    private int CalculateMultiplier(float drunkenness)
    {
        // Use FloorToInt so that values in each 100-unit tier (e.g. 200 up to below 300) represent the corresponding multiplier (e.g. 2x)
        return Mathf.Clamp(Mathf.FloorToInt(drunkenness / 100f), 1, 6);
    }

    private void TriggerGameOver()
    {
        if (isGameOverTriggered) return;
        isGameOverTriggered = true;

        Debug.Log("[DrunkennessSystem] Drunkenness reached 0! Triggering Game Over.");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerGameOver();
        }
        else
        {
            Debug.LogWarning("[DrunkennessSystem] GameManager.Instance is null! Reloading active scene as fallback.");
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
    }
}
