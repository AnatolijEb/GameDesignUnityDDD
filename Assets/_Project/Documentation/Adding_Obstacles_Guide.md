# Guide: Adding New Obstacles

This guide explains how to add new types of obstacles to "Don't Drink and Drive" without writing any additional code. The system is entirely data-driven.

## 1. Create the Obstacle Prefab
Every obstacle must be a Unity Prefab.
1. Place your 3D model (or primitive) in a scene.
2. **Tag:** Set the GameObject's Tag to **"Obstacle"**.
3. **Collider:** Add a Collider component (e.g., Box Collider, Sphere Collider).
4. **Trigger:** Enable the **"Is Trigger"** checkbox on the collider.
5. **Component:** Add the **`ObstacleBase`** component to the object.
6. **Save:** Drag the object from the Hierarchy into your Project window (e.g., `Assets/_Project/Prefabs/Obstacles/`) to create a Prefab.

## 2. Create the Obstacle Data (ScriptableObject)
Each obstacle needs a data asset to define its gameplay properties and weights.
1. In the Project window, right-click and select **Create > DDD > Obstacle Type**.
2. Name the new asset (e.g., `SO_Obstacle_Hydrant`).
3. Configure the asset in the Inspector:
    - **Display Name:** The name shown in console logs.
    - **Prefab:** Assign the Prefab you created in Step 1.
    - **Spawn Weight:** How frequently this obstacle appears (1 = rare, 10 = common).
    - **Min Difficulty Tier:** The minimum game tier required for this to spawn (0 = spawns from start).
    - **Pizzas Lost:** How many pizzas are removed on contact (for future life system).

## 3. Register the Obstacle in the Spawner
To make the new obstacle appear on the road, you must register it with the road chunk's spawner.
1. Locate the **`RoadChunk_Basic`** prefab in `Assets/_Project/Prefabs/World/`.
2. Open the prefab and select the root object.
3. Find the **`Obstacle Spawner`** component in the Inspector.
4. Expand the **"Obstacle Types"** list.
5. Increase the list **Size** or replace an existing slot.
6. Drag your new **ScriptableObject** (from Step 2) into the slot.
7. Save the prefab.

## Summary Checklist
- [ ] Is the Prefab tagged "Obstacle"?
- [ ] Is "Is Trigger" enabled on the Prefab's collider?
- [ ] Does the Prefab have the `ObstacleBase` component?
- [ ] Does the ScriptableObject reference the correct Prefab?
- [ ] Is the ScriptableObject added to the `Obstacle Spawner` list on the `RoadChunk_Basic` prefab?

---

## Technical Note for Developers
- The spawning logic is handled in `ObstacleSpawner.cs`.
- It performs a weighted random selection based on the `spawnWeight` of the registered `ObstacleTypeSO` assets.
- It respects the `minDifficultyTier` and current difficulty settings from the `DifficultyManager`.