using UnityEngine;

public class PizzaPickup : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerLifeSystem lifeSystem = other.GetComponent<PlayerLifeSystem>();
            if (lifeSystem != null)
            {
                lifeSystem.AddLife();
                Debug.Log("[PizzaPickup] Collected! Pizza disappeared.");
                Destroy(gameObject);
            }
        }
    }
}
