# Gara Platform Standalone Product Specification (003-standalone-product)

This document specifies the requirements for migrating the Gara Startup Project Showcase from a single-page presentation slide deck with a simulator overlay into a standalone, multi-page enterprise web application product.

## 1. Architectural Goals
- **Decoupling**: Separate the presentation pitching deck from the core business product. The pitching deck remains a landing page (`/index.html`), while the platform app is divided into dedicated pages under `/app/` (e.g., `/app/dashboard.html`, `/app/jobs.html`, `/app/admin.html`).
- **Modularity**: Split the massive inline JavaScript and CSS in the old `index.html` into clean, modular CSS stylesheet systems (`global.css`, `components.css`) and ES6 Javascript client-side modules (`auth.js`, `api.js`, `dashboard.js`, `jobs.js`, `admin.js`).
- **Real File Uploads (Azure Blob)**: Replace mock file sizes with a real upload module integrating the `@azure/storage-blob` SDK. File uploads from project founders must save directly into the Azure Storage Account Blob container while maintaining the 500MB project storage cap (BR-04).
- **Secure Cookie Sessions**: Transition OIDC/Mock SSO token exchange from local storage to secure, HTTP-only cookies to prevent cross-site scripting (XSS) session theft.

---

## 2. Functional Requirements

### FR-100: Landing & Navigation Portal
- The public root `/` (or `/index.html`) serves as a clean, premium product landing page showing a marketing introduction, discovery feed of active projects, and a call-to-action to enter the platform.
- A global navigation bar coordinates navigation across pages based on the user's active session role.

### FR-101: Standalone Student Job Board (`/app/jobs.html`)
- A dedicated page for students to browse roles.
- Features search, category filtering (Engineering, Design, Business, Marketing), and detailed requirement cards.
- Provides a clean application submission flow with status indicators (Pending, Approved, Rejected).
- Strictly enforces the 3-pending applications limit (BR-05) and hides jobs of suspended projects (BR-08).

### FR-102: Standalone Founder Dashboard (`/app/dashboard.html`)
- A dedicated page for startup founders to manage their portfolio profiles.
- Features editable pitch statements, a markdown description editor, and milestone checklists.
- Includes a real file upload drag-and-drop area showing current storage usage out of the 500MB cap.
- Updating milestones automatically triggers the inactivity clock reset and activation status restore (BR-08).

### FR-103: Standalone Admin Panel (`/app/admin.html`)
- Restricted to users with the `Manager` role.
- Features statistical summaries of all incubator projects.
- Provides controls to run manual dormancy checks, view inactive warning flags, and export detailed CSV reports.

---

## 3. Non-Functional & Security Constraints
- **Session Security**: Stateless JWTs must be stored in secure, HTTP-only, SameSite cookies.
- **Access Control**: Every backend API endpoint must check roles (`checkRole(['Founder', 'Manager', 'Student'])`) and return proper `403 Forbidden` JSON responses on failure.
- **Latency & Performance**: All Javascript dependencies must load asynchronously. Responsive CSS grids must support screen layouts down to 360px.
