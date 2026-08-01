# Plan: Gara Showcase Platform Implementation

## Overview
This plan establishes a step-by-step roadmap for implementing the full Gara Startup Project Showcase system, transitioning from a static presentation mock to a robust, cafeteria-scale production application. 

It implements all requirements defined in the [project-showcase-prd.md](file:///C:/Users/hypha/Documents/Harness/Project%20Showcase%20Pitching/.harness/specs/project-showcase-prd.md) and enforces core business rules (limit of 3 concurrent applications, 14/30 days inactivity cflags, and 500MB storage caps) using a Node.js + Express backend, PostgreSQL database, Azure Terraform IaC, and GitHub Actions CI/CD pipelines.

---

## 1. Architectural Approach

### 💻 Backend Services
- **Framework**: Node.js with Express.js REST APIs.
- **ORM/Query Builder**: Knex.js or direct `pg` pool client for lightweight, SQL-controlled queries.
- **SSO Integration**: Passport.js or `openid-client` library configured to delegate authentication entirely to the university SSO OIDC provider.
- **Dormancy Background Worker**: A cron scheduler (using `node-cron` or Azure functions) running daily to check project inactivity and flag/suspend projects accordingly.

### 🎨 Frontend Presentation
- **Structure**: Single Page Application (SPA) utilizing modular, component-based Vanilla JavaScript.
- **Routing**: History API or hash-based routing (`#/discovery`, `#/jobs`, `#/admin`).
- **Styling**: Vanilla CSS utilizing custom variables, CSS Grid, Flexbox, glassmorphic cards, and micro-interactions.

### 🗄️ Database (PostgreSQL)
- Schema migrations managed via Knex migrations.
- Tables: `users` (RBAC), `projects` (Storage caps), `milestones` (Roadmaps), `jobs` (Recruitment), `applications` (3-limit check).

### ☁️ Infrastructure & DevOps
- **IaC**: Modular Terraform configurations inside `/terraform` (VNet, Azure SQL/PostgreSQL, Azure Container Registry, Azure Container Apps, Azure Key Vault).
- **CI/CD**: GitHub Actions pipeline performing build, test, container scan (Trivy), push to ACR, and deployment to ACA via OIDC federated login.

---

## 2. Implementation Phases

### Phase 1: Environment Setup & Infrastructure IaC
1. Initialize the Node.js project (`package.json`, TypeScript or ES6 config, eslint, prettier).
2. Setup the `/terraform` directory structure:
   - `provider.tf`: Configure Azure provider.
   - `variables.tf`: Define inputs for Dev, Staging, Prod.
   - `main.tf`: Provision VNet, Key Vault, ACR, Container Apps, PostgreSQL Flex.
3. Validate Terraform templates according to [.agents/skills/references/azure-terraform-iac.md](file:///C:/Users/hypha/Documents/Harness/Project%20Showcase%20Pitching/.agents/skills/references/azure-terraform-iac.md).

### Phase 2: Database Schema & Migrations
1. Write database schema migration scripts:
   - Create `users`, `projects`, `milestones`, `jobs`, `applications` tables.
   - Set up correct indices on `applications(student_id, status)` and `projects(last_updated_at)`.
2. Spin up a local Docker PostgreSQL database for local development and run migrations.

### Phase 3: Core Backend REST APIs & Business Rules
1. Implement the Express server structure (`/src/app.js`, `/src/routes/`, `/src/controllers/`, `/src/models/`).
2. Write project CRUD controllers with **BR-04** validation (enforcing the 500MB storage cap).
3. Write job board and recruitment controllers.
4. Implement the recruitment application endpoint with **BR-05 / FR-006** concurrency check:
   ```javascript
   // Reject 4th concurrent pending application
   const pendingCount = await db('applications')
     .where({ student_id: studentId, status: 'Pending' })
     .count();
   if (pendingCount >= 3) {
     return res.status(400).json({ error: 'Bạn đã đạt giới hạn tối đa 3 đơn ứng tuyển đồng thời.' });
   }
   ```
5. Implement the daily background checker for **BR-08 / FR-007** (monitoring 14-day inactivity warnings and 30-day suspensions).
6. Implement CMS milestone update router (resets `last_updated_at` and restores status to `Active`).

### Phase 4: SSO Integration & RBAC Middleware
1. Configure OpenID Connect authentication client routes (`/login`, `/login/callback`, `/logout`).
2. Implement auth middleware to populate `req.user` from JWT identity tokens.
3. Write Role-Based Access Control (RBAC) authorization middleware (`checkRole(['Founder', 'Manager'])`).

### Phase 5: Frontend Migration & API Bindings
1. Port Slide 4's simulator HTML/CSS/JS structures into real application pages.
2. Build an API service module (`src/public/js/api.js`) to handle server-side fetches.
3. Bind SSO Login/Logout UI buttons to backend routes.
4. Integrate the Discovery Feed, Job Board (filtering out suspended project postings), and Admin oversight panel with live backend database endpoints.

### Phase 6: DevSecOps CI/CD Pipelines
1. Configure GitHub Actions workflows in `.github/workflows/`:
   - `build-test.yml`: Run linter, tests, and Trivy security scanning.
   - `deploy.yml`: OIDC login to Azure, Docker build and push to ACR, deploy container revision to ACA.
2. Align configs to [.agents/skills/references/github-actions-cicd.md](file:///C:/Users/hypha/Documents/Harness/Project%20Showcase%20Pitching/.agents/skills/references/github-actions-cicd.md).

### Phase 7: System Verification & Testing
1. Write integration tests for:
   - Application limit enforcement.
   - Inactivity flag cron scheduling.
   - Storage limits.
2. Conduct manual flow checks using the `/hs-verify` skill.

---

## 3. Verification Criteria
- [ ] IaC: Terraform plans execute successfully without replacing active state.
- [ ] SSO: Unauthenticated users are correctly redirected to the SSO mock server.
- [ ] Limit checks: Submitting a 4th application returns a `400 Bad Request` and blocks the write.
- [ ] Health tracking: Background job accurately marks projects inactive on Day 15 and suspended on Day 31.
- [ ] Storage checks: Uploading files exceeding 500MB is rejected.
