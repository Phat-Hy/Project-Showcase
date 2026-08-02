# Standalone Product Migration Plan (003-standalone-product)

This migration plan outlines the detailed steps to transition the Gara Startup Project Showcase platform from an interactive prototype simulator into a complete, secure, production-ready enterprise standalone application.

---

## 1. Migration Phases

### Phase 1: Frontend Decoupling & Static Structure (COMPLETED)
- Split slide presentation into a clean SPA `/index.html` landing page.
- Create `/src/public/app/` containing role-based standalone pages:
  - `login.html`: Dedicated login gateway and mock authentication selector.
  - `student.html`: Student discovery feed, jobs board, and personal profiles.
  - `founder.html`: Founder project profile management and roadmap milestones.
  - `admin.html`: Incubator manager dashboard metrics.

### Phase 2: Modular Javascript client & Cookie Auth (COMPLETED)
- Decouple credentials by transmitting session JWT payloads in secure, HTTP-only cookie headers (`res.cookie('token')`).
- Query PostgreSQL database on `/api/auth/me` to sync real-time student profile metrics.
- Enforce profile completion checks (contact link and CV URL) to authorize applications (BR-04).

### Phase 3: Azure Blob Storage Integration (Real Uploads)
- Add `@azure/storage-blob` and `multer` dependencies to `package.json`.
- Create `/src/services/blobService.js` to manage stream uploads to Azure Blob Container `"media"`.
- Update `POST /api/projects/:id/upload` to receive actual multipart file buffers, evaluate file size against the **500MB** storage quota limit, stream to Azure storage, and update database `storage_used_bytes` records.
- Replace simulator inputs in `founder.html` with real HTML file selection selectors.

### Phase 4: Infra IaC (Terraform) & Secret Updates
- Update `terraform/main.tf` to define a Container App secret for `AZURE_STORAGE_CONNECTION_STRING`.
- Inject the connection string secret directly into the environment parameters of the Container App container template.

### Phase 5: Real OIDC University SSO & CSV Analytics
- Integrate OpenID Connect authentication to replace the mock SSO chooser page.
- Build CSV performance report exporter endpoint (`GET /api/admin/reports/csv`) for managers.
- Push changes to trigger GitHub Actions CI/CD to verify Jest tests and deploy.
