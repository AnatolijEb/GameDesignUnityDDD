using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    [Header("Tiers - configure in Inspector")]
    [SerializeField] private DifficultyTierConfig[] tiers = new DifficultyTierConfig[]
    {
        new DifficultyTierConfig { tierIndex = 0, tierLabel = "No Traffic", maxObstaclesPerChunk = 0, spawnChance = 0.0f, speedBonus = 0.0f },
        new DifficultyTierConfig { tierIndex = 1, tierLabel = "Light Traffic", maxObstaclesPerChunk = 1, spawnChance = 0.4f, speedBonus = 0.5f },
        new DifficultyTierConfig { tierIndex = 2, tierLabel = "Moderate Traffic", maxObstaclesPerChunk = 2, spawnChance = 0.65f, speedBonus = 1.5f },
        new DifficultyTierConfig { tierIndex = 3, tierLabel = "Heavy Traffic", maxObstaclesPerChunk = 3, spawnChance = 0.85f, speedBonus = 3.0f }
    };

    [Header("Safe Start")]
    [Tooltip("Chunks that spawn with zero obstacles at game start, regardless of tier.")]
    [Range(0, 10)]
    [SerializeField] private int initialSafeChunks = 3;

    [Header("Runtime - read only")]
    [SerializeField] private int currentTierIndex = 0;
    [SerializeField] private int deliveryCount = 0;

    private float initialBaseSpeed;
    private int chunksActivated = 0;

    public DifficultyTierConfig CurrentTierConfig => tiers[currentTierIndex];
    public int MaxTiers => tiers.Length;

    public bool ClaimSafeStartChunk()
    {
        chunksActivated++;
        return chunksActivated <= initialSafeChunks;
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
            return;
        }

        if (RunSpeedManager.Instance != null)
        {
            initialBaseSpeed = RunSpeedManager.Instance.baseSpeed;
        }
    }

    private void Start()
    {
        // If Instance was set in Awake, but RunSpeedManager was not yet initialized or baseSpeed was 0, 
        // we might want to ensure we have it. But Awake usually works if RunSpeedManager is also in scene.
        if (initialBaseSpeed == 0 && RunSpeedManager.Instance != null)
        {
            initialBaseSpeed = RunSpeedManager.Instance.baseSpeed;
        }
        
        ApplyCurrentTier();
    }

    public void OnDeliveryCompleted()
    {
        deliveryCount++;
        // Advance tier, clamp to last tier
        int targetTier = Mathf.Min(deliveryCount, tiers.Length - 1);
        if (targetTier != currentTierIndex)
        {
            currentTierIndex = targetTier;
            ApplyCurrentTier();
        }
        // TODO: notify PizzaLifeSystem here once implemented
        // PizzaLifeSystem.Instance?.Deliver();
    }

    private void ApplyCurrentTier()
    {
        DifficultyTierConfig cfg = tiers[currentTierIndex];
        Debug.Log($"[Difficulty] Applying Tier {currentTierIndex}: {cfg.tierLabel} (Obstacles: {cfg.maxObstaclesPerChunk}, Chance: {cfg.spawnChance})");
        
        // Apply speed
        if (RunSpeedManager.Instance != null)
        {
            RunSpeedManager.Instance.baseSpeed = initialBaseSpeed + cfg.speedBonus;
        }

        // All currently active spawners update immediately (mid-run chunks keep their
        // already-spawned obstacles, new chunks pick up the new config automatically)
        foreach (var spawner in FindObjectsByType<ObstacleSpawner>(FindObjectsSortMode.None))
        {
            spawner.Configure(currentTierIndex, cfg.maxObstaclesPerChunk, cfg.spawnChance);
        }
    }
}
