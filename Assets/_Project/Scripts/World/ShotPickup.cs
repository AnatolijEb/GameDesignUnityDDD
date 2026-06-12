using UnityEngine;

public class ShotPickup : MonoBehaviour
{
    [Header("Shot Settings")]
    [SerializeField] private float drunkennessIncrease = 200f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (DrunkennessSystem.Instance != null)
            {
                DrunkennessSystem.Instance.AddDrunkenness(drunkennessIncrease);
                Debug.Log($"[ShotPickup] Collected! Drunkenness increased by {drunkennessIncrease}.");
                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning("[ShotPickup] DrunkennessSystem.Instance is null!");
            }
        }
    }
}
