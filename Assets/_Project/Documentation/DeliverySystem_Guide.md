# Delivery System & Success Boundaries

This document explains how successful deliveries are triggered, how to configure the distance boundaries, and how the visual indicators are set up.

## 1. How a Delivery is Triggered
A delivery is considered successful when the player passes through a **Delivery Gate**.
- **Script:** `DeliveryGateHandler.cs` (attached to the `DeliveryGate_Placeholder` prefab).
- **Trigger:** Uses a `BoxCollider` set to `isTrigger`. When the player root (tagged "Player") enters this collider, it calls `DifficultyManager.Instance.OnDeliveryCompleted()`.

## 2. Configuring the Delivery Boundary (Distance)
The frequency and position of successful delivery stages are controlled by the distance the player has traveled.
- **Location:** `RoadChunkManager` object in the scene.
- **Setting:** `Distance Between Deliveries`.
- **How it works:** The `RoadChunkManager` tracks the total distance of spawned chunks. When the next spawn position exceeds the current distance milestone, it attaches a `DeliveryGate_Placeholder` to the new chunk.

## 3. Visual Indicators (Green Markers)
To mark the successful delivery point in the world, green squares are placed at the edges of the road.
- **Prefab:** `Assets/_Project/Prefabs/Delivery/DeliveryGate_Placeholder.prefab`.
- **Visuals:** The prefab contains two green cubes (`LeftMarker` and `RightMarker`) positioned at the boundaries of the road (X = ±7.25).
- **Customization:** You can adjust the size or material of these markers directly in the prefab if you want them to be more or less prominent.

## 4. Adjusting the Physical "Gate" Size
If you want to change the physical area that counts as a delivery:
1. Open the `DeliveryGate_Placeholder` prefab.
2. Select the root object.
3. Adjust the **Box Collider** size.
   - **X-Scale:** Should stay around 14-15 to cover the entire road width.
   - **Y-Scale:** Determines how high the "invisible wall" is.
   - **Z-Scale:** Determines the "thickness" of the trigger area.

## 5. Summary of Inspector Settings
| Location | Setting | Description |
| :--- | :--- | :--- |
| **RoadChunkManager** | `Distance Between Deliveries` | How many units the player must travel between each difficulty increase. |
| **DeliveryGate Prefab** | `Box Collider Size` | The physical boundary the player must cross. |
| **DifficultyManager** | `Tiers` | What happens (speed/obstacles) after each successful boundary is crossed. |
