# Sprint 1 Specification: Foundation & Platformer Prototype

**Sprint:** 1 (Weeks 1–2)  
**Status:** Ready to Start 🚀  
**Owner:** thejustinjames

---

## 🎯 Sprint Goal

**Build a minimal Unity playable prototype that:**
1. Integrates `MontyGame.Core` (the game logic we already have)
2. Implements gentle, forgiving platformer physics (key to success for 5–7 year-olds)
3. Validates that the platforming **feel is right** before scaling to full game
4. Tests the Core ↔ Unity integration architecture
5. Delivers a 5-tile test level where player can move, jump, and see tile effects work

**Success Criteria:**
- ✅ Player can move left/right, jump, and land on tiles
- ✅ Gentle platforming (no instant death; bounce-back on obstacles)
- ✅ 5 tiles render correctly with simple placeholders
- ✅ `GameEngine.RollDice()` and `GameEngine.MovePlayer()` called from Unity and work
- ✅ Platforming feels fun and forgiving (subjective, but testable)
- ✅ No crashes; clean architecture (thin MonoBehaviours, all rules in Core)

---

## 📋 Requirements

### Functional

1. **Unity Project Setup**
   - Create 2D project, target resolution 1920×1080
   - Folder structure: Assets/Scenes, Assets/Scripts, Assets/Sprites, Assets/Prefabs
   - Reference MontyGame.Core (project reference or NuGet)

2. **Player Controller**
   - Character prefab (placeholder sprite or simple box)
   - Input: Arrow keys (left/right), Space (jump)
   - Movement: Walk/run left/right at configurable speed
   - Jump physics: 1.5–2.0 unit height, ~0.4 sec duration, forgiving (coyote time 0.1 sec)
   - Collision: Gentle bounce-back on obstacles (no instant death)
   - Animation state: Idle, Walk, Jump, Land, Hurt (placeholder animations OK)

3. **Board / Tile System**
   - Create 5-tile test board (horizontal line for simplicity)
   - Tile prefab with:
     - Visual representation (placeholder: colored cubes or circles)
     - Tile ID label
     - Tile type indicator (normal, portal, etc.)
   - Board manager that loads from Core's `World1` factory (read-only for MVP)

4. **Core Integration**
   - Create GameController MonoBehaviour that:
     - Instantiates `GameEngine` (from MontyGame.Core)
     - Calls `engine.RollDice()` when player clicks roll button
     - Calls `engine.MovePlayer(playerId, moveAmount)` 
     - Listens to Core events (if available) or polls state
     - Updates UI to show current position
   - No game rules in Unity scripts (all in Core)
   - Hard constraint: Zero `UnityEngine` references in MontyGame.Core

5. **Simple UI**
   - Roll Dice button
   - Position display ("Tile X of 25")
   - Turn indicator ("Player 1: Dino")
   - Story text area (for beats)

6. **Test Level (5 Tiles)**
   - Tile 1: Start (Normal)
   - Tile 2: Normal (test basic movement)
   - Tile 3: Portal (test tile effect triggering)
   - Tile 4: Whirlpool (test negative effect)
   - Tile 5: Goal (test victory condition)

### Non-Functional

- **Performance:** 60 FPS minimum (profiled)
- **Architecture:** Thin MonoBehaviour shells calling Core; no game rules in Unity
- **Code Quality:** Clear, commented, follows Unity conventions
- **Testing:** Manual playtesting; feel is the metric

---

## 🏗️ Architecture: Core ↔ Unity Integration

### Reference Diagram

```
MontyGame.Core (Pure C#, no UnityEngine)
├── GameEngine
│   ├── Board (25 tiles, all effects)
│   ├── Players (position tracking)
│   └── Turn order (solo + multiplayer)
├── Events (OnPlayerMoved, OnTileEffect, OnVictory, etc.)
└── All rules validated by 78 tests ✅

         ↑ (Method calls + Event listeners)
         |
         ↓

Unity Shell (MonoBehaviours)
├── GameController (wires UI to GameEngine)
├── PlayerController (input, animation, physics)
├── BoardManager (renders Core's board state)
├── UIManager (displays state: position, turn, etc.)
└── EffectsManager (listens to Core events, plays animations)

         ↓ (User sees & hears)
         
Player
├── Sees: Sprites, tiles, animations
├── Hears: Sounds, music
└── Does: Click button, move character
```

### Key Integration Points

**How Unity calls Core:**
```csharp
// In Unity's GameController.cs
GameEngine engine = new GameEngine(world1Factory, randomSeeded);

// On button click:
int roll = engine.RollDice();
var result = engine.MovePlayer(currentPlayerId, roll);

// Check state:
int currentTile = engine.GetPlayerPosition(currentPlayerId);
bool isVictory = engine.GetCurrentGameState() == GameState.Victory;
```

**How Core notifies Unity (if events available):**
```csharp
// Core raises events:
engine.OnPlayerMoved += (playerId, newTile) => {
    // Update UI, animate player movement
};

engine.OnTileEffect += (tileId, effect) => {
    // Play portal shimmer, whirlpool spin, etc.
};

engine.OnVictory += (winnerId) => {
    // Show victory screen
};
```

**If no event system, Unity polls:**
```csharp
// In Update():
if (engine.GetCurrentGameState() == GameState.Victory) {
    ShowVictoryScreen();
}
```

---

## 📊 Breakdown: Tasks & Estimates

### Phase 1: Setup & Integration (Days 1–2)

**Tasks:**
- [ ] Create Unity 2D project (1920×1080, Asset Store imports if needed)
- [ ] Set up folder structure (Scenes, Scripts, Sprites, Prefabs, Resources)
- [ ] Add MontyGame.Core reference (project ref or build NuGet package)
- [ ] Create basic scene hierarchy (Canvas for UI, 2D Camera, Physics2D)
- [ ] Configure physics (gravity ~9.81, layers)
- [ ] Create GameController MonoBehaviour (skeleton)

**Deliverable:** Unity project runs, can instantiate GameEngine without errors

### Phase 2: Player Controller & Physics (Days 2–4)

**Tasks:**
- [ ] Create Player prefab with Rigidbody2D and BoxCollider2D
- [ ] Implement input handling (arrow keys, space)
- [ ] Implement gentle jump physics (1.5–2.0 height, 0.4 sec, coyote time 0.1 sec)
- [ ] Implement collision handling (bounce-back on obstacles, no instant death)
- [ ] Create animator controller (Idle → Walk → Jump → Land)
- [ ] Placeholder sprite (or colored box)
- [ ] Test feel: Can easily jump small gaps? Forgiving on timing?

**Deliverable:** Player controller feels forgiving; jump feels good

### Phase 3: Board & Tiles (Days 4–5)

**Tasks:**
- [ ] Create Tile prefab (visual, collider, tile ID)
- [ ] Create BoardManager (loads 5-tile test board)
- [ ] Instantiate 5 tiles in a horizontal line
- [ ] Add tile labels (tile 1, tile 2, etc.)
- [ ] Create placeholder sprites or use colored boxes
- [ ] Add collision to tiles (platforms)

**Deliverable:** 5-tile board renders; player can walk/jump across tiles

### Phase 4: Core Integration (Days 5–6)

**Tasks:**
- [ ] Wire GameController to GameEngine
- [ ] On button click: Call engine.RollDice()
- [ ] Move player based on result
- [ ] Display position ("Tile X of 25")
- [ ] Listen for tile effects from Core
- [ ] Update UI: current player, turn count
- [ ] Test: Full loop works (roll → move → land → effect → display)

**Deliverable:** Click button → core rolls dice → player moves → tile effect → UI updates

### Phase 5: Testing & Polish (Days 6–7)

**Tasks:**
- [ ] Playtest full 5-tile level (does it feel good?)
- [ ] Verify no crashes
- [ ] Check 60 FPS (profile with Profiler)
- [ ] Tune physics if needed (jump height, gravity, coyote time)
- [ ] Clean up code, add comments
- [ ] Create simple scene save

**Deliverable:** Polished, playable 5-tile prototype; feel validated

---

## ⚡ Critical Decisions to Make NOW

**Before starting, lock these in:**

1. **Physics parameters** (from `/Docs/adr/ARCHITECTURE_PLATFORMER_PHYSICS.md`):
   - Jump height: 1.5–2.0 units?
   - Jump duration: 0.4 seconds?
   - Coyote time: 0.1 seconds?
   - Gravity: 9.81 standard?

2. **Tile size & spacing:**
   - How wide is each tile? (e.g., 2 units?)
   - How far apart? (e.g., 1 unit gap?)
   - Affects jump difficulty, platforming feel

3. **Core integration method:**
   - Project reference or NuGet package?
   - Where does Core assembly live relative to Unity?

4. **Player sprite:**
   - Use placeholder box for now, or quick pixel sprite?
   - Dino, Cat, or generic?

---

## 🧪 Testing Strategy

### Manual Playtesting (Primary)

**Gold Standard:** Can a 5–7 year-old play this without frustration?

**Specific tests:**
- [ ] Can player jump small 1-unit gap? (Should be easy)
- [ ] Can player jump 2-unit gap? (Should be doable)
- [ ] If player misses jump, does bounce-back feel fair? (Not punishing)
- [ ] Does platforming feel responsive? (Not sluggish, not too twitchy)

### Automated Verification

**Unit tests (existing, still passing):**
- [ ] `dotnet test` stays green (78 tests)
- [ ] `dotnet run --project src/MontyGame.Cli -- --auto` still works

**New tests (Unity):**
- [ ] Player moves correct distance when engine returns movement amount
- [ ] Tile effects trigger when player lands
- [ ] Victory detected when player reaches tile 5 (goal)

---

## 📦 Deliverables

**End of Sprint 1:**

- ✅ Unity project with MontyGame.Core integrated
- ✅ 5-tile playable level
- ✅ Player controller with gentle platforming physics
- ✅ Core connected to UI (roll → move → display)
- ✅ No crashes, 60 FPS
- ✅ Platforming feel validated (subjective, but tested)

**Code ready for:**
- Sprint 2: Expand to full 25-tile board
- Sprint 2: Add visual effects for tile types

---

## 🎯 Success Looks Like

**Player perspective:**
1. Game starts, sees 5 tiles
2. Clicks "Roll Dice"
3. Character moves 1–6 tiles
4. Player jumps/navigates platforming section
5. Lands on tile; sees effect (if any)
6. Clicks "Roll" again; next player's turn
7. Game feels **fun, forgiving, engaging**

**Developer perspective:**
1. Unity calls `GameEngine.RollDice()` ← Core handles logic
2. Core returns movement amount
3. Unity animates player to new position ← Pure presentation
4. Core events tell Unity what happened ← Clean separation
5. No game rules leaked into Unity scripts ← Architecture maintained

---

## 🚀 Next Step: Start Building

**Option A: Code directly** (recommended for Sprint 1 — need to feel it out)
- Follow the phases above
- Iterate on physics feel
- Manual architecture decisions

**Option B: Use CodeEasy** (after Sprint 1 prototype is solid)
- Once we have a working 5-tile level and physics feel is locked in
- Later sprints (2–4) can use CodeEasy specs for scaling

**Recommendation:** Option A now (explore and tune), Option B later (scale to 25 tiles with confidence).

---

**Ready to start?** Move to section below for immediate next steps.

---

## 🔧 Immediate Next Steps

1. **Create Unity project:**
   ```bash
   mkdir Unity
   cd Unity
   # or use Unity Hub to create 2D project
   ```

2. **Add MontyGame.Core reference:**
   - Option 1: Project reference (simpler for now)
     - Copy/symlink MontyGame.sln into Unity folder
   - Option 2: NuGet package (later)
     - Publish Core to local NuGet or GitHub Packages

3. **Create first scene:**
   - New 2D scene: "TestLevel_5Tiles"
   - Add Camera, Canvas, Physics2D settings

4. **Create Player controller:**
   - Player prefab with Rigidbody2D
   - Input handling (arrow keys, space)
   - Jump physics (tuned for forgiving feel)

5. **Iterate & validate:**
   - Playtest platforming feel
   - Adjust physics parameters until it feels right
   - Once feel is locked, move to Sprint 2

---

**Estimated Duration:** 7–10 days (depending on iteration on physics feel)

**Start Date:** 2026-07-06 (or whenever you kick off)

**Status:** Ready! ✅
