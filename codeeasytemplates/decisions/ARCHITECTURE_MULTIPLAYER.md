# ADR-003: Local Multiplayer Architecture (Turn-Based)

**Status:** Accepted

**Date:** 2026-07-06

**Affected Components:** Game Manager, UI, Input Handling, Turn System

---

## Context

MontyGame supports local multiplayer: 2–4 players pass a controller and take turns. We need to decide:
- How to manage turn order
- How to handle player input (controller pass, whose turn?)
- How to display whose turn it is (UI feedback)
- How to handle win conditions (first to finish, highest score, etc.)

Must be intuitive for 5–7 year-olds (simple UI, clear feedback).

---

## Options Considered

**Option 1: Free-For-All (All Players Moving Simultaneously)**
- **Pros:** Fast-paced, exciting, less waiting
- **Cons:** Chaotic for young kids, harder to track, control conflicts
- **Effort:** High (collision, simultaneous updates)
- **Scalability:** Confusing for 5–7 year-olds

**Option 2: Turn-Based (Classic Board Game Style)**
- **Pros:** Clear, easy to understand, classic board game feel, less controller passing
- **Cons:** Slower pacing, more waiting (but that's okay for turn-taking education)
- **Effort:** Low
- **Scalability:** Perfect for 5–7 year-olds; teaches fairness

**Option 3: Async Online (Not MVP, but future)**
- **Pros:** Supports remote play
- **Cons:** Requires backend, network code, out of scope for MVP
- **Effort:** Very High
- **Scalability:** Post-MVP only

---

## Decision

**We chose: Option 2 (Turn-Based Local Multiplayer)**

---

## Rationale

Turn-based is ideal for MontyGame's educational goals:

1. **Teaches Turn-Taking:** Kids learn to wait, celebrate others' successes, handle their turn
2. **Clear for Young Kids:** "It's your turn" is simple; simultaneous inputs would confuse
3. **Classic Feel:** Matches board game root (Snakes & Ladders)
4. **Simple Implementation:** No complex collision handling; clear state machine
5. **Multiplayer in MVP:** Can ship with turn-based; async online comes later

---

## Architectural Details

### Turn Order Management

**GameManager (Orchestrates multiplayer flow):**
```csharp
public class GameManager : MonoBehaviour {
    public int currentPlayerId;              // 0, 1, 2, 3
    public PlayerState[] players;            // All player states
    public int totalPlayers;
    
    public void AdvanceTurn() {
        currentPlayerId = (currentPlayerId + 1) % totalPlayers;
        UIManager.ShowPlayerTurn(currentPlayerId);
    }
    
    public void CheckWinCondition() {
        var winner = players.FirstOrDefault(p => p.currentTile >= 25);
        if (winner != null) EndGame(winner);
    }
}
```

### Turn Flow (Per Turn)

```
1. GameManager: "Player 1's turn"
2. UI: Display player 1 character, big button "ROLL DICE"
3. Player 1: Tap ROLL button
4. GameManager: Roll 1–6, move player 1
5. Player 1: Control character through platforming challenge
6. Player 1: Land on tile
7. GameManager: Apply tile effect (warp, penalty, boss, etc.)
8. UI: Celebration (if milestone reached) or prompt
9. GameManager: AdvanceTurn()
10. UI: "Player 2's turn" (clear feedback)
11. Repeat until someone reaches tile 25
```

### UI for Turn-Based

**Main Game UI (During Gameplay):**
```
┌─────────────────────────────┐
│  Player 1's Turn            │  ← Clear feedback
├─────────────────────────────┤
│                             │
│    [GAME BOARD 25 TILES]    │
│                             │
│    Player 1 (Dino): Tile 12 │  ← Position display
│    Player 2 (Cat):  Tile 8  │
│    Player 3: --             │  ← Not in this game
│                             │
│      [ ROLL DICE ]          │  ← Big button, easy to tap
│                             │
└─────────────────────────────┘
```

**Scoreboard (During Gameplay):**
```
1st: Player 1 (Dino)  - Tile 12
2nd: Player 3 (Robot) - Tile 9
3rd: Player 2 (Cat)   - Tile 5
```

**Victory Screen:**
```
┌─────────────────────────────┐
│                             │
│  🎉 PLAYER 1 WINS! 🎉     │
│                             │
│  Dino returned home!        │
│  [PLAY AGAIN]  [MAIN MENU]  │
│                             │
└─────────────────────────────┘
```

### Player Selection Screen

**Before Game Starts:**
```
How many players? [2] [3] [4]

Player 1: [SELECT CHARACTER]  → Dino / Cat / [Future]
Player 2: [SELECT CHARACTER]  → Dino / Cat / [Future]
Player 3: [SELECT CHARACTER]  → Dino / Cat / [Future]
Player 4: [SELECT CHARACTER]  → Dino / Cat / [Future]

[START GAME]
```

### Win Condition

**For MVP:**
- **First to Tile 25 wins**
- Clear visual feedback (confetti, fanfare, big text)
- Show final standings (1st, 2nd, 3rd, 4th place)

**Post-MVP Options:**
- Highest score by turn X
- Most power-ups collected
- Fastest time (fewest turns to reach goal)

---

## State Management

**GameManager holds global state:**
```csharp
public enum GameState {
    MainMenu,
    CharacterSelect,
    Playing,
    PlayerTurn,
    AnimatingTileEffect,
    Victory,
    GameOver
}

public GameState gameState;
public int currentPlayerId;
public PlayerState[] players;
```

**Transitions:**
```
MainMenu → CharacterSelect → Playing → PlayerTurn → AnimatingTileEffect → PlayerTurn → ... → Victory
```

---

## Input Handling

**Controller Pass (Local Multiplayer):**
```csharp
public void OnPlayerInput_RollDice() {
    if (gameState != GameState.PlayerTurn) return; // Ignore if not their turn
    if (currentPlayerId != GetCurrentInputPlayer()) return; // Safety check
    
    int roll = Random.Range(1, 7);
    AdvancePlayer(currentPlayerId, roll);
}
```

**UI Button System:**
- During Player 1's turn, only "ROLL DICE" button is active
- During Player 2's turn, same button is active, but Player 2's input is accepted
- Clear visual feedback ("Waiting for Player 2...")

---

## Implications

### Code Structure
- **GameManager.cs** — Turn order, game state, win conditions
- **PlayerState.cs** — Per-player data (position, character, turn info)
- **UIManager.cs** — Display current player, scoreboard, turn indicators
- **InputManager.cs** — Handle whose input is active

### Testing
- **Unit Tests:** Turn order calculation, win condition detection
- **Integration Tests:** Full 2–4 player game flow
- **Playtest:** Verify kids understand whose turn it is

### Performance
- Minimal overhead; pure state machine

### Future Decisions
- **Online Multiplayer:** Extend with network sync (post-MVP)
- **AI Opponents:** Add bots that take automated turns (post-MVP)
- **Difficulty Per Player:** Each player can choose difficulty (post-MVP)

---

## Success Metrics

**We'll know this works when:**
- [ ] Turn order is clear to kids (they don't get confused about who plays next)
- [ ] No input conflicts (only current player can roll)
- [ ] UI clearly shows whose turn it is
- [ ] Victory is celebrated appropriately
- [ ] 2–4 player games work without crashes

---

## Related Decisions

- **ADR-002: Board State** — Tracks per-player positions
- **ADR-001: Platformer Physics** — Same physics for all players
- **Design Doc:** `/Docs/GAME_DESIGN_IDEATION.md` (section 7: Multiplayer Specifics)

---

## Revision History

| Version | Date | Author | Status |
| --- | --- | --- | --- |
| 1.0 | 2026-07-06 | Claude | Accepted |

---

**Decision Owner:** thejustinjames  
**Last Updated:** 2026-07-06
