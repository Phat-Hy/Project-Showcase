# Standalone Product Migration Checklist (003-standalone-product)

This document tracks the progress of the migration from the pitch deck overlay simulator into a standalone web application product.

## Progress Tracking

| Phase | Task | Status | Details |
|---|---|---|---|
| 1 | **Frontend Decoupling** | COMPLETED | Separate landing pages, dashboard, jobs board, and admin portal. |
| 1 | **Modular CSS/JS** | COMPLETED | Extract inline styles to global/component stylesheets and client modules. |
| 2 | **Cookie-Based Sessions** | COMPLETED | Deliver JWT session tokens via secure, HTTP-only cookies instead of url queries. |
| 3 | **Real Azure Blob Storage** | TODO | Integrate `@azure/storage-blob` Multer stream and track real project files (BR-04). |
| 4 | **IaC Connection Secrets** | TODO | Inject storage connection string variables into Container App env via Terraform. |
| 5 | **Integration Testing** | TODO | Update Jest suite for cookies/multipart uploads and deploy. |
