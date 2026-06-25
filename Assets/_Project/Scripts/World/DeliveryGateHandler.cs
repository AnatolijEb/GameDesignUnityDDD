using UnityEngine;

public class DeliveryGateHandler : MonoBehaviour
{
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("Player")) return;

        hasTriggered = true;
        Debug.Log("[Delivery] Gate triggered! Delivery completed.");

        // TODO: call PizzaLifeSystem.Instance?.Deliver() once implemented
        
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
    }
}
