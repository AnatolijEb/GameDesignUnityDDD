using UnityEngine;

namespace Obstacles
{
    public class ObstacleSpawnPoint : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, Vector3.one);
        }
    }
}
