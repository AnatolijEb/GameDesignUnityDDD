# Architecture Milestone: Modular Prototype Setup

## Timestamp

2026-06-09_15-44

## Project Summary

“Don’t Drink and Drive” is a 3D PC endless-runner prototype where the player controls a drunk pizza delivery rider through balance-based steering instead of direct steering.

## Current Gameplay State

* automatic forward movement
* balance-based steering
* random drift
* counterbalancing with keyboard input
* visual tilt
* third-person camera follow
* wall collision as current game-over condition

## Current Scene Hierarchy

[Scene]
├── GameManagers
│   └── GameManager
├── Player
│   └── PlayerVisual
├── CameraRig
│   └── Main Camera
├── World
│   ├── Road
│   ├── WallLeft
│   ├── WallRight
│   └── SpawnReferencePoints
├── UI
└── Lighting

*Note: The actual hierarchy matches the intended structure perfectly.*

## Script Architecture

### GameManager

Responsible for game state handling and restart/game-over methods.

### PlayerBalanceController

Responsible for balance value, random drift, keyboard input and visual tilt.

### PlayerMovementController

Responsible for automatic forward movement and sideways movement based on balance.

### PlayerCollisionHandler

Responsible for detecting wall contact and triggering game over or restart.

### ThirdPersonCameraFollow

Responsible for smooth third-person camera follow from behind and above.

## Important Gameplay Preservation Notes

The current gameplay feel should be preserved in future changes:

* Do not replace balance steering with direct steering.
* Do not use Rigidbody motorcycle physics.
* Do not make tilt itself a death condition.
* Do not change movement and balance values without intentional tuning.
* The root Player object should move.
* PlayerVisual should handle visual tilt.
* Camera should follow the Player root, not PlayerVisual.

## Current Prefab Setup

* Player.prefab
* RoadChunk_Basic.prefab
* WallSegment.prefab
* Obstacle_CubePlaceholder.prefab
* DeliveryGate_Placeholder.prefab

## Current Limitations

* no obstacle gameplay
* no pizza life system
* no delivery gates
* no score system
* no HUD
* no difficulty scaling
* no audio
* no final assets
* no itch.io build setup yet

## Recommended Next Development Steps

1. Add obstacle collision with pizza loss.
2. Add pizza life system with 4 starting pizzas.
3. Add delivery gates that refill pizzas and increase score.
4. Add basic HUD for score and pizzas.
5. Add difficulty scaling after each delivery gate.
6. Add simple night atmosphere and placeholder environment objects.
7. Add audio and visual polish.
8. Prepare WebGL or Windows build for itch.io.

## Manual Checks

* Press Play and verify forward movement.
* Verify that A/D and arrow keys affect balance only.
* Verify that balance causes sideways movement.
* Verify that PlayerVisual tilts.
* Verify that the camera follows the Player root.
* Verify that wall collision restarts or ends the run.
* Verify that no old PlayerController or CameraFollow component is still active unless intentionally kept as backup.
