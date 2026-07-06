# MontyGame — Project Guide for Claude

**MontyGame** is a **game development project** for ages 5–7: a platformer-board hybrid blending Snakes & Ladders mechanics with Donkey Kong platforming, set in a colorful space-dinosaur adventure.

**Status:** Planning phase complete ✓ → Development phase (Sprint 1 starting)  
**Tech Stack:** Unity Personal Edition  
**Target:** MVP (World 1 complete) in 8 weeks

---

## Repository Structure

| Folder | Purpose |
| --- | --- |
| `Docs/` | Game design, world layouts, narrative beats. See `GAME_DESIGN_IDEATION.md` and `WORLD_1_LAYOUT.md`. |
| `Planning/` | Sprint roadmap, task lists, milestones. See `SPRINT_ROADMAP.md` and `TODO.md`. |
| `Research/` | Reference imagery, mood boards, character art. `/MontyDrawings/` has hand-drawn Dino & Cat. |
| `Assets/` | (Will be added in Sprint 1) Unity project folder: `Scenes/`, `Scripts/`, `Sprites/`, `Audio/`, `Prefabs/`. |

## Key Files & Documents

- `README.md` — public overview
- `LICENSE` — **All Rights Reserved** (proprietary; owner retains full rights, may commercialize)
- `PRIVACY.md` — privacy policy (no data collected at ideation stage)
- `CLAUDE.md` — this file; project conventions
- `Docs/GAME_DESIGN_IDEATION.md` — complete game design (high concept, mechanics, learning goals, characters, worlds)
- `Docs/WORLD_1_LAYOUT.md` — detailed board layout (25 tiles, tile types, story beats, educational mapping)
- `Planning/SPRINT_ROADMAP.md` — timeline & sprint breakdown (4 sprints + polish = ~8 weeks)
- `Planning/TODO.md` — master task list (prioritized by sprint)

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

### Sprint 1: Foundation & Prototype (Weeks 1–2)
Set up Unity project, test platformer feel on 5-tile prototype, validate core mechanics.
**→ Deliverable:** 5-tile playable prototype

### Sprint 2: Board & Tile System (Weeks 3–4)
Build full World 1 board (25 tiles), implement all tile types (portal, whirlpool, elevator, hyperspace, boss), board state management.
**→ Deliverable:** Complete board with all mechanics

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
- **Language:** C# (Unity standard)
- **Organization:** Separate scripts by system (Player, Board, UI, Manager, etc.)
- **Asset Management:** Keep sprites, audio, prefabs in organized subfolders under `Assets/`
- **Version Control:** Git-tracked; add `.gitignore` for Unity (bin/, obj/, Library/, etc.)
- **No code yet:** This is planning phase. Development starts Sprint 1 after planning docs are committed.

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
- [ ] **Win Condition:** First to finish? Highest score? Most power-ups?
- [ ] **Tie-Breaking:** If multiple players reach goal simultaneously, how is winner determined?
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

**Last Updated:** 2026-07-06  
**Status:** Planning phase complete. Ready for Sprint 1 development.
