# MontyGame Development — Master TODO List

**Status:** Planning ✓ → **World 1 core logic BUILT & TESTED** ✓ (Code Easy AutoCode spec #61, 2026-07-06) → Unity shell next  
**Target:** MVP Complete by end of Sprint 4 (8 weeks)

---

## CORE GAME LOGIC — built by AutoCode (spec #61) ✓

All game *rules* now live in `src/MontyGame.Core` (pure C#, no UnityEngine — see
`Docs/adr/`). Verify any time: `dotnet test` (78 green) and
`dotnet run --project src/MontyGame.Cli -- --auto` (full playthrough to victory).

- [x] Solution scaffold (`MontyGame.sln`: Core + Tests + Cli)
- [x] 25-tile World 1 board model (all tile types: Normal, TimePortal, Whirlpool, Elevator, HyperspaceJump, Mystery, Boss)
- [x] Dice (1–6) + movement cards (Jump 3 / Dash 4 / Slow Step 2) with injected seedable `IRandom`
- [x] Tile effects engine (portal +3..+5, whirlpool −2..−4, elevator +2, hyperspace warp, mystery, boss gate; clamped 1–25, no eliminations)
- [x] Turn engine: solo + 2–4 pass-and-play, win at tile 25
- [x] CLI playthrough simulator (`--auto`, story beats printed, exit 0 on victory)
- [x] xunit suite: dice bounds, clamping, every tile effect, boss gating, win condition, turn rotation, seeded determinism

**Core still to build** (were `should`/`could` in the workshop bundle — promote to
`must` in `codeeasytemplates/workshops/montygame-core-world1.workshop.json` and
re-ingest, or spec directly):
- [ ] Dino stomp & Cat pounce abilities (`Characters.cs`)
- [ ] Story-beat events raised by the engine (`StoryBeats.cs`)
- [ ] Game-state save/load (`GameState.cs`, System.Text.Json)

---

## IDEATION & PLANNING (Pre-Development)

### Design & Documentation
- [x] High-concept game design ideation
- [x] Character design (Dino & Cat from child-drawn sketches)
- [x] Three-world story arc (escape → journey → return home)
- [x] Board tile mechanics (portals, whirlpools, elevators, hyperspace)
- [x] Educational mapping (four learning goals)
- [x] Tech stack decision (Unity Personal)
- [x] MVP priority (balanced: playable + pretty + educational)
- [ ] **LOCKED IN:** Finalize any remaining open questions from GAME_DESIGN_IDEATION.md
  - [x] Win condition — first to reach tile 25 (implemented + tested in `GameEngine`)
  - [x] Tie-breaking — not possible: turns are sequential; first to land on 25 ends the game
  - [ ] Voice acting (silent or dialogue?)
  - [ ] Character cosmetics (unlockables post-MVP?)

### Repository & Documentation
- [x] Initialize GitHub repository (public, All Rights Reserved)
- [x] Create LICENSE (proprietary)
- [x] Create PRIVACY.md (no data collection at ideation stage)
- [x] Create project CLAUDE.md (working conventions)
- [x] Create game design document
- [x] Create sprint roadmap
- [x] Create World 1 board layout
- [x] Create development TODO (this file)
- [ ] Commit all planning docs to git

---

## SPRINT 1: Foundation & Prototype (Weeks 1–2)

> **Reframed after the core build:** the *logic* items below (dice roll, card
> draw, tile advancement, the roll → move → land → effect → next-turn loop) are
> **done in `MontyGame.Core`** — Sprint 1 is Unity *shell* work: scenes, sprites,
> input and platforming feel, driven by the real `GameEngine` (thin
> MonoBehaviours only; no rules in Unity scripts — ADR 0001).

### Unity Project Setup
- [ ] Create Unity project (2D, target resolution 1920x1080)
- [ ] Set up folder structure:
  - [ ] `Assets/Scenes/` (scene files)
  - [ ] `Assets/Scripts/` (organized by system: Player, Board, UI, etc.)
  - [ ] `Assets/Sprites/` (characters, tiles, UI)
  - [ ] `Assets/Audio/` (music, SFX)
  - [ ] `Assets/Prefabs/` (player, tiles, UI elements)
  - [ ] `Assets/Resources/` (configs, data)
- [ ] Configure project settings (layer setup, physics, input mapping)
- [ ] Set up version control (git ignore for Unity)
- [ ] Create basic scene hierarchy template

### Core Mechanics Development
- [ ] **Player Controller:**
  - [ ] Create player prefab (placeholder sprite)
  - [ ] Implement gentle platforming physics (jump, fall, collision)
  - [ ] Test bounce-back on obstacles (no instant death)
  - [ ] Add walk/run state machine
- [ ] **Movement System:**
  - [ ] Dice roll mechanic (1–6 tiles)
  - [ ] Card draw mechanic (placeholder)
  - [ ] Tile advancement system (player moves tile-to-tile)
- [ ] **Test Level:**
  - [ ] Create 5-tile test scene (bare-bones visuals)
  - [ ] Lay out tiles in a simple horizontal line
  - [ ] Add basic obstacles (1–2 types only)
  - [ ] Test movement end-to-end

### Testing & Validation
- [ ] Verify core game loop works (roll → move → land → tile effect → next turn)
- [ ] Test platformer feel is forgiving (easy to jump, recoverable from mistakes)
- [ ] Verify no game-breaking bugs
- [ ] Document any physics/feel adjustments needed

**Sprint 1 Deliverable:** 5-tile playable prototype with working movement & platforming mechanics

---

## SPRINT 2: Board & Tile System (Weeks 3–4)

> **Reframed:** the board model, every tile effect, and board state already exist
> and are tested in `MontyGame.Core` (`Board`, `Tile`, `TileEffects`). Sprint 2 is
> *rendering* that board in Unity — prefabs, visuals, and feedback for effects the
> core computes. "Tile effect logic" below = wire visuals to core events, don't
> re-implement rules.

### Board Implementation
- [ ] Create World 1 board layout in Unity (25 tiles, winding path — rendered from the core's `World1` factory)
- [ ] Implement tile prefab system:
  - [ ] Normal tile (no effect)
  - [ ] Portal tile (warp forward)
  - [ ] Whirlpool tile (spin backward)
  - [ ] Elevator tile (rise up)
  - [ ] Hyperspace jump tile (random teleport)
  - [ ] Boss tile (mini-boss trigger)
  - [ ] Goal tile (victory condition)
- [ ] Create board manager (track player position, tile visited, board state)
- [ ] Implement tile effect logic (movement bonuses, penalties, triggers)

### Visual & Audio Foundation
- [ ] Create placeholder art for each tile type (visual distinction)
- [ ] Add visual feedback (tile highlight on landing, effect animations)
- [ ] Create basic particle effects (portal shimmer, whirlpool spin, etc.)
- [ ] Add placeholder audio cues (tile landing, effect triggered)

### UI Development (Framework)
- [ ] Create HUD canvas (position display, turn counter)
- [ ] Implement position tracker ("Tile 7 of 25")
- [ ] Add turn counter display
- [ ] Create simple state manager (menu, playing, game over)

### Testing
- [ ] Test all tile types trigger correctly
- [ ] Verify board navigation (forward, backward, random warps)
- [ ] Test edge cases (landing on boss tile, reaching goal)
- [ ] Verify no softlock states

**Sprint 2 Deliverable:** Full World 1 board with all tile mechanics functional

---

## SPRINT 3: Characters & Animation (Weeks 5–6)

### Character Art & Sprites
- [ ] Convert hand-drawn Dino to sprite sheet:
  - [ ] Idle pose (multiple frames for breathing/wiggling)
  - [ ] Walk/run cycle
  - [ ] Jump (ascent + apex + descent)
  - [ ] Landing pose
  - [ ] Stomp ability (wind-up + impact)
  - [ ] Celebrate pose
  - [ ] Hurt/bounce-back pose
- [ ] Convert hand-drawn Cat to sprite sheet:
  - [ ] Idle pose
  - [ ] Walk/run cycle
  - [ ] Jump cycle
  - [ ] Landing pose
  - [ ] Pounce ability (dash direction)
  - [ ] Celebrate pose
  - [ ] Hurt/bounce-back pose
- [ ] Create UI character portraits (for character select, HUD)

### Character Controller
> Ability *rules* (stomp negates an obstacle penalty; pounce +1 movement with
> cooldown) belong in `MontyGame.Core/Characters.cs` — still to build, see the
> CORE GAME LOGIC section at the top. The items below are the Unity *feel*:
> input, animation, screen-shake, sound.
- [ ] Implement character select (Dino vs. Cat)
- [ ] Create character-specific stat system (jump height, speed, abilities)
- [ ] Implement Dino stomp ability (ground shake, obstacle clear)
- [ ] Implement Cat pounce ability (quick dash)
- [ ] Test ability feedback (feel satisfying for kids)

### Animation System
- [ ] Set up animator controllers (state machine per character)
- [ ] Implement jump → land transition
- [ ] Implement ability animations
- [ ] Implement celebrate/win animation
- [ ] Test animation timing (not too fast, not too slow)

### Audio Implementation
- [ ] Create placeholder music loop (1–2 musical themes)
- [ ] Implement SFX:
  - [ ] Jump sound
  - [ ] Landing sound
  - [ ] Obstacle collision sound (gentle, not harsh)
  - [ ] Power-up pickup sound
  - [ ] Portal activation sound
  - [ ] Boss encounter music
  - [ ] Victory fanfare
- [ ] Create volume controls in settings

**Sprint 3 Deliverable:** Animated, selectable characters with special abilities; audio foundation

---

## SPRINT 4: Game Modes & UI (Weeks 7–8)

> **Reframed:** solo mode, 2–4 player turn order, and the victory condition are
> implemented + tested in `MontyGame.Core.GameEngine`. Sprint 4 builds the Unity
> UI/flow around them (menus, HUD, turn indicator, victory screens).

### Game Mode Implementation
- [ ] **Solo Mode:**
  - [ ] Single-player campaign (Dino or Cat, player's choice)
  - [ ] Progress tracking (tiles completed, current position)
  - [ ] Goal reached → victory screen
  - [ ] Restart/replay option
- [ ] **Multiplayer Mode (Local, Turn-Based):**
  - [ ] 2–4 player support
  - [ ] Player order tracking (whose turn?)
  - [ ] Controller pass system (turn indicator, button prompt)
  - [ ] Per-player scoreboard (positions: 1st, 2nd, 3rd)
  - [ ] Victory when first player reaches goal
  - [ ] Victory screen (standings, replay option)

### Main UI & Flow
- [ ] **Main Menu:**
  - [ ] Title screen with MontyGame logo
  - [ ] Mode selection (Solo vs. Multiplayer)
  - [ ] Character select (Dino or Cat, or alternate per player)
  - [ ] Difficulty select (Easy for MVP, placeholder for future)
- [ ] **Gameplay HUD:**
  - [ ] Current player indicator
  - [ ] Dice roll / card draw button
  - [ ] Movement value display
  - [ ] Position tracker
  - [ ] Power-up inventory (if collected)
- [ ] **Game Over / Victory Screen:**
  - [ ] Winner announcement (solo: "You won!", multiplayer: "Player 1 wins!")
  - [ ] Stats summary (turns taken, power-ups used)
  - [ ] Replay button
  - [ ] Main menu button
- [ ] **Pause Menu:**
  - [ ] Resume option
  - [ ] Settings (volume, language if applicable)
  - [ ] Quit to menu

### Story Scenes
- [ ] **Intro Scene:**
  - [ ] Simple narrative setup (Dino & Cat lost in jungle, need Portal Key)
  - [ ] Character introduction
  - [ ] Controls/objective explanation
- [ ] **Ending Scene:**
  - [ ] Victory celebration
  - [ ] "You did it! Dino & Cat are going home!"
  - [ ] Festive animation / music

### Input & Controls
- [ ] Keyboard controls (arrow keys to move, space to jump, mouse for UI)
- [ ] Controller support (Xbox/generic gamepad)
- [ ] Mobile input (touch detection for UI buttons)

### Testing
- [ ] Play through full solo game start-to-finish
- [ ] Test multiplayer mode (2, 3, 4 players; verify turn flow)
- [ ] Verify all UI buttons respond
- [ ] Check pause/resume functionality
- [ ] Test story scenes trigger correctly

**Sprint 4 Deliverable:** Fully playable game (solo + multiplayer); complete game flow

---

## POLISH & RELEASE BUFFER (Week 9+)

### Quality Assurance
- [ ] **Playtesting:**
  - [ ] Internal testing (team plays through multiple times)
  - [ ] Target audience testing (if possible: 5–7 year-olds)
  - [ ] Collect feedback on difficulty, fun factor, educational clarity
- [ ] **Bug Fixes:**
  - [ ] Fix any softlocks or crashes
  - [ ] Fix animation timing issues
  - [ ] Fix physics quirks (stuck on obstacles, missed jumps, etc.)
- [ ] **Balancing:**
  - [ ] Obstacle density (too hard? too easy?)
  - [ ] Boss difficulty (achievable for target age?)
  - [ ] Power-up frequency (useful without trivializing challenge)
  - [ ] Tile flow (board feels rewarding to navigate)

### Visual Polish
- [ ] Particle effects (portal shimmer, power-up glow, celebration burst)
- [ ] Screen shake on impact (feedback without being jarring)
- [ ] Character emotes (smile on victory, surprised look on obstacle hit)
- [ ] Smooth transitions between scenes
- [ ] Visual UI polish (button hover states, smooth animations)

### Performance
- [ ] Profile frame rate (target 60 FPS)
- [ ] Optimize sprite rendering
- [ ] Optimize audio loading
- [ ] Test on target hardware (Windows/Mac laptops)

### Build & Release
- [ ] Build for Windows (exe)
- [ ] Build for Mac (app)
- [ ] Test both builds on clean machines
- [ ] Create simple user manual / instructions
- [ ] Prepare for distribution (GitHub release? Store?)

---

## POST-MVP PHASES (Not in Scope Yet)

- [ ] **Phase 2:** Worlds 2 & 3 (Space Station, Rocket Canyon)
- [ ] **Phase 3:** Online multiplayer support
- [ ] **Phase 4:** iPad / iOS port
- [ ] **Phase 5:** Character cosmetics, difficulty modes, progression tracking
- [ ] **Phase 6:** Accessibility features (colorblind mode, high-contrast, etc.)

---

## Notes & Context

**Key Files to Reference:**
- `/Docs/GAME_DESIGN_IDEATION.md` — Complete game design
- `/Docs/WORLD_1_LAYOUT.md` — Detailed board layout (25 tiles)
- `/Planning/SPRINT_ROADMAP.md` — Timeline & dependencies
- `/Research/MontyDrawings/` — Character reference art
- `/Research/` — Mood boards for aesthetic guidance
- `/CLAUDE.md` — Project-level conventions & decisions

**Guiding Principles:**
1. **Gentle, Forgiving Platforming** — no instant death, bouncing back on mistakes
2. **Four Learning Goals** — counting, spatial reasoning, pattern recognition, turn-taking
3. **Story-Driven** — narrative gives context and emotional investment
4. **Bright & Playful** — aesthetics match target age (5–7) and mood boards
5. **Quality Over Quantity** — one world done well, not three half-done

**Blockers to Watch:**
- Physics feel (if platforming doesn't feel good, everything fails)
- Board layout (if layout is boring, game lacks replayability)
- Art polish (if it looks amateurish, kids won't engage)
- Multiplayer turn flow (if it's confusing, family play breaks down)

---

**Last Updated:** 2026-07-06  
**Status:** Ready for Sprint 1 Development
