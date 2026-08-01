# Agent Context (AGENTS.md)

Welcome! This document provides orientation for any AI coding assistant collaborating on the **Gara Startup Project Showcase** repository.

## Project Overview
Gara is a university-scale startup portfolio and recruitment showcase. It allows student founders to document milestones and post roles, and connects cross-disciplinary students (SE, GD, Biz) to form teams under university incubator supervision.

## Directory Map
- `index.html`: The core presentation mockup and interactive MVP simulator.
- `plan.md`: Master platform implementation plan.
- `.harness/specs/`: Requirements specifications (BRD, PRD, and phase progress files).
- `.agents/`: Antigravity environment configuration (custom rules, agents, skills).
- `.claude/`: Claude Code environment configuration.

## Development & Build Commands
- **Local Dev Server**: Use `npx serve .` to run a local static file server.
- **Harness Verification**: Run `npx harness-score` to assess the safety and quality parameters of the workspace harness.
- **Testing**: Tests will be run via `npm test` once backend services are initialized.

## Architectural Conventions
1. **Separation of Concerns**: Keep business rules strictly enforced on the server APIs (e.g. 3 pending application limit, 500MB storage caps).
2. **Vanilla Design**: Use custom CSS properties, flexbox/grid layout, and vanilla ES6+ JS for frontend DOM operations. Avoid adding bulky UI libraries unless approved.
3. **Responsive first**: Design all interfaces to scale gracefully down to 360px viewport widths and support multiple desktop resolutions.

## Active Rules & Skills
- **Gara Development Skill**: Located in [.agents/skills/gara-development.md](file:///C:/Users/hypha/Documents/Harness/Project%20Showcase%20Pitching/.agents/skills/gara-development.md) and [.claude/skills/gara-development/SKILL.md](file:///C:/Users/hypha/Documents/Harness/Project%20Showcase%20Pitching/.claude/skills/gara-development/SKILL.md). Consult it before writing any code.
- **DevOps Skill**: Refer to [.agents/skills/devops.md](file:///C:/Users/hypha/Documents/Harness/Project%20Showcase%20Pitching/.agents/skills/devops.md) for Azure Terraform and GitHub Actions deploy guidelines.
