using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Obstacle Types")]
    [SerializeField] private ObstacleTypeSO[] availableObstacleTypes;

    [Header("Spawn Config")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private int maxObstaclesPerChunk = 2;
    [SerializeField] [Range(0f, 1f)] private float spawnChance = 0.6f;
    [SerializeField] private int currentDifficultyTier = 0;

    [Header("Lane Safety")]
    [SerializeField] private int laneCount = 3;
    [SerializeField] private bool guaranteeFreeLane = true; // always keep at least one of 3 lanes clear

    [Header("Pizza Pickup")]
    [SerializeField] private GameObject pizzaPickupPrefab;
    [SerializeField] [Range(0f, 1f)] private float pizzaSpawnChance = 0.1f;

    [Header("Shot Pickup")]
    [SerializeField] private GameObject shotPickupPrefab;
    [SerializeField] [Range(0f, 1f)] private float shotSpawnChance = 0.05f;

    private RoadChunk roadChunk;
private const float roadWidth = 15f;

    private void Awake()
    {
        roadChunk = GetComponent<RoadChunk>();
    }

    private bool hasSpawned = false;

    private IEnumerator Start()
    {
        // Wait one frame to allow for dynamic configuration if needed
        yield return null;

        if (DifficultyManager.Instance != null)
        {
            var cfg = DifficultyManager.Instance.CurrentTierConfig;
            Configure(cfg.tierIndex, cfg.maxObstaclesPerChunk, cfg.spawnChance);
        }

        SpawnObstacles();
        SpawnPizza();
        SpawnShot();
    }

    public void Configure(int tier, int maxObstacles, float chance)
    {
        bool wasZero = maxObstaclesPerChunk == 0;
        currentDifficultyTier = tier;
        maxObstaclesPerChunk = maxObstacles;
        spawnChance = chance;

        // If we transitioned from no-obstacles to having obstacles, try spawning if we haven't yet
        if (wasZero && maxObstaclesPerChunk > 0 && !hasSpawned)
        {
            SpawnObstacles();
            SpawnPizza();
            SpawnShot();
        }
    }

    private List<Transform> availablePointsAfterObstacles = new List<Transform>();

    private void SpawnObstacles()
    {
        if (hasSpawned) return;

        if (DifficultyManager.Instance != null &&
            DifficultyManager.Instance.ClaimSafeStartChunk())
        {
            return;
        }

        if (maxObstaclesPerChunk <= 0) 
        {
            availablePointsAfterObstacles = new List<Transform>(spawnPoints.Where(p => p != null));
            return;
        }

        if (availableObstacleTypes == null || availableObstacleTypes.Length == 0) 
        {
            availablePointsAfterObstacles = new List<Transform>(spawnPoints.Where(p => p != null));
            return;
        }

        // Filter eligible types
        var eligibleTypes = availableObstacleTypes
            .Where(t => t != null && t.minDifficultyTier <= currentDifficultyTier)
            .ToList();

        if (eligibleTypes.Count == 0) 
        {
            availablePointsAfterObstacles = new List<Transform>(spawnPoints.Where(p => p != null));
            return;
        }

        hasSpawned = true;
        // Copy and shuffle spawn points
        List<Transform> shuffledPoints = new List<Transform>(spawnPoints.Where(p => p != null));
        Shuffle(shuffledPoints);

        HashSet<int> occupiedLanes = new HashSet<int>();
        int obstaclesSpawned = 0;
        float laneWidth = roadWidth / laneCount;
        float halfRoadWidth = roadWidth / 2f;

        availablePointsAfterObstacles.Clear();

        foreach (var point in shuffledPoints)
        {
            if (obstaclesSpawned >= maxObstaclesPerChunk) 
            {
                availablePointsAfterObstacles.Add(point);
                continue;
            }

            // Roll spawn chance
            if (Random.value > spawnChance) 
            {
                availablePointsAfterObstacles.Add(point);
                continue;
            }

            // Calculate lane
            float localX = point.localPosition.x;
            int lane = Mathf.Clamp(Mathf.FloorToInt((localX + halfRoadWidth) / laneWidth), 0, laneCount - 1);

            // Lane safety check
            if (guaranteeFreeLane)
            {
                // If this lane is not already occupied, and adding it would block all lanes
                if (!occupiedLanes.Contains(lane))
                {
                    if (occupiedLanes.Count + 1 >= laneCount)
                    {
                        availablePointsAfterObstacles.Add(point);
                        continue; // Skip to keep at least one lane free
                    }
                }
            }

            // Pick obstacle type via weighted random
            ObstacleTypeSO pickedType = PickWeightedObstacle(eligibleTypes);
            if (pickedType == null || pickedType.prefab == null) 
            {
                availablePointsAfterObstacles.Add(point);
                continue;
            }

            // Instantiate
            Transform parent = roadChunk != null && roadChunk.obstacleParent != null ? roadChunk.obstacleParent : transform;
            GameObject obstacleInstance = Instantiate(pickedType.prefab, point.position, point.rotation, parent);
            obstacleInstance.tag = "Obstacle";
            obstacleInstance.name = $"Obstacle_{pickedType.displayName}";

            occupiedLanes.Add(lane);
            obstaclesSpawned++;
        }

        if (obstaclesSpawned > 0)
        {
            Debug.Log($"[Spawner] Spawned {obstaclesSpawned} obstacles on chunk {gameObject.name}");
        }
    }

    private void SpawnPizza()
    {
        if (pizzaPickupPrefab == null) return;
        if (availablePointsAfterObstacles.Count == 0) return;
        
        // Roll spawn chance
        if (Random.value > pizzaSpawnChance) return;

        // Pick a random available point
        int randomIndex = Random.Range(0, availablePointsAfterObstacles.Count);
        Transform point = availablePointsAfterObstacles[randomIndex];
        availablePointsAfterObstacles.RemoveAt(randomIndex); // Prevent overlap

        // Instantiate
        Transform parent = roadChunk != null && roadChunk.obstacleParent != null ? roadChunk.obstacleParent : transform;
        GameObject pizzaInstance = Instantiate(pizzaPickupPrefab, point.position, point.rotation, parent);
        pizzaInstance.name = "PizzaPickup";
        pizzaInstance.tag = "Untagged"; // Pizza script handles detection, or we could tag it "Pizza"
        
        Debug.Log($"[Spawner] Spawned Pizza on chunk {gameObject.name} at {point.name}");
    }

    private void SpawnShot()
    {
        if (shotPickupPrefab == null) return;
        if (availablePointsAfterObstacles.Count == 0) return;
        
        // Roll spawn chance
        if (Random.value > shotSpawnChance) return;

        // Pick a random available point
        int randomIndex = Random.Range(0, availablePointsAfterObstacles.Count);
        Transform point = availablePointsAfterObstacles[randomIndex];
        availablePointsAfterObstacles.RemoveAt(randomIndex); // Prevent overlap

        // Instantiate
        Transform parent = roadChunk != null && roadChunk.obstacleParent != null ? roadChunk.obstacleParent : transform;
        GameObject shotInstance = Instantiate(shotPickupPrefab, point.position, point.rotation, parent);
        shotInstance.name = "ShotPickup";
        
        Debug.Log($"[Spawner] Spawned Shot on chunk {gameObject.name} at {point.name}");
    }

    private ObstacleTypeSO PickWeightedObstacle(List<ObstacleTypeSO> types)
    {
        int totalWeight = types.Sum(t => t.spawnWeight);
        if (totalWeight <= 0) return types[0];

        int randomValue = Random.Range(0, totalWeight);
        int currentWeight = 0;

        foreach (var type in types)
        {
            currentWeight += type.spawnWeight;
            if (randomValue < currentWeight)
            {
                return type;
            }
        }

        return types[0];
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
