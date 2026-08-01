# Gara Startup Project Showcase

An interactive, responsive presentation slide deck and full platform implementation roadmap for the Gara Startup Project Showcase, built for university incubator ecosystems.

## Directory Structure
- `index.html`: Interactive pitch deck SPA with slides, MVP simulator mock, and presenter notes panel.
- `plan.md`: Master implementation plan for the full-scale platform development.
- `.harness/`: Harness specification folder.
  - `specs/INDEX.md`: Specifications register.
  - `specs/project-showcase-brd.md`: Business Requirements Document.
  - `specs/project-showcase-prd.md`: Product Requirements Document.
  - `specs/001-pitch-deck/`: Pitch deck project logs.
  - `specs/002-platform-development/`: Complete platform development logs, plans, and specs.
- `.agents/`: Local agent configuration directory (skills, rules, specialized agents) for Antigravity runtime.
- `.claude/`: Claude Code runtime configuration directory.

## Run Locally
Double-click `index.html` to run in any browser (Chrome, Opera, etc.), or run a local dev server:
```bash
npx serve .
```

## Verify Harness Maturity
To check the quality and safety score of the project's agentic harness:
```bash
npx harness-score
```
