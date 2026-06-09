using UnityEngine;
using System.Collections.Generic;

public class RoadChunkManager : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public RunSpeedManager runSpeedManager;
    public GameObject[] roadChunkPrefabs;
    public Transform activeChunksParent;

    [Header("Settings")]
    public int initialChunks = 5;
    public int chunksAhead = 5;
    public float chunkLength = 30f;
    public float despawnZ = -80f;

    private List<GameObject> activeChunks = new List<GameObject>();
    private float lastSpawnZ = 0f;

    private void Start()
    {
        if (runSpeedManager == null) runSpeedManager = RunSpeedManager.Instance;
        if (player == null) player = GameObject.FindWithTag("Player")?.transform;

        // Detect existing chunks in parent
        if (activeChunksParent != null)
        {
            foreach (Transform child in activeChunksParent)
            {
                if (child.GetComponent<RoadChunk>() != null)
                {
                    activeChunks.Add(child.gameObject);
                }
            }
            // Sort by Z to keep the queue order correct
            activeChunks.Sort((a, b) => a.transform.position.z.CompareTo(b.transform.position.z));
        }

        // Initial spawn (if not enough chunks are in the scene already)
        while (activeChunks.Count < initialChunks)
        {
            SpawnChunk();
        }
    }

    private void Update()
    {
        // Check if we need to spawn a new chunk
        // We look at the last spawned chunk. If it has moved back enough that we need another one ahead.
        if (activeChunks.Count > 0)
        {
            GameObject lastChunk = activeChunks[activeChunks.Count - 1];
            if (lastChunk.transform.position.z < (chunksAhead - 1) * chunkLength)
            {
                SpawnChunk();
            }
        }

        // Cleanup old chunks
        for (int i = activeChunks.Count - 1; i >= 0; i--)
        {
            if (activeChunks[i].transform.position.z < despawnZ)
            {
                GameObject oldChunk = activeChunks[i];
                activeChunks.RemoveAt(i);
                Destroy(oldChunk);
            }
        }
    }

    private void SpawnChunk()
    {
        if (roadChunkPrefabs == null || roadChunkPrefabs.Length == 0) return;

        float spawnZ = 0f;
        if (activeChunks.Count > 0)
        {
            spawnZ = activeChunks[activeChunks.Count - 1].transform.position.z + chunkLength;
        }

        int randomIndex = Random.Range(0, roadChunkPrefabs.Length);
        GameObject chunkObj = Instantiate(roadChunkPrefabs[randomIndex], new Vector3(0, 0, spawnZ), Quaternion.identity, activeChunksParent);
        
        activeChunks.Add(chunkObj);
    }
}
