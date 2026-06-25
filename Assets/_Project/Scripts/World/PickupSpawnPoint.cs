using UnityEngine;

/// <summary>
/// Unified pickup spawn marker. Designers place these manually under
/// SpawnLocations/PickupSpawns inside a chunk variant.
///
/// A marker does NOT hold prefab references — the actual Pizza / Shot prefabs are
/// configured centrally on the chunk's <see cref="PickupSpawner"/> component, so the
/// same references never have to be re-assigned on every marker.
///
/// Each marker independently decides, at runtime, whether it spawns a Pizza, a Shot,
/// or nothing. Exactly ONE outcome is chosen per marker, so a marker can never spawn
/// both a Pizza and a Shot.
///
/// IMPORTANT: The three values below are WEIGHTS (relative likelihoods), NOT percentages.
/// The chance of an outcome = thatWeight / (pizzaWeight + shotWeight + emptyWeight).
/// Example: pizza=1, shot=1, empty=8  ->  10% pizza, 10% shot, 80% nothing.
/// Defaults favor "empty" so pickups stay uncommon.
/// </summary>
public class PickupSpawnPoint : MonoBehaviour
{
    public enum PickupKind { None, Pizza, Shot }

    [Header("Spawn Weights (relative likelihoods, NOT percentages)")]
    [Min(0f), Tooltip("Relative weight for spawning a Pizza at this marker.")]
    public float pizzaWeight = 1f;
    [Min(0f), Tooltip("Relative weight for spawning a Shot at this marker.")]
    public float shotWeight = 1f;
    [Min(0f), Tooltip("Relative weight for spawning nothing. Keep high so pickups stay uncommon.")]
    public float emptyWeight = 8f;

    /// <summary>
    /// Picks a single outcome based on the configured weights.
    /// Guaranteed to return exactly one kind, so Pizza and Shot are mutually exclusive.
    /// </summary>
    public PickupKind Roll()
    {
        float pizza = Mathf.Max(0f, pizzaWeight);
        float shot = Mathf.Max(0f, shotWeight);
        float empty = Mathf.Max(0f, emptyWeight);
        float total = pizza + shot + empty;

        if (total <= 0f) return PickupKind.None;

        float roll = Random.value * total;
        if (roll < pizza) return PickupKind.Pizza;
        roll -= pizza;
        if (roll < shot) return PickupKind.Shot;
        return PickupKind.None;
    }

    // Visible only in the Scene view (editor gizmo); never rendered during gameplay.
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 0.85f, 1f, 1f);
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.color = new Color(0.2f, 0.85f, 1f, 0.2f);
        Gizmos.DrawSphere(transform.position, 0.5f);
        Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 1.5f);
    }
}
