# MontyGame Development — Sprint Roadmap

**Target:** MVP launch with World 1 (Dino Jungle) complete  
**Approach:** Balanced (playable + pretty + educational)  
**Tech Stack:** Unity Personal Edition  
**Scope:** Solo mode + turn-based local multiplayer (2–4 players)

---

## Sprint Structure

Each sprint is ~1–2 weeks. Total MVP: ~6–8 weeks.

### Sprint 1: Foundation & Prototype (Weeks 1–2)

**Goal:** Prove core game loop works; test platformer feel.

**Tasks:**
- [ ] Set up Unity project structure (scenes, prefabs, scripts folders)
- [ ] Create basic player character controller (gentle platforming physics)
- [ ] Build a 5-tile test level (bare-bones visuals, focus on feel)
- [ ] Implement dice roll / card draw mechanic (UI placeholder)
- [ ] Test movement (1–6 tile advancement)
- [ ] Verify bounce-back on obstacles (no instant death)

**Deliverable:** Playable 5-tile prototype; core mechanics validated

---

### Sprint 2: Board & Tile System (Weeks 3–4)

**Goal:** Build the full board system; implement all tile types.

**Tasks:**
- [ ] Design & layout World 1 board (20–25 tiles; see WORLD_1_LAYOUT.md)
- [ ] Create tile prefabs: Normal, Portal, Whirlpool, Elevator, Hyperspace, Boss
- [ ] Implement tile effect logic (movement bonuses, penalties, triggers)
- [ ] Build board state manager (tracking player position, tile visited, etc.)
- [ ] Add visual feedback for landing on special tiles
- [ ] Implement turn counter & progress tracker UI

**Deliverable:** Full World 1 board with all tile mechanics working

---

### Sprint 3: Characters & Animation (Weeks 5–6)

**Goal:** Bring Dino & Cat to life; add polish.

**Tasks:**
- [ ] Convert hand-drawn characters to sprite sheets (Dino & Cat)
- [ ] Implement character controller (switch between Dino/Cat)
- [ ] Add jump/landing animations
- [ ] Add special ability animations (stomp, pounce)
- [ ] Idle, walk, celebrate animations
- [ ] Add sound effects (jump, land, power-up, wall-hit)

**Deliverable:** Animated, character-switched gameplay; audio foundation

---

### Sprint 4: Game Modes & UI (Weeks 7–8)

**Goal:** Implement solo mode, local multiplayer, and complete UI.

**Tasks:**
- [ ] Build main menu (character select, mode select)
- [ ] Solo mode: single player campaign with progression tracking
- [ ] Multiplayer mode: 2–4 player turn-based (pass controller, scoreboard, whose turn?)
- [ ] Game over screen (winner announcement, replay button)
- [ ] Story scenes: intro + ending
- [ ] Pause menu, settings (volume control)

**Deliverable:** Fully playable game (solo + multiplayer); complete game flow

---

### Polish & Release Buffer (Week 9+)

**Goal:** Fix bugs, polish, balancing, final testing.

**Tasks:**
- [ ] Playtesting with target age group (5–7 year-olds if possible)
- [ ] Difficulty tweaking (obstacle density, boss challenge)
- [ ] Visual polish (particle effects, screen shake, juice)
- [ ] Performance optimization
- [ ] Build for Windows & Mac
- [ ] Create simple manual / instructions

**Deliverable:** Polished, ready-to-play MVP

---

## Dependencies & Critical Path

1. **Sprint 1 → 2:** Core mechanics must feel right before building full board
2. **Sprint 2 → 3:** Board layout final before animating character movement
3. **Sprint 3 → 4:** Characters ready before implementing game modes
4. **Sprint 4 → Polish:** All core features functional before polish pass

---

## Success Criteria (MVP Done)

✅ Game is playable start-to-finish (World 1 only)  
✅ Both solo and multiplayer modes work  
✅ Gentle platforming feel (no frustration for young players)  
✅ All four learning goals represented in gameplay  
✅ Story provides narrative context (intro → ending)  
✅ Visually appealing and age-appropriate  
✅ Runs on Windows & Mac  
✅ No major bugs; polished feel

---

## Future Phases (Post-MVP)

- **Phase 2:** Worlds 2 & 3
- **Phase 3:** Online multiplayer
- **Phase 4:** iPad / iOS port
- **Phase 5:** Cosmetics, character unlocks, difficulty modes
