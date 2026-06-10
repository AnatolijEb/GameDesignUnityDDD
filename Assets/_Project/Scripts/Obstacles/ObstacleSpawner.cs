using UnityEngine;
using System.Collections.Generic;

namespace Obstacles
{
    public class ObstacleSpawner : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject[] obstaclePrefabs;

        [Header("Spawn Points (Optional)")]
        [SerializeField] private ObstacleSpawnPoint[] spawnPoints;

        [Header("Spawn Areas")]
        [SerializeField] private ObstacleSpawnArea[] spawnAreas;

        [Header("Settings")]
        [SerializeField] private int minObstacles = 1;
        [SerializeField] private int maxObstacles = 3;

        private void Start()
        {
            SpawnObstacles();
        }

        private void SpawnObstacles()
        {
            if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) return;

            int count = Random.Range(minObstacles, maxObstacles + 1);
            if (count == 0) return;

            Transform obstacleParent = transform.Find("ObstacleParent");
            if (obstacleParent == null)
            {
                GameObject parentObj = new GameObject("ObstacleParent");
                parentObj.transform.SetParent(transform);
                parentObj.transform.localPosition = Vector3.zero;
                obstacleParent = parentObj.transform;
            }

            for (int i = 0; i < count; i++)
            {
                Vector3 spawnPos = Vector3.zero;
                Quaternion spawnRot = Quaternion.identity;

                // Priority 1: Spawn Areas
                if (spawnAreas != null && spawnAreas.Length > 0)
                {
                    ObstacleSpawnArea area = spawnAreas[Random.Range(0, spawnAreas.Length)];
                    spawnPos = area.GetRandomPoint();
                    spawnRot = area.transform.rotation;
                }
                // Priority 2: Spawn Points
                else if (spawnPoints != null && spawnPoints.Length > 0)
                {
                    // For points, we usually want to avoid duplicates if possible
                    // But for areas, we just pick random points.
                    ObstacleSpawnPoint point = spawnPoints[Random.Range(0, spawnPoints.Length)];
                    spawnPos = point.transform.position;
                    spawnRot = point.transform.rotation;
                }
                else
                {
                    continue;
                }

                GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
                GameObject instance = Instantiate(prefab, obstacleParent);
                
                // Use SetPositionAndRotation to correctly place in world space
                instance.transform.SetPositionAndRotation(spawnPos, spawnRot);
            }
        }
    }
}
