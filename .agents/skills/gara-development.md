---
name: gara-development
description: Custom development guidelines for the Gara Startup Project Showcase system. Use when writing backend APIs, designing the frontend, writing SSO auth logic, enforcing business rules, or setting up DevOps pipelines for the Gara project.
---
# Gara Showcase Development Skill

## Purpose
This skill governs the development of the Gara Startup Project Showcase platform. It defines the core architecture, data schemas, business rules, and technical standards required to build a compliant, secure, and performant product.

## 1. Technical Stack & Architecture
- **Frontend**: Single Page Application (SPA) using HTML, CSS (Vanilla CSS, custom properties), and modern JavaScript (ES6+). Responsive layouts are mandatory.
- **Backend**: Node.js with Express.js REST APIs.
- **Database**: PostgreSQL (specifically Azure Database for PostgreSQL Flexible Server as IaC).
- **Authentication**: OAuth2/OIDC delegated entirely to the university SSO. No local credential storage.
- **Hosting & Infrastructure**: Azure Container Apps (ACA) or App Service, Azure Container Registry (ACR), Azure Key Vault (AKV).

## 2. Core Data Schemas

### User Profile (RBAC)
- `id`: UUID (Primary Key)
- `email`: String (Unique, verified via university OIDC)
- `name`: String
- `role`: Enum ('Student', 'Founder', 'Manager', 'Guest')
- `student_id`: String (Nullable, required for Student/Founder)

### Project (Startup)
- `id`: UUID (Primary Key)
- `name`: String (Unique)
- `pitch`: String (Short summary)
- `description`: Text (Markdown rich-text)
- `status`: Enum ('Active', 'At-Risk', 'Suspended')
- `last_updated_at`: Timestamp
- `storage_used_bytes`: BigInt (Max 500MB = 524,288,000 bytes)

### Milestone
- `id`: UUID
- `project_id`: UUID (Foreign Key)
- `title`: String
- `description`: Text
- `done`: Boolean
- `date_completed`: Timestamp (Nullable)
- `created_at`: Timestamp

### Job Listing
- `id`: UUID
- `project_id`: UUID (Foreign Key)
- `title`: String
- `category`: Enum ('Engineering', 'Business', 'Design', 'Marketing')
- `description`: Text
- `requirements`: Text
- `status`: Enum ('Open', 'Closed')

### Application
- `id`: UUID
- `student_id`: UUID (Foreign Key)
- `job_id`: UUID (Foreign Key)
- `status`: Enum ('Pending', 'Approved', 'Rejected')
- `created_at`: Timestamp

## 3. Mandatory Business Rules (BR-XX)

### BR-05 / FR-006: Application Concurrency Limit
- Students can have at most **3 concurrent pending applications**.
- Before saving a new application, query the database:
  `SELECT COUNT(*) FROM applications WHERE student_id = :studentId AND status = 'Pending'`
- If count >= 3, reject with status `400 Bad Request` and error message: `"Bạn đã đạt giới hạn tối đa 3 đơn ứng tuyển đồng thời."`

### BR-08 / FR-007: Dormancy Monitoring & Suspension
- **14-day rule (At-Risk)**: If `last_updated_at` is > 14 days ago, mark project status as `At-Risk`.
- **30-day rule (Suspended)**: If `last_updated_at` is > 30 days ago, mark project status as `Suspended`.
- **Milestone activation**: Adding or updating a milestone resets `last_updated_at` to `NOW()` and automatically restores status to `Active` (if previously suspended or at-risk).
- **Recruitment suspension**: Any job listings belonging to a `Suspended` project are hidden from the public Job Board.

### BR-04: Storage Cap
- Total uploaded file size per project must be checked before saving. If `storage_used_bytes` + `new_file_size` > 500MB, abort upload and return `400 Bad Request`.

## 4. DevOps & Cloud Infrastructure
Follow the standard DevOps reference skills:
- **Infrastructure as Code**: Provision resources using Azure Terraform as specified in [.agents/skills/references/azure-terraform-iac.md](file:///.agents/skills/references/azure-terraform-iac.md).
- **CI/CD Pipeline**: Deploy via GitHub Actions following [.agents/skills/references/github-actions-cicd.md](file:///.agents/skills/references/github-actions-cicd.md) using federated credentials (OIDC).

<HARD-GATE>
Do NOT modify database schemas, implement authentication routes, or commit code changes before a concrete, reviewed step-by-step implementation plan exists in the workspace.
</HARD-GATE>
