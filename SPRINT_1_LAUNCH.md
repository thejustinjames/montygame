# 🚀 Sprint 1 Launch: Building the Unity Shell

**Date:** 2026-07-06  
**Duration:** Weeks 1–2 (7–10 days, depending on iteration)  
**Goal:** Playable 5-tile prototype with forgiving platformer physics + Core integration

---

## 🎯 What You're Building

A **minimal playable game** that proves:
1. ✅ MontyGame.Core integrates cleanly with Unity
2. ✅ Platformer physics feel good (forgiving, responsive, fun)
3. ✅ Player can move/jump and trigger board mechanics
4. ✅ Architecture is clean (no game rules in Unity scripts)

**By end of Sprint 1:** You'll have a foundation to scale to the full 25-tile game in Sprint 2.

---

## 📊 Current Status

```
✅ Core Game Logic
   - 25-tile board model
   - All tile effects (portal, whirlpool, elevator, hyperspace, boss)
   - Dice + card movement
   - Turn engine (solo + 2-4 multiplayer)
   - 78 tests passing
   - Full demo playthrough working
   
❌ Unity Shell (YOU ARE HERE)
   - No project yet
   - Need to build: Player controller, tile rendering, UI, integration
```

---

## 🔥 Immediate Next Steps (In Order)

### Step 1: Create Unity Project (30 min)

**Option A: Unity Hub (Recommended)**
```
1. Open Unity Hub
2. Create New Project
3. Choose: 2D
4. Unity Version: 2022.3 LTS or later
5. Name: "MontyGame" 
6. Location: /Users/justin/Documents/GitHub/MontyGame/MontyGame-Unity
7. Create
```

**Option B: Command Line**
```bash
cd /Users/justin/Documents/GitHub/MontyGame
# Let Unity CLI create the project (if installed)
unity -createProject MontyGame-Unity -projectPath ./MontyGame-Unity
```

### Step 2: Reference MontyGame.Core (15 min)

**Easiest approach for now:**

1. In Unity, go to `Assets` folder
2. The MontyGame.sln and src/ folder already exist in the repo
3. Add project reference in your C# IDE to `src/MontyGame.Core/MontyGame.Core.csproj`
4. Build to verify no errors

**Or manually:**
```bash
# Copy Core DLL to Unity project
cp src/MontyGame.Core/bin/Debug/net8.0/MontyGame.Core.dll \
   MontyGame-Unity/Assets/Plugins/MontyGame.Core.dll
```

### Step 3: Open Quick Start Guide (5 min)

Read: `/SPRINT_1_QUICK_START.md`

This has:
- Phase-by-phase breakdown (5 phases)
- Code snippets for each phase
- What to test at each step
- Common issues + fixes

### Step 4: Build Phase 1 (Day 1–2)

**Follow SPRINT_1_QUICK_START.md:**

**Phase 1: Player Controller**
- Create `Assets/Scripts/Player/PlayerController.cs`
- Add input handling (arrow keys, space)
- Jump physics: 1.5–2.0 height, 0.4 sec duration, coyote time 0.1 sec
- Create Player prefab

**Goal:** Can you move left/right and jump? Does it feel responsive?

### Step 5: Continue Phases 2–5 (Days 2–7)

**Phase 2:** Board & Tiles (5 tiles in a row)  
**Phase 3:** GameEngine integration (wire Core to Unity)  
**Phase 4:** Player movement integration (roll → move → display)  
**Phase 5:** Polish & validation (playtest, tune, commit)

Each phase has code snippets in SPRINT_1_QUICK_START.md.

---

## 🎮 Daily Checkpoint Checklist

**Day 1-2 (Phase 1: Player Controller)**
- [ ] Unity project created
- [ ] MontyGame.Core referenced (no build errors)
- [ ] Player prefab created
- [ ] Can move left/right with arrow keys
- [ ] Can jump with space
- [ ] Commit: "Phase 1: Player controller with input"

**Day 2-3 (Phase 2: Board & Tiles)**
- [ ] 5 tiles render in scene
- [ ] Tiles are spaced correctly
- [ ] Player can walk/jump across tiles
- [ ] Commit: "Phase 2: Board and tile rendering"

**Day 3-4 (Phase 3: GameEngine Integration)**
- [ ] GameController script wires to GameEngine
- [ ] Button click calls engine.RollDice()
- [ ] Debug.Log shows roll value
- [ ] Commit: "Phase 3: Core integration (GameEngine wired)"

**Day 4-5 (Phase 4: Player Movement)**
- [ ] Roll → player animates movement
- [ ] Position display shows current tile
- [ ] Player moves correct distance based on roll
- [ ] Commit: "Phase 4: Player movement from Core"

**Day 5-7 (Phase 5: Polish & Validation)**
- [ ] Playtest 10+ times
- [ ] Physics feel validated (forgiving, fun)
- [ ] No crashes in 5+ min play session
- [ ] 60+ FPS maintained
- [ ] Final commit: "Sprint 1 complete: 5-tile prototype"

---

## 🎯 Critical Physics Decisions

**Lock these in before you start Phase 1:**

| Parameter | Recommended | Range | Why |
| --- | --- | --- | --- |
| Jump Force | 10 | 8–12 | Controls height; higher = floatier |
| Gravity | 9.81 | 9.81 | Standard physics |
| Jump Duration | 0.4 sec | 0.3–0.5 sec | Time from ground to apex |
| Coyote Time | 0.1 sec | 0.05–0.15 sec | Grace window after leaving ground |
| Movement Speed | 6 units/sec | 5–8 | Should feel responsive, not too fast |

**How to calculate jump height from force + gravity:**
```
Height = (force^2) / (2 * gravity)
Height = (10^2) / (2 * 9.81) ≈ 5.1 units

Adjust force to get desired height:
- Want 1.5 units? force ≈ 5.4
- Want 2.0 units? force ≈ 6.3
```

**Test early:** Try different values, feel which is most forgiving for young hands.

---

## 📁 File Structure (After Sprint 1)

```
MontyGame/
├── MontyGame.sln                          (existing)
├── src/MontyGame.Core/                    (existing, don't touch)
├── tests/MontyGame.Core.Tests/            (existing, don't touch)
├── src/MontyGame.Cli/                     (existing, don't touch)
│
├── MontyGame-Unity/                       (NEW - your Unity project)
│   ├── Assets/
│   │   ├── Scenes/
│   │   │   └── TestLevel_5Tiles.unity
│   │   ├── Scripts/
│   │   │   ├── Player/
│   │   │   │   └── PlayerController.cs
│   │   │   ├── Board/
│   │   │   │   ├── Tile.cs
│   │   │   │   └── BoardManager.cs
│   │   │   ├── GameController.cs
│   │   │   ├── PlayerMover.cs
│   │   │   └── UnityRandom.cs
│   │   ├── Prefabs/
│   │   │   ├── Player.prefab
│   │   │   └── Tile.prefab
│   │   ├── Sprites/
│   │   │   └── (placeholder boxes OK for now)
│   │   └── Plugins/
│   │       └── MontyGame.Core.dll         (if not project ref)
│   └── ProjectSettings/
│
├── Planning/
│   ├── SPRINT_1_SPEC.md                   (full detailed spec)
│   └── SPRINT_1_QUICK_START.md            (code snippets + phases)
└── SPRINT_1_LAUNCH.md                     (this file)
```

---

## ✅ Success Criteria

**Sprint 1 is complete when:**

✅ Player moves left/right with arrow keys  
✅ Player jumps with space (forgiving, responsive)  
✅ 5 tiles render and are navigable  
✅ Clicking "Roll Dice" rolls (shows in Debug.Log)  
✅ Player moves based on roll amount  
✅ No crashes after 5+ minutes of play  
✅ 60+ FPS maintained  
✅ Core tests still passing (78/78)  
✅ Code is clean (no game rules in Unity)  
✅ Ready to scale to 25 tiles in Sprint 2  

---

## 🛠️ Tools You'll Need

**Required:**
- Unity 2022.3 LTS or later (free)
- Visual Studio or Rider (for C# editing)
- Git (for commits)

**Already installed (checked earlier):**
- .NET 8 SDK ✅
- MontyGame.Core ✅
- MontyGame.Cli ✅

---

## 🚦 Troubleshooting

**Q: "MontyGame.Core not found" error**
- A: Make sure project reference or DLL is in right location
- Check: `Assets/Plugins/MontyGame.Core.dll` exists OR project reference is set

**Q: Player jumps too high / too low**
- A: Adjust `jumpForce` in PlayerController.cs (recommended: 10 for 1.5–2.0 units)

**Q: Player feels sluggish**
- A: Increase `moveSpeed` in PlayerController.cs (recommended: 6)

**Q: Player gets stuck on tiles**
- A: Check tile collider shape; make sure it's a simple box, not complex

**Q: Core tests failed after I changed something**
- A: Don't modify Core during Sprint 1; only work in MontyGame-Unity/

---

## 📞 Resources

**While building:**
- `/SPRINT_1_QUICK_START.md` — Code snippets + step-by-step
- `/SPRINT_1_SPEC.md` — Detailed requirements
- `/Docs/adr/ARCHITECTURE_PLATFORMER_PHYSICS.md` — Physics reference
- `src/MontyGame.Core/GameEngine.cs` — Core API reference

**If stuck:**
- Commit what you have (`git add . && git commit -m "WIP: [phase name]"`)
- Check console for errors
- Verify Core tests still pass (`dotnet test`)
- Review the code snippets in SPRINT_1_QUICK_START.md

---

## 🎉 Timeline Overview

```
Today (2026-07-06):
  └─ Read this file
  └─ Create Unity project
  └─ Reference MontyGame.Core

Days 1-2 (Phase 1):
  └─ Player controller + input

Days 2-3 (Phase 2):
  └─ Board + tiles

Days 3-4 (Phase 3):
  └─ Core integration

Days 4-5 (Phase 4):
  └─ Player movement

Days 5-7 (Phase 5):
  └─ Polish + validation

End of Week 1:
  └─ ✅ Sprint 1 complete
  └─ 5-tile playable prototype
  └─ Ready for Sprint 2 (25-tile board)
```

---

## 🚀 Ready?

**Next action:** 
1. Create Unity project (30 min)
2. Read `/SPRINT_1_QUICK_START.md` (5 min)
3. Start Phase 1 (Player Controller)
4. Commit daily

**You've got a rock-solid core. Now make it beautiful.** 🎮

---

**Questions?** Check the docs, or you can iterate as you build.

**Commits?** Push to main as you hit checkpoints. Keep momentum!

---

**Start now. First commit in 30 min.** 🚀
