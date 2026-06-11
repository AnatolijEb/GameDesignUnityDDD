using UnityEngine;
using System.Collections.Generic;

public class BuildingRandomizer : MonoBehaviour
{
    public List<GameObject> buildingPrefabs;
    public float scale = 4.0f;
    public float xOffset = 18f;
    public int buildingsPerSide = 4;
    public float chunkLength = 30f;

    void Awake()
    {
        // Clear existing preview buildings from prefab
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        if (buildingPrefabs == null || buildingPrefabs.Count == 0) return;

        float spacing = chunkLength / buildingsPerSide;
        float startZ = -chunkLength / 2f + spacing / 2f;

        for (int i = 0; i < buildingsPerSide; i++)
        {
            float z = startZ + i * spacing;

            // Left
            SpawnBuilding(new Vector3(-xOffset, 0, z), 90f);
            // Right
            SpawnBuilding(new Vector3(xOffset, 0, z), -90f);
        }
    }

    void SpawnBuilding(Vector3 localPos, float rotationY)
    {
        GameObject prefab = buildingPrefabs[Random.Range(0, buildingPrefabs.Count)];
        GameObject b = Instantiate(prefab, transform);
        b.transform.localPosition = localPos;
        b.transform.localRotation = Quaternion.Euler(0, rotationY, 0);
        b.transform.localScale = Vector3.one * scale;
    }
}
