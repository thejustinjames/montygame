# MontyGame — Project Guide for Claude

**MontyGame** is a **game development project** for ages 5–7: a platformer-board hybrid blending Snakes & Ladders mechanics with Donkey Kong platforming, set in a colorful space-dinosaur adventure.

**Status:** Planning ✓ → **World 1 game logic BUILT & TESTED** ✓ (Code Easy AutoCode, spec #61, 2026-07-06) → **Unity game PLAYABLE** ✓ (2026-07-13)  
**Tech Stack:** Unity Personal Edition (the playable game) + **MontyGame.Core** — an engine-agnostic .NET 8 class library (see `Docs/adr/0001-engine-agnostic-core-library.md`)  
**Target:** MVP (World 1 complete) in 8 weeks  
**Proof it works:** `dotnet test` → 78/78 green · `dotnet run --project src/MontyGame.Cli -- --auto` → full seeded playthrough to victory · Unity project opens and plays end-to-end

> ⚠️ **Read `Docs/UNITY_SHELL.md` before touching the Unity project.** Two things surprise
> everyone: the real project is at `MontyGame-Unity/MontyGame/` (nested one level deeper
> than expected), and **the Unity build does not consume `MontyGame.Core`** — Unity can't
> load a .NET 8 / C# 11 assembly, so the rules are re-implemented Unity-side and the board
> there is 10×10 (100 squares), not the 25-tile World 1. Core stays the tested design
> reference; the Unity scripts are what actually runs.

---

## Repository Structure

| Folder | Purpose |
| --- | --- |
| `MontyGame.sln` | Solution: core library + tests + CLI simulator (built by AutoCode spec #61) |
| `src/MontyGame.Core/` | **The game rules, built & tested** — Board, Tile, Dice, IRandom, MovementCards, TileEffects, GameEngine. Pure C#, **zero UnityEngine references** (hard constraint). |
| `src/MontyGame.Cli/` | Console playthrough simulator — `dotnet run --project src/MontyGame.Cli -- --auto` plays a full seeded World 1 game |
| `tests/MontyGame.Core.Tests/` | xunit suite (78 tests): dice bounds, movement clamping, every tile effect, boss gating, win condition, turn rotation, seeded determinism |
| `Docs/` | Game design + build docs. Design: `GAME_DESIGN_IDEATION.md`, `WORLD_1_LAYOUT.md`. Generated: `WORKSHOP-CONTEXT.md` (build context), `BUILD-SUMMARY-spec-61.md`, `PROJECT-OVERVIEW.md`, `adr/` (3 ADRs). |
| `Planning/` | Sprint roadmap, task lists, milestones. See `SPRINT_ROADMAP.md` and `TODO.md`. |
| `Research/` | Reference imagery, mood boards, character art. `/MontyDrawings/` has hand-drawn Dino & Cat. |
| `codeeasytemplates/` | Code Easy factory templates — see `TEMPLATES_INDEX.md` (every JSON template + how to load) and `processes/WORKSHOP_TO_BUILD_WALKTHROUGH.md` (the end-to-end demo script for this repo's real build) |
| `MontyGame-Unity/MontyGame/` | **The playable game** (Unity 6000.5.2f1). Scripts live directly in `Assets/`; art in `Assets/Resources/`. The whole scene is built in code on Play — no editor setup. See `Docs/UNITY_SHELL.md`. |

## Key Files & Documents

- `Docs/UNITY_SHELL.md` — **what the playable game actually is**: board, mechanics (coins, diamond, pterodactyls, spaceships, the Hulk), the 7 avatars, procedural audio, script map, and how to batch-compile it
- `README.md` — public overview
- `LICENSE` — **All Rights Reserved** (proprietary; owner retains full rights, may commercialize)
- `PRIVACY.md` — privacy policy (no data collected at ideation stage)
- `CLAUDE.md` — this file; project conventions
- `Docs/GAME_DESIGN_IDEATION.md` — complete game design (high concept, mechanics, learning goals, characters, worlds)
- `Docs/WORLD_1_LAYOUT.md` — detailed board layout (25 tiles, tile types, story beats, educational mapping)
- `Planning/SPRINT_ROADMAP.md` — timeline & sprint breakdown (4 sprints + polish = ~8 weeks)
- `Planning/TODO.md` — master task list (prioritized by sprint)
- `Docs/WORKSHOP-CONTEXT.md` — the build context Code Easy generated (tech stack, structure, hard constraints for the C# core)
- `Docs/BUILD-SUMMARY-spec-61.md` — what AutoCode built (9/9 tasks) and how
- `Docs/adr/` — decisions that bind the code: 0001 engine-agnostic core (no UnityEngine in Core), 0002 deterministic seeded RNG (all randomness via injected `IRandom`), 0003 .NET 8 + xunit
- `codeeasytemplates/TEMPLATES_INDEX.md` — every Code Easy JSON template for this repo + how to load each; `workshops/montygame-core-world1.workshop.json` is the bundle that produced spec #61 (edit + re-ingest for the next phase)

## Working on the built core (read before touching `src/`)

```bash
dotnet build                                       # must stay at 0 errors
dotnet test                                        # must stay green (78 tests)
dotnet run --project src/MontyGame.Cli -- --auto   # full playthrough must reach victory, exit 0
```

**Hard constraints (from the ADRs — do not violate):**
1. **Zero `UnityEngine` references in `MontyGame.Core`** — the Unity shell consumes it, never the reverse. Keep the public API engine-neutral (plain C# types, events/callbacks).
2. **All randomness through the injected `IRandom`** — never `new Random()` inline. Seeded games must stay reproducible (tests depend on it).
3. **Forgiving design** — position clamps to tiles 1–25, no eliminations, no instant game-over. The rules' source of truth is `Docs/GAME_DESIGN_IDEATION.md` + `Docs/WORLD_1_LAYOUT.md`; the tests assert them.

**Still to build in the core** (were `should`/`could` in the workshop bundle — promote to `must` and re-ingest, or spec directly): Dino stomp & Cat pounce abilities (`Characters.cs`), story-beat events (`StoryBeats.cs`), game-state save/load (`GameState.cs`). Then the Unity shell sprint wires scenes/sprites/input to `GameEngine`.

---

## Game Concept (LOCKED IN)

### High Concept
Help Dino & Cat escape a multi-world adventure and return home by navigating three themed worlds. Each world is a 25–40 tile board with platforming challenges. Mix of board game (portals, whirlpools, elevators, hyperspace jumps) + platformer (jump, dodge, collect power-ups) + story (narrative arc across worlds).

### Core Loop
1. Roll dice (1–6) or draw a card (mixed mechanics)
2. Move that many tiles across the board
3. Traverse a gentle platforming challenge on the path
4. Land on a tile (check its type: normal, portal, whirlpool, elevator, hyperspace, boss)
5. Apply tile effect (movement bonus, penalty, trigger)
6. Pass turn to next player (multiplayer) or continue (solo)

### Three Worlds
1. **Dino Jungle** (MVP) — lush green jungle, ancient ruins
2. **Space Station** — sleek blue/purple tech world with robots
3. **Rocket Canyon** — orange/red desert with flying rockets

**Story Arc:** Dino & Cat lost in jungle → escape through space station → return home via rocket canyon

### Characters
- **Dinosaur:** Fast jumper, stomp ability (ground shake, obstacle clear)
- **Cat:** Better air control, pounce ability (quick dash)

---

## Design Decisions (ALL LOCKED IN)

### Gameplay
✅ **Platforming Feel:** Gentle, forgiving, exploratory (no instant death; bouncing back on obstacles)  
✅ **Movement:** Mix of dice rolls (1–6) + card draws  
✅ **Solo + Multiplayer:** Both supported (solo campaign + local turn-based 2–4 players)  
✅ **Board Mechanics:** Thematic alternatives instead of snakes/ladders (Time Portals ↑, Whirlpools ↓, Elevators ⬆, Hyperspace Jumps →)

### Learning & Education
✅ **Four Learning Goals (all integrated):**
- Counting/Math (dice rolls, tile positions, movement calculation)
- Spatial Reasoning (platforming, gap jumping, directional thinking)
- Pattern Recognition (predicting board flow, tile clusters, portal locations)
- Turn-Taking Fairness (multiplayer teaches patience and social skills)

### Narrative
✅ **Story:** Full arc with intro + world-to-world beats + ending  
✅ **Story Beats:** Setup (Dino & Cat lost) → Mid-game (halfway there) → Boss warning → Victory  

### Aesthetics
✅ **Style:** Bright, playful, hand-drawn (inspired by child-drawn characters)  
✅ **Characters:** Custom-drawn Dino & Cat (colorful, friendly, expressive)  
✅ **Color Palette:** Vibrant primaries + pastels (greens, blues, oranges, purples)  
✅ **Audio:** Cheerful chiptune-style music, encouraging sound effects

### Tech & Scope
✅ **Engine:** Unity Personal Edition (free for development)  
✅ **Platforms:** Windows/Mac standalone for MVP; iOS later  
✅ **MVP Scope:** World 1 only (25 tiles, both solo & multiplayer, complete game flow)  
✅ **MVP Priority:** Balanced (playable + pretty + educational all matter equally)

---

## Development Phases

> **2026-07-06 update:** the *game-logic* halves of Sprints 1–2 and part of 4 are
> already done in `MontyGame.Core` (board, all tile types, dice/cards, turn engine
> for solo + 2–4 players, win condition — all tested). The sprints below are now
> primarily **Unity shell work**: scenes, sprites, input, animation and audio wired
> as thin MonoBehaviours over the core (ADR 0001).

### Sprint 1: Foundation & Prototype (Weeks 1–2)
Set up Unity project, reference `MontyGame.Core`, test platformer feel on a 5-tile prototype driven by the real `GameEngine`.
**→ Deliverable:** 5-tile playable prototype

### Sprint 2: Board & Tile System (Weeks 3–4)
Render the full World 1 board (25 tiles) from the core's `World1` factory; visualize the tile effects the core already computes (portal, whirlpool, elevator, hyperspace, boss).
**→ Deliverable:** Complete board with all mechanics on screen

### Sprint 3: Characters & Animation (Weeks 5–6)
Convert hand-drawn characters to sprite sheets, implement character controllers, special abilities, animation system, audio foundation.
**→ Deliverable:** Animated, playable characters with sound

### Sprint 4: Game Modes & UI (Weeks 7–8)
Implement solo mode, multiplayer mode (turn-based local), main menu, HUD, game-over flow, story scenes.
**→ Deliverable:** Fully playable game (solo + multiplayer)

### Polish & Release (Week 9+)
Playtesting, bug fixes, balancing, visual/audio polish, performance optimization, build for Windows & Mac.
**→ Deliverable:** Polished, ready-to-ship MVP

See `Planning/SPRINT_ROADMAP.md` for detailed sprint breakdown.

---

## Key References & Research

### Characters
- Hand-drawn Dino: `/Research/MontyDrawings/IMG_3635.jpeg` (colorful stripes, heart-shaped eye, playful)
- Hand-drawn Cat: `/Research/MontyDrawings/IMG_3638.jpeg` (with wings, expressive, adorable)

### Mood Boards (in `/Research/`)
- **Dinosaurs:** `trex.jpeg`, `trex2.jpeg`, `dinosours.jpeg`, `terradactile.jpeg`, `trex_getoblaster.jpeg` (colorful, retro style)
- **Space/Robots:** `r2d2.jpeg`, `c3po.jpeg`, `robot1.jpeg`, `alien.webp`, `codey.jpeg`, `pngtree-robot-toys-...png` (friendly, bright)
- **Rockets/Space:** `rocket.jpeg`, `rocket2.jpeg`, `spaceship.jpeg`, `ufo.jpg`, `space.jpeg`, `Star-Destroyer_ab6b94bb.jpeg`
- **Game Reference:** `snakesNladders.webp` (board game structure inspiration)
- **Other:** `backdrop.jpeg`, `reading-in-space-stockcake.jpg`, `donky.webp` (aesthetic direction)

---

## Working Conventions

### Git & Commits
- **Repo:** Public at https://github.com/thejustinjames/montygame (`origin`, `main` branch)
- **Commits:** Clear, descriptive messages. Example: "Sprint 2: Implement portal tile mechanic"
- **Push:** Only when explicitly asked or after documented code changes
- `.gitkeep` files in empty folders (Docs, Planning) — don't delete these

### Code & Development
- **Language:** C# — game rules live in `src/MontyGame.Core` (plain .NET 8), Unity scripts are thin shells that call it
- **Organization:** Separate scripts by system (Player, Board, UI, Manager, etc.); no rules/logic in MonoBehaviours
- **Asset Management:** Keep sprites, audio, prefabs in organized subfolders under `Assets/`
- **Version Control:** Git-tracked; `.gitignore` covers bin/, obj/ (extend for Unity's Library/ when the shell lands)
- **Quality bar for any core change:** `dotnet test` green + the CLI `--auto` playthrough reaches victory
- **AutoCode:** this repo builds through Code Easy (toolchain auto-detected as **dotnet**: `dotnet build`/`dotnet test` gate every phase). Templates + the exact process: `codeeasytemplates/TEMPLATES_INDEX.md` and `codeeasytemplates/processes/WORKSHOP_TO_BUILD_WALKTHROUGH.md`

### Documentation & Communication
- **Design-First:** Update design docs before coding features
- **Comments:** Only when WHY is non-obvious; prefer clear naming
- **Game Design Thinking:** Lean into game mechanics, player experience, learning outcomes — not just software architecture
- **Collaborative:** When in doubt about design direction, reference the locked-in decisions or ask before implementing

### Testing & QA
- **Target Age:** 5–7 year-olds. Keep platforming forgiving, UI simple, story encouraging
- **Playtesting:** Prioritize feel (is it fun?) over metrics
- **Difficulty:** Easy for World 1 (intro), escalate in Worlds 2–3
- **Educational:** Verify all four learning goals are present in gameplay (counting, spatial reasoning, pattern recognition, turn-taking)

---

## Guiding Principles

1. **Gentle, Forgiving Design** — No instant death. Obstacles are avoidable or recoverable. Kids should feel encouraged, not frustrated.
2. **Four Learning Goals** — Every mechanic should reinforce counting, spatial reasoning, pattern recognition, or turn-taking fairness.
3. **Story Matters** — Narrative context (Dino & Cat lost → journey → return home) gives emotional investment and breaks up gameplay.
4. **Quality Over Quantity** — One world done beautifully beats three half-baked worlds.
5. **Bright & Playful** — Match the mood boards: colorful, retro, friendly. Nothing dark or scary.
6. **Both Modes From Day One** — Solo and multiplayer both matter for MVP. Family play + solo play are equal priorities.

---

## Open Questions (To Lock In)

These can be decided as we build, but discussing now helps:
- [x] **Win Condition:** First to reach tile 25 (implemented + tested in `GameEngine`; revisit only if design changes)
- [x] **Tie-Breaking:** Not possible — turns are strictly sequential, the first player to land on tile 25 ends the game immediately
- [ ] **Difficulty Scaling:** Should Worlds 2–3 have harder obstacles, or stay consistent per world?
- [ ] **Character Voice:** Do Dino & Cat speak (voice lines), or silent gameplay with sound effects?
- [ ] **Cosmetics:** Should there be character customization (color changes, hats) or just story progression?
- [ ] **Monetization:** Free game? One-time purchase? (Defer post-MVP)

---

## Success Criteria (MVP Complete)

✅ Game is playable start-to-finish (World 1)  
✅ Both solo and multiplayer modes work flawlessly  
✅ Platforming feels gentle & forgiving (no frustration)  
✅ All four learning goals represented in active gameplay  
✅ Story provides narrative context (intro → world transitions → ending)  
✅ Visually appealing, age-appropriate, polished  
✅ Runs on Windows & Mac  
✅ No major bugs or softlocks  

---

## Project Owner

GitHub: `thejustinjames`  
Email: `gottolovemondays@gmail.com`

---

## Related Files

- **Global instructions:** `~/.claude/CLAUDE.md` (personal workflow preferences)
- **Memory:** `.claude/projects/-Users-justin-Documents-GitHub-MontyGame/memory/` (project context for future sessions)

---

**Last Updated:** 2026-07-13  
**Status:** Core logic built & tested (78 tests). Unity game playable end-to-end: 10×10 board, 2-player pass-and-play, 7 avatars, coins/diamond, flying hazards, the roaming Hulk, procedural chiptune audio. Next: original art to replace the placeholder avatars, then port the World-1 flavor (25-tile board, story beats) into the Unity build.
