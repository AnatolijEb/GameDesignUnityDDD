# Gameplay Tuning and Technical Fixes Guide

## Timestamp
2026-06-11_15-53

## 1. Player Steering & Balance Tuning
The steering feel is controlled by two components on the **Player** object:

### Player Balance Controller
- **Counter Force:** Controls how quickly the player responds to input (A/D or Arrow keys) to change their lean angle.
  - *Increase:* More responsive, snappier lean.
  - *Decrease:* Heavier, more sluggish feel.

### Player Movement Controller
- **Steer Strength:** Controls the actual sideways translation speed based on the current lean angle.
  - *Increase:* The player moves faster across the road for the same amount of tilt.
  - *Decrease:* Gentler, slower sideways movement.

---

## 2. Camera System & Perspective
The camera has been stabilized to prevent perspective distortion during sideways movement.

- **Stabilization:** The camera now maintains a fixed forward rotation. It looks at a point straight ahead rather than directly targeting the player's center. This ensures road lines stay parallel and prevents the "skewing" effect.
- **Settings:**
  - **FOV:** Set to **50** for reduced edge distortion.
  - **Offset:** Optimized to `(0, 3, -8)` to match the new player height.

---

## 3. Collision & Physics Setup
Recent fixes addressed issues where the player was "floating" over obstacles.

- **Player Alignment:** 
  - **Scale:** Standardized to **1.8**.
  - **Position:** Y-position is set to **0.9** (center height) to ensure the bottom of the cube sits at Y=0 (road surface).
- **Physics Optimization:**
  - **Rigidbody:** Set to `Continuous Speculative` for better detection of high-speed moving triggers.
  - **Interpolation:** Enabled to smooth out the movement between script-based translation and the physics engine.
- **Obstacles:** All obstacle prefabs (Car, Trash Can, Construction Block) have been scaled by 2.5x to match the road and player scale, and use `isTrigger = true`.

---

## 4. Map & Progression Settings
- **Distance Between Deliveries:** Set in the **RoadChunkManager**. This is the only value needed to change the "boundary" length of a delivery stage.
- **Safe Start:** Configured in the **DifficultyManager**. Determines how many chunks at the start of a run are guaranteed to be empty.
- **Player Tag:** The Player root object must be tagged with **"Player"** for Delivery Gates to trigger.

---

## 5. Console Debugging
Key events are logged to the console for easier debugging:
- `[Collision] Hit: <Object>`: When hitting a wall or obstacle.
- `[Delivery] Gate triggered!`: Upon successful delivery.
- `[Difficulty] Applying Tier X`: When the game progresses to the next difficulty level.
- `[Spawner] Spawned X obstacles`: Feedback on chunk generation.
