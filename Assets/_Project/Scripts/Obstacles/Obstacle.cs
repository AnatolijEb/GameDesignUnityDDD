using UnityEngine;

namespace Obstacles
{
    public enum ObstacleType
    {
        Car,
        TrashCan,
        ConstructionBlock
    }

    public class Obstacle : MonoBehaviour
    {
        [SerializeField] private ObstacleType type;

        public ObstacleType Type => type;
    }
}
