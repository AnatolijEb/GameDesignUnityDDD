using UnityEngine;

using UnityEngine;

[CreateAssetMenu(fileName = "NewObstacleType", menuName = "DDD/Obstacle Type")]
public class ObstacleTypeSO : ScriptableObject
{
    [Header("Identity")]
    public string displayName;
    public GameObject prefab;
    public Vector3 spawnScale = Vector3.one;

    [Header("Spawning")]
    public int spawnWeight = 5; // 1–10, default 5, used for weighted random
    public int minDifficultyTier = 0; // default 0, obstacle only spawns at or above this tier

    [Header("On Contact")]
    public int pizzasLostOnContact = 2; // default 2
    public ObstacleEffectType effectType;
    public float effectDuration; // seconds, for timed effects
    public bool grantsExtraPizza = false; // default false

    [Tooltip("Optional: Effekt, der beim Kontakt ausgelöst wird (z.B. Rampe). Wenn gesetzt, löst dieses Hindernis den Effekt aus und kostet KEIN Leben. Leer lassen = normales Hindernis.")]
    public PlayerEffectSO contactEffect;
}
