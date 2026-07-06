# MontyGame Core — build context

## Tech Stack
- .NET 8 SDK (LTS), C# 12
- xunit for tests, System.Text.Json for serialization
- No external NuGet packages beyond xunit — the core must stay dependency-free for Unity

## Structure
- `MontyGame.sln` — solution at repo root
- `src/MontyGame.Core/` — pure game-logic class library (Board, Tile, Dice, MovementCards, TileEffects, GameEngine, Characters, StoryBeats, GameState)
- `src/MontyGame.Cli/` — console playthrough simulator (`dotnet run --project src/MontyGame.Cli -- --auto`)
- `tests/MontyGame.Core.Tests/` — xunit suite

## Hard constraints
- **Zero `UnityEngine` references in MontyGame.Core** — it will be consumed by thin MonoBehaviour shells later; keep public API engine-neutral (plain C# types, events/callbacks, no static singletons)
- All randomness through an injected `IRandom` (seedable) — never `new Random()` inline
- Forgiving design: players never drop below tile 1, no eliminations, no instant game-over
- Game rules source of truth: `Docs/GAME_DESIGN_IDEATION.md` and `Docs/WORLD_1_LAYOUT.md`

## Patterns
- Engine exposes state + `TakeTurn()`; UI layers (CLI now, Unity later) render and drive it
- Story beats and tile effects surface as events the shell can subscribe to
- Tests assert rules from the design docs (tile effect ranges, clamping, win at tile 25)
## Target Users

- Solo child player (campaign)
- 2–4 players local pass-and-play
- Unity shell developer (consumes the core API)

## Target Platforms

- Cross-platform .NET 8 (macOS/Windows/Linux) — Unity Personal shell later per the locked stack

## Delivery Context

- Context: greenfield — a brand-new build from scratch.

## Monetization / Business Model

- Monetization: one-time purchase or perpetual license.

## Risks & Assumptions

- Risk: aggressive/tight timeline.
- Risk: Unity integration assumptions must hold (core API stays engine-neutral).

## Domain Model / Key Entities

- Board
- Tile
- Player
- Character (Dino, Cat)
- Dice / Movement card
- Tile effect
- Story beat
- Game state

## Architecture Style

- Architecture: a library/SDK with no application UI.

## Testing Strategy

- Testing: unit tests for core logic.
- Testing: maintain ≥ 80% test coverage.
- Testing: test-driven development.

## Team Skills & Capabilities

- Team skill: C# / .NET.
- Team context: small team or solo — favour simple, low-maintenance stacks; avoid single-person dependency.

**Align technology choices with the team's skills above — prefer what they can build & maintain; avoid introducing tech the team can't support (single-person dependency risk).**

## Fitness Functions (architecture gates — keep these passing)

- Fitness function: test_coverage() > 80%.
- Fitness function: unity_references_in_core() == 0.

