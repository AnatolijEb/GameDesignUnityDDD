using UnityEngine;

/// <summary>
/// Central pickup spawner for a chunk. Lives on the chunk root (RoadChunk_Basic).
///
/// Prefab references (Pizza, drink variants) are assigned ONCE here and inherited by every
/// Prefab Variant — they never have to be re-assigned on individual markers.
///
/// At runtime this component iterates every <see cref="PickupSpawnPoint"/> marker under
/// the chunk's SpawnLocations/PickupSpawns container, asks each marker to roll an outcome
/// (Pizza, Shot, or nothing), and spawns at most one pickup per marker. Because each
/// marker rolls a single outcome, a marker can never spawn both a Pizza and a Shot.
/// When a marker rolls "Shot", one entry from <see cref="drinkVariants"/> is picked (weighted
/// random) so different drinks (Beer, Wine, ...) can appear with different drunkenness values.
///
/// Spawned pickups are parented under RuntimeContent so they move with the chunk and are
/// kept separate from authored content.
/// </summary>
[RequireComponent(typeof(RoadChunk))]
public class PickupSpawner : MonoBehaviour
{
    /// <summary>
    /// One kind of drink that can appear at a "Shot" marker. Add as many as you like
    /// (Beer, Wine, Vodka, ...) — each with its own visual prefab, drunkenness value and
    /// relative spawn likelihood. The prefab itself only needs a visual (Renderer); the
    /// ShotPickup component and its Collider are added automatically at spawn time.
    /// </summary>
    [System.Serializable]
    public class DrinkVariant
    {
        public GameObject prefab;
        [Tooltip("Shown in debug logs when this drink is collected.")]
        public string drinkName = "Drink";
        [Tooltip("How much drunkenness this drink adds when collected.")]
        public float drunkennessValue = 200f;
        [Min(0f), Tooltip("Relative likelihood this variant is picked over the other drink variants at a 'Shot' marker.")]
        public float spawnWeight = 1f;
    }

    [Header("Pickup Prefabs (configured centrally — not per marker)")]
    [Tooltip("The existing Pizza pickup prefab. Spawned unchanged so its gameplay behavior is preserved.")]
    [SerializeField] private GameObject pizzaPrefab;
    [Tooltip("Every drink kind that can spawn at a 'Shot' marker.")]
    [SerializeField] private DrinkVariant[] drinkVariants;
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

            DrinkVariant drinkVariant = kind == PickupSpawnPoint.PickupKind.Shot ? PickRandomDrinkVariant() : null;
            GameObject prefab = kind switch
            {
                PickupSpawnPoint.PickupKind.Pizza => pizzaPrefab,
                PickupSpawnPoint.PickupKind.Shot => drinkVariant?.prefab,
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
            EnsurePickupGameplay(instance, kind, drinkVariant);

            if (logSpawnDebug)
            {
                Debug.Log($"[PickupSpawner] Spawned {kind} using '{prefab.name}' at {marker.transform.position}.", instance);
            }
        }
    }

    /// <summary>
    /// Picks one configured drink variant using weighted random selection (same scheme as
    /// PickupSpawnPoint's own weights). Returns null if no variant is configured.
    /// </summary>
    private DrinkVariant PickRandomDrinkVariant()
    {
        if (drinkVariants == null || drinkVariants.Length == 0) return null;

        float total = 0f;
        foreach (DrinkVariant variant in drinkVariants)
        {
            if (variant == null || variant.prefab == null) continue;
            total += Mathf.Max(0f, variant.spawnWeight);
        }
        if (total <= 0f) return null;

        float roll = Random.value * total;
        foreach (DrinkVariant variant in drinkVariants)
        {
            if (variant == null || variant.prefab == null) continue;
            float weight = Mathf.Max(0f, variant.spawnWeight);
            if (roll < weight) return variant;
            roll -= weight;
        }
        return null;
    }

    private static void EnsurePickupGameplay(GameObject instance, PickupSpawnPoint.PickupKind kind, DrinkVariant drinkVariant)
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
                ShotPickup shotPickup = instance.GetComponentInChildren<ShotPickup>(true);
                if (shotPickup == null)
                {
                    shotPickup = instance.AddComponent<ShotPickup>();
                }
                if (drinkVariant != null)
                {
                    shotPickup.Configure(drinkVariant.drinkName, drinkVariant.drunkennessValue);
                }
                instance.tag = "Drink";
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
