# MontyGame

A colorful space-dinosaur board-and-platformer game for 5–7 year-olds — a Snakes & Ladders
board crossed with arcade hazards, built for (and with) a kid whose own drawings are in it.

## Status

**Built, and playing on a real iPad.** A 10×10 board for 1–6 players, six pickable
characters, coins, rubies and a roving diamond to chase, pterodactyls and spaceships that
swoop in, a Hulk who prowls the top of the board hunting whoever is closest, a procedural
chiptune soundtrack, and a birthday celebration when somebody wins. The game rules are also
modelled separately as a tested .NET library (78 passing tests).

Open `MontyGame-Unity/MontyGame/` in Unity and press Play, or see `Docs/IPAD_DEPLOY.md` to
put it on an iPad.

## Repository structure

| Folder | Purpose |
| --- | --- |
| `MontyGame-Unity/MontyGame/` | **The playable game** (Unity 6). Everything is built in code on Play — no editor setup. |
| `src/MontyGame.Core/` | Engine-agnostic .NET 8 library modelling the World 1 rules (board, dice, tile effects, turn engine). |
| `src/MontyGame.Cli/` | Console simulator — plays a full seeded game to victory. |
| `tests/` | xunit suite (78 tests) over the core rules. |
| `Docs/` | Design documents, specs, and architecture decisions. |
| `Planning/` | Roadmap, task list, milestones. |
| `Research/` | Reference imagery, mood boards, the original hand drawings. |

## Where to start reading

- `Docs/UNITY_SHELL.md` — what the playable game actually is, and how to run it
- `Docs/IPAD_DEPLOY.md` — building and installing it on an iPad
- `Docs/GAME_DESIGN_IDEATION.md` — the design: concept, mechanics, learning goals, story
- `Docs/WORLD_1_LAYOUT.md` — the intended 25-tile World 1 board
- `Planning/SPRINT_ROADMAP.md` — the plan to MVP

## Quick check that the core rules still work

```bash
dotnet test                                        # 78 tests, must stay green
dotnet run --project src/MontyGame.Cli -- --auto   # full seeded playthrough to victory
```

## License

All Rights Reserved — see `LICENSE`. Note that the current in-game character art is
placeholder material cut from reference images and must be replaced with original art
before any distribution.
