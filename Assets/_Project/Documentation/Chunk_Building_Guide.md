# Chunk Building Guide

This project builds the endless road from **modular chunks**. Every chunk shares one
base prefab — **`RoadChunk_Basic`** — and every new chunk you author is a **Prefab
Variant** of it. This keeps geometry, walls, scrolling, buildings, materials and the
core scripts identical across all chunks while letting you customize obstacles,
pickups and coins per variant.

- Base prefab: `Assets/_Project/Prefabs/World/RoadChunk_Basic.prefab`
- Example variants: `RoadChunk_Empty.prefab`, `RoadChunk_ObstacleExample.prefab`
- Manager: `RoadChunkManager` (in `Assets/_Project/Scenes/SampleScene.unity`)

---

## 1. The shared base prefab (`RoadChunk_Basic`)

`RoadChunk_Basic` contains everything that must be identical on every chunk:

```
RoadChunk_Basic            (RoadChunk, WorldScrollMover, ObstacleSpawner, PickupSpawner)
├── Geometry
│   ├── Road               (Mat_Road, BoxCollider)
│   ├── WallLeft           (Mat_Wall, trigger, tag "Wall")
│   └── WallRight          (Mat_Wall, trigger, tag "Wall")
├── Environment
│   ├── Buildings          (7 fixed buildings — identical on every chunk)
│   └── Decorations        (empty container for static scenery)
├── AuthoredContent
│   ├── Obstacles          (you place obstacle prefabs here, per variant)
│   └── Coins              (you place Coin placeholders here, per variant)
├── SpawnLocations
│   ├── PickupSpawns       (you place PickupSpawnPoint markers here, per variant)
│   └── ObstacleSpawns     (5 ObstacleSpawnPoint markers used by runtime difficulty spawning)
└── RuntimeContent         (runtime-spawned obstacles & pickups are parented here)
```

**Fixed length:** the chunk is **30 units** long. This is intentional and must not
change — `RoadChunkManager` spaces chunks by exactly 30 so they connect seamlessly.
Variable-length chunks are **not** supported.

---

## 2. Creating a new chunk as a Prefab Variant

1. In the Project window, select `Assets/_Project/Prefabs/World/RoadChunk_Basic.prefab`.
2. Right-click → **Create → Prefab Variant** (or drag it into a scene, then drag the
   instance back into the Project — Unity will offer **Prefab Variant**).
3. Name it `RoadChunk_<Something>` and place it in `Assets/_Project/Prefabs/World/`.
4. Double-click the variant to open it and add your authored content (see below).

> Do **not** duplicate `RoadChunk_Basic` and edit the copy. A duplicate would not
> inherit future fixes. Always create a **Variant**.

### Why variants?
Because all chunks share `RoadChunk_Basic`, any fix made to the base — new building,
material change, script field, container rename — **automatically propagates** to every
variant. A duplicated prefab would silently drift out of sync. Variants give you "edit
once, update everywhere" while still allowing per-chunk customization through overrides.

---

## 3. What you must NOT change in a variant

Leave the inherited base structure intact. Do not modify or remove:

- `Geometry/Road`, `Geometry/WallLeft`, `Geometry/WallRight`
- `Environment/Buildings` (buildings stay identical on every chunk — **do not** randomize)
- The `RoadChunk`, `WorldScrollMover`, `ObstacleSpawner`, `PickupSpawner` components
- The `RoadChunk.chunkLength` value (must stay **30**)
- The container objects and the `RoadChunk` references that point to them

You **only** add content under `AuthoredContent/*`, `SpawnLocations/PickupSpawns`,
and (rarely) `Environment/Decorations`.

---

## 4. Placing obstacles (manual)

- Drag an existing obstacle prefab from `Assets/_Project/Prefabs/Obstacles/` into
  **`AuthoredContent/Obstacles`** inside your variant.
- Existing obstacle prefabs stay reusable and keep their collider, tag (`Obstacle`)
  and `ObstacleBase` behavior — instantiate them as nested prefab instances.
- Position them in **local** space (the road spans x = −7.5 … +7.5; lanes are roughly
  x = −5 / 0 / +5).
- Runtime difficulty spawning (`ObstacleSpawner`) only ever writes to `RuntimeContent`,
  so it will **never** move, overwrite or delete your manually placed obstacles.

### Keep at least one safe route
Always leave one continuous lane open from the start to the end of the chunk so the
player can survive. The simplest rule: **never block all three lanes at the same Z**.
In `RoadChunk_ObstacleExample` the center lane (x = 0) is left completely clear and the
coins trace that safe path.

---

## 5. Unified pickup markers (`PickupSpawnPoint`)

There is **one** marker type for all pickups — no separate Pizza/Shot markers.

**To add markers:**
1. Inside your variant, create an empty GameObject under
   **`SpawnLocations/PickupSpawns`**.
2. Add the **`PickupSpawnPoint`** component.
3. Position it where a pickup could appear. It shows a blue gizmo in the Scene view and
   is **invisible during gameplay**.

**How spawning works:**
- At runtime the chunk's central **`PickupSpawner`** finds every `PickupSpawnPoint`
  under `PickupSpawns` and asks each marker to roll a single outcome:
  **Pizza, Shot, or nothing**.
- Only **one** pickup spawns per marker — a marker can **never** spawn both a Pizza and
  a Shot.
- The selected pickup is instantiated from the existing `PizzaPickup` / `ShotPickup`
  prefabs (gameplay behavior preserved) and parented under **`RuntimeContent`**, so it
  moves with the chunk.

### Pizza / Shot / empty probabilities
The three fields on `PickupSpawnPoint` are **WEIGHTS, not percentages**:

```
chance(outcome) = thatWeight / (pizzaWeight + shotWeight + emptyWeight)
```

Example: `pizzaWeight = 2`, `shotWeight = 1`, `emptyWeight = 7` →
20% Pizza, 10% Shot, 70% nothing. Defaults favor **empty** so pickups stay uncommon.
Because the weights are normalized at roll time, any non-negative values are valid and
can never produce both pickups.

### Central prefab references
The actual Pizza and Shot **prefab references live once on `PickupSpawner`** (on the
chunk root, inherited from the base). You do **not** assign prefabs on each marker — set
weights per marker, leave the prefab wiring to the base.

---

## 6. Coin placeholders

- Prefab: `Assets/_Project/Prefabs/Collectibles/Coin_Placeholder.prefab`
  (a small flat yellow cube using `Mat_Coin`).
- Drag it into **`AuthoredContent/Coins`** in your variant and position it.
- Coins currently have **no** collection, scoring, animation or sound — they only
  appear and move with the chunk. The setup is prefab-based so a real model and
  collection logic can be added later without touching the chunks.

---

## 7. Reusing and creating materials

**Reuse first.** Existing URP materials live in `Assets/_Project/Materials/`:

| Material | Use for |
|----------|---------|
| `Mat_Road` | road geometry |
| `Mat_Wall` | left/right boundaries |
| `Mat_Player` | player |
| `Mat_ShotPickup` | shot pickup |
| `DeliveryGate_Green` | delivery gate markers |

Do not create a new material when one of these fits.

**When you do need a new material:**
- Put it under `Assets/_Project/Materials/Construction/` (e.g. `Mat_Coin` already lives
  there).
- Use the existing materials as a reference for **shader, color intensity, smoothness
  and overall style**.
- It **must** use a **URP-compatible shader** — use `Universal Render Pipeline/Lit`
  (or Simple Lit / Unlit), matching the rest of the project.

---

## 8. Adding a finished variant to `RoadChunkManager`

1. Open `Assets/_Project/Scenes/SampleScene.unity`.
2. Select the GameObject with the **`RoadChunkManager`** component.
3. In the **Road Chunk Prefabs** array, add your new variant.
4. All entries must be Prefab Variants of `RoadChunk_Basic` and use the 30-unit length —
   the manager logs a warning at Play if a chunk has no `RoadChunk`, the wrong length,
   or a missing container.

The manager keeps randomly selecting chunks from this array, so every variant you add
gets mixed into the endless road. Delivery gates and difficulty scaling are unchanged.

---

## 9. Testing a chunk

1. Enter **Play** mode in `SampleScene`.
2. **Alignment:** chunks should tile seamlessly with no gaps or overlaps (every chunk is
   30 units; the manager spaces them by 30).
3. **Spawning:** confirm new chunks appear ahead and old ones despawn behind.
4. **Scrolling:** chunks should move toward the player at the current run speed
   (`WorldScrollMover`).
5. **Pickups:** play a few times; markers should sometimes produce a Pizza, sometimes a
   Shot, often nothing — never both at one marker. Spawned pickups appear under each
   chunk's `RuntimeContent`.
6. **Obstacles:** your manually placed obstacles always appear; runtime obstacles appear
   only under `RuntimeContent` and never block your safe lane.
7. **Console:** watch for `[RoadChunkManager]` validation warnings about any
   misconfigured variant.
