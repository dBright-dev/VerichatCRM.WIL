# Verichat Presentation Runbook

This file is the speaker/demo guide for the Verichat implementation. It separates the Task 2 assignment rubric and Task 1 design proposal from features that can be demonstrated in the current repository. It is not a claim that every rubric item or proposal feature is complete.

For the code architecture, local setup, environment variables, and test commands, see [README.md](README.md). The source documents supplied for review are:

- `Task 2.txt`: assessment instructions for front end, back end, hosting, GitHub pipelines, and presentation.
- `Nova Team - INSY7315 -Task 1 - Documentation.pdf`: business case, requirements, planned architecture, security, deployment, and DevOps proposal.

## Suggested 8-10 minute presentation

| Time | Section | Evidence to show |
| --- | --- | --- |
| 0:00-1:00 | Business problem and users | Fragmented client, order, inquiry, and support work; explain the admin, sales, procurement/client, and support roles. |
| 1:00-2:00 | Implemented architecture | Show `README.md`'s architecture diagram. Explain the actual .NET API, MVC staff site, Expo app, EF Core, and SQL Server. Clarify that Firebase/Firestore appears in Task 1 as the proposal, not as the current runtime. |
| 2:00-3:15 | Role dashboards | Sign in as a prepared staff account. Show that role cards and recent activity come from API/database records; describe order totals as order value, not recognized or paid revenue. |
| 3:15-4:45 | Sales journey | Show an inquiry/client and a deal in the pipeline. Use the existing quotation workflow to issue a quote for a prepared deal. Explain that issuing the quote is a staff action; the system does not automatically email a PDF. |
| 4:45-6:30 | Mobile client journey | Register a new client account, browse the database-backed catalogue, submit a cart, and show the new client record/order in the staff client directory/profile. |
| 6:30-7:15 | Quote visibility | Show a staff-issued quotation on the client's mobile account, including its line items. Explain that clients can only read their own orders and quotes. |
| 7:15-8:30 | Security and quality | Show role checks, the generic denied-access page, API health endpoint, the xUnit test project, and `.github/workflows/ci.yml`. State the verified local results and the scope of the tests. |
| 8:30-10:00 | Limitations and next steps | Distinguish verified implementation from proposal items and explain the deployment, security-hardening, and feature gaps listed below. |

## Demo preparation

1. Use a development or staging database, never a production database with real client information.
2. Supply the database connection, JWT signing key, administrator bootstrap values, and web/mobile API URL through user secrets or environment settings. Do not put credentials in the presentation, screenshots, GitHub, or either README.
3. Start the API and web app using the commands in the repository README. Start Expo against an API address reachable from the demo device.
4. The API startup seeds example clients and catalogue products only when the corresponding tables are empty. It does not create normal staff demo logins. Provision a staff user using the configured administrator/bootstrap process before presenting.
5. Prepare a client account, product stock, inquiry/deal, and staff-issued quote before the live walkthrough if time is limited. Avoid making a live quote the first time the quotation path is exercised.
6. Run the automated checks before the session. Do not describe CI as a deployment pipeline.

Latest local verification: 11 xUnit tests pass, the .NET solution builds with zero warnings/errors, the API publishes, an idempotent EF migration script generates without a JWT secret, and the mobile TypeScript and lint checks pass. Re-run the checks before presenting if the code changes.

## Evidence against the Task 2 rubric

| Assessment area | Current evidence | Status and qualification |
| --- | --- | --- |
| Front end: visual design, navigation, branding, responsive screens | Branded Razor staff screens, role-aware navigation, Expo mobile tabs and screens, shared theme/styles | Core UI flows are implemented. A formal cross-device visual and accessibility audit is not recorded. |
| Front end: affordances, feedback, accessibility | Buttons and navigation affordances, loading/empty/error states, mobile accessibility labels, and a generic denied-access page | Partially covered. Keyboard/screen-reader and contrast testing still need to be documented and verified across devices. |
| Back end: code structure and business logic | .NET controllers/services, Core domain models, Infrastructure data layer, order validation, inventory updates, inquiry/deal/quote/ticket flows | Core API flows are implemented. Not every proposal workflow is complete. |
| Back end: database and API integration | EF Core SQL Server context, migrations, repositories, authenticated JSON APIs | Implemented for current entities. This is a relational SQL Server design, not the Firestore schema in Task 1. |
| Back end: security | Password hashing, JWT authentication, role policies, client ownership checks, secret configuration removed from current appsettings | Important baseline controls exist. The previously committed signing key must be rotated in hosting and Git history remains; CSP and a full security review are outstanding. |
| Hosting | A hosted API base URL is configured for mobile/web defaults; API applies migrations at startup | Local operation is testable, but production hosting, database connectivity, uptime, and HTTPS/CORS behavior have not been verified as part of this code change. |
| GitHub pipelines | `.github/workflows/ci.yml` runs .NET restore/test/build and mobile lint/typecheck on PRs and main/master pushes | CI is implemented. Automatic deployment, protected-branch rules, required approvals, and GitHub environment secrets are not configured by this workflow. |
| Presentation | This runbook provides a demo sequence and evidence map | Use the actual running apps and avoid presenting planned or unverified features as delivered. |

## Task 1 proposal items that are not fully delivered

The Task 1 PDF is useful for the business requirements, but several implementation decisions and deliverables differ. The repository README is the current source for how the code actually works.

| Proposal item | Current implementation |
| --- | --- |
| Firebase Authentication, Firestore, Cloud Functions, and Firebase Hosting | ASP.NET Core JWT authentication, SQL Server/EF Core, .NET API and MVC web project, and Expo mobile app. The Firebase deployment diagram and cost model do not describe the current stack. |
| Ranked inquiry management and visual deal pipeline | Inquiry scoring/queue and pipeline endpoints/screens are present. Demonstrate from database-backed examples. |
| One-click branded quotation PDF emailed to client | Staff can create a quotation record and a client can view its data/line items. The PDF and email helper classes are placeholders and are not wired into the quote flow. |
| Client reordering and order tracking | Mobile catalogue/cart creates a multi-line order against the signed-in client's account; the staff client profile shows the order. |
| Medical Student account, student discount, and checkout | A partial catalogue screen and role constant exist. Student registration, discount pricing, and a complete role-authorized student checkout flow are not implemented. |
| Complaint submission and SLA lifecycle | Ticket APIs, staff hub, ticket data, and SLA fields are present. Confirm the exact client/mobile ticket submission path before demonstrating this as an end-to-end client journey. |
| Automated 60-day equipment warranty notices | The API can list assets expiring within a supplied day window. Scheduled reminders/notifications are not implemented. |
| Global usage/error/load analytics | `/api/health` reports API health. The dashboards show operational CRM data, not system usage, error rates, or load. |
| Comprehensive security, TLS/CSP, and XSS protections | JWT/RBAC, password hashing, HTTPS redirection, and validation exist. A CSP and a complete XSS/security audit are not verified; do not repeat the proposal's stronger security claims as implemented facts. |
| Gitflow, branch protection, CI/CD deployment | A CI workflow runs checks. Branch protections/review policies are GitHub settings, and the current workflow does not deploy. |

## Before claiming production readiness

- Rotate the JWT key that was previously stored in the repository; provision a new secret through the deployment platform and invalidate old tokens.
- Verify the hosted API, web origin, database connection, migrations, CORS allow-list, and mobile checkout against a staging environment.
- Add a deployment job with environment approvals and secrets only after the actual hosting targets and rollback procedure are confirmed.
- Complete integration/end-to-end tests for registration, stock-safe checkout, client ownership, quotations, ticket creation, and role-denied screens.
- Implement or explicitly remove proposal commitments for quotation PDF/email, student discounts, client complaint submission, warranty reminders, and global system analytics.
- Update the Task 1 architecture, ERD, security, deployment, cost, and CI/CD sections to match the SQL Server/.NET implementation before using that PDF as an as-built technical description.

