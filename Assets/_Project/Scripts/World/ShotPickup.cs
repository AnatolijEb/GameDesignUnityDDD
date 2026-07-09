using UnityEngine;

public class ShotPickup : MonoBehaviour
{
    [Header("Shot Settings")]
    [SerializeField] private string drinkName = "Shot";
    [SerializeField] private float drunkennessIncrease = 200f;

    /// <summary>
    /// Overrides this pickup's display name and drunkenness value. Used by PickupSpawner to turn
    /// a plain visual prefab into a specific drink variant (Beer, Wine, ...) at spawn time.
    /// </summary>
    public void Configure(string name, float value)
    {
        drinkName = name;
        drunkennessIncrease = value;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (DrunkennessSystem.Instance != null)
            {
                DrunkennessSystem.Instance.AddDrunkenness(drunkennessIncrease);
                Debug.Log($"[ShotPickup] Collected {drinkName}! Drunkenness increased by {drunkennessIncrease}.");
                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning("[ShotPickup] DrunkennessSystem.Instance is null!");
            }
        }
    }
}
