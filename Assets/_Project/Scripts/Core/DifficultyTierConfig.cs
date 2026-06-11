using UnityEngine;
using System;

[Serializable]
public class DifficultyTierConfig
{
    [Header("Identification")]
    public int tierIndex;
    public string tierLabel;   // e.g. "Tier 0 - No Traffic"

    [Header("Obstacle Spawning")]
    [Range(0, 10)]
    [Tooltip("Maximum number of obstacles that can spawn in a single road chunk.")]
    public int maxObstaclesPerChunk;   // 0 means no obstacles

    [Range(0f, 1f)]
    [Tooltip("Probability (0 to 1) that an individual spawn point will attempt to spawn an obstacle.")]
    public float spawnChance;

    [Header("Speed")]
    [Range(0f, 10f)]
    [Tooltip("Flat speed increase added to the base movement speed for this difficulty tier.")]
    public float speedBonus;   // added on top of initial base speed, not cumulative
}
