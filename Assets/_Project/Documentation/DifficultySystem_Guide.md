# Difficulty Management System - Technical Guide

This document provides a comprehensive overview of how difficulty is implemented and managed in "Don't Drink and Drive".

## 1. System Overview
The difficulty system controls the progression of the game by increasing the challenge after every successful pizza delivery. It centrally manages:
- **World Speed:** Increases the base scrolling speed of the environment.
- **Obstacle Density:** Controls how many obstacles spawn and how likely they are to appear.
- **Progression:** Tracks deliveries and advances through difficulty "Tiers".

## 2. Core Components

### DifficultyManager.cs
The central singleton component (located on the `GameManagers` object). It holds the list of difficulty tiers and tracks the current game state.
- **Current Tier Index:** The index of the active difficulty setting.
- **Delivery Count:** How many deliveries the player has completed in the current run.
- **Initial Base Speed:** Captured at start from `RunSpeedManager` to calculate bonuses correctly.

### DifficultyTierConfig.cs
A data class used to define each difficulty level. It is configured as a list within the `DifficultyManager` inspector.
- **Tier Label:** A descriptive name for the level (e.g., "Moderate Traffic").
- **Max Obstacles Per Chunk:** Limits the total number of obstacles that can appear in one road segment.
- **Spawn Chance:** The probability (0.0 to 1.0) that a spawn point will actually instantiate an obstacle.
- **Speed Bonus:** A flat value added to the game's base speed for that specific tier.

---

## 3. Key Features

### Safe Start Zone
To give the player time to adjust, the system includes a "Safe Start" feature.
- **Initial Safe Chunks:** Set this value (e.g., 3) in the `DifficultyManager`.
- **Behavior:** The first $N$ chunks spawned in a run will have **zero obstacles**, regardless of the current difficulty tier.

### Dynamic Reconfiguration
When a tier changes (after a delivery), the `DifficultyManager`:
1. Updates the `RunSpeedManager`'s base speed.
2. Finds all `ObstacleSpawner` components currently in the scene and updates their configuration.
   - *Note:* Already spawned obstacles remain, but the next chunks to spawn obstacles will use the new settings.

---

## 4. Configuration & Tuning (The Inspector)

### Adjusting Safe Start
Adjust the **Initial Safe Chunks** slider to set how many "free" segments the player gets at the start.

### Customizing Tiers
In the **Tiers** list, you can add or remove elements. For each tier, you can tune:
- **Obstacle Spawning:** Higher `Max Obstacles` combined with higher `Spawn Chance` creates denser traffic.
- **Speed:** The `Speed Bonus` directly impacts how fast the world moves toward the player.

### How to Progress
Difficulty advances automatically when a `DeliveryGate` is triggered. The gate calls `DifficultyManager.Instance.OnDeliveryCompleted()`, which increments the delivery count and checks if the tier should be advanced.

---

## 5. Implementation Flow for Developers

1. **Start of Run:** `DifficultyManager` initializes, captures the base speed, and prepares the Safe Start counter.
2. **Chunk Spawning:** As the `RoadChunkManager` spawns a new chunk, the `ObstacleSpawner` on that chunk calls `ClaimSafeStartChunk()`.
   - If it returns `true`, the spawner skips logic.
   - If `false`, it pulls the `CurrentTierConfig` and spawns obstacles based on those weights.
3. **Delivery:** Player hits a `DeliveryGate`.
   - `DifficultyManager` advances the `currentTierIndex`.
   - `ApplyCurrentTier()` is called to update speeds and existing spawners.

---

## 6. How to Extend the System

### Adding New Difficulty Parameters
If you want to add a new parameter (e.g., "Drift Intensity"), follow these steps:
1. Add the variable to `DifficultyTierConfig.cs`.
2. Update `ApplyCurrentTier()` in `DifficultyManager.cs` to pass this new value to the relevant system (e.g., the Player Controller).

### Adding More Tiers
Simply increase the **Size** of the Tiers array in the Inspector and fill in the new values. The system automatically clamps to the last tier if the delivery count exceeds the number of defined tiers.
