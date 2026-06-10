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

    private RoadChunk roadChunk;
    private const float roadWidth = 15f;

    private void Awake()
    {
        roadChunk = GetComponent<RoadChunk>();
    }

    private IEnumerator Start()
    {
        // Wait one frame to allow for dynamic configuration if needed
        yield return null;
        SpawnObstacles();
    }

    public void Configure(int tier, int maxObstacles, float chance)
    {
        currentDifficultyTier = tier;
        maxObstaclesPerChunk = maxObstacles;
        spawnChance = chance;
    }

    private void SpawnObstacles()
    {
        if (availableObstacleTypes == null || availableObstacleTypes.Length == 0) return;

        // Filter eligible types
        var eligibleTypes = availableObstacleTypes
            .Where(t => t != null && t.minDifficultyTier <= currentDifficultyTier)
            .ToList();

        if (eligibleTypes.Count == 0) return;

        // Copy and shuffle spawn points
        List<Transform> shuffledPoints = new List<Transform>(spawnPoints.Where(p => p != null));
        Shuffle(shuffledPoints);

        HashSet<int> occupiedLanes = new HashSet<int>();
        int obstaclesSpawned = 0;
        float laneWidth = roadWidth / laneCount;
        float halfRoadWidth = roadWidth / 2f;

        foreach (var point in shuffledPoints)
        {
            if (obstaclesSpawned >= maxObstaclesPerChunk) break;

            // Roll spawn chance
            if (Random.value > spawnChance) continue;

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
                        continue; // Skip to keep at least one lane free
                    }
                }
            }

            // Pick obstacle type via weighted random
            ObstacleTypeSO pickedType = PickWeightedObstacle(eligibleTypes);
            if (pickedType == null || pickedType.prefab == null) continue;

            // Instantiate
            Transform parent = roadChunk != null && roadChunk.obstacleParent != null ? roadChunk.obstacleParent : transform;
            GameObject obstacleInstance = Instantiate(pickedType.prefab, point.position, point.rotation, parent);
            obstacleInstance.tag = "Obstacle";

            occupiedLanes.Add(lane);
            obstaclesSpawned++;
        }
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
