# Workshop → Build: how it all hangs together

This is the exact end-to-end flow, demonstrated **live on this repo** (spec #61,
2026-07-06). Every artifact referenced below is real and checked in — use this
document to give the workshop.

```
 TEMPLATE                WORKSHOP                    FACTORY                     BUILD LOOP
┌───────────────┐   ┌──────────────────┐   ┌─────────────────────────┐   ┌─────────────────────────┐
│ .workshop.json │→ │ fill in answers,  │→ │ ingest → requirements,  │→ │ implementer builds task  │
│ (this folder)  │   │ CSV requirements, │   │ ADRs, build context,   │   │ → validator reviews     │
│                │   │ decisions, docs   │   │ draft SPEC (tasks)     │   │ → gates (build/test/    │
│                │   │                   │   │                        │   │   startup) → fix loop   │
└───────────────┘   └──────────────────┘   └─────────────────────────┘   └─────────────────────────┘
```

## Step 1 — Start from a template (JSON, machine-consumable)

- **This build's template:** [`../workshops/montygame-core-world1.workshop.json`](../workshops/montygame-core-world1.workshop.json)
  — a complete workshop submission: vision, user types, a MoSCoW requirements CSV,
  3 ADRs, an architecture context doc, team skills, testing strategy, fitness functions.
- **Generic starting points:** [`../workshops/examples/`](../workshops/examples/) — five
  shipped bundles (SaaS, internal tool, REST API, marketplace, mobile). In the Code Easy
  dashboard, the Workshop tab's **📦 Load an example…** picker loads one; tweak and go.
- Requirements use **MoSCoW** — and it's enforced: `must` rows become build tasks,
  `should`/`could` are recorded for later phases, `won't` is recorded but **never built**
  (see the "Unity MonoBehaviour integration — won't" row in our CSV).

## Step 2 — Workshop → ingest (deterministic, no LLM required)

Three equivalent entry points consume the same bundle shape:
1. Dashboard **Workshop** tab (6-step wizard; Import button takes a bundle)
2. Public page `/workshop` (browser-only; exports the bundle)
3. MCP: `codeeasy_workshop_ingest { repo, submission }`  ← what we used

What ingestion produced here:
| Input | Became |
|---|---|
| Requirements CSV (11 rows) | 11 rows in the `requirements` table (MoSCoW → priority) |
| 3 decisions | 3 ADRs in `design_decisions` |
| Architecture context | `Docs/../docs/WORKSHOP-CONTEXT.md` (our hand-written `CLAUDE.md` was **not** touched — never clobbered) |
| Everything | **Spec #61** "MontyGame Core (World 1)" — a draft spec with 7 concrete tasks |

## Step 3 — Plan gate

`codeeasy_start_spec` generates the plan and runs the **actionability gate**: every task
must name a concrete deliverable (file, route, or imperative action). Our tasks all name
files (`src/MontyGame.Core/Board.cs`, …) so the gate passed and queued:
7 implement tasks + an auto `[VALIDATE]` + an auto `[STARTUP_TEST]`.

## Step 4 — The autonomous loop

- **Implementer** (claude CLI, model pinned to sonnet) claims each task, writes code in
  this repo, and completes with the changed files.
- **Validator** reviews each handoff read-only and submits passed / needs_changes;
  rejections reopen the task and spawn a fix cycle.
- **Toolchain-aware gates:** the moment `MontyGame.sln` appeared, validation switched to
  the **dotnet profile** — `dotnet build` + `dotnet test` gate each phase, and the final
  startup test runs `dotnet run --project src/MontyGame.Cli -- --auto` (a full seeded
  World 1 playthrough must finish and exit 0).
- Guardrails throughout: max files per task, blocked paths, model **deny-list**
  (`fable` is blocked), optional engine escalation (off by default).

## Step 5 — What came out (the proof)

- `MontyGame.sln`, `src/MontyGame.Core/` (board, dice, tile effects, turn engine),
  `src/MontyGame.Cli/` (playthrough simulator), `tests/MontyGame.Core.Tests/`
- Verify any time: `dotnet test` and
  `dotnet run --project src/MontyGame.Cli -- --auto`
- The core has **zero UnityEngine references** — Sprint 1 in Unity is thin
  MonoBehaviour shells over this tested library (see ADR "Engine-agnostic core library").

## Re-running the demo (for the live workshop)

1. Open the dashboard → **Workshop** tab → **Import** →
   `codeeasytemplates/workshops/montygame-core-world1.workshop.json`
   (or 📦 Load an example for a generic one)
2. Walk the 6 steps — everything is pre-filled; show the boilerplate chips
3. **⚡ Ingest & create spec** → open the **Agents** tab → start the spec
4. Watch the queue: implement → validate → startup test → done
