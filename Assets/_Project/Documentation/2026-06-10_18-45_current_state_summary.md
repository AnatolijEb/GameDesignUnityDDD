# Project Status Summary: Don't Drink and Drive

## Timestamp
2026-06-10_18-45

## Project Overview
"Don't Drink and Drive" is a Unity 3D endless runner prototype where the player controls a drunk pizza delivery rider. The core mechanic is **balance-based steering**, where input affects the player's lean, and the lean determines the sideways movement.

## Current Technical Architecture
The project uses a **World-Scrolling Architecture**:
- **Player Position:** The player remains fixed at approximately Z = 0.
- **World Movement:** Road chunks and obstacles move toward the player along the negative Z-axis.
- **Procedural Generation:** Chunks are spawned ahead and destroyed behind the player to create an infinite track.

## Implemented Systems

### 1. Core Management
- **GameManager:** Handles game state, game-over triggers, and scene restarts.
- **RunSpeedManager:** Manages global scrolling speed (default: 10 units/s) and tracks distance traveled.

### 2. Player Mechanics
- **PlayerBalanceController:** Handles the balance state (-1 to 1), random drift logic, and visual tilt (rotation of `PlayerVisual`).
- **PlayerMovementController:** Calculates sideways translation based on the current balance angle while keeping the Z-position locked.
- **PlayerCollisionHandler:** Detects triggers with tags "Wall" and "Obstacle" to trigger a game reset.

### 3. World & Environment
- **RoadChunkManager:** Spawns `RoadChunk_Basic` prefabs ahead of the player. It automatically detects and manages initial chunks placed in the scene.
- **RoadChunk:** A container for road geometry and side walls. Chunks are 30 units long and 15 units wide.
- **WorldScrollMover:** Moves objects backward based on the `RunSpeedManager` speed.

### 4. Obstacle System
- **ObstacleSpawner:** Randomly instantiates obstacles (Cars, Trash Cans, Construction Blocks) on each chunk.
- **ObstacleSpawnArea:** A volume-based system that allows for truly random placement of obstacles across the road width and length.
- **Obstacle Types:** Functional prefabs for Cars, Trash Cans, and Construction Blocks with appropriate scaling and trigger colliders.

### 5. Camera
- **ThirdPersonCameraFollow:** Smoothly follows the player's X position from a fixed offset `(0, 8, -16)`, providing an optimized view for the runner perspective.

## Current Scene Setup
- **Hierarchy:** Organized into logical groups: `GameManagers`, `Player`, `CameraRig`, `World`, `Lighting`, and `UI`.
- **Scaling:** The game uses a consistent scale where the player is (4, 4, 4) and the road is 15 units wide.
- **Initial State:** A reference chunk starts at Z=0, allowing for a seamless gameplay start.

## Manual Verification / How to Play
1. Press **Play**.
2. Use **A/D** or **Arrow Keys** to counterbalance the random drift.
3. Observe the player's sideways movement responding to the tilt.
4. Avoid the randomly spawned obstacles.
5. Hitting a wall or an obstacle will restart the level.

## Next Recommended Steps
1. Implement a **Pizza Life System** (e.g., 4 pizzas, losing one per obstacle hit instead of instant game over).
2. Add **Delivery Gates** as checkpoints that refill lives or increase score.
3. Add a **Score/HUD** to display distance and pizza count.
4. Implement **Difficulty Scaling** (speed increase over time).
