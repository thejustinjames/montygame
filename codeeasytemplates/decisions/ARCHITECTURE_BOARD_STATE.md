# ADR-002: Board State & Tile System Architecture

**Status:** Accepted

**Date:** 2026-07-06

**Affected Components:** Board Manager, Tile System, Game State, Data Persistence

---

## Context

World 1 has 25 tiles. Each tile has:
- Position on board
- Type (Normal, Portal, Whirlpool, Elevator, Hyperspace, Boss, Goal)
- Effects (movement bonuses/penalties)
- Story context (which world beat, dialogue)

We need to decide:
- How to store tile data (ScriptableObjects vs. JSON vs. runtime)
- How to manage board state (single manager vs. distributed)
- How to track player position & visited tiles
- How to support multiplayer (multiple players on same board)

---

## Options Considered

**Option 1: Runtime-Only State (In-Memory)**
- **Pros:** Simple, fast, no I/O
- **Cons:** Can't persist progress, no inspector visibility, hard to debug
- **Effort:** Low
- **Scalability:** Poor (doesn't scale to multiple worlds or save/load)

**Option 2: JSON Data Files**
- **Pros:** Human-readable, portable, easy to edit
- **Cons:** File I/O overhead, custom parser, fragile
- **Effort:** Medium
- **Scalability:** Good for multiple worlds, but hand-editing is error-prone

**Option 3: Unity ScriptableObjects**
- **Pros:** Native to Unity, inspector-editable, automatic serialization, type-safe
- **Cons:** Binary format (can't hand-edit), Unity-only
- **Effort:** Low
- **Scalability:** Excellent for expansion; add worlds trivially

---

## Decision

**We chose: Option 3 (Unity ScriptableObjects)**

---

## Rationale

ScriptableObjects align best with MontyGame's needs:

1. **Inspector-Editable:** Game designers can preview tile layout without coding
2. **Type-Safe:** No parsing errors; data is guaranteed correct at compile time
3. **Native to Unity:** Leverages built-in serialization; no custom code
4. **Low Effort:** Quick to set up; straightforward pattern
5. **Scalability:** Adding World 2 and 3 means creating new ScriptableObject instances
6. **Team Workflow:** Non-programmers can modify tiles without touching code

We accept the binary format limitation because our workflow is inspector-based, not hand-edited.

---

## Architectural Details

### Data Structure

**TileConfig (Base for each tile):**
```csharp
[System.Serializable]
public class TileConfig {
    public int tileId;              // 1–25 for World 1
    public TileType type;           // Normal, Portal, Whirlpool, etc.
    public Vector2 position;        // World position
    public string name;             // "Jungle Entrance", "Hidden Waterfall"
    public string description;      // Flavor text for designers
    
    // Effect data (varies by tile type)
    public TileEffect[] effects;    // Portal warp amount, whirlpool pull amount, etc.
}

[System.Serializable]
public class TileEffect {
    public string effectType;       // "warp_forward", "pull_back", "none"
    public int amount;              // Amount to move
}
```

**TileDatabase (Container for all tiles):**
```csharp
[CreateAssetMenu(fileName = "TileDatabase", menuName = "MontyGame/Tile Database")]
public class TileDatabase : ScriptableObject {
    [SerializeField] public List<TileConfig> tiles = new List<TileConfig>();
    
    public TileConfig GetTile(int tileId) { /* lookup */ }
    public TileEffect GetEffect(int tileId) { /* apply effect */ }
}
```

### Board Manager (Runtime State)

**Tracks:**
- Current player position(s)
- Visited tiles (for analytics/progression)
- Special tile states (boss defeated?, portal used?)
- Game progress (current world, current player)

```csharp
public class BoardManager : MonoBehaviour {
    private TileDatabase tileDatabase;
    private Dictionary<int, PlayerState> playerStates; // Per-player position
    private HashSet<int> visitedTiles;
    private bool bossTileDefeated;
    
    public void AdvancePlayer(int playerId, int moveAmount) { }
    public void ApplyTileEffect(int tileId) { }
    public PlayerState GetPlayerState(int playerId) { }
}

[System.Serializable]
public class PlayerState {
    public int currentTile;
    public Character character;     // Dino or Cat
    public int powerUpsHeld;
}
```

### Tile Prefab System

**Runtime Tile (instantiated from tile config):**
```csharp
public class Tile : MonoBehaviour {
    public TileConfig config;
    
    public void OnPlayerLand(PlayerState player) {
        // Apply effect, trigger animation, play sound
        ApplyEffect(config.effects[0]);
    }
}
```

### File Organization

```
Assets/
├── Data/
│   └── Worlds/
│       ├── World1/
│       │   ├── TileDatabase.asset          (ScriptableObject)
│       │   └── Tiles/
│       │       ├── Tile_001_Start.asset
│       │       ├── Tile_005_Portal.asset
│       │       ├── Tile_008_Whirlpool.asset
│       │       └── ... (25 total)
│       ├── World2/                         (Future)
│       └── World3/                         (Future)
├── Scripts/
│   ├── Board/
│   │   ├── BoardManager.cs
│   │   ├── Tile.cs
│   │   └── TileEffect.cs
│   └── Data/
│       ├── TileConfig.cs
│       └── TileDatabase.cs
├── Prefabs/
│   └── Tiles/
│       ├── NormalTile.prefab
│       ├── PortalTile.prefab
│       └── ...
```

---

## State Management for Multiplayer

**Per-Game Instance:**
- One BoardManager per game session
- Multiple PlayerStates in dictionary (keyed by player ID)
- Shared board (all players on same 25-tile path)
- Turn order tracked separately (see ADR-003)

**Example (2-player game):**
```
PlayerState {
    playerId: 1,
    currentTile: 12,
    character: Dino
}

PlayerState {
    playerId: 2,
    currentTile: 8,
    character: Cat
}
```

---

## Implications

### Code Structure
- **Scripts:** TileConfig, TileDatabase, BoardManager, Tile, TileEffect (5 core files)
- **Assets:** One TileDatabase ScriptableObject per world; one Tile ScriptableObject per tile
- **Prefabs:** Generic tile prefabs (customized via TileConfig)

### Testing
- **Unit Tests:** BoardManager state management, tile effect application
- **Inspector Tests:** Verify TileDatabase loads correctly, tiles render in editor
- **Integration Tests:** Full board flow (start → finish)

**Example test:**
```csharp
[Test]
public void ApplyPortalEffect_AdvancesPlayerCorrectAmount() {
    var tile = boardManager.GetTile(5); // Portal tile
    var effect = tile.effects[0];       // Warp forward +4
    
    playerState.currentTile = 5;
    boardManager.ApplyTileEffect(5);
    
    Assert.AreEqual(9, playerState.currentTile);
}
```

### Performance
- **Load Time:** Instant (ScriptableObjects are pre-serialized)
- **Memory:** ~1–2 MB per world (25 tiles × ~80 KB each)
- **Runtime:** No per-frame overhead; tile effects applied only on landing

### Future Decisions
- **Save/Load (Post-MVP):** Serialize PlayerState to JSON; TileDatabase remains static
- **Dynamic Tiles (Post-MVP):** If we want obstacle variations, extend TileConfig with randomization
- **Level Editor (Post-MVP):** Inspector tool to visually arrange tiles

---

## Success Metrics

**We'll know this works when:**
- [ ] All 25 World 1 tiles load without errors
- [ ] Inspector shows all tile data clearly
- [ ] Board Manager correctly tracks player position
- [ ] Tile effects apply correctly (warps, pulls, etc.)
- [ ] Adding World 2 takes <1 hour (vs. 3+ hours for World 1)

---

## Related Decisions

- **ADR-001: Platformer Physics** — Tile traversal uses physics engine
- **ADR-003: Multiplayer Architecture** — Depends on BoardManager state tracking
- **Design Doc:** `/Docs/WORLD_1_LAYOUT.md` — Details each tile's configuration

---

## Revision History

| Version | Date | Author | Status |
| --- | --- | --- | --- |
| 1.0 | 2026-07-06 | Claude | Accepted |

---

**Decision Owner:** thejustinjames  
**Last Updated:** 2026-07-06
