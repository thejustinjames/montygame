# Architecture Decision Record (ADR) Template

**Purpose:** Document key architectural decisions in a structured way so future decisions are informed by context, rationale, and tradeoffs.

**Format:** ADR (Architecture Decision Record) — inspired by ADR 0001 pattern.

---

## ADR-NNNN: [Decision Title]

**Status:** Proposed / Accepted / Deprecated / Superseded

**Date:** [YYYY-MM-DD]

**Affected Components:** [List affected systems: e.g., Board System, Player Controller, UI]

---

## Context

**Why does this decision matter?**

Describe the issue or question that motivated this decision:
- What problem are we solving?
- What's the context (constraints, requirements, business drivers)?
- What options were we considering?
- Why couldn't we defer this decision?

**Example:**
> We need to decide how to store tile state on the World 1 board. The board has 25 tiles, each with position, type, and effects. Multiple players may traverse the same board in multiplayer mode, so state must be consistent. We need to decide between:
> 1. Runtime-only state (in memory, reset on game over)
> 2. Persistent data files (JSON, loaded at game start)
> 3. ScriptableObjects (Unity-native, inspector-editable)

---

## Options Considered

**Option 1: [Name]**
- **Pros:** [List 2–3 specific benefits]
- **Cons:** [List 2–3 specific drawbacks]
- **Effort:** [High / Medium / Low]
- **Scalability:** [Will this hold for future worlds?]

**Option 2: [Name]**
- **Pros:** [...]
- **Cons:** [...]
- **Effort:** [...]
- **Scalability:** [...]

**Option 3: [Name]**
- **Pros:** [...]
- **Cons:** [...]
- **Effort:** [...]
- **Scalability:** [...]

**Example:**
> **Option 1: Runtime-Only State**
> - Pros: Simple, no file I/O, fast
> - Cons: Can't persist progress, no inspector visibility, hard to debug
> - Effort: Low
> - Scalability: Won't scale if we add save/load feature later
>
> **Option 2: JSON Data Files**
> - Pros: Human-readable, portable, easy to hand-edit
> - Cons: File I/O overhead, need custom parser, error-prone
> - Effort: Medium
> - Scalability: Good for multiple worlds
>
> **Option 3: ScriptableObjects**
> - Pros: Native to Unity, inspector-editable, automatic serialization
> - Cons: Binary files (can't hand-edit), Unity-only, version conflicts
> - Effort: Low
> - Scalability: Excellent for future expansion

---

## Decision

**We chose: [Option Name] because [brief reason]**

---

## Rationale

**Why this option?**

Explain the reasoning:
- Which pros were most important?
- Which cons were acceptable?
- How does this align with project goals?
- What made this better than alternatives?

**Example:**
> We chose **Option 3: ScriptableObjects** because:
>
> 1. **Best alignment with Unity:** We're using Unity as our engine; leveraging its native serialization reduces complexity and maintenance burden.
> 2. **Inspector visibility:** Game designers can preview and debug tile layouts without writing code or parsing files.
> 3. **Low effort:** Setup is straightforward; ScriptableObjects are a standard Unity pattern.
> 4. **Scalability:** We can easily add new worlds by creating new ScriptableObject instances. Multi-world expansion becomes trivial.
> 5. **Future-proof:** If we add save/load later, we can layer persistent data on top of ScriptableObjects without refactoring the core system.
>
> We accepted the con (binary format) because our workflow doesn't require hand-editing tile data; the Inspector provides a better UX for configuration.

---

## Implications

**What changes because of this decision?**

- **Code structure:** [How does this affect file organization, class structure, etc.?]
- **Testing:** [How do we test this? Unit test strategy?]
- **Performance:** [Any perf implications?]
- **Future decisions:** [Does this constrain or inform future choices?]
- **Documentation:** [What must be documented for the team?]

**Example:**
> **Implications:**
>
> - **Code structure:** We'll create a `TileDatabase.cs` that loads ScriptableObject tile data and exposes it to the Board Manager. Each world gets a separate folder: `Assets/Worlds/World1/`, `Assets/Worlds/World2/`, etc.
> - **Testing:** We'll unit test TileDatabase; mock ScriptableObject data for tests that don't need the full editor.
> - **Performance:** ScriptableObjects are pre-serialized, so load time is instant. No runtime parsing overhead.
> - **Future decisions:** If we add a tile editor, we can build a custom inspector tool on top of this structure. If we add multiplayer syncing, ScriptableObjects remain the source of truth.
> - **Documentation:** Designers need a guide: "How to add a new world to the board system" (template in `/codeeasytemplates/processes/`).

---

## Consequences

**Good Outcomes (if this works as planned):**
- [ ] [Benefit 1]
- [ ] [Benefit 2]
- [ ] [Benefit 3]

**Potential Risks (if assumptions prove wrong):**
- [ ] [Risk 1]
- [ ] [Risk 2]

**How we'll measure success:**
- [ ] [Metric 1]
- [ ] [Metric 2]

**Example:**
> **Good Outcomes:**
> - [ ] Tile configuration is quick and visual (Inspector-based)
> - [ ] Adding new worlds takes 30 minutes, not 3 hours
> - [ ] No runtime parsing errors; data is type-safe
>
> **Potential Risks:**
> - [ ] Team unfamiliar with ScriptableObjects (mitigation: create tutorial)
> - [ ] Version control conflicts on ScriptableObject files (mitigation: one asset per world, not shared)
>
> **Success Metrics:**
> - [ ] All 25 World 1 tiles load without errors
> - [ ] Inspector shows all tile data correctly
> - [ ] World 2 implementation takes <1 hour (vs. 3+ hours for world 1)

---

## Related Decisions

**Links to related ADRs or decisions:**
- [ ] [ADR-0001: Board Architecture](ARCHITECTURE_BOARD_STATE.md) — related, but not dependent
- [ ] [ADR-0002: Multiplayer State](ARCHITECTURE_MULTIPLAYER.md) — depends on this decision
- [ ] [ADR-0003: Performance Optimization](../decisions/ARCHITECTURE_PLATFORMER_PHYSICS.md) — informs future perf work

---

## Revision History

| Version | Date | Author | Status | Notes |
| --- | --- | --- | --- | --- |
| 1.0 | 2026-07-06 | Claude | Proposed | Initial draft for team review |
| 1.1 | [Date] | [Author] | Accepted | [Changes made] |

---

## References & Examples

**Where this decision is implemented:**
- `Assets/Worlds/World1/TileDatabase.cs` (or similar path)
- `Assets/Data/World1/Tiles_ScriptableObject.asset`

**Example code pattern:**
```csharp
[CreateAssetMenu(fileName = "TileDatabase", menuName = "MontyGame/Tile Database")]
public class TileDatabase : ScriptableObject {
    [SerializeField] public List<TileConfig> tiles;
    // Each tile has: id, type, position, effects
}
```

**Learning resources:**
- Unity ScriptableObject documentation
- [Link to team wiki or guide if it exists]

---

**Template Version:** 1.0  
**Last Updated:** 2026-07-06  
**For Questions:** Reference `/codeeasytemplates/decisions/` folder for filled examples.
