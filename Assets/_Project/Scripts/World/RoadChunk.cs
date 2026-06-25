using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data holder describing the shared structure of a road chunk.
/// RoadChunk_Basic is the base prefab; every authored chunk is a Prefab Variant of it.
/// The chunk length is fixed at 30 units — variable-length chunks are intentionally NOT supported,
/// because RoadChunkManager relies on a single shared length so variants connect seamlessly.
/// </summary>
public class RoadChunk : MonoBehaviour
{
    [Header("Fixed length — keep at 30 (do not author variable-length chunks)")]
    public float chunkLength = 30f;

    [Header("Geometry")]
    public Transform road;
    public Transform wallLeft;
    public Transform wallRight;

    [Header("Containers")]
    [Tooltip("AuthoredContent/Obstacles — designers place obstacle prefabs here. Never written to at runtime.")]
    public Transform authoredObstacles;
    [Tooltip("AuthoredContent/Coins — designers place Coin placeholders here.")]
    public Transform coins;
    [Tooltip("SpawnLocations/PickupSpawns — designers place PickupSpawnPoint markers here.")]
    public Transform pickupSpawns;
    [Tooltip("RuntimeContent — everything spawned at runtime (obstacles, pickups) is parented here.")]
    public Transform runtimeContent;

    public Vector3 EndPosition => transform.position + Vector3.forward * chunkLength;

    /// <summary>
    /// Returns a list of structural problems with this chunk. Empty list means the chunk is valid.
    /// Used by RoadChunkManager to warn about misconfigured variants.
    /// </summary>
    public List<string> GetValidationIssues(float expectedLength)
    {
        var issues = new List<string>();
        if (!Mathf.Approximately(chunkLength, expectedLength))
            issues.Add($"chunkLength is {chunkLength} but the expected shared length is {expectedLength}.");
        if (road == null) issues.Add("Geometry/Road reference is missing.");
        if (runtimeContent == null) issues.Add("RuntimeContent container reference is missing.");
        if (pickupSpawns == null) issues.Add("SpawnLocations/PickupSpawns container reference is missing.");
        if (authoredObstacles == null) issues.Add("AuthoredContent/Obstacles container reference is missing.");
        return issues;
    }
}
