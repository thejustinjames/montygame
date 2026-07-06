# CodeEasy Workflow: Sprint Execution with Architectural Decisions

**Purpose:** End-to-end workflow for executing a sprint, from design decisions through autonomous build to validation.

**Applies to:** Sprint 1, 2, 3, 4 (any major sprint)

---

## Workflow Phases

### Phase 1: Preparation (Days 1–2)

**Goal:** Lock in architectural decisions and create spec for autonomous build

**Steps:**

1. **Review Sprint Goals**
   - Consult `/Planning/SPRINT_ROADMAP.md` for sprint focus
   - Identify which architectural decisions are needed
   - List unknowns or open questions

2. **Create/Update Architectural Decisions**
   - Use `/codeeasytemplates/decisions/TEMPLATE_ADR.md` as template
   - Reference existing ADRs (ADR-001 Platformer Physics, ADR-002 Board State, ADR-003 Multiplayer)
   - Decide on new ADRs for this sprint
   - Get stakeholder approval (if needed)

   **Example (Sprint 2):**
   - ADR-002: Board State (already locked in)
   - ADR-004: Tile Prefab System (new for this sprint)
   - ADR-005: Asset Pipeline (how sprites are loaded)

3. **Create Sprint Spec**
   - Use `/codeeasytemplates/specs/SPRINT_TEMPLATE.md`
   - Fill in:
     - Sprint goals & requirements
     - Acceptance criteria
     - Constraints & dependencies
     - Link all architectural decisions
   - Reference `/Docs/WORLD_1_LAYOUT.md`, `/Docs/GAME_DESIGN_IDEATION.md` for context

   **Example (Sprint 2 Spec):**
   ```
   Title: "Sprint 2: Board & Tile System"
   Requirements: All 25 tiles, all tile types, board navigation working
   Dependencies: Sprint 1 core mechanics complete
   Decisions:
     - ADR-002: Board State & Tile System (tiles as ScriptableObjects)
     - ADR-004: Tile Prefab System (generic prefabs + configs)
   ```

4. **Validation Checklist**
   - [ ] Sprint goals are clear
   - [ ] Acceptance criteria are measurable
   - [ ] All architectural decisions are documented
   - [ ] Dependencies are listed and available
   - [ ] No blockers or unknowns
   - [ ] Spec is ready for autonomous build

---

### Phase 2: Spec Creation & Validation (Day 2–3)

**Goal:** Create spec in CodeEasy, validate completeness, get approval

**Steps:**

1. **Register Project with CodeEasy** (if not done)
   ```bash
   ! codeeasy_register_repo
   # Point to: /Users/justin/Documents/GitHub/MontyGame
   ```

2. **Create Spec in CodeEasy**
   ```bash
   ! codeeasy_create_spec
   
   # Fill in (from sprint prep):
   # - Title: "Sprint 2: Board & Tile System"
   # - Description: Requirements + acceptance criteria
   # - Context: Link to WORLD_1_LAYOUT.md, SPRINT_ROADMAP.md
   # - Architectural Decisions: ADR-002, ADR-004, etc.
   ```

3. **Validate Spec Completeness**
   ```bash
   ! codeeasy_check_validation
   
   # CodeEasy will verify:
   # - Requirements are clear
   # - Acceptance criteria are measurable
   # - Architectural decisions are referenced
   # - Dependencies are identified
   # - No missing information
   ```

4. **Fix Any Validation Issues**
   - If validation fails, update spec based on feedback
   - Re-run validation until passing

5. **Get Stakeholder Sign-Off**
   - Share spec with team
   - Get approval: "Yes, this is what we're building"
   - Mark spec as "Approved"

---

### Phase 3: Autonomous Build Execution (Days 3–8)

**Goal:** CodeEasy (Codex + Claude) builds the feature, validates it

**Steps:**

1. **Start Autonomous Build**
   ```bash
   ! codeeasy_start_spec
   
   # Codex (multi-agent implementer) will:
   # - Parse spec
   # - Break into tasks
   # - Create PRs with implementation
   # - Run tests & validation
   ```

2. **Monitor Progress**
   ```bash
   ! codeeasy_get_spec_status
   # Returns: Current phase (design, implementation, testing, etc.)
   
   ! codeeasy_get_autonomous_status
   # Returns: Which tasks are in progress, completed, blocked
   
   ! codeeasy_get_messages
   # Returns: What Codex is doing (detailed log)
   ```

3. **Intervene If Blocked**
   - If Codex gets stuck, send clarification
   ```bash
   ! codeeasy_send_message
   # Example: "ADR-002 specifies ScriptableObjects; use that for tile config"
   ```

4. **Review Generated PRs**
   - Once Codex creates PRs, review code quality
   - Check if it aligns with architectural decisions
   - Comment on PRs if needed

5. **Validation Phase**
   - Claude validator reviews code
   - Suggests improvements if needed
   ```bash
   ! codeeasy_submit_validation
   # Reports: Code quality, test coverage, adherence to spec
   ```

---

### Phase 4: Testing & Integration (Days 8–9)

**Goal:** Verify feature is correct, integrate into main branch

**Steps:**

1. **Automated Testing**
   ```bash
   ! codeeasy_get_build_completions
   # Check: Unit tests pass, integration tests pass, no crashes
   ```

2. **Manual Playtesting** (if applicable)
   - If it's a gameplay feature, playtest with target age group
   - Verify it feels right (fun, forgiving, educational)

3. **Code Review**
   - Final review of merged PRs
   - Ensure code follows conventions from `CLAUDE.md`
   - Verify architectural decisions are respected

4. **Merge to Main**
   ```bash
   ! git merge [PR-branch]
   ! git push origin main
   ```

5. **Document Completion**
   - Update `Planning/TODO.md` (mark sprint tasks complete)
   - Update `CLAUDE.md` if new conventions emerged

---

### Phase 5: Sprint Retrospective (Day 10)

**Goal:** Reflect on sprint, capture learnings, plan improvements

**Steps:**

1. **Update ADRs** (if learnings emerged)
   - Did architectural decisions hold up?
   - Any needed revisions?
   - Document lessons learned

2. **Retrospective Questions**
   - What went well?
   - What was harder than expected?
   - What do we adjust for next sprint?
   - Any new architectural patterns to codify?

3. **Prep for Next Sprint**
   - Update `/Planning/SPRINT_ROADMAP.md` with learnings
   - Identify architectural decisions for next sprint
   - Begin Phase 1 prep for Sprint N+1

---

## Example: Sprint 2 Full Flow

### Sprint 2: Board & Tile System

**Phase 1 (Days 1–2): Preparation**
- Review goals: "Implement 25-tile board with all tile types"
- Create ADR-004: Tile Prefab System
- Create Sprint 2 Spec (10 requirements, 3 phases)
- Verify all dependencies (Sprint 1 complete? ✓)
- Spec is ready

**Phase 2 (Days 2–3): Spec Validation**
- Create spec in CodeEasy
- Validation passes: ✓ Clear requirements, ✓ Measurable criteria, ✓ ADRs linked
- Team approves: "Yes, let's build this"

**Phase 3 (Days 3–8): Autonomous Build**
- Start Codex build
- Day 4: "Codex created tile prefabs, board manager"
- Day 5: "All tile types implemented, tests passing"
- Day 6: "Claude validator reviewed code, 2 suggestions"
- Day 7: "PRs merged, board system complete"

**Phase 4 (Days 8–9): Testing & Integration**
- All unit tests pass: ✓
- Integration test pass: ✓
- Manual playtest: Board feels natural, tile effects work
- Merge to main: ✓

**Phase 5 (Day 10): Retrospective**
- ADRs held up: ✓
- Codex was 80% effective; manual fixes needed on tile ordering
- Next sprint: Use more detailed tile position specs
- Sprint 2 complete: ✓

---

## Decision Points in Workflow

### Should We Use CodeEasy AutoCode?

**Use CodeEasy when:**
- ✅ Spec is clear & requirements are locked
- ✅ Architectural decisions are documented
- ✅ Feature is medium-to-large (multi-file, complex logic)
- ✅ Acceptance criteria are measurable

**Use Manual Implementation when:**
- ✅ Spec is still forming; need exploration first
- ✅ Highly novel or experimental work
- ✅ Single-file changes or quick fixes
- ✅ Learning/prototyping phase (like Sprint 1)

### For MontyGame Specifically:

| Sprint | Use CodeEasy? | Reason |
| --- | --- | --- |
| Sprint 1 | No (Manual) | Prototype; need to find right platformer feel; experimental |
| Sprint 2 | Yes | Board system is clear; ADR-002 locked in; medium complexity |
| Sprint 3 | Partial | Character sprites are creative work (manual); animation logic (CodeEasy) |
| Sprint 4 | Yes | Game modes are well-specified; multiplayer is clear |

---

## Checklist for Each Sprint

**Before Starting Sprint:**
- [ ] Sprint goals documented in `/Planning/SPRINT_ROADMAP.md`
- [ ] All architectural decisions drafted or linked
- [ ] Spec template filled in (goals, requirements, acceptance criteria)
- [ ] Dependencies verified (prior sprint complete?)
- [ ] No unknowns or blockers

**During Sprint (CodeEasy):**
- [ ] Spec created in CodeEasy
- [ ] Validation passes
- [ ] Autonomous build started
- [ ] Progress monitored (daily check)
- [ ] Blockers resolved immediately

**Sprint Completion:**
- [ ] All tests pass
- [ ] Code reviewed & merged
- [ ] `Planning/TODO.md` updated
- [ ] Retrospective captured
- [ ] ADRs updated with learnings

---

## Tools & Integration

**CodeEasy MCP Commands:**
```bash
codeeasy_register_repo              # One-time setup
codeeasy_create_spec                # Create sprint spec
codeeasy_check_validation           # Validate before build
codeeasy_start_spec                 # Launch autonomous build
codeeasy_get_spec_status            # Check overall status
codeeasy_get_autonomous_status      # Detailed progress
codeeasy_get_messages               # See what Codex is doing
codeeasy_send_message               # Send clarification to Codex
codeeasy_submit_validation          # Final validation after build
codeeasy_get_build_completions      # See test results
```

**Related Files:**
- `/codeeasytemplates/decisions/` — ADR templates & filled examples
- `/codeeasytemplates/specs/` — Sprint spec templates
- `/Planning/SPRINT_ROADMAP.md` — Sprint timeline & dependencies
- `/Planning/TODO.md` — Task tracking

---

## Troubleshooting

**Q: Codex build failed or got stuck**
- A: Check `codeeasy_get_messages` for logs
- Send clarification via `codeeasy_send_message`
- If still stuck, check if spec is too ambiguous or ADR is missing

**Q: Code generated doesn't match architectural decision**
- A: Likely spec didn't clearly reference the ADR
- Update spec, restart build

**Q: Test failures during build**
- A: Codex will iterate; monitor progress
- If tests consistently fail, may indicate spec is incomplete

**Q: How long does a sprint take?**
- A: Preparation (2 days) + Autonomous Build (5–7 days) + Testing (1–2 days) = 8–11 days per sprint

---

**Workflow Version:** 1.0  
**Last Updated:** 2026-07-06  
**For Questions:** See `README.md` in `codeeasytemplates/` folder
