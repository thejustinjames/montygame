# JSON Templates Index — everything we load into Code Easy

Every machine-consumable template in this folder, and exactly how to load each
one. These are the files the workshop demo uses; the `.md` files elsewhere in
`codeeasytemplates/` are human documentation only.

## Workshop bundles (→ requirements + ADRs + context + a draft spec)

One file = one complete workshop submission. Three ways to load any of them:
- **Dashboard:** Workshop tab → **Import** → pick the file (or **📦 Load an example…** for the shipped ones)
- **MCP:** `codeeasy_workshop_ingest { repo: "MontyGame", submission: <file contents> }`
- **API:** `POST /api/workshop/MontyGame/ingest` with the file as the JSON body

| File | What it is |
|---|---|
| [`workshops/montygame-core-world1.workshop.json`](workshops/montygame-core-world1.workshop.json) | **The MontyGame template** — produced spec #61 (World 1 C# core: board, dice, tile effects, turn engine, CLI simulator, xunit suite). Edit + re-ingest for the next phase (e.g. promote the `should` rows — Characters, Story beats — to `must`). |
| [`workshops/examples/saas-team-app.workshop.json`](workshops/examples/saas-team-app.workshop.json) | Example: multi-tenant SaaS (subscriptions, SSO, GDPR/SOC2) |
| [`workshops/examples/internal-admin-tool.workshop.json`](workshops/examples/internal-admin-tool.workshop.json) | Example: VPN-only Django admin tool (SSO, RBAC, audit log) |
| [`workshops/examples/public-rest-api.workshop.json`](workshops/examples/public-rest-api.workshop.json) | Example: headless public REST API (keys/scopes, rate limits, webhooks) |
| [`workshops/examples/marketplace-two-sided.workshop.json`](workshops/examples/marketplace-two-sided.workshop.json) | Example: two-sided marketplace (Stripe Connect, reviews, KYC) |
| [`workshops/examples/mobile-field-app.workshop.json`](workshops/examples/mobile-field-app.workshop.json) | Example: offline-first cross-platform field app |
| [`workshops/examples/index.json`](workshops/examples/index.json) | Machine index of the examples (id/name/description/file) |

## Spec templates (→ a spec directly, skipping the workshop)

`phases[].tasks` in the `spec_json` shape. Load via:
- **Dashboard:** Agents tab → spec wizard → **Templates**
- **MCP:** `codeeasy_create_spec { repo, title, content, specJson: <template phases> }` then `codeeasy_start_spec`

| File | What it is |
|---|---|
| [`specs/json/cli-application.json`](specs/json/cli-application.json) | CLI tool (TypeScript + Commander), 3 phases / 8 tasks |
| [`specs/json/express-api.json`](specs/json/express-api.json) | REST API (Express + TS + SQLite), 4 phases / 12 tasks |
| [`specs/json/react-component.json`](specs/json/react-component.json) | React component (TS), 2 phases / 5 tasks |
| [`specs/json/browser-automation.json`](specs/json/browser-automation.json) | Playwright automation, 4 phases / 12 tasks |
| [`specs/json/minimal.json`](specs/json/minimal.json) | Blank scaffold (schema/guardrails/context, empty phases) — Code Easy **refuses** to start it until tasks are added, by design |
| [`specs/json/index.json`](specs/json/index.json) | Machine index of the spec templates |

## Related (not JSON, but part of the flow)

- [`processes/WORKSHOP_TO_BUILD_WALKTHROUGH.md`](processes/WORKSHOP_TO_BUILD_WALKTHROUGH.md) — the end-to-end demo script (template → workshop → factory → build), grounded in this repo's real spec #61 run
- `docs/WORKSHOP-CONTEXT.md` (repo root `docs/`) — the build context the MontyGame ingest generated
- Requirements CSV shape used inside every bundle: `type,title,value,rationale,priority,source` with MoSCoW priorities (`must` builds now, `should`/`could` later, `won't` recorded but never built). Blank template: `GET /api/workshop/requirements-template.csv`
