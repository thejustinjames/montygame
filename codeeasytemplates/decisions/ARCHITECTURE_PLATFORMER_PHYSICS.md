# ADR-001: Platformer Physics & Movement

**Status:** Accepted

**Date:** 2026-07-06

**Affected Components:** Player Controller, Movement System, Collision, Animation

---

## Context

MontyGame targets 5–7 year-olds. The platforming must feel **gentle, forgiving, and encouraging** — not punishing. We need to decide on:
- Jump physics (curve, height, timing)
- Collision behavior (instant death vs. bounce-back)
- Movement speed & acceleration
- Wall/obstacle interaction (stick vs. slide vs. bounce)

These decisions directly impact whether kids feel successful or frustrated.

---

## Options Considered

**Option 1: Tight, Precision-Based Physics (like Mario)**
- **Pros:** Skill-based, satisfying for experienced players, classic feel
- **Cons:** Unforgiving, kids will die frequently, frustrating for 5–7 year-olds
- **Effort:** High (lots of tuning)
- **Scalability:** Good for skill-based games; wrong fit for MontyGame

**Option 2: Floaty, Forgiving Physics (like Fall Guys)**
- **Pros:** Forgiving, hard to die, encourages exploration, feels gentle
- **Cons:** Less skill-based, may feel less responsive, can feel "loose"
- **Effort:** Medium (balance is tricky)
- **Scalability:** Good for all-ages, casual games; fits MontyGame better

**Option 3: Collision-Based (no jumping physics, just move tiles)**
- **Pros:** Simplest to implement, no physics edge cases
- **Cons:** Loses platforming feel entirely, not engaging for kids
- **Effort:** Low
- **Scalability:** Doesn't work; defeats game concept

---

## Decision

**We chose: Option 2 (Floaty, Forgiving Physics)**

---

## Rationale

MontyGame's target audience is 5–7 year-olds. The game should feel **encouraging and achievable**, not punishing. Tight platforming would create frustration; forgiving physics creates confidence and joy.

Key reasons:
1. **Age-appropriate:** Young kids need immediate success feedback, not challenge/failure loops
2. **Educational goal:** Kids learn through success, not failure. Forgiving physics = more learning
3. **Story context:** Dino & Cat are helping kids escape, not testing their skills. The tone should be playful, not competitive
4. **Multiplayer fairness:** In turn-based multiplayer, nobody wants to watch another player struggle for 5 minutes on one jump
5. **Engagement:** Forgiving physics = more time exploring, fewer restarts = better retention

---

## Architectural Details

### Jump Physics

**Settings to implement:**
```
Jump Height: 1.5–2.0 grid units (adjustable per test)
Jump Duration: ~0.4 seconds (rise + fall)
Gravity: 9.81 (standard)
Apex Control: Full (player can adjust jump height mid-air)
```

**Feel Profile:**
- Jump should feel **floaty** (slight air hang at apex)
- Gentle ascent, gentle descent
- Should be **easy to clear small gaps** (1–2 units)
- Should be **achievable for young fingers** (not twitch-heavy)

### Collision Behavior

**Obstacle Hit:**
- NOT instant death
- Player bounces back 0.5–1.0 unit gently
- Brief animation (flash, bounce sound)
- Player can immediately retry
- No loss of life or score

**Pit Fall:**
- Player falls through pit
- Bounces back to start of tile with gentle animation
- No penalty beyond time (learning moment, not failure)

**Wall Collision:**
- Player can push against walls (doesn't stick)
- Slight slide instead of hard stop
- Player can jump off walls if close enough (encourages platforming skill without requiring it)

### Movement & Speed

**Horizontal Movement:**
```
Walk Speed: 5–6 grid units/second
Run Speed: 8–10 grid units/second
Acceleration: Instant (no feel-heavy buildup)
Direction Change: Instant (responsive to input)
```

**Feel Profile:**
- Should be **responsive** (input feels immediate)
- Should be **not too fast** (kids can track the character)
- Should be **easy to control** (forgiving input window)

### Input Tolerance

**Forgiving Input Windows:**
- Coyote time (jump grace window): 0.1–0.15 seconds after leaving platform
- Jump buffer: 0.1 seconds (press jump slightly before landing, still works)
- No frame-perfect inputs required

---

## Implementation Pattern

**Physics Controller Script:**
```csharp
[SerializeField] float jumpForce = 10f;        // Affects height
[SerializeField] float gravity = 9.81f;
[SerializeField] float moveSpeed = 6f;
[SerializeField] float coyoteTime = 0.1f;
[SerializeField] float jumpBuffer = 0.1f;
[SerializeField] float bounceForce = 5f;       // For obstacle hits

// Apply physics each frame
// Handle input with tolerance windows
// Bounce on collision (not hard stop)
```

**Collision Handling:**
```csharp
OnObstacleHit() {
    velocity = new Vector2(-bounceForce, bounceForce); // Gentle bounce back
    PlayBounceAnimation();
    PlayBounceSound();
    // No score loss, no death
}

OnPitFall() {
    // Fall through, bounce at bottom, respawn at tile start
    SetPosition(tileStartPos);
    PlayFallAnimation();
}
```

---

## Implications

### Code Structure
- **Player Controller:** `Assets/Scripts/Player/PlayerController.cs` handles movement, jumping, collision
- **Physics Config:** `Assets/Data/PhysicsConfig.asset` (ScriptableObject for easy tuning)
- **Input Handler:** `Assets/Scripts/Input/PlayerInput.cs` with tolerance windows

### Testing
- Unit tests for jump physics (apex height, duration)
- Collision tests (bounce behavior, pit fallthrough)
- Playtesting with target age group (5–7 year-olds)
  - **Pass criteria:** Kids clear obstacles without frustration
  - **Fail criteria:** Kids get stuck or frustrated in first 5 minutes

### Performance
- Physics-based (Rigidbody2D preferred) is more efficient than frame-by-frame logic
- No heavy computation; 60 FPS easily achievable

### Future Decisions
- **Difficulty Scaling (Worlds 2–3):** Increase obstacle density, not physics tightness. Forgiving physics remains constant.
- **Accessibility:** Can add "extra forgiving" toggle (bigger coyote window, slower obstacles)

### Documentation
- **Designer Guide:** How to adjust physics parameters for feel
- **Animation Guide:** Character animations synced to physics (jump curves, landing, bounce)

---

## Success Metrics

**We'll know this works when:**
- [ ] Kids (5–7 year-olds) play for >10 minutes without frustration
- [ ] No complaints about "cheap deaths" or unfair obstacles
- [ ] Kids feel confident attempting platforming challenges
- [ ] Jump feels responsive and "right" to adults too (not too floaty)
- [ ] Physics remain consistent across all 25 World 1 tiles

**Playtesting Checklist:**
- [ ] Jump feels good (not too high, not too short)
- [ ] Bouncing off obstacles feels fair
- [ ] Pit fallthrough feels forgiving
- [ ] Character feels responsive to input
- [ ] No edge cases (stuck in walls, impossible jumps, etc.)

---

## Related Decisions

- **ADR-002: Board State & Tile System** — will use this physics engine for tile traversal
- **ADR-003: Multiplayer Turn-Based** — relies on consistent player movement
- **Design Doc:** `/Docs/GAME_DESIGN_IDEATION.md` (section 2: Core Gameplay Loop) — references gentle platforming requirement

---

## Risk & Mitigation

| Risk | Probability | Impact | Mitigation |
| --- | --- | --- | --- |
| Physics feel is wrong (too floaty or too tight) | High | High | Prototype in Sprint 1; iterate based on playtesting |
| Physics don't feel responsive enough | Medium | High | Adjust acceleration, coyote time, input buffer in tuning phase |
| Performance issues with physics engine | Low | Medium | Profile early; Rigidbody2D is lightweight |

---

## Revision History

| Version | Date | Author | Status | Notes |
| --- | --- | --- | --- |
| 1.0 | 2026-07-06 | Claude | Accepted | Initial architecture decision for Sprint 1 |

---

## Next Steps

1. **Sprint 1 Prototype:** Build basic player controller with forgiving physics
2. **Playtesting:** Get feedback from target age group
3. **Iteration:** Adjust parameters (jump height, coyote time, bounce strength) as needed
4. **Lock-in:** Once feel is right, freeze physics parameters for Sprints 2–4
5. **Document:** Capture final tuned values in `/Docs/PHYSICS_TUNING.md`

---

**Decision Owner:** thejustinjames  
**Last Updated:** 2026-07-06
