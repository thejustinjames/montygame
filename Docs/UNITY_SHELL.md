# The Unity Game (as actually built)

This is the playable thing. It describes what is in the Unity project **today** —
not what the design docs plan for. Where the two disagree, this file is the truth
about the code and `GAME_DESIGN_IDEATION.md` / `WORLD_1_LAYOUT.md` are the truth
about the intent.

**Last verified:** 2026-07-13 (compiles clean in Unity 6000.5.2f1, 0 `error CS`)

---

## Where it lives, how to run it

The real Unity project is at **`MontyGame-Unity/MontyGame/`** — Unity Hub nested it
one level deeper than you'd expect. Its `Assets/` folder is the one Unity loads.
Editing a sibling `MontyGame-Unity/Assets/` does nothing; that folder was deleted.

- Unity **6.5 (6000.5.2f1)**, built-in render pipeline (the "deprecated" warning is harmless).
- Open the project in Unity Hub and press **Play**. There is no scene to set up.
- Batch compile check (no editor needed):
  ```bash
  /Applications/Unity/Hub/Editor/6000.5.2f1/Unity.app/Contents/MacOS/Unity \
    -batchmode -quit -nographics -projectPath MontyGame-Unity/MontyGame -logFile /tmp/unity.log
  grep -c "error CS" /tmp/unity.log   # must be 0
  ```

## Two hard facts about the architecture

1. **Everything is built in code on Play**, via `[RuntimeInitializeOnLoadMethod]` in
   `GameBootstrap.cs` — camera, board, tiles, tokens, hazards, UI. There is no manual
   editor setup, no prefabs to wire, no scene to hand-edit. Drive everything from scripts.
2. **The Unity build does NOT use `MontyGame.Core`.** Core targets .NET 8 and uses C# 11
   (`required`, file-scoped namespaces), which Unity can't load. So the board rules are
   **re-implemented Unity-side**, mirroring Core in spirit but not in code. Core remains
   the tested source of truth for the *design*; the Unity scripts are what actually runs.
   This is a knowing divergence from ADR 0001 — revisit if the two drift far enough to hurt.

### It's a 10×10 board, not the 25-tile World 1

Unity plays a **100-square** snakes-and-ladders board (`BoardLayout.cs`), not the 25-tile
World 1 in `Docs/WORLD_1_LAYOUT.md`. Numbering is boustrophedon: square 1 bottom-left,
rows alternate direction, 100 top-left. Win by reaching **100**.

---

## What's in the game

### Board (`BoardLayout.cs` — single source of truth for both rendering and movement)
- **Ladders / "up" jumps:** 1→20, 3→22, 8→26, 28→77, 36→57, 51→72, 71→92.
  Most are spaceships; **36 is the Hulk** carrying you up.
- **Snakes / "down" slides:** 17→6, 47→26, 62→18, 87→24, 95→56.
  Most are vortexes; **62 is a T-Rex** dragging you down.
- **Coins:** squares 5, 14, 33, 45, 58, 66, 73, 84, 91. Taken by the first player to
  **land** on them (passing over doesn't collect).
- **Boss:** square 100, which is also the win.

### Turn loop (`GameController.cs`)
Two-player pass-and-play. Press **ROLL** → animated tumbling die → walk square by
square → land → effects apply. Camera zooms in on the active player and pulls back
between turns.

- **Roll again on a 6**, and the rolls are *summed* — the game shows the equation
  ("6 + 2 = 8"), which is deliberate counting practice. Capped at 4 chained rolls.
- Effects (ladder/snake/coin) apply **only on the square you land on**, never in passing.

### Hazards, bonuses, and the Hulk
- **Pterodactyls** (2, roaming): touch one and you're dragged back to square 10 —
  **unless you hold a coin or diamond**, which is consumed as a shield instead.
- **Spaceships** (2, roaming): touch one and you're beamed up to square 90.
- **Diamond:** a bonus that re-spawns a few squares *ahead* of the current player every
  ~3 rolls, so there's always something to chase. Shields like a coin.
- **The HULK** (added 2026-07-13): a third mover who plays **in reverse**.
  - Drops in at square **100** (~35% chance on any turn he's off the board), with a roar.
  - Each turn he rolls 1–6 and stomps that many squares **down** the board, flashing a
    warning ("HULK MOVES 4!") before he moves.
  - He roams at most **20 squares** — he never goes below square **80**. A roll that would
    take him past it means he gets bored and leaves the board (he can drop in again later).
    So he is effectively a **guardian of the run-up to the goal**.
  - **If he lands on a player's square, that player is thrown back to square 50** with a
    roar and a smash. Note: it only triggers on *his* landing — a player landing on *his*
    square is safe, and coins do **not** shield against him.

### Characters / avatars
Character-select gallery on start; each player picks (player 2 can't take player 1's pick).
Seven avatars, sprites in `Assets/Resources/avatar_1..7.png`:

| # | Name | Source |
| --- | --- | --- |
| 1 | Dino | the kid's hand drawing (`Research/MontyDrawings/`) |
| 2 | Cat | the kid's hand drawing |
| 3 | Rex | pink boombox T-rex (`Research/trex_getoblaster.jpeg`) |
| 4 | Robo | retro tin robot (`Research/robot1.jpeg`) |
| 5 | Saucer | flying saucer (`Research/ufo.jpg`) |
| 6 | Goldbot | gold protocol droid (`Research/c3po.jpeg`) |
| 7 | Bleep | blue/white astromech (`Research/r2d2.jpeg`) |

The gallery grid sizes itself from `AvatarCount` (max 4 per row, short last row centered),
so adding an 8th avatar is just a PNG in `Resources/` plus a name in `AvatarNames`.

> ⚠️ **These are placeholders, not shippable art.** Goldbot, Bleep, and the Hulk are
> recognizable trademarked characters and the T-rex/saucer are someone else's copyrighted
> images. Fine for a family build; they must be replaced with original art before this is
> distributed or commercialized (the LICENSE reserves the right to).

### Audio (`Sfx.cs`) — all procedural, zero audio files
Every sound is synthesized in code at runtime. Music and SFX have separate on/off toggles.

- **Music:** an 8-bar 1980s-arcade chiptune loop (pulse melody, square bass, kick + hats).
- **SFX:** `click`, `roll`, `coin`, `up`, `down`, `hit`, `shield`, `win`, and the Hulk's
  `roar` (arrival) and `smash` (when he catches you) — a low growling fundamental with
  harmonics, a fast wobble for the "grrr", and rasp noise, deliberately driven past full
  scale so the soft-clip turns it into a snarl.

To audition a new sound without opening Unity, re-implement its math in a throwaway Python
script, render a WAV, and `afplay` it — that's how the roar was tuned.

---

## Script map (`MontyGame-Unity/MontyGame/Assets/`)

| File | Role |
| --- | --- |
| `GameBootstrap.cs` | Builds the entire scene on Play: camera, backdrop, 100 squares, ladder/snake art, coins, tokens, `_GameManager`. |
| `BoardLayout.cs` | Board data + square→world math. Shared by rendering and movement so they can't disagree. |
| `GameController.cs` | The game: turn loop, dice animation, camera, coins/diamond, flyers, the Hulk, character select, IMGUI HUD. |
| `BoardEffects.cs` | Animated snake trails (swirling vortex, jungle vine) and bobbing pickups. |
| `Sfx.cs` | Procedural sound engine + chiptune music. |
| `PlayerController.cs` | Leftover platformer move/jump from the 5-tile test. **Unused by the board game.** |

All UI is **IMGUI (`OnGUI`)** — deliberately, so there's no canvas to wire up in the editor.
