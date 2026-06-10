# Map Generation System Overview

This document describes the world-scrolling endless runner architecture used in "Don't Drink and Drive".

## System Architecture
The game uses a **World-Scrolling** approach where the player remains at a fixed Z-position while the environment (Road Chunks) moves backward toward the player.

### Involved Scripts

#### 1. RoadChunkManager.cs
The central controller for spawning and destroying road segments.
- **Key Fields:**
    - `roadChunkPrefabs`: Array of GameObject prefabs used for generation.
    - `activeChunksParent`: The transform under which spawned chunks are organized.
    - `initialChunks`: Number of chunks spawned at game start.
    - `chunksAhead`: Number of chunks maintained in front of the player.
    - `chunkLength`: The distance between chunk origins (Z-axis).
    - `despawnZ`: The Z-position behind the player at which chunks are destroyed.
- **Key Methods:**
    - `Start()`: Initializes the chunk list and spawns the starting track.
    - `Update()`: Monitors the last chunk's position to trigger new spawns and checks old chunks for despawning.
    - `SpawnChunk()`: Instantiates a random prefab at the calculated next Z-position.

#### 2. RoadChunk.cs
Attached to the root of every road chunk prefab. Holds metadata and child references.
- **Key Fields:**
    - `chunkLength`: The physical length of the chunk.
    - `road`, `wallLeft`, `wallRight`, `obstacleParent`: References to child components for easier access.
- **Properties:**
    - `EndPosition`: Calculates the world position of the chunk's end.

#### 3. WorldScrollMover.cs
Attached to every spawned chunk. Handles the actual movement.
- **Logic:** In `Update()`, it translates the object along `Vector3.back` based on `RunSpeedManager.CurrentSpeed`.

#### 4. RunSpeedManager.cs
A singleton that provides the global scroll speed.
- **Key Fields:**
    - `baseSpeed`: Initial scrolling speed.
    - `speedIncreasePerSecond`: Rate of acceleration.
    - `maxSpeed`: Maximum allowed speed.
- **Properties:**
    - `CurrentSpeed`: The speed currently applied to all world objects.

## Chunk Configuration
- **Dimensions:** 
    - **Width:** 15 units (Road scale).
    - **Length:** 30 units (Standard `chunkLength`).
- **Active Count:** The system maintains roughly 10 active chunks at any time (5 initial + 5 ahead, minus those already despawned).

---

## Core Script Content

### GameManager.cs
```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void TriggerGameOver()
    {
        RestartGame();
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
```

### PlayerCollisionHandler.cs
```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerCollisionHandler : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            // TODO: replace with pizza loss once life system is implemented
            GameManager gm = Object.FindFirstObjectByType<GameManager>();
            if (gm != null)
            {
                gm.TriggerGameOver();
            }
            else
            {
                // Fallback to reloading the active scene directly
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }
}
```

### PlayerMovementController.cs
```csharp
using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{
    [Header("Steering")]
    public float steerStrength = 4f;
    public PlayerBalanceController balanceController;

    private float initialZ;

    private void Awake()
    {
        if (balanceController == null)
        {
            balanceController = GetComponent<PlayerBalanceController>();
        }
        
        initialZ = transform.position.z;
    }

    private void Update()
    {
        // 1. Steering (Lean translates to sideways movement) - Move in World Space to ignore tilt
        if (balanceController != null)
        {
            transform.Translate(Vector3.right * balanceController.BalanceAngle * steerStrength * Time.deltaTime, Space.World);
        }

        // 2. Lock Z position (since the world moves toward the player)
        Vector3 pos = transform.position;
        pos.z = initialZ;
        transform.position = pos;
    }
}
```
