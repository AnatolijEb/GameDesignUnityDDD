# Milestone: Obstacle System & Difficulty Progression

## Timestamp
2026-06-11_15-00

## Overview
This update implements the core gameplay loop elements: randomized obstacle spawning with lane safety, a tiered difficulty system, and delivery gates that progress the game state.

---

## 1. Obstacle System (Implementation & Spawning)

### Scripts
- **ObstacleSpawner.cs**: 
  - Attached to each `RoadChunk`.
  - Handles weighted random spawning of obstacles based on `ObstacleTypeSO` weights.
  - **Lane Safety**: Ensures that at least one lane (out of 3) remains clear, preventing impossible-to-avoid configurations.
  - **Dynamic Configuration**: Supports mid-run updates via a `Configure()` method.

### Assets & Prefabs
- **Prefabs**: Created `Obstacle_Car`, `Obstacle_TrashCan`, and `Obstacle_ConstructionBlock` using primitive shapes.
- **Components**: Each prefab uses the `ObstacleBase` component linked to specific `ObstacleTypeSO` data.
- **Collision**: All obstacles use `BoxCollider` set to `isTrigger = true` and the "Obstacle" tag.

---

## 2. Difficulty & Progression System

### Scripts
- **DifficultyTierConfig.cs**: A data container for difficulty parameters (tier index, labels, obstacle count, spawn chance, and speed bonuses).
- **DifficultyManager.cs**: 
  - A singleton placed on the `GameManagers` object.
  - Manages the current `currentTierIndex` and `deliveryCount`.
  - **Progression**: When a delivery is completed, the game advances to the next tier (up to Tier 3).
  - **Global Impact**: Updates `RunSpeedManager.baseSpeed` and reconfigures all active `ObstacleSpawner` components in the scene.

### Pre-defined Tiers
1. **Tier 0 (No Traffic)**: 0 obstacles, 0% chance, 0 speed bonus.
2. **Tier 1 (Light Traffic)**: 1 max obstacle, 40% chance, +0.5 speed bonus.
3. **Tier 2 (Moderate Traffic)**: 2 max obstacles, 65% chance, +1.5 speed bonus.
4. **Tier 3 (Heavy Traffic)**: 3 max obstacles, 85% chance, +3.0 speed bonus.

---

## 3. Delivery Mechanics

### Scripts
- **DeliveryGateHandler.cs**: 
  - Handles the player passing through checkpoints.
  - Triggers the `OnDeliveryCompleted` event in the `DifficultyManager`.

### Assets
- **DeliveryGate_Placeholder.prefab**: Updated with the handler script and a wide trigger collider (14 units) to ensure detection across the road.

---

## Gameplay Preservation & Integration
- **Collision**: `PlayerCollisionHandler.cs` now detects the "Obstacle" tag, triggering a restart (instant game over).
- **Movement**: Difficulty-based speed increases are applied to the `RunSpeedManager`, affecting world-scrolling speed globally.
- **Future-Proofing**: Clearly marked stubs for the `PizzaLifeSystem` have been added to the collision and delivery logic.

## Manual Verification Checklist
- [ ] Observe obstacles spawning randomly on new road chunks.
- [ ] Verify that at least one lane remains clear when multiple obstacles spawn.
- [ ] Pass through a Delivery Gate and verify (via Inspector) that `currentTierIndex` and `deliveryCount` increase.
- [ ] Confirm the world movement speed increases after a delivery.
- [ ] Confirm hitting an obstacle restarts the scene.
