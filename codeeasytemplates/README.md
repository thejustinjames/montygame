# MontyGame — CodeEasy Templates & Workflows

**Purpose:** Templates and workflows for automating MontyGame development using the CodeEasy MCP server. Test architectural decisions, validate designs, and run autonomous multi-agent builds.

**Structure:**
- `specs/` — Specification templates for different types of work
- `workflows/` — Process automation workflows for recurring tasks
- `decisions/` — Architectural decision templates and frameworks
- `processes/` — End-to-end process documentation

---

## Quick Start

### 1. Register MontyGame with CodeEasy

```bash
# In Claude Code or terminal:
! codeeasy_register_repo
# Point to: /Users/justin/Documents/GitHub/MontyGame
```

### 2. Use a Spec Template

```bash
# Create a spec for a feature or bug:
! codeeasy_create_spec

# Reference a template from specs/ folder
# Fill in the architectural decisions from decisions/ folder
```

### 3. Launch Autonomous Build

```bash
# After spec is created:
! codeeasy_start_spec

# Codex (multi-agent implementer) + Claude validator will execute
# Monitor progress:
! codeeasy_get_spec_status
! codeeasy_get_autonomous_status
```

---

## Template Categories

### Specs (Feature, Bug, Refactor)

Specifications that define WHAT to build and WHY. CodeEasy uses specs to drive autonomous multi-agent builds.

**Format:** Each spec template includes:
- **Problem Statement** — what's broken or missing
- **Requirements** — acceptance criteria
- **Constraints** — technical limits, dependencies
- **Success Metrics** — how to verify it's done
- **Architectural Decisions** — links to decision templates
- **References** — design docs, related code

**When to use:** Before implementing any non-trivial feature or fix

### Workflows (Sprint Automation)

Recurring multi-step processes that CodeEasy can automate or coordinate.

**Examples:**
- Sprint kickoff (create tasks, assign, validate scope)
- Feature implementation (spec → build → test → integrate)
- Code review (review changes, suggest fixes, validate)
- Release prep (build, test, create release notes)

**When to use:** Repetitive processes that benefit from automation

### Decisions (Architecture & Design)

Captured architectural decisions that inform specs and workflows.

**Format:** ADR-style (Architecture Decision Record)
- **Decision** — what we're deciding
- **Context** — why we need to decide
- **Options Considered** — alternatives + tradeoffs
- **Chosen Option** — what we picked + why
- **Rationale** — reasoning for future reference
- **Implications** — downstream impact

**When to use:** Before specs; lock in design choices upfront

### Processes (End-to-End)

Full process documentation for complex workflows (e.g., "How to implement a new game world").

**Format:** Step-by-step guide with decision points

---

## MontyGame Workflow Phases

### Phase 1: Sprint Planning & Architecture (Automated)
- Use `decision-templates/` to lock in Sprint decisions
- Create specs for each Sprint task (using `specs/SPRINT_TEMPLATE.md`)
- CodeEasy validates architectural consistency

### Phase 2: Implementation (Autonomous)
- `codeeasy_start_spec` → Codex multi-agent build
- Claude validator reviews and suggests improvements
- Automated PRs created for review

### Phase 3: Integration & Testing (Coordinated)
- CodeEasy coordinates test runs
- Automated regression checks
- Merge to main on validation

### Phase 4: Release (Automated)
- Build artifacts
- Generate release notes
- Create GitHub release

---

## Using Specs with CodeEasy

### Create a Spec

```bash
! codeeasy_create_spec
```

**Fill in:**
1. **Title** — concise name (e.g., "Sprint 2: Implement Portal Tile Mechanic")
2. **Description** — problem statement & requirements
3. **Acceptance Criteria** — how to verify it's done
4. **Constraints** — tech limits, dependencies
5. **Context** — background, design docs to reference
6. **Architectural Decisions** — link to decision templates

### Start Autonomous Build

```bash
! codeeasy_start_spec
```

**Codex will:**
1. Analyze the spec
2. Break it into tasks
3. Implement each task (multi-agent)
4. Run tests & validation
5. Create PR for review

### Monitor & Iterate

```bash
! codeeasy_get_spec_status
! codeeasy_get_autonomous_status
! codeeasy_get_messages  # See what Codex is doing
```

### Validate & Fix

Claude validator can suggest improvements:
```bash
! codeeasy_submit_validation
```

---

## MontyGame-Specific Templates

### Sprint Specifications

Each sprint gets a spec that defines all work:
- Sprint 1: Foundation & Prototype
- Sprint 2: Board & Tile System
- Sprint 3: Characters & Animation
- Sprint 4: Game Modes & UI

See `specs/SPRINT_TEMPLATE.md` for the template.

### Architectural Decisions to Lock In

Core decisions that affect implementation:
- `decisions/ARCHITECTURE_PLATFORMER_PHYSICS.md` — Jump curve, collision, gravity
- `decisions/ARCHITECTURE_BOARD_STATE.md` — Tile system, player position tracking
- `decisions/ARCHITECTURE_MULTIPLAYER.md` — Turn order, player management
- `decisions/ARCHITECTURE_UI_FLOW.md` — Menu structure, game states

### Process Templates

End-to-end processes for recurring work:
- `processes/PROCESS_IMPLEMENT_WORLD.md` — How to add World 2 or 3
- `processes/PROCESS_ADD_OBSTACLE_TYPE.md` — How to add a new obstacle
- `processes/PROCESS_CHARACTER_ANIMATION.md` — How to add character animations

---

## Testing the CodeEasy Integration

### Test 1: Create a Spec for Sprint 1

```bash
! codeeasy_create_spec
# Title: "Sprint 1: Foundation & Prototype"
# Use specs/SPRINT_TEMPLATE.md as reference
# Fill in architectural decisions from decisions/ folder
```

### Test 2: Validate Spec Completeness

```bash
! codeeasy_check_validation
# CodeEasy will check:
# - Requirements are clear
# - Acceptance criteria are measurable
# - Architectural decisions are documented
# - No missing dependencies
```

### Test 3: Start Autonomous Build (Dry-Run)

```bash
! codeeasy_get_analysis_config
# See what CodeEasy would do without actually running
```

### Test 4: Run with Limited Scope

Start a small spec first (e.g., "5-Tile Prototype" instead of full Sprint 1):
```bash
! codeeasy_create_spec
# Title: "Test: Build 5-Tile Platformer Prototype"
# Scope: Intentionally small for testing
# Reference: decisions/ARCHITECTURE_PLATFORMER_PHYSICS.md
```

---

## When to Use CodeEasy AutoCode vs. Manual Implementation

### Use CodeEasy AutoCode When:
- ✅ Spec is clear and complete
- ✅ Requirements are well-defined
- ✅ Architectural decisions are locked in
- ✅ Acceptance criteria are measurable
- ✅ Task is medium-to-large (multi-file changes, complex logic)

### Use Manual Implementation When:
- ✅ Spec is still forming (need exploration first)
- ✅ Highly novel or experimental work
- ✅ Single-file, quick fixes
- ✅ Learning/prototyping phase

### For MontyGame:
- **Sprint 1 Prototype:** Manual (explore, test feel)
- **Sprint 2+ Core Features:** CodeEasy (specs are clear, requirements locked)
- **Bug Fixes:** Manual or CodeEasy depending on complexity
- **Refactoring:** CodeEasy (specs are architectural decisions)

---

## Folder Structure Explained

```
codeeasytemplates/
├── README.md (this file)
├── specs/
│   ├── SPRINT_TEMPLATE.md         (Template for any sprint spec)
│   ├── FEATURE_TEMPLATE.md        (Template for features)
│   ├── BUG_TEMPLATE.md            (Template for bugs)
│   ├── REFACTOR_TEMPLATE.md       (Template for refactoring)
│   └── TEST_EXAMPLES/
│       ├── sprint1_prototype.md   (Filled example: Sprint 1 spec)
│       └── sprint2_board.md       (Filled example: Sprint 2 spec)
├── workflows/
│   ├── WORKFLOW_SPRINT_KICKOFF.md (Start of sprint automation)
│   ├── WORKFLOW_FEATURE_BUILD.md  (Feature → implementation → merge)
│   └── WORKFLOW_RELEASE.md        (Build → test → release)
├── decisions/
│   ├── TEMPLATE_ADR.md            (Architecture Decision Record template)
│   ├── ARCHITECTURE_PLATFORMER_PHYSICS.md
│   ├── ARCHITECTURE_BOARD_STATE.md
│   ├── ARCHITECTURE_MULTIPLAYER.md
│   └── ARCHITECTURE_UI_FLOW.md
└── processes/
    ├── PROCESS_IMPLEMENT_WORLD.md
    ├── PROCESS_ADD_OBSTACLE_TYPE.md
    └── PROCESS_CHARACTER_ANIMATION.md
```

---

## Next Steps

1. **Create Architectural Decision Records** (from `decisions/` folder)
   - Lock in core tech choices for platformer, board, multiplayer, UI

2. **Create Sprint 1 Spec** (using `specs/SPRINT_TEMPLATE.md`)
   - Reference the architectural decisions
   - Define acceptance criteria

3. **Test CodeEasy Validation** (dry-run)
   - `codeeasy_check_validation` to ensure spec is complete

4. **Small Test Run** (optional)
   - Create a micro-spec (e.g., "5-Tile Platformer Test")
   - Run `codeeasy_start_spec` to see CodeEasy in action
   - Iterate based on results

5. **Expand to Full Sprint**
   - Once process is proven, scale to full Sprint 1 spec

---

## Resources & Documentation

- **CodeEasy MCP:** `codeeasy_*` MCP tools (see Claude Code docs)
- **MontyGame Design:** `/Docs/GAME_DESIGN_IDEATION.md`
- **MontyGame Roadmap:** `/Planning/SPRINT_ROADMAP.md`
- **World 1 Layout:** `/Docs/WORLD_1_LAYOUT.md`

---

**Last Updated:** 2026-07-06  
**Status:** Template framework ready for testing

---

## JSON templates (machine-consumable — these drive Code Easy directly)

**→ Full index of every JSON template + how to load each: [TEMPLATES_INDEX.md](TEMPLATES_INDEX.md)**


The `.md` files above are human documentation; the **JSON files are the actual
templates** Code Easy consumes:

- `workshops/montygame-core-world1.workshop.json` — the filled MontyGame workshop
  bundle that produced spec #61 (11 requirements, 3 ADRs, build context). Re-usable:
  edit and re-ingest via the dashboard Workshop tab (Import) or
  `codeeasy_workshop_ingest`.
- `workshops/examples/` — the five shipped example bundles (SaaS, internal tool,
  REST API, marketplace, mobile) + index. Same shape; load one, tweak, ingest.
- `specs/json/` — the spec templates (`cli-application`, `express-api`,
  `react-component`, `browser-automation`, `minimal` scaffold) with `phases[].tasks`
  in the `spec_json` shape `codeeasy_create_spec` consumes.
