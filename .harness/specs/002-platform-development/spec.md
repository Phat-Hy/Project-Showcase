# Spec: Gara Showcase Platform Implementation

**Status:** approved

## Goal
Implement the full, cafeteria-scale production release of the Gara Startup Project Showcase system according to the product requirements. The release includes the complete backend server (REST APIs, SSO integration, background dormancy worker), database schemas, frontend SPA client, Azure IaC provisioning using Terraform, and DevSecOps pipelines.

## Requirements & Scope

### 1. Security & Authentication
- **OIDC SSO**: Delegate all authentication to the university identity provider via OpenID Connect (OIDC).
- **RBAC**: Enforce Role-Based Access Control (Guest, Student, Founder, Manager).

### 2. Startup Profiles & Storage Limits
- **CRUD Operations**: Verified founders can create, edit, and publish project profiles.
- **Storage Cap (BR-04)**: Capped at a hard limit of 500MB total storage per project. Uploads exceeding this must be blocked.

### 3. Recruitment & Application Limits
- **Job Board**: Startups can post open roles.
- **Concurrency Limit (BR-05 / FR-006)**: Students are restricted to a maximum of 3 concurrent pending applications. Subsequent applications must be blocked.

### 4. Incubator Oversight & Dormancy Monitoring (BR-08 / FR-007)
- **14-Day Inactivity Warn**: Mark project status as "At-Risk" if no updates are made for 14 days.
- **30-Day Inactivity Warn**: Mark project status as "Suspended" if no updates are made for 30 days.
- **Milestone Re-activation**: Adding or updating a milestone resets the dormancy tracker and restores status to "Active".
- **Job Board Isolation**: Any recruitment listings belonging to a suspended project are immediately hidden from the public Job Board.

### 5. Infrastructure & DevSecOps
- **IaC**: Provision CAF-compliant Azure infrastructure (VNet, App Service/ACA, PostgreSQL Flex, Key Vault, ACR) using Terraform.
- **CI/CD**: Configure GitHub Actions pipelines utilizing OIDC federated login to Azure.
- **Shift-Left Security**: Integrate dependency scanning (Trivy) and static code analysis (CodeQL) into the pipeline.

---

## Acceptance Criteria
- [ ] SSO redirects unauthenticated users and initializes local JWT sessions on callback.
- [ ] Project profile uploads block files that cause total storage to exceed 500MB.
- [ ] Student dashboard rejects the 4th pending application with a clear alert banner.
- [ ] Daily background script changes project status to At-Risk on Day 15 and Suspended on Day 31.
- [ ] Job postings of suspended projects are omitted from public job lists.
- [ ] Terraform files successfully plan and apply to Dev, Staging, and Prod.
- [ ] GitHub Actions builds, tests, runs security scans, and deploys container revisions using OIDC.
