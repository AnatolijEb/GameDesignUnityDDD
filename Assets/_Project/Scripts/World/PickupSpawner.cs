using UnityEngine;

/// <summary>
/// Central pickup spawner for a chunk. Lives on the chunk root (RoadChunk_Basic).
///
/// Prefab references (Pizza, Shot) are assigned ONCE here and inherited by every
/// Prefab Variant — they never have to be re-assigned on individual markers.
///
/// At runtime this component iterates every <see cref="PickupSpawnPoint"/> marker under
/// the chunk's SpawnLocations/PickupSpawns container, asks each marker to roll an outcome
/// (Pizza, Shot, or nothing), and spawns at most one pickup per marker. Because each
/// marker rolls a single outcome, a marker can never spawn both a Pizza and a Shot.
///
/// Spawned pickups are parented under RuntimeContent so they move with the chunk and are
/// kept separate from authored content. The existing Pizza / Shot prefabs are instantiated
/// unchanged, preserving their gameplay behavior.
/// </summary>
[RequireComponent(typeof(RoadChunk))]
public class PickupSpawner : MonoBehaviour
{
    [Header("Pickup Prefabs (configured centrally — not per marker)")]
    [Tooltip("The existing Pizza pickup prefab. Spawned unchanged so its gameplay behavior is preserved.")]
    [SerializeField] private GameObject pizzaPrefab;
    [Tooltip("The existing Shot pickup prefab. Spawned unchanged so its gameplay behavior is preserved.")]
    [SerializeField] private GameObject shotPrefab;
    [SerializeField] private bool logSpawnDebug;

    private RoadChunk roadChunk;

    private void Awake()
    {
        roadChunk = GetComponent<RoadChunk>();
    }

    private void Start()
    {
        SpawnPickups();
    }

    private void SpawnPickups()
    {
        Transform searchRoot = (roadChunk != null && roadChunk.pickupSpawns != null)
            ? roadChunk.pickupSpawns
            : transform;

        PickupSpawnPoint[] markers = searchRoot.GetComponentsInChildren<PickupSpawnPoint>(true);
        if (markers.Length == 0) return;

        Transform parent = (roadChunk != null && roadChunk.runtimeContent != null)
            ? roadChunk.runtimeContent
            : transform;

        foreach (PickupSpawnPoint marker in markers)
        {
            if (marker == null) continue;

            PickupSpawnPoint.PickupKind kind = marker.Roll();

            GameObject prefab = kind switch
            {
                PickupSpawnPoint.PickupKind.Pizza => pizzaPrefab,
                PickupSpawnPoint.PickupKind.Shot => shotPrefab,
                _ => null
            };

            if (prefab == null) continue; // None, or prefab reference not assigned

            Object spawnedObject = Instantiate((Object)prefab, marker.transform.position, marker.transform.rotation, parent);
            GameObject instance = spawnedObject as GameObject;
            if (instance == null && spawnedObject is Component spawnedComponent)
            {
                instance = spawnedComponent.gameObject;
            }

            if (instance == null)
            {
                Debug.LogWarning($"[PickupSpawner] Could not instantiate pickup prefab '{prefab.name}' as a GameObject.", prefab);
                continue;
            }

            instance.name = $"{prefab.name}_Runtime";
            EnsurePickupGameplay(instance, kind);

            if (logSpawnDebug)
            {
                Debug.Log($"[PickupSpawner] Spawned {kind} using '{prefab.name}' at {marker.transform.position}.", instance);
            }
        }
    }

    private static void EnsurePickupGameplay(GameObject instance, PickupSpawnPoint.PickupKind kind)
    {
        if (instance == null) return;

        switch (kind)
        {
            case PickupSpawnPoint.PickupKind.Pizza:
                if (instance.GetComponentInChildren<PizzaPickup>(true) == null)
                {
                    instance.AddComponent<PizzaPickup>();
                }
                break;
            case PickupSpawnPoint.PickupKind.Shot:
                if (instance.GetComponentInChildren<ShotPickup>(true) == null)
                {
                    instance.AddComponent<ShotPickup>();
                }
                break;
        }

        Collider pickupCollider = instance.GetComponentInChildren<Collider>(true);
        if (pickupCollider == null)
        {
            pickupCollider = instance.AddComponent<SphereCollider>();
        }

        pickupCollider.isTrigger = true;

        if (instance.GetComponent<PickupHoverMotion>() == null)
        {
            instance.AddComponent<PickupHoverMotion>();
        }

        if (instance.GetComponentInChildren<Renderer>(true) == null)
        {
            Debug.LogWarning($"[PickupSpawner] Spawned {kind} pickup '{instance.name}' has no Renderer, so it will be invisible.", instance);
        }
    }
}
