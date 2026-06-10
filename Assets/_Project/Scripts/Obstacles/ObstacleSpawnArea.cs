using UnityEngine;

namespace DDD.Obstacles
{
    public class ObstacleSpawnArea : MonoBehaviour
{
        public Vector3 size = new Vector3(10f, 1f, 10f);

        public Vector3 GetRandomPoint()
        {
            return transform.position + new Vector3(
                Random.Range(-size.x / 2f, size.x / 2f),
                0,
                Random.Range(-size.z / 2f, size.z / 2f)
            );
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawCube(transform.position, size);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, size);
        }
    }
}
