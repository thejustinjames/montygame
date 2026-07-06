# MontyGame Development — Complete Planning & Templates Index

**Date:** 2026-07-06  
**Status:** Planning phase complete ✓ → Ready for Sprint 1 development  
**Tech Stack:** Unity Personal Edition  
**Target:** MVP (World 1) in 8 weeks

---

## 🎯 Quick Start

**You want to...**

| Goal | Go To |
| --- | --- |
| Understand the game concept | `/Docs/GAME_DESIGN_IDEATION.md` |
| See the World 1 layout (25 tiles) | `/Docs/WORLD_1_LAYOUT.md` |
| View the sprint timeline | `/Planning/SPRINT_ROADMAP.md` |
| See all tasks to do | `/Planning/TODO.md` |
| Use CodeEasy to automate development | `/codeeasytemplates/README.md` |
| Understand architectural decisions | `/codeeasytemplates/decisions/` |
| Create a sprint spec | `/codeeasytemplates/specs/SPRINT_TEMPLATE.md` |
| Run the sprint execution workflow | `/codeeasytemplates/workflows/WORKFLOW_SPRINT_EXECUTION.md` |
| View mood boards & art direction | `/Docs/RESEARCH_MOOD_BOARD.md` |
| Project conventions & guidelines | `/CLAUDE.md` |

---

## 📚 Complete Folder Structure

```
MontyGame/
├── README.md                           (Public overview)
├── LICENSE                             (All Rights Reserved)
├── PRIVACY.md                          (Privacy policy)
├── CLAUDE.md                           (Project guide & conventions)
├── PLANNING_INDEX.md                   (This file)
│
├── Docs/
│   ├── GAME_DESIGN_IDEATION.md        (Complete game design: mechanics, characters, worlds)
│   ├── WORLD_1_LAYOUT.md              (Detailed 25-tile board layout)
│   ├── RESEARCH_MOOD_BOARD.md         (All 24+ reference images, color palettes, aesthetic)
│   └── [Post-Sprint files]
│       ├── PHYSICS_TUNING.md          (Final platformer physics parameters)
│       ├── UI_SPECIFICATIONS.md       (UI mockups, flow)
│
├── Planning/
│   ├── SPRINT_ROADMAP.md              (4-sprint timeline, dependencies, critical path)
│   ├── TODO.md                        (Master task list organized by sprint)
│   └── [Post-Sprint tracking]
│       ├── Sprint1_Tasks.md
│       ├── Sprint2_Tasks.md
│       └── etc.
│
├── Research/
│   ├── *.jpeg, *.webp, *.png          (24+ mood board images)
│   └── MontyDrawings/
│       ├── IMG_3635.jpeg              (Hand-drawn Dinosaur)
│       └── IMG_3638.jpeg              (Hand-drawn Cat)
│
├── codeeasytemplates/                 (CodeEasy templates & workflows)
│   ├── README.md                      (Quick start for CodeEasy)
│   ├── decisions/
│   │   ├── TEMPLATE_ADR.md            (Architecture Decision Record template)
│   │   ├── ARCHITECTURE_PLATFORMER_PHYSICS.md      (ADR-001: Jump physics, collision)
│   │   ├── ARCHITECTURE_BOARD_STATE.md             (ADR-002: Tile system, board manager)
│   │   └── ARCHITECTURE_MULTIPLAYER.md             (ADR-003: Turn-based local multiplayer)
│   ├── specs/
│   │   ├── SPRINT_TEMPLATE.md         (Template for any sprint spec)
│   │   ├── FEATURE_TEMPLATE.md        (Template for features)
│   │   ├── BUG_TEMPLATE.md            (Template for bugs)
│   │   └── [Examples]
│   │       ├── sprint1_prototype.md   (Filled example: Sprint 1)
│   │       └── sprint2_board.md       (Filled example: Sprint 2)
│   └── workflows/
│       └── WORKFLOW_SPRINT_EXECUTION.md (End-to-end sprint execution workflow)
│
└── Assets/                            (Created in Sprint 1)
    ├── Scenes/
    ├── Scripts/
    ├── Sprites/
    ├── Audio/
    ├── Prefabs/
    └── Data/
```

---

## 🎮 Game Concept Overview

### High Concept
**Platformer-Board Hybrid** for 5–7 year-olds: Help Dino & Cat navigate three themed worlds by combining board game progression (dice rolls, special tiles) with platforming challenges (jump, dodge, collect power-ups).

### Core Loop
1. Roll dice (1–6 tiles) or draw a card
2. Move that many tiles across the board
3. Traverse a gentle platforming challenge
4. Land on a tile (apply its effect: warp, penalty, reward, etc.)
5. Next player's turn (multiplayer) or continue (solo)

### Three Worlds
- **Dino Jungle** (MVP) — 25 tiles, jungle aesthetic, ancient ruins
- **Space Station** (Post-MVP) — 25+ tiles, tech world, friendly robots
- **Rocket Canyon** (Post-MVP) — 25+ tiles, desert, retro rockets

### Characters
- **Dinosaur:** Fast jumper, stomp ability
- **Cat:** Better air control, pounce ability

### Learning Goals (All integrated)
- **Counting/Math:** Dice rolls, tile positions, movement calculations
- **Spatial Reasoning:** Platforming, gap jumping, directional thinking
- **Pattern Recognition:** Predicting board flow, identifying tile clusters
- **Turn-Taking Fairness:** Multiplayer teaches patience and social skills

---

## 🏗️ Architectural Decisions (LOCKED IN)

### ADR-001: Platformer Physics
**Decision:** Gentle, forgiving platforming (no instant death; bounce-back on obstacles)  
**Rationale:** Target age is 5–7 years; kids need success, not punishment  
**Key Settings:** 1.5–2.0 unit jump height, 0.4 sec duration, instant controls, no coyote time penalty  
**Document:** `/codeeasytemplates/decisions/ARCHITECTURE_PLATFORMER_PHYSICS.md`

### ADR-002: Board State & Tile System
**Decision:** Unity ScriptableObjects for tile data (inspector-editable, type-safe)  
**Rationale:** Native to Unity, easy to expand to World 2 & 3, designers can modify without code  
**Key Design:** TileDatabase (container) + TileConfig (per-tile) + BoardManager (runtime state)  
**Document:** `/codeeasytemplates/decisions/ARCHITECTURE_BOARD_STATE.md`

### ADR-003: Local Multiplayer (Turn-Based)
**Decision:** Turn-based, not simultaneous (classic board game style)  
**Rationale:** Clear for young kids, teaches turn-taking fairness, simple implementation  
**Key Design:** GameManager orchestrates turn order; UI clearly shows whose turn  
**Document:** `/codeeasytemplates/decisions/ARCHITECTURE_MULTIPLAYER.md`

**All architectural decisions inform Sprint specs and guide autonomous builds.**

---

## 📋 Sprint Breakdown

| Sprint | Duration | Focus | Deliverable |
| --- | --- | --- | --- |
| **1** | Weeks 1–2 | Foundation & Platformer Prototype | 5-tile playable prototype; core mechanics validated |
| **2** | Weeks 3–4 | Board & Tile System | Full 25-tile World 1; all tile types working |
| **3** | Weeks 5–6 | Characters & Animation | Animated Dino & Cat; sound effects; ability system |
| **4** | Weeks 7–8 | Game Modes & UI | Solo + multiplayer modes; menus; story scenes |
| **Polish** | Week 9+ | Testing & Release | Playtesting, bug fixes, optimization, Windows/Mac builds |

**Critical Path:** 1 → 2 → 3 → 4 (dependencies must complete before next sprint starts)

**Detailed Breakdown:** See `/Planning/SPRINT_ROADMAP.md`

**Task List:** See `/Planning/TODO.md` (organized by sprint, with checklists)

---

## 🤖 CodeEasy Integration & Templates

### When to Use CodeEasy AutoCode

**Use for:** Medium-to-large features where spec is clear & requirements are locked  
**For MontyGame:** Sprints 2–4 (Sprint 1 is exploration/prototyping)

### Template Files

**Architectural Decisions (ADR format):**
- `/codeeasytemplates/decisions/TEMPLATE_ADR.md` — Template format
- `/codeeasytemplates/decisions/ARCHITECTURE_PLATFORMER_PHYSICS.md` — Filled example (ADR-001)
- `/codeeasytemplates/decisions/ARCHITECTURE_BOARD_STATE.md` — Filled example (ADR-002)
- `/codeeasytemplates/decisions/ARCHITECTURE_MULTIPLAYER.md` — Filled example (ADR-003)

**Sprint Specs:**
- `/codeeasytemplates/specs/SPRINT_TEMPLATE.md` — Template for any sprint
- `/codeeasytemplates/specs/FEATURE_TEMPLATE.md` — For individual features
- `/codeeasytemplates/specs/TEST_EXAMPLES/` — Filled examples

**Workflows:**
- `/codeeasytemplates/workflows/WORKFLOW_SPRINT_EXECUTION.md` — End-to-end sprint flow

**Quick Start:**
- `/codeeasytemplates/README.md` — How to use CodeEasy with MontyGame

### Sprint Workflow

1. **Prepare (Days 1–2):** Lock in architectural decisions, create sprint spec
2. **Validate (Days 2–3):** Create spec in CodeEasy, validate completeness
3. **Build (Days 3–8):** Run `codeeasy_start_spec` → Codex builds autonomously
4. **Test (Days 8–9):** Run tests, playtest, merge to main
5. **Retrospective (Day 10):** Capture learnings, update ADRs, prep next sprint

**Full Workflow:** See `/codeeasytemplates/workflows/WORKFLOW_SPRINT_EXECUTION.md`

---

## 🎨 Art & Aesthetic Direction

### Overall Vibe
🎨 **Bright, playful, retro-futuristic** (1950s–1980s sci-fi meets indie games)

**Colors:** Vibrant primaries + pastels (reds, blues, greens, purples, oranges)  
**Style:** Hand-drawn, chunky lines, friendly features  
**Tone:** Non-threatening, whimsical, encouraging for 5–7 year-olds

### By World

**Dino Jungle (MVP):**
- Primary Reference: `trex_getoblaster.jpeg` (playful dino with boombox)
- Colors: Greens, blues, oranges (#2D5016, #6BA547, #FF8C42)
- Aesthetic: Tropical, ancient ruins, no darkness

**Space Station (Future):**
- Primary Reference: `pngtree-robot-toys-friendly-robots-waving-retro-sci-fi-design-bright-colors-png-image_17663791.png` (bright toy robots)
- Colors: Blues, purples, neon whites (#1A3A52, #7B68EE, #FFFFFF)
- Aesthetic: Sleek, tech-forward, friendly robots

**Rocket Canyon (Future):**
- Primary References: `rocket.jpeg`, `ufo.jpg`, `space.jpeg`
- Colors: Oranges, reds, yellows (#FF6B35, #D32F2F, #FFC107)
- Aesthetic: Retro rockets, desert, high-speed action

### Character Designs

**Dinosaur:** From `/Research/MontyDrawings/IMG_3635.jpeg`
- Colorful stripes (blue, green), orange spikes, heart-shaped eye
- Playful, curious, friendly

**Cat:** From `/Research/MontyDrawings/IMG_3638.jpeg`
- Orange/brown coloring, blue wings, expressive eye
- Quick-witted, confident, playful

**Full Mood Board:** See `/Docs/RESEARCH_MOOD_BOARD.md` (documents all 24+ reference images)

---

## ✅ Planning Phase Completion Checklist

- [x] Game concept finalized (high concept, mechanics, learning goals)
- [x] Three worlds designed (themes, obstacles, story)
- [x] Characters designed (Dino & Cat with abilities)
- [x] World 1 board layout (25 tiles, all types, story beats)
- [x] Sprint roadmap (4 sprints + polish)
- [x] Task list (organized by sprint with checklists)
- [x] Architectural decisions documented (ADR-001, ADR-002, ADR-003)
- [x] CodeEasy templates created (specs, workflows, decisions)
- [x] Mood boards & art direction cataloged
- [x] Project conventions documented (`CLAUDE.md`)
- [x] All planning committed to git

---

## 🚀 Next Steps

### To Start Sprint 1:

1. **Set up Unity Project** (using specs from `/codeeasytemplates/`)
   - Create folder structure
   - Configure physics, layers, input
   - Create basic scenes

2. **Manual Build Sprint 1** (not using CodeEasy)
   - Prototype gentle platformer physics
   - Test 5-tile level
   - Validate core mechanics feel right
   - Reference: `/codeeasytemplates/decisions/ARCHITECTURE_PLATFORMER_PHYSICS.md`

3. **Lock in Physics Feel**
   - Playtesting with target age group (if possible)
   - Finalize jump height, gravity, collision behavior
   - Document tuned values in new `/Docs/PHYSICS_TUNING.md`

### To Start Sprint 2:

1. **Create Sprint 2 Spec** (using `/codeeasytemplates/specs/SPRINT_TEMPLATE.md`)
   - Reference ADR-002 (Board State)
   - Define 25-tile board requirements
   - Lock acceptance criteria

2. **Use CodeEasy AutoCode**
   - `codeeasy_create_spec` → Fill in Sprint 2 spec
   - `codeeasy_check_validation` → Ensure completeness
   - `codeeasy_start_spec` → Autonomous build
   - `codeeasy_get_spec_status` → Monitor progress

3. **Test & Integrate**
   - Verify all 25 tiles load and work
   - Test tile effects (warp, pull, elevator, hyperspace)
   - Playtest board navigation

---

## 📖 Key Documents (Quick Reference)

| Document | Purpose | Location |
| --- | --- | --- |
| Game Design | Complete game concept, mechanics, learning goals | `/Docs/GAME_DESIGN_IDEATION.md` |
| World 1 Layout | 25-tile board, tile types, story beats | `/Docs/WORLD_1_LAYOUT.md` |
| Mood Board | 24+ reference images, color palettes, aesthetics | `/Docs/RESEARCH_MOOD_BOARD.md` |
| Sprint Roadmap | 4-sprint timeline, dependencies, critical path | `/Planning/SPRINT_ROADMAP.md` |
| Task List | All tasks organized by sprint, with checklists | `/Planning/TODO.md` |
| Project Guide | Conventions, working principles, guidelines | `/CLAUDE.md` |
| CodeEasy Quick Start | How to use CodeEasy with MontyGame | `/codeeasytemplates/README.md` |
| Platformer Physics ADR | Architectural decision on jump physics | `/codeeasytemplates/decisions/ARCHITECTURE_PLATFORMER_PHYSICS.md` |
| Board State ADR | Architectural decision on tile system | `/codeeasytemplates/decisions/ARCHITECTURE_BOARD_STATE.md` |
| Multiplayer ADR | Architectural decision on turn-based gameplay | `/codeeasytemplates/decisions/ARCHITECTURE_MULTIPLAYER.md` |
| Sprint Execution Workflow | End-to-end sprint process (prep → build → test) | `/codeeasytemplates/workflows/WORKFLOW_SPRINT_EXECUTION.md` |

---

## 🎯 Success Criteria (MVP Complete)

✅ **Game is playable start-to-finish** (World 1, 25 tiles)  
✅ **Both solo and multiplayer modes work** (turn-based local, 2–4 players)  
✅ **Platforming feels gentle & forgiving** (no instant death, bounce-back on obstacles)  
✅ **All four learning goals present** (counting, spatial reasoning, pattern recognition, turn-taking)  
✅ **Story provides narrative context** (intro → world transitions → ending)  
✅ **Visually appealing & polished** (bright, playful, age-appropriate)  
✅ **Runs on Windows & Mac** (standalone executables)  
✅ **No major bugs or softlocks** (fully playable, no game-breaking issues)

---

## 📞 Project Owner

**GitHub:** `thejustinjames`  
**Email:** `gottolovemondays@gmail.com`

---

## 🔗 Memory & Persistence

**Project Memory:** `/Users/justin/.claude/projects/-Users-justin-Documents-GitHub-MontyGame/memory/`
- `montygame-overview.md` — Project scope, repo, licensing
- `montygame-tech-decisions.md` — Tech stack, MVP approach, key decisions
- `MEMORY.md` — Index of memory files

**These memories persist across sessions**, so future Claude assistants will have complete context.

---

## 📅 Timeline at a Glance

```
Now (Week 0):          Planning phase complete ✓
Weeks 1–2 (Sprint 1):  Foundation & platformer prototype
Weeks 3–4 (Sprint 2):  Board & tile system (all 25 tiles)
Weeks 5–6 (Sprint 3):  Characters & animation (Dino & Cat, sound)
Weeks 7–8 (Sprint 4):  Game modes & UI (solo, multiplayer, menus)
Week 9+:               Polish, playtesting, final build
Target:                MVP ready to ship (end of week 8–9)
```

---

**Planning Framework Version:** 1.0  
**Last Updated:** 2026-07-06  
**Status:** Ready for Sprint 1 development 🚀
