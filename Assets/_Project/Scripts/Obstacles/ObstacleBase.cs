using UnityEngine;

using UnityEngine;

public class ObstacleBase : MonoBehaviour
{
    [SerializeField] private ObstacleTypeSO obstacleData;
    public ObstacleTypeSO Data => obstacleData;

    public virtual void OnPlayerContact()
    {
        // Default: lose pizzas. Override for special behavior.
        // PizzaLifeSystem not yet implemented — leave as comment stub:
        // PizzaLifeSystem.Instance?.LosePizzas(obstacleData.pizzasLostOnContact);
    }
}
