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
  ⚠️ **This silently does nothing while the Unity Editor is open** — the editor holds the
  project lock, so the run aborts after a few lines and `grep -c "error CS"` cheerfully
  reports 0 having compiled nothing. Always confirm the log actually contains a
  `Csc Library/...Assembly-CSharp.dll` line before believing it.

- Compile check that works **with the editor open** (drives Roslyn from the .NET SDK against
  Unity's own assemblies — Unity's bundled `csc` shim is broken, its shebang points at a
  build-farm path). Build a response file so the reference list survives the shell:
  ```bash
  U=/Applications/Unity/Hub/Editor/6000.5.2f1/Unity.app/Contents/Resources/Scripting
  { echo "-target:library"; echo "-nologo"; echo "-noconfig"; echo "-nostdlib+"; echo "-langversion:9"
    echo "-out:/tmp/check.dll"
    find "$U/UnityReferenceAssemblies/unity-4.8-api" -maxdepth 2 -name "*.dll" | sed 's/^/-r:/'
    find "$U/Managed/UnityEngine" -name "*.dll" | sed 's/^/-r:/'
    ls Assets/*.cs
  } > /tmp/build.rsp
  dotnet ~/.dotnet/sdk/*/Roslyn/bincore/csc.dll @/tmp/build.rsp | grep "error CS"
  ```
  (`-maxdepth 2` matters: the `Facades/netstandard.dll` one level down is required, and
  without it you get ~100 bogus CS0012 errors that look like real breakage.)

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
- **Rubies:** squares 11, 21, 27, 39, 43, 55, 64, 76, 82, 93 — ten of them, none sharing a
  square with a ladder, snake, coin or their landing spots. Land-only, like coins.
- **Boss:** square 100, which is also the win.

### Turn loop (`GameController.cs`)
**1–6 players**, pass-and-play. The first screen asks how many are playing (1 = beat the
board solo), then each player picks a character in turn — no two can take the same one.
Press **ROLL** → animated tumbling die → walk square by square → land → effects apply.
Camera zooms in on the active player and pulls back between turns.

Tokens sit in a 3×2 arrangement within a square so six can share one without hiding each
other; each player has a colour used for their token, their HUD row, the turn banner and
the ROLL button. `MaxPlayers`, `TokenOffsets` and `TokenColors` on `GameController` are the
single source of truth — `GameBootstrap` spawns one token per possible player from them.

- **Roll again on a 6**, and the rolls are *summed* — the game shows the equation
  ("6 + 2 = 8"), which is deliberate counting practice. Capped at 4 chained rolls.
- Effects (ladder/snake/coin) apply **only on the square you land on**, never in passing.

### Hazards, bonuses, and the Hulk
- **Pterodactyls** (2, roaming): touch one and you're dragged back to square 10 — but
  **only once per player per game** (`Player.pteroUsed`). After that he swoops and misses.
  A coin or diamond shields you instead, and being shielded does *not* use up your one grab.
- **Spaceships** (2, roaming): touch one and you're beamed up to square 90 — but **only if
  you're behind 90**. Already past it? The ship flies by and nothing happens (it must never
  drag a leader backwards).
- **Diamond:** a bonus that re-spawns a few squares *ahead* of the current player every
  ~3 rolls, so there's always something to chase. Shields like a coin.
- **Rubies:** carry at most **3** (`MaxRubies`). Landing on a snake **automatically spends
  one** and cancels the fall — no prompt, because a rescue that just happens delights a
  5-year-old more than a yes/no question mid-turn. Full pockets? The ruby stays on the board
  for someone else rather than being wasted. Rubies stop **snakes only** — coins stop the
  pterodactyl, and nothing stops the Hulk. Each pickup means exactly one thing.
- **The HULK on square 36** (the ladder): carries you up — then **shakes every ruby out of
  your pockets** (`HulkRobs()`, shared with the roaming Hulk so the two can't drift apart).
- **The HULK** (added 2026-07-13): an extra mover who hunts the players.
  - Drops in at square **100** (~35% chance on any turn he's off the board), with a roar.
  - Each turn he **rolls the die himself — tinted green** — then stomps that many squares
    **toward the nearest player**, up or down the board (`NearestPlayer()`), flashing a
    warning ("HULK MOVES 4!") first. Because the board is boustrophedon, that reads on
    screen as him prowling left and right along the rows.
  - He prowls the top of the board only: **never below square 80** (20 squares of range).
    A roll that would carry him past either end means he gets bored and leaves (he can drop
    in again later). So he's a **guardian of the run-up to the goal**.
  - **If he lands on a player's square, that player is thrown back to square 50** with a
    roar and a smash, **and loses every ruby**. It only triggers on *his* landing — a player
    landing on *his* square is safe, and coins do **not** shield against him.
  - **A snake can catch the Hulk too.** Land him on a snake head and he's dragged down like
    anyone else — and from then on he's a **climber** (`hulkClimbing`): he ignores the
    players and only heads back toward 100, whatever his die says. Reaching the top, he
    stomps off the board and may drop in again later, fresh. A snake is therefore the only
    thing in the game that gets him off your back — though he'll still smash anyone he
    happens to land on during the climb, down where the players actually are.

### Screens
- **HOW TO PLAY** — a table of every piece and what it does. The text lives in `RuleRows` in
  `GameController.cs`, deliberately next to the code implementing it, so the rules screen
  can't quietly drift out of sync with the actual rules. **Update it when you change a rule.**
- **CREDITS** — "CODED BY DADDY / FOR MONTY / HAPPY 5th BIRTHDAY!", with ticker tape falling
  and balloons rising. Both are pure IMGUI: the balloon is a procedurally generated texture
  (ellipse + knot + highlight), and every particle's path comes from a hash of its index, so
  there's no particle state — it animates straight off the clock.

### Characters / avatars
Character-select gallery after the player count; each player picks in turn, and an avatar
someone already took shows as **TAKEN**. Six on offer, sprites in `Assets/Resources/`:

| Slot | Name | Source |
| --- | --- | --- |
| 1 | Cat | the kid's hand drawing (`Research/MontyDrawings/`) |
| 2 | *(retired)* | an earlier drawing of the same cat — still on disk, **not offered** |
| 3 | Rex | pink boombox T-rex (`Research/trex_getoblaster.jpeg`) |
| 4 | Robo | retro tin robot (`Research/robot1.jpeg`) |
| 5 | Saucer | flying saucer (`Research/ufo.jpg`) |
| 6 | Goldbot | gold protocol droid (`Research/c3po.jpeg`) |
| 7 | Bleep | blue/white astromech (`Research/r2d2.jpeg`) |

`VisibleAvatars` controls which slots the gallery offers, so retiring or restoring one is a
one-line change; the grid sizes itself from that list (max 4 per row, short last row centred).

> **Watch the EXIF.** The kid's drawings are phone photos, and slot 1 carried an
> `Orientation: LeftBottom` tag: Preview and macOS honoured it and showed the cat upright,
> but **Unity ignores EXIF and renders the raw pixels**, so in-game she lay on her side. The
> fix is to bake the rotation into the pixels and drop the tag —
> `magick in.png -auto-orient -strip out.png`. Do this to any new photo-sourced sprite.

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
