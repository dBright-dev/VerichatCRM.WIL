# Verichat CRM & Medical Logistics System
## Proposal vs. Implementation — Gap Analysis and Traceability Report

**Prepared:** 30 October 2026
**Repository audited:** `D:\Documents\Documents\GitHub\VerichatCRM.WIL` (git HEAD `c27ff7c`, 169 commits, branch `dev2`)
**Upstream:** https://github.com/dBright-dev/VerichatCRM.WIL.git

---

## 1. Purpose and method

This document compares the two **Task 1 planning deliverables** produced by NOVA Team in August 2026 against what was **actually built** in the repository today. It goes section by section through both documents — requirements, roles, user stories, UX journeys, the Work Breakdown Structure, non-functional requirements, the domain and design models, the sequence and state diagrams, the deployment plan, the ERD/JSON schema, the design and architecture patterns, the cloud topology, the security strategy, the Gitflow and CI/CD design, the cost model, and the change-management plan — and records for each item whether it was **implemented, partially implemented, replaced with a different approach, or not built at all**.

A second pass records the reverse direction: what was built that was **never proposed**, and defects found while reading the code.

### 1.1 Sources read

| Source | Content | Pages |
| --- | --- | --- |
| `WIL Task 1 - Documentation.pdf` | The formal Task 1 portfolio: introduction, requirements, roles, user stories, journeys, WBS, NFRs, analysis artefacts, data schema, architecture, security, DevOps, costs, change management, annexures | 51 |
| `Prototype Documentation.pdf` | The Task 1 prototype: architectural navigation & interface map, 15 desktop/web screens, 5 mobile screens, responsive design rules, colour palette, per-screen information flows | 35 |
| `EXPO station layout and posters.pdf` | SOCSWIL EXPO 2026 exhibit logistics, 5–10 minute presentation format, judging flow, station/visitor flow, display-board content plan | 51 |
| The repository itself | All `.cs`, `.cshtml`, `.razor`, `.ts`, `.tsx`, `.csproj`, `.json`, migration and config files; git history and branches | — |

### 1.2 Rating legend

| Code | Meaning |
| --- | --- |
| **[Built]** | Present, wired end-to-end, usable by a real user |
| **[Partial]** | Present but incomplete, mocked, disabled, or only working on one client |
| **[Replaced]** | Delivered, but with a materially different technology or design than proposed |
| **[Not built]** | Absent from the codebase |
| **[Unplanned]** | Built, but never appeared in either proposal |

---

## 2. Executive verdict

The single most important finding is that **the implementation abandoned the entire proposed technology stack and replaced it with a different one**, while keeping the *business* scope broadly intact and then extending it.

The Task 1 documents specified a **serverless Firebase application**: a React SPA on Firebase Hosting, Firebase Authentication, Cloud Firestore as the system of record, and Cloud Functions as the API layer, deployed to three Firebase projects through a Gitflow + CI/CD pipeline on Azure/GitHub Actions.

The repository contains a **conventional .NET 10 three-tier application**: an ASP.NET Core Web API with JWT bearer authentication, Entity Framework Core over SQL Server, a Razor MVC staff front end, and an Expo React Native client — hosted on `runasp.net` via Web Deploy, with **no CI/CD pipeline running today** (two workflow files were written and then deleted; see §11.2).

The repository's own README is candid about this:

> "The Task 1 proposal's Firebase Authentication, Firestore, Firebase Hosting, and Cloud Functions are not used by this implementation."
> — [README.md](file:///D:/Documents/Documents/GitHub/VerichatCRM.WIL/README.md)

Nothing in the shipped code references Firebase — a repo-wide search for `firebase` across all `.cs`, `.ts`, `.tsx`, `.json` and `.csproj` files returns **zero matches**. The migration away was complete, not partial.

### 2.1 Scorecard against the proposal

| Proposal area | Verdict | Notes |
| --- | --- | --- |
| Client / account management | **[Built]** + extended | Unified profile, key personnel, timeline, rep assignment, client map |
| Ranked inquiry scoring | **[Built]** — Computed, overridable | `InquiryScoring` derives the score from sector, contract status, estimated value and urgency when the rep does not type one; a typed score wins and is never overwritten (§8) |
| Visual sales pipeline + forecasting | **[Built]** | Kanban with drag-and-drop, weighted forecast, monthly goal widget |
| One-click PDF quotation | **[Partial]** | PDF generation is fully built server-side but **the button is disabled in the staff UI** |
| E-commerce / self-service portal | **[Built]** (no payments) | Catalogue, cart, multi-line checkout, stock deduction, reordering |
| Complaint & issue hub | **[Built]** + extended | SLA deadlines, auto-escalation background service, activity timeline, photos |
| Equipment lifecycle & 60-day alerts | **[Built]** | Warranty/calibration expiry query, service logs |
| Quick-Cart for medical students | **[Not built]** | No student role path, no discount engine |
| Digital Health / Telemedicine / RPM | **[Partial]** | Mobile screens exist; no video calling, no LIMS/HMS integration |
| Bluetooth device syncing | **[Not built]** | Zero references; vitals are typed in manually |
| Secure checkout / digital invoicing | **[Not built]** | No payment gateway, no invoice entity |
| Proposed tech stack (Firebase) | **[Replaced]** | .NET 10 + EF Core + SQL Server + JWT |
| Layered N-tier architecture | **[Built]** | Faithfully followed — the one architecture idea that survived intact |
| Observer pattern (stock warnings) | **[Replaced]** | Firestore listeners → SignalR + polling warehouse-alert table |
| Facade pattern (quotation generator) | **[Replaced]** | Cloud Function → `QuotationService` + `IPdfGenerator` |
| Cloud topology / 3 Firebase projects | **[Not built]** | Two environments only (Development + base config) |
| ERD (9 Firestore collections) | **[Built]** — as SQL | All 9 concepts exist as tables, plus 18 more |
| Gitflow branching | **[Not followed]** | Ad-hoc per-person/per-layer branches |
| GitHub Actions CI/CD | **[Built, then deleted]** | A working `ci.yml` and a scaffold `android-build.yml` were both removed in `e029d66`; `.github/workflows/` is now **empty** while the README still documents `ci.yml` |
| Security strategy (OWASP, CSP, TLS 1.3) | **[Partial]** | RBAC and sanitiser packages exist; sanitiser never runs; TLS 1.3 not enforced |
| Cost model (ZAR Firebase tiers) | **[Replaced]** | Priced a stack that was not used; §13 now re-costs the Monster ASP plans actually in use |
| Change management / UAT / training | **[Partial]** | Documented in Task 1; **no user guide was ever produced** |

---

## 3. Scope comparison

### 3.1 In-scope functional deliverables (Task 1 §1.3)

| Proposed deliverable | Status | Where it landed |
| --- | --- | --- |
| Smart Customer Management — unified profiles, key-personnel directories | **[Built]** | `VerichatCRM.Core/Entities/Client.cs` (`Client` + owned `KeyPersonnel`), `Controllers/ClientsController.cs`, `Views/Clients/Index.cshtml`, `Views/Clients/Profile.cshtml`, `ClientTimelineNote` |
| Ranked Inquiry Scoring — dynamic ranking on urgency, deal size, sector via business rules | **[Built]** (was **[Partial]**) | `Core/Models/Inquiry/InquiryScoring.cs` now derives the score from four weighted factors — sector rank (25), contract status (20), estimated deal value (35) and stated urgency (20) — clamped to 0–100, with `MidpointRounding.AwayFromZero` so the band boundaries behave. `InquiryService.CreateAsync` reads the client's `Sector`/`ContractStatus`, and when no value is typed it falls back to the most recent `Deal.Revenue` for that client. `Views/Inquiry/Form.cshtml` still accepts a hand-typed score, and a typed score wins: that is the deliberate "manually ranked triage" override the requirement was restated to allow. Sorting by score is unchanged (`InquiryService.cs`). No AI, and no learning from outcomes. |
| Visual Sales Management — pipeline board + revenue forecasting | **[Built]** | `Views/Pipeline/Index.cshtml` (HTML5 drag-and-drop Kanban with a per-card `<select>` fallback and optimistic-move rollback), `PipelineStages.cs`, weighted forecast in `PipelineApiService` |
| Quotation Tools — one-click itemised PDF | **[Partial]** | Server side is complete: `Infrastructure/Data/External/PdfGenerator.cs` renders a branded A4 QuestPDF (navy `#0A1F44` / cyan `#00B4D8`, bill-to block, line-item table, VAT totals, terms, footer) exposed at `GET /api/Quotation/{id}/pdf`. **The staff UI never calls it** — `Views/Quotation/Index.cshtml:107` renders the button `disabled` with `title="Enabled once the backend workstream is wired up"`, and no view invokes the existing `QuotationController.Save` action. |
| E-Commerce & Specialised Portals — client self-service portal + Quick-Cart | **[Partial]** | Self-service portal: **[built]** in the Expo app (`medicalCatalogueShop.tsx`, `cartScreen.tsx`, `clientHomescreen.tsx`, `MobileAccountController`). Quick-Cart for students: **missing** — no discount logic anywhere (`discount` matches 0 files) and `Medical Student` appears in no authorisation gate. |
| Complaint & Issue Hub — tracking numbers, SLA routing, resolution roadmap | **[Built]** | `Entities/Ticket.cs`, `TicketPriorities.GetSlaHours()` (Critical 4h / High 12h / Medium 24h / other 48h), `Services/TicketService.cs`, `BackgroundService SlaEscalationService` (5-minute sweep), `Views/Support/TicketHub.cshtml`, `TicketDetail.cshtml` |

### 3.2 Out-of-scope statements

Task 1 declared accounting/payroll/tax integration out of scope — correctly honoured; there is no accounting integration.

It also declared that **"Back-end coding is excluded from the initial Task 1 Prototype, which focuses strictly on design, navigation, and info-flow using dummy data."** That was true of Task 1. The delivered system is now a full back end, so this statement is superseded rather than violated.

**Residual dummy data.** The proposal's dummy-data posture left traces that were never cleaned up, and some of them are actively misleading in a production build:

| Location | Fake content |
| --- | --- |
| `Views/Admin/Rules.cshtml` | A static hand-typed table of four "security rules" with no model and no API — the UI pretends a rules engine exists |
| `Views/Home/Index.cshtml` | Was: hard-coded hero stats ("15+ Catalog Items", "12 Active Deals") and marketing copy that still said **"JWT authentication and Firestore rules"** — a Firebase-era sentence in a Firebase-free app. Both are gone: the public landing page was rebuilt on 2026-10-31 (own `_LandingLayout`, no invented KPIs, no Firebase reference). What remains static by design is the copy and the four-item showcase in `ViewModels/Home/LandingContent.cs`, which is hand-written because the page must render for anonymous visitors with the API or database unreachable — so the showcase names track the seeded catalogue, not a live one |
| `AnalyticsController.DemoCharts()` / `SalesController.DemoMapData()` | Hard-coded May–Oct figures and six demo clients, gated behind `?demo=true` in Development |
| `Infrastructure/Data/DataSeeder.cs` | Inserts 4 demo clients and 6 demo products on every fresh database — **directly contradicting** the comments in `Program.cs`, `DataSeeder` and `AdminBootstrapper`, and the `//Bootstrap` note in `appsettings.json`, all of which assert the application "creates NO demo data" |
| `Views/Procurement/LogisticsTracker.cshtml` | A decorative "Live Map View" box with three static pins and non-functional Track / View Details buttons. This view was orphaned — no controller action returned it — and is now deleted (§18 defect 29). The live delivery screens are `Logistics/Tracker.cshtml` and `Logistics/Assets.cshtml` |

---

## 4. Requirements comparison (Task 1 §2.1)

| # | Proposed requirement | Delivered | Evidence |
| --- | --- | --- | --- |
| 2.1.1 | Client-centric profile & account management — unified profiles, contract statuses, contact networks | **[Built]** | `Client.ContractStatus`, `KeyPersonnel` owned collection, `ClientTimelineNote`, `GET /api/Client/{clientId}/timeline` merging tickets + activities + inquiries + notes + appointments + quotations |
| 2.1.2 | Omnichannel online sales & self-service e-commerce — direct online transactions, **secure checkout**, **immediate digital invoicing** | **[Partial]** | Checkout and stock validation are real (`OrderService` validates quantities and per-SKU stock, deducts `Product.StockQty`, prices from the database not the client). **There is no payment processing and no invoice**: searches for `stripe`, `paypal`, `payfast`, `Paystack`, `invoice` all return 0 files. An order is created, not paid for. |
| 2.1.3 | Dynamic ranked inquiry scoring — "automatically evaluating incoming web/email submissions against client priority benchmarks rather than simple flags" | **[Partial]** — now substantially met | This row was **[not built]** when the audit ran, because scoring was a manual integer field and nothing read the client's `sector` or `contractStatus` — the two columns the ERD said would "feed the ranked inquiry scoring engine". `Core/Models/Inquiry/InquiryScoring.cs` is that engine: four weighted factors (estimated value 35, sector rank 25, contract status 20, urgency 20), the sector rank table reading `Clients.Sector` and the contract band reading `Clients.ContractStatus`, applied whenever the person entering the enquiry leaves the score blank. **The remaining gap against the sentence is real and should not be glossed:** submissions arriving by e-mail are still not parsed — the queue is fed by web and mobile forms only — and there is no learning or feedback from outcomes, so "dynamically evaluated against benchmarks" is arithmetic on four fields, not a model. |
| 2.1.4 | Automated issue resolution & complaint management — submission → routing → resolution approval | **[Built]** | `TicketService.CreateAsync` stamps `SlaDeadline`; `RunSlaEscalationCheckAsync` marks `IsSlaBreached`, increments `EscalationLevel`, sets status `Escalated` and writes a system activity; `PATCH /assign` for routing; resolution requires notes on both API and UI |
| 2.1.5 | Specialised equipment lifecycle & SLA automation — notifications 60 days before service or warranty expiry | **[Built]** | `GET /api/Asset/warranty-expiring?daysAhead=60` (default 60), `AssetRepository.GetWarrantyExpiringAsync`, `Asset.NextServiceDue` auto-set to service date + 6 months in `AddServiceLogAsync`, `WarehouseAlert` table for stock thresholds |

**Note on "routing to technical or logistics teams."** Task 1 §1.3 promised SLA-based routing *to* a team. The implementation escalates and re-badges tickets and exposes an `assigned-to-me` feed for Field Support Engineers, but **automatic assignment to a team based on ticket category does not exist** — assignment is always a manual admin action via `PATCH /api/SupportTicket/{id}/assign`.

---

## 5. User roles comparison (Task 1 §2.2)

Five roles were proposed. Seven were defined. One of the seven is inert.

| Proposed role | Implemented role | Status |
| --- | --- | --- |
| System Administrator | `System Administrator` | **[Full access]**; only role that can create admins, assign reps, manage users, run SLA sweeps |
| Sales Representative / Account Executive | `Sales Representative` | **[Built]** — Inquiry queue, leads, client map, pipeline, quotations, analytics |
| Procurement Manager (Client) | `Procurement Manager` | **[Partial]** — Has a dashboard, catalogue CRUD and logistics tracker — but see the permission defect in §12 |
| Field Support Engineer / Technical Support | `Field Support Engineer` | **[Built]** — Ticket hub, asset tracking, service logs |
| Medical Student — Quick-Cart ordering | `Medical Student` | **[Inert]** — Present in `UserRoles.All` but excluded from `UserRoles.SelfRegisterable`, absent from every `[Authorize(Roles=...)]` string, and given zero permissions in `VerichatCRM.Web/Authorization/Perm.cs`. A Medical Student who somehow registered would see an empty sidebar and be redirected to `/Account/NoAccess` from every page. |
| — (not proposed) | `Logistics` | **[Unplanned]** — fifth staff role; ticket/logistics/asset permissions |
| — (not proposed) | `Client User` | **[Unplanned]**; this is the mobile-app persona that absorbed both the Procurement Manager's self-service and the Medical Student's cart. For this role the implementation uses the convention **`User.UserId` *is* the `ClientId`** (see `EnsureClientAccountAsync`) |

**Registration constraint honoured.** Task 1's admin story — "I want to configure role-based permissions … so that Sales Reps, Support Engineers and clients only see the data relevant to their role" — and the self-registration rule were respected: `AuthService.Register` rejects any role outside `SelfRegisterable`, so **self-registration can never mint an administrator**. The first admin comes only from `AdminBootstrapper`, and only when the `Users` table is empty, from `Bootstrap__AdminEmail/AdminPassword/AdminName`.

---

## 6. User story traceability (Task 1 §2.3)

Fifteen stories were written. Each is traced below.

### System Administrator

| Story | Status | Evidence |
| --- | --- | --- |
| Configure role-based permissions per user type | **[Built]** | `Perm.cs` 27-permission × 7-role matrix; `AdminController.Users` + `PUT /api/auth/users/{id}/role` |
| Global analytics dashboard: usage, **error rates**, **system load** | **[Partial]** | Revenue/tickets/deliveries analytics is rich (`DashboardAnalyticsController`, `Views/Analytics/Index.cshtml`). **Error rates and system load are absent** — no metrics, no APM, no request logging. `/api/health` reports DB reachability, pending migrations and JWT-key presence, which is the closest thing to a load signal. |
| Update **Firestore Security Rules** from a controlled deployment process | **[Not built]** | Obsolete with the stack change. The nearest analogue — `Views/Admin/Rules.cshtml` — is a static mock table. There is no deployment process to control (see §11). |

### Sales Representative / Account Executive

| Story | Status | Evidence |
| --- | --- | --- |
| View dynamic lead scores to prioritise urgent, high-value deals | **[Built]** | Scores are computed by the weighted rules engine when left blank, sort the queue and filter (`minScore`), and can still be typed over by hand — see §4/2.1.3. **Not** machine-learned, and not derived from e-mail. |
| Move deals through pipeline stages on a visual board | **[Built]** | `Views/Pipeline/Index.cshtml` Kanban; `PATCH /api/Pipeline/deals/{dealId}/stage` validates against `PipelineStages.All` |
| Generate a branded quotation PDF directly from a deal without leaving the platform | **[Partial]** | PDF exists and is branded; **the staff UI cannot reach it** (disabled button). The mobile client *can* request a quote and the staff *can* view it, so the flow works only from the client side. Also note `QuotationService` always creates staff quotes with `DealId = null`, so "from a deal" is not actually wired. |

### Procurement Manager (Client)

| Story | Status | Evidence |
| --- | --- | --- |
| Single-click reorders for medical consumables | **[Built]** | `POST /api/mobile/reorder/{sku}` (qty 1, self-heals the client row); `clientHomescreen.tsx` 1-tap reorder |
| Log a complaint with **photo evidence** and receive a ticket number | **[Built]** | `Ticket.PhotoUrls` list, `POST /api/SupportTicket`, tracking number `VC-TCK-yyMMdd-####`. Photo upload is validated on the detail page. |
| See the delivery status of my last three orders | **[Partial]** | Deliveries exist per client (`GET /api/Delivery/client/{clientId}`) and the staff Logistics Tracker renders them. **The mobile client has no delivery-status screen** — `Procurement/LogisticsTracker` was the orphaned fake view and is now deleted. The over-exposure that used to sit alongside it is closed: `DeliveryController.GetByClient` and `Track` now return 403 unless the caller is entitled to that client (§20 item 4). What remains is the missing mobile screen. |

### Field Support Engineer

| Story | Status | Evidence |
| --- | --- | --- |
| Issues automatically routed by SLA parameters | **[Partial]** | SLA deadlines and escalation are automatic; *routing to a person* is manual |
| Log service history against a specific piece of equipment so repeat faults are visible | **[Built]** | `AssetServiceLog` (service date, performer, description, outcome, parts replaced), `Views/Logistics/AssetDetail.cshtml` service-history timeline |
| A mobile view of assigned tickets to update job status from a client site | **[Not built]** | `GET /api/SupportTicket/assigned-to-me` exists for this role, but **the Expo app has no ticket screen at all**. The mobile app is client-facing; engineers have no mobile surface. |

### Medical Student

| Story | Status | Evidence |
| --- | --- | --- |
| Quick-Cart to buy stethoscopes and lab coats without administrative delay | **[Partial]** | The catalogue and cart work and *do* contain a lab coat (`VC-LB-006`) and clinical items — but there is no student-specific path, no "Student Kit" pack, and no role recognition |
| Student-discounted pricing applied automatically at checkout | **[Not built]** | No discount concept exists anywhere in the codebase |
| Order confirmation and tracking on my phone | **[Partial]** | Orders appear in the mobile app and profile; **no confirmation screen, no tracking screen, no push notification of order state** |

**Stories: 15 proposed → 7 fully delivered, 6 partial, 3 not delivered** (Firestore rules, engineer mobile view, student discount).

---

## 7. UX journey maps (Task 1 §2.4)

The four journey maps appear on pages 8–10 of the Task 1 PDF as images only (no extractable text); the goals were stated in the body text.

| Journey | Stated goal | Delivered? |
| --- | --- | --- |
| 1 — Corporate Procurement Manager | Instant reordering of recurring medical consumables | **[Built]** — Mobile catalogue + cart + reorder endpoint |
| 2 — Internal Sales Representative | Ranked inquiry → PDF quotation | **[Partial]** — Ranked inquiry is manual; **PDF step is blocked in the UI** |
| 3 — Internal Support/Logistics Team | Resolve a technical complaint within SLA | **[Built]** — Ticket hub, SLA countdown, escalation, resolution notes |
| 4 — Medical Student | Fast, small-volume purchase of training gear | **[Partial]** — Generic client flow only; no student identity or pricing |

Journey 2 depended on Firestore live snapshot listeners ("the Sales Progress Tracker board reflects a deal's stage the moment it changes"). The real-time requirement was met differently — see §10.2.

---

## 8. Prototype screen-by-screen (Prototype Documentation)

The prototype document enumerated 15 desktop/web screens and 5 mobile screens. This is the most direct comparison available.

### 8.1 Desktop / web (staff & admin)

| # | Prototype screen | Status | Implementation |
| --- | --- | --- | --- |
| — | Landing page / homepage with top nav (Home, About Us, Catalog, Digital Health, Contact), hero, category cards, quick-inquiry widget to `sales@verichat.co.zw` | **[Partial]** | `Views/Home/Index.cshtml` exists as a public marketing page with quick actions and role cards, but the stats are hard-coded, the copy is stale (Firestore), and there is **no mailto inquiry widget and no "Digital Health" nav item** |
| — | Login page showing multiple roles; selecting a role navigates to that dashboard | **[Built]** — with a security caveat | `Views/Account/Login.cshtml`. The role dropdown's choice is **deliberately discarded** — `WebAuthService` sends email + password only, and the landing dashboard is chosen from the *server-issued* role. This is the correct behaviour and is better than the prototype implied. |
| 1 | E-commerce product catalog & search with filter drawer, autocomplete, grid, multi-category filtering, **Quote-Request / Cart toggle** | **[Partial]** | `Views/Procurement/ProductCatalog.cshtml` — card grid, category chips, client-side search via `js/product-catalog.js`. **No autocomplete. The sort `<select>` is decorative with no handler.** No cart/RFQ toggle on the web at all — the web catalogue is administrative, not transactional. |
| 2 | Digital Health & Telemedicine portal — HMS/LIMS status monitors, e-prescription interface, RPM panel, **telemedicine video console** | **[Partial]** | Split across two clients. Web has `TelehealthController.Appointments` (booking/approval/assignment + meeting URL). Mobile has `BookAppointmentScreen.tsx`, lab results, prescriptions, vitals. **No video console, no HMS/LIMS connection** — `MobileService.GetLabResultsAsync` returns `LimsConnected = true` as a **hard-coded constant**, and nothing ever writes a `LabResultRecord` or `MobilePrescription` row. |
| 3 | Admin management & orders dashboard — order & RFQ pipeline, inventory tracking, **wellness program tracker** | **[Partial]** | Admin dashboard **[built]** (`AdminController.Dashboard` → shared `Views/Dashboard/Index.cshtml`). Inventory tracking **works** via `WarehouseAlerts`. **Enterprise corporate wellness tracker for client employees: not built** — the wellness tables that exist (`WellnessGoal`, `VitalReading`, `WeeklyExerciseLog`) are per-*mobile-user* personal tracking, not an employer-level programme. |
| 4 | App Home / Dashboard (mobile) | **[Built]** | `DashboardScreen.tsx`, `clientHomescreen.tsx`, `Dashboard.tsx` |
| 5 | System Administrator screen — nav of dashboard, client directory, client profile, asset tracking, inquiry queue, pipeline board, quotation builder, product catalog, ticket hub, warehouse alerts, analytics, **rules engine** | **[Partial]** — 11 of 12 | All present in `_Sidebar.cshtml` except **Rules Engine**, which is a static mock view. Note the real app puts *all* of this in one root controller set; the `Areas/Admin` and `Areas/Sales` folders implied by the layout are **empty directories with no files and no `MapAreaControllerRoute`**. |
| 6 | Client Directory and Profile with Add Client | **[Built]** | `Clients/Index.cshtml`, `Profile.cshtml`, `Create.cshtml` — plus unplanned rep-assignment history and an interactive Leaflet client map |
| 7 | Asset Tracking — Total/Calibration Overdue/Due This Month/Warranty Expiring KPIs, status indicators, **Schedule Service** action | **[Partial]** | `Logistics/Assets.cshtml` has all four KPI cards and colour-coded statuses. **"Schedule Service" does not schedule anything** — it is a plain link to `/Support/TicketHub` with no asset prefill. |
| 8 | Inquiry Queue — header summary, sector/urgency/sort filters, table, status indicators, View buttons, "scored by AI" | **[Partial]** | `Inquiry/Index.cshtml` + `Form.cshtml` deliver the queue, KPIs, status chips, score badges and full CRUD. **"Scored by AI" is still not true** — but the queue is no longer blank-by-default: `InquiryScoring` derives a 0–100 score from four weighted factors and a typed score overrides it (§8, defect 11 in §20). Arithmetic, not learning, and the screen's wording should say so. |
| 9 | Sales Pipeline — Total Pipeline / Weighted Forecast / Deals This Period, Kanban with 5 named stages | **[Built]** | Exact stage names match: Initial Inquiry, Site Assessment, Proposal Review, Quotation Sent, Closed Won (plus `Lost`). Drag-and-drop with rollback. Unplanned additions: monthly goal widget and a "Mobile quote requests" panel. |
| 10 | Quotation Builder — split table/client summary, + Add Row, auto subtotal/VAT/grand, terms checkbox, **Generate PDF** | **[Built]** | `Quotation/Index.cshtml` has the editable line-item table with live JS recalculation and the client summary card. **Generate PDF is enabled** — it posts to the working `GET /api/Quotation/{id}/pdf` and there is a Save action beside it, which closes UAT-02 (§20 item 3). The currency bug that used to sit on the same row — totals rendered `toLocaleString('en-US', {currency:'USD'}) while the rest of the application is in ZAR — is pinned: the PDF and the totals are rand (§18 defect 21). |
| 11 | Product Catalog — tabbed categories, search & sort, grid with images | **[Partial]** | Built, minus the working sort. Category taxonomy is inconsistent (see §12). |
| 12 | Ticket Hub — Open/Critical/Avg Resolution/SLA Breaches stats, table with severity, status, assignee, type | **[Built]** | `Support/TicketHub.cshtml` + `TicketDetail.cshtml`. Exceeds the prototype: escalation badges, breach flags, photo grid, SLA countdown card, activity timeline. |
| 13 | Logistics Tracker — order cards + **Live Map View** with vehicle positions | **[Partial]** | `Logistics/Tracker.cshtml` renders real delivery cards from `/api/delivery` with origin→destination, ETA, weight and status badges. **The live map does not exist** — no GPS/telematics feed, no vehicle positions. The only "map" in the app is the Leaflet *client* map (`Sales/Map.cshtml`), which plots static client coordinates, not moving vehicles. |
| 14 | Warehouse Alerts — critical alert banner, stock table with supplier and pre-calculated reorder quantity, "Stock Level vs Minimum Required" bar chart | **[Partial]** | `Procurement/WarehouseAlerts.cshtml` has the table, statuses and the chart element. **Three defects:** the `Supplier` column is always empty (`Supplier = string.Empty`); reorder quantity is computed locally as `Threshold*2 − CurrentStock`, not sourced; and the chart's `StockLevels` view-model is never populated, so it renders nothing. **"Reorder Now" is broken at runtime** — `js/warehouse-alerts.js` calls `api.post('/api/purchaseorder', …)` but `js/api.js` is never loaded on that page, so the click throws a `ReferenceError`, and no web service proxies `/api/purchaseorder` anyway. |
| 15 | BI Analytics — KPI row, Monthly Revenue vs Target line, Revenue by Sector pie, Resolution Time by Week line | **[Built]** | `Views/Analytics/Index.cshtml` with four Chart.js charts (sales-vs-target grouped bars, floating-bar funnel, sector doughnut, revenue line) fed by `/api/analytics/dashboard` + `/api/dashboard/analytics`. Exceeds the prototype by adding a conversion funnel. The "resolution time by week" chart is not among the four. |

### 8.2 Mobile screens

The prototype's mobile design specified a bottom tab bar of **Home, Shop, Telehealth, Vitals, Profile**, a barcode scanner, Bluetooth device sync, video calls, QR prescriptions and goal rings.

**Important context for this section.** The mobile app went through two phases. An earlier build did implement the wellness/telehealth surface — vitals, health goals, exercise logs, BLE device sync, lab results, prescriptions and telehealth appointment forms — matching the prototype. Those screens were subsequently **deleted**. The project's own mobile documentation states it plainly:

> "Deleted mobile screens/services include wellness tracking, vitals, health goals/exercise, BLE device sync, lab results, prescriptions, and telehealth-only appointment forms. `react-native-ble-plx` was removed from mobile dependencies/config. The CRM API still contains some historical wellness/device models and routes; those are legacy server code and are not referenced by the current mobile app."
> — [VerichatMobile/feature.md:109](file:///D:/Documents/Documents/GitHub/VerichatCRM.WIL/VerichatMobile/feature.md)

So the mobile half of the Prototype document's Digital Health design was built, then **deliberately walked back**, leaving orphaned server-side entities. The statuses below reflect the **current** app.

| Prototype element | Status | Evidence |
| --- | --- | --- |
| Bottom tab navigation (5 tabs) | **[Built]** | Five visible tabs — Home, Catalogue, Consult, My Assets, Profile — implemented by a **custom** `src/components/app-tabs.tsx` (native) and `app-tabs.web.tsx` (web) rather than a `(tabs)` route group; `src/app/` is a flat 18-file directory with 13 routes suppressed via `href: null`. Staff accounts see only **Home + Profile** (Catalogue/Consult/My Assets are hidden from them). |
| Quick Actions grid (Order PPE / Book Telemedicine / Log Health Check / View Prescriptions) | **[Partial]** | `clientHomescreen.tsx` renders four quick actions — Catalogue, Book consultation, My equipment, Quote requests. Two of the prototype's four (Log Health Check, View Prescriptions) are gone with the retired wellness feature. |
| Live Vitals summary widget | **[Retired]** | The `Vitals` type and `ClientDashboard.vitals` still exist in `src/constants/dashboardService.ts:3-8,27` and ~20 lines of orphaned `vital*` styles remain at `clientHomescreen.tsx:386-405` — but **nothing reads them**. Server side, `GET /api/mobile/vitals` and `POST /api/mobile/vitals` still respond; no client calls them. |
| **Bluetooth / device syncing** (glucose meter, SpO2, BP machine, sphygmomanometer, thermometer, scale) | **[Not built]** | Never shipped and now removed: `react-native-ble-plx` was deleted from dependencies, and there are **zero references to `bluetooth` in the mobile source**. `MobileDevice` (with an `ExternalId` column commented "BLE MAC/UUID") and `POST /api/mobile/devices/{id}/sync` survive server-side only; the sync merely replays stored readings and flags the device connected. |
| Barcode scanner on catalogue search | **[Built]** | `components/barcode-scanner-modal.tsx` + `expo-camera`, wired in three screens: `medicalCatalogueShop.tsx` (sets the search query), `EquipmentStatusScreen.tsx` and `DashboardScreen.tsx` (filter by `assetId`/`serialNumber`). Camera permission text and `barcodeScannerEnabled: true` are declared in `app.json`. |
| Targeted Medical Packs / Student Kits | **[Not built]** | No pack or bundle concept |
| 1-tap reordering of recurring consumables | **[Partial]** | `POST /api/mobile/reorder/{sku}` exists and `catalogueService.quickReorder` wraps it — but **the function has no UI button at all**, despite `feature.md` documenting the flow. `clientHomescreen.tsx` shows a static "Recurring order" box fed by `nextRecurringOrder`, which the server **hard-codes to `null`**. |
| Location-based delivery estimator for Harare | **[Not built]** | No delivery-cost or zone calculation. `RealTimeTracker.ts` types a `currentLocation: { lat, lng }` field, but no location permission or plugin is declared in `app.json`. |
| Telemedicine video call UI with in-app chat | **[Not built]** | Replaced by a **booking** flow: `BookAppointmentScreen.tsx` posts to `POST /api/appointments` with a topic sheet (New Quote / Equipment Calibration / Equipment Service / Invoicing Issue / Product Consultation / Other) and a hand-built month calendar. "Join consultation call" opens a **manually pasted** `meetingUrl` via `Linking.openURL`, and only once a rep has supplied one. `clientHomescreen.tsx` carries the honest note *"no video call is created automatically"*. |
| Instant lab results viewer (PDFs from connected LIMS) | **[Retired]** | Screen deleted. `LabResultRecord`, `GET /api/mobile/lab-results` and `POST /api/mobile/lab-results/{id}/viewed` remain server-side, with `LimsConnected` **hard-coded `true`** and no code path anywhere that creates a lab-result row. |
| QR-coded e-prescription to a preferred pharmacy | **[Retired]** | Screen deleted, and `react-native-qrcode-svg` is now an **unused dependency with zero references**. `MobilePrescription` + `GET /api/mobile/prescriptions` remain, read-only, with no writer. |
| Wellness tracker: progress rings, Steps/Water/Sleep/Exercise goals | **[Retired]** | Deleted. `WellnessGoal`, `VitalReading`, `WeeklyExerciseLog` and the four `MobileVitalsController` endpoints persist server-side. **Note:** `GoalsScreen.tsx` is **not** the wellness screen — it exports `SalesGoalCard`, a CRM monthly-sales-target card reading `GET /api/salesgoals/my-goals`. |
| Profile screen: notifications toggle, dark mode, address, purchase history, badges | **[Partial]** | `profileScreen.tsx` (contact details + delivery address, `PATCH /api/mobile/profile`) and `settingsScreen.tsx` (push / biometric / dark-mode switches with optimistic update and rollback). **Purchase history: not shown.** **Loyalty badges: not built.** The push toggle writes to `MobileSettings` but has no delivery mechanism behind it (see §18). |
| Biometric login | **[Unplanned]** | Not in the prototype; `expo-local-authentication` with `disableDeviceFallback: true`, auto sign-in on `index.tsx`, opt-in flag stored in **SecureStore**, not the server |
| Offline resilience | **[Unplanned]** | Not in the prototype; a genuine engineering addition — see §17 |

### 8.3 Design system

The prototype specified a seven-token palette. The web theme (`wwwroot/css/verichat-theme.css`, 253 lines) and the mobile brand theme implement a dark/light navy brand consistent in spirit with it; the PDF generator hard-codes navy `#0A1F44` and cyan `#00B4D8` rather than the prototype's `#0066FF` / `#090D16` tokens, and Chart.js is loaded from CDN at **two different versions** (4.4.1 and 4.4.4) across views, with SignalR at 8.0.0 in the layout and 8.0.7 per page.

**There is no design-system implementation library on either client.** The web app uses vendored Bootstrap 5 and jQuery; the mobile app uses plain `StyleSheet.create` plus `react-native-reanimated`, `expo-linear-gradient`, `react-native-svg` and `MaterialCommunityIcons`. NativeWind, Tailwind, Gluestack, Tamagui, unistyles and React Native Paper are all **absent from `package.json`** — `src/constants/theme.ts` still carries the Expo template comment pointing at them as options that were never taken.

Mobile has a **duplicated, half-dead theme layer**: `src/constants/crmTheme.ts` (`CRMTheme`, the real brand palette used by 13 files) sits alongside `src/constants/theme.ts` (the untouched Expo template `Colors`/`Fonts`/`Spacing` module, consumed only by dead starter components), and raw hex values such as `#004e7a`, `#0057a8` and `#f5f7fa` are re-typed inline in `explore.tsx` and `services/ClientCommunicationScreenStyles.ts` instead of coming from `CRMTheme`.

Six web CSS files (`brand.css`, `components.css`, `layout.css`, `pages.css`, `site.css`, plus two `.backup.css` copies) are **never referenced by any layout**.

---

## 9. Non-functional requirements comparison (Task 1 §2.5)

| NFR category | Proposed | Delivered |
| --- | --- | --- |
| Information security & access control — multi-tenant protection, RBAC limits, privacy safeguards | **[Partial]** | RBAC is genuinely enforced twice (role strings on API endpoints + a 27-permission matrix in the web layer) and row ownership is checked in most client-scoped endpoints. **Multi-tenancy is single-database, single-schema, filter-based** — not the collection-level isolation Firestore Rules gave. Leaks exist: `DeliveryController.GetByClient`/`Track` and `SupportTicketController.GetById` perform no ownership check, and `NotificationHub.Subscribe(role)` lets any authenticated user join any role's notification group. |
| Performance — uptime, rapid checkouts under volume, cloud elasticity | **[Untested]** | No load or performance tests exist. Two known N+1/full-table patterns work against this: `ClientService.GetAllAsync` runs a deals-total query per client, and `DeliveryLogService.UpdateStatusAsync` loads **every** delivery to mutate one. |
| Scalability | **[Replaced]** | SQL Server vertical scaling replaced Firestore's serverless horizontal scaling. `EnableRetryOnFailure(3, ≤10s)` is the only resilience measure. |
| Usability — "User-Friendly Simplicity", operable without a technical background | **[Built]** | Consistent sidebar, KPI-first screens, empty states, optimistic UI with rollback on the Kanban, branded 404/403/500 pages, a skip-link for keyboard users |
| Availability | **[Partial]** | `/health` (process) and `/api/health` (DB + migration + JWT-key readiness, HTTP 503 when degraded) are genuinely useful. No uptime monitoring, no alerting. |
| Accessibility — contrast, keyboard navigation, scalable text | **[Partial]** | One skip-link and semantic `<dl>`/`<table>` markup. No ARIA on the Kanban, and **drag-and-drop stage changes have a keyboard-accessible `<select>` fallback**, which is a real plus. No automated a11y check in CI — because there is no CI. |

---

## 10. Architecture comparison

### 10.1 Layered N-tier (Task 1 §5.2) — **the one idea that survived intact**

Task 1: *"Verichat will be using a layered (N-tier) architecture: presentation, business logic/API, and data are separated so each layer only calls into the layer below it, and each can be developed, scaled, and tested independently."*

This was honoured almost exactly, and is now the six-project .NET solution in `VerichatCRM.slnx`:

| Proposed layer | Implemented as |
| --- | --- |
| Presentation | `VerichatCRM.Web` (Razor MVC staff UI) + `VerichatMobile` (Expo client) |
| Business logic / API | `VerichatCRM.API` (21 controllers + 15 services) |
| Data | `VerichatCRM.Infrastructure` (EF Core `AppDbContext`, 8 repositories, migrations) |
| Domain | `VerichatCRM.Core` (34 entity files, 7 "enum" files, 14 model files, 9 interfaces) |

The dependency direction is clean: Infrastructure → Core, API → Core + Infrastructure, Web → Core + Infrastructure (for shared DTOs only), Tests → API + Web.

**One layering violation:** several API controllers inject `AppDbContext` directly instead of going through a repository or service — `ClientController`, `InquiryController`, `SalesGoalsController`, `OrderService`, `ProductService` and `MobileService` all query the `DbContext` themselves. The repository layer is therefore only *partially* in the call path, so "each layer only calls into the layer below it" holds at the project level but not at the class level.

### 10.2 Design patterns (Task 1 §5.1)

**Observer — proposed for "Sales Stock Warnings."**
Task 1: a bulk order exceeding warehouse stock should push state updates from the subject (inventory) to observers (procurement/support) and trigger instant alerts, delivered via Firestore live snapshot listeners.

Delivered **[replaced]** as three cooperating mechanisms, none of them the Observer pattern in its literal form:
1. `WarehouseAlertService.CheckAndCreateAlertsAsync` — invoked by `ProductService` on every catalogue mutation; writes one open alert per low-stock SKU, `Critical` at zero else `Warning`.
2. `SlaEscalationService : BackgroundService` — a **5-minute polling loop**, not a push subscription.
3. `NotificationHub` (SignalR) at `/hubs/notifications` — real push, but used for **appointment and quotation** notifications only, not stock.

The proposal's specific trigger — *"warning banner displayed on the quotation screen"* when a bulk order exceeds stock — **is not implemented**. `OrderService` *rejects* an order whose quantity exceeds `StockQty` rather than warning, and the quotation screen performs no stock check at all.

**Facade — proposed for the "One-Click Quotation Generator."**
Task 1: one simplified interface hiding the orchestration of client profile + deal + product catalogue, executed in a Cloud Function with the Admin SDK.

Delivered **[replaced]** as `QuotationService` + `IPdfGenerator` behind `POST /api/Quotation` and `GET /api/Quotation/{id}/pdf`. The orchestration is real (client profile, line items, live database prices, VAT at 15 %, `ValidUntil` +30 days). It is a plain application service rather than a formal Facade, and — because the UI button is disabled — the "one click" part is unfulfilled. The mobile path (`POST /api/Quotation/request`) *is* effectively a one-click facade and works.

**UML object diagrams** (Task 1 §5.1, page 35) depicted instances of these patterns. They remain conceptually accurate; the class names differ.

### 10.3 Cloud topology & network segregation (Task 1 §5.3) — **not implemented**

Proposed: separate Firebase projects for development, staging and production, each with its own IAM roles and Security Rules, so a compromised dev environment cannot reach production data; all traffic over HTTPS/TLS; privileged operations through Cloud Functions using the Admin SDK.

Delivered:

| Proposed | Actual |
| --- | --- |
| Three isolated Firebase projects | **Two configuration files only** — `appsettings.json` and `appsettings.Development.json`, in each project. There is no `appsettings.Staging.json`, and a search for `IsProduction` / `IsStaging` across API and Web returns **zero matches**, so the code has no environment-specific behaviour beyond the Development branch used for CORS fallback and demo data. |
| IAM role segregation | None. One SQL Server database, one connection string. |
| TLS 1.3 everywhere with forward secrecy | `UseHttpsRedirection()` on both apps, and HSTS outside Development on the Web app. **But the committed production connection string sets `Encrypt=False`**, so the API→database hop is explicitly unencrypted — the opposite of the proposal. |
| Privileged operations server-side | **[Built]** — Correctly honoured, and by accident of architecture: EF Core and the SQL credential never leave the server, so a client cannot write directly to the data layer. This is the strongest sense in which the proposal's *intent* was met. |
| gRPC Firestore channel / Firebase SDK | Not applicable |
| Hosting | Azure (per the WBS resources column) → **`runasp.net`** shared hosting via Web Deploy publish profiles (`site94515-WebDeploy.pubxml`, `site94518-WebDeploy{,1,2}.pubxml`, `FTPProfile.pubxml`) |

The topology diagram on page 37 (Client apps → Hosting / Auth / Functions / Firestore) has been replaced by the diagram the README actually draws: Staff browser → MVC Web → JSON → API → Services → EF Core → SQL Server, with Expo → API in parallel. Structurally the fan-out shape (many clients, one auth, one API, one store) survived; only the vendors changed.

### 10.4 Deployment plan (Task 1 §4.3)

The UML deployment diagram (page 32) showed `<<device>> Client (Verichat SPA React)` → `<<execution environment>> Firebase Hosting (index.html + assets)`, `Firebase Auth (Auth token service)`, `Cloud Functions (api.js REST API, pdfGenerator.js)` → `<<database>> Firebase (clients, deals, quotations, products, complaints)`, with `<<HTTPS/REST>>`, `<<TPS>>` and `<<Admin SDK, verify token>>` stereotypes.

| Diagram node | Realised as |
| --- | --- |
| Client — Verichat SPA (React) | **[Replaced]** — **Two** clients: a Razor MVC server-rendered app (not a React SPA) and an Expo React Native app |
| Firebase Hosting — index.html + assets | **[Replaced]** — IIS/Kestrel serving `wwwroot` with `MapStaticAssets()` |
| Firebase Auth — auth token service | **[Replaced]** — `JwtHelper` + `AddJwtBearer` (HS256, issuer `VerichatCRM`, audience `VerichatCRMUsers`, 120-minute lifetime) |
| Cloud Functions — `api.js` | **[Replaced]** — 21 ASP.NET Core controllers |
| Cloud Functions — `pdfGenerator.js` | **[Built]** — `Infrastructure/Data/External/PdfGenerator.cs` using **QuestPDF** — a genuinely faithful realisation of the node in the diagram |
| Firestore database | **[Replaced]** — SQL Server, 27 `DbSet`s |
| `<<Admin SDK, verify token>>` edge | **[Built]** — Equivalent: `OnMessageReceived` extracts `?access_token` for `/hubs/notifications` only, and `TokenValidationParameters` validate issuer, audience, lifetime and signing key |

---

## 11. Workflow comparison — Gitflow and CI/CD

### 11.1 Branching (Task 1 §6.2) — **not followed**

Task 1 committed to Gitflow with a branch-to-environment map, protected `main` and `develop`, mandatory PR review, Conventional Commits, and `feature/<issue-number>-<short-description>` naming.

| Proposed branch | Purpose → environment | Reality |
| --- | --- | --- |
| `main` | Always production-ready; every commit a tagged release → Firebase production | Exists, but **`git tag` returns zero tags** — no release has ever been tagged |
| `develop` | Integration → Firebase staging | Exists; **there is no staging environment to deploy it to** |
| `feature/*` | One branch per user story → not deployed | Exactly **one** such branch exists: `feature/product-catalog-ordering` |
| `release/*` | Freeze scope, bump version, final UAT → staging | **Never created** |
| `hotfix/*` | Patch urgent production bug → production | **Never created** |
| — | — | Actual long-lived branches instead encode *layer* and *person*: `api`, `web`, `mobile`, `mutsa`, `dev2`, `develop`, `feature-debug`, `section1---mutsa`, `section2--rai` |

The naming convention was also not adopted: `section1---mutsa` and `dev2` reference neither an issue number nor a short description.

**Conventional Commits:** 25 of 169 commit subjects (~15%) use the `feat:`/`fix:`/`docs:`/`chore:` prefix. The remaining ~85% are free-form, e.g. *"Rendering, Removing and Repairing"*, *"Confirm catalogue displays products from database"*, *"Permission Unit Tests"*. Some *do* follow the format properly (`feat(mobile): add offline API state and sync banner`), so the convention was adopted late and only for the mobile workstream.

**Branch protection:** cannot be verified from a clone; the README itself flags that *"GitHub branch protection, reviewer requirements, deployment environments and production secrets must be configured in the repository settings separately."* Fourteen numbered pull requests were merged (`Merge pull request #14 …`), so PR review did happen in practice.

### 11.2 CI/CD (Task 1 §6.3) — **the largest single gap** (now half-closed: verification restored, deployment still absent)

Task 1 specified a nine-stage pipeline: Push/PR → CI Lint & Test (eslint + unit tests + Firestore Rules unit tests) → CI Build (`npm run build`) → PR Review → Merge to develop → Staging Deploy (`firebase deploy --project staging`) → UAT/QA → `release/*` branch → Manual Approval (GitHub Environment protection) → Deploy to main (`firebase deploy --project production`).

> **Status after §20 item 2: `ci.yml` is restored.** It is back in the working copy exactly as `e029d66` deleted it — two jobs, `dotnet` (restore → test → build on `VerichatCRM.slnx`) and `mobile` (`npm ci` → `expo lint` → `tsc --noEmit`), triggered on push to `main`/`master` and every pull request. `android-build.yml` was **not** restored, deliberately: every one of its four jobs fails for reasons listed below, and reviving it would buy a permanently red badge rather than a real check. What this section describes from here on is the state as found, kept because the diagnosis still matters — and **what still does not exist**: no deploy stage, no staging or release environment, no manual approval gate, and no branch protection, so the pipeline can verify a build but cannot publish one. Until it runs on the remote, its verdict is unproven: locally the equivalent commands are green (0 errors, 71 tests passing, `tsc` clean), and the NU1902/NU1903 restore warnings in all six .NET projects (defect 60) will appear in its log.

**When the audit ran, the repository had no CI/CD pipeline.** The `.github/workflows/` directory existed on disk but contained **no files**, and no YAML was tracked in git:

```
$ ls .github/workflows/
(empty)
$ git ls-files .github
(no output)
$ git ls-files "*.yml" "*.yaml"
(no output)
```

The sharper finding is that **two workflows were written and then deleted.** Commit `e029d66` (*"docs: document mobile features and retire legacy modules"*, 2026-10-25, dBright-dev) removed both pipeline definitions in the same 3,167-line deletion that retired the mobile wellness/telehealth screens (see §19.2b):

```
$ git show --stat e029d66
 .github/workflows/android-build.yml   | 120 ----
 .github/workflows/ci.yml              |  34 --
 VerichatMobile/src/app/VitalsTab.tsx  | 189 ------
 … (16 more retired mobile files)
```

**.github/workflows/ci.yml** (added `7adceef`, deleted `e029d66`) was a genuinely usable two-job pipeline that matched the team's *actual* stack rather than the proposed Firebase one:

```yaml
jobs:
  dotnet:   # ubuntu-latest, setup-dotnet 10.0.x
    - dotnet restore VerichatCRM.slnx
    - dotnet test VerichatCRM.Tests/VerichatCRM.Tests.csproj -c Release --no-restore
    - dotnet build  VerichatCRM.slnx -c Release --no-restore
  mobile:   # ubuntu-latest, working-directory: VerichatMobile, node 22 + npm cache
    - npm ci
    - npx expo lint
    - npx tsc --noEmit
```

This triggered on `push` to `main`/`master` and on every `pull_request`. It was the closest thing the project had to WBS item 9.5, and it was deleted rather than fixed — with no replacement and no explanation in the commit message.

**.github/workflows/android-build.yml** (added `70bcc21`, deleted `e029d66`) was a 120-line, four-job pipeline that **could never have passed**, which is the likeliest reason it disappeared. Its defects, each verifiable against the current tree:

| Defect | Evidence |
|---|---|
| `npm ci` at the repository root | there is **no root `package.json`**, and the workflow never sets `working-directory: VerichatMobile` — fails on the second step |
| `npm test -- --watchAll=false` | `VerichatMobile/package.json` scripts are only `start`, `reset-project`, `android`, `ios`, `web`, `lint` — **no `test` script and no test runner installed** |
| `cd android && ./gradlew assembleDebug` | runs from the repo root; `android/` exists only as `VerichatMobile/android/` |
| `NODE_VERSION: '18.x'` | contradicts `ci.yml`'s Node 22 and the repo's own toolchain |
| `curl -X POST https://monsterasp.example.com/deploy` in both `deploy-web` and `update-api` | a **placeholder `.example.com` domain** — the "deploy" steps were always theatre; they only printed `echo` text and called a URL that does not exist |
| `MONSTERASP_API_KEY` secret | never configurable, because the endpoint is fictional |

So the proposal's nine-stage pipeline (Push/PR → CI Lint & Test → CI Build → PR Review → Merge to develop → Staging Deploy → UAT/QA → `release/*` → Manual Approval → Deploy to production) was **never realised in either its Firebase form or its .NET form**: `ci.yml` was real but deleted, and `android-build.yml` was a scaffold with fictional deployment targets.

The consequence for documentation is that **the README now describes a deleted file**:

> "`VerichatCRM.Tests/  xUnit automated tests" … ".github/workflows/ci.yml  Pull request and branch CI checks"
> — [README.md](file:///D:/Documents/Documents/GitHub/VerichatCRM.WIL/README.md)

and

> "**GitHub Actions** — `.github/workflows/ci.yml` restores, tests, and builds the .NET solution and runs Expo lint and TypeScript checks on pull requests and pushes to `main` or `master`."

Both sentences were accurate at `7adceef` and are false today. **The cheapest real fix is to restore `ci.yml` verbatim from git history** (`git show 7adceef:.github/workflows/ci.yml > .github/workflows/ci.yml`) — it needs no rewriting because it already matches the stack — and to delete the phantom `android-build.yml` references. Note also that a restored `ci.yml` would fail today for a different reason: `dotnet test` has no mocking library available and the mobile `npx tsc --noEmit` step now surfaces the type errors behind the dead exports catalogued in §18.

Finally, the Task 1 WBS costed item 9.5 ("Implement Automated Deployment", 3 days, Mutsa, GitHub Actions + Azure) and that budget was consumed without producing a live pipeline.

---

## 12. Data schema comparison — ERD vs. actual SQL

### 12.1 The storage-technology decision was reversed

Task 1 §4.4 devoted a full rationale to choosing Firestore over a relational engine, on four grounds. Every one of them was overtaken:

| Stated reason for Firestore | What happened |
| --- | --- |
| **Catalogue flexibility** — heterogeneous product families would force a wide, mostly-empty table or costly subtype joins | The catalogue stayed a single flat `Product` table (`SKU`, `ProductName`, `Category`, `Price`, `StockQty`, `LowStockThreshold`, `ImageUrl`). No per-category attributes were ever needed, so the flexibility argument never bound. |
| **Real-time pipeline updates** via live snapshot listeners | Delivered instead by SignalR (`NotificationHub`) plus explicit refetch/optimistic-update in `Views/Pipeline/Index.cshtml`. The stated *outcome* — the board updates when a stage changes — is achieved. |
| **Native stack integration & security** — Firebase Auth/Hosting/Functions already present; Security Rules enforce RBAC at the data layer | Replaced by JWT bearer + `[Authorize(Roles=…)]` + a web-side permission matrix. Enforcement moved from the data layer to the API layer, which is a *weaker* boundary in one specific sense: it depends on every developer remembering the row-ownership check, and three endpoints forgot (see §9). |
| **Usage-based cost model** producing R0 → R2 374,84/month tiers | Obsolete — see §13. |

The proposal even acknowledged the trade-off honestly ("Firestore has no server-side JOIN operator… mitigated by denormalising a few frequently read fields and resolving cross-collection references in Cloud Functions"). Ironically, the SQL implementation *does* have joins and uses them heavily — `GET /api/Client/{clientId}/timeline` joins six tables — so the denormalisation workaround became unnecessary.

### 12.2 Entity-by-entity mapping

The Task 1 ERD (page 34) drew nine collections. All nine exist as SQL tables, with the proposed attributes largely preserved.

| Proposed Firestore collection | Proposed fields | SQL reality | Match |
| --- | --- | --- | --- |
| **Clients** | ClientID `PK`, ClientName, Sector, Address, Contacts, ContractStatus | `Client`: `ClientId`, `ClientName`, `Sector`, `Address`, `ContactPerson/Email/Phone`, `ContractStatus`, **[extra]** `AssignedRepId`, **[extra]** `LastContactDate`, **[extra]** timestamps | **[Built]** + extended |
| **KeyPersonnel** (subcollection of Clients) | ID `PK`, ClientID `FK`, Name, Role, Email, Phone | `KeyPersonnel` as an **owned collection** on `Client` → table `KeyPersonnel` | **[Built]** (as owned type, not subcollection) |
| **Inquiries** | InquiryID `PK`, ClientID `FK`, DateReceived, SourceChannel, RankScore, Status | `Inquiry`: `InquiryId`, `ClientId?`, `Source`, `Status`, `Score` — **named `Score`, and manually entered**. **[extra]** `ContactName/Email/Phone`, `Company`, `Subject`, `Description`, `OwnerUserId`, `ResponseDueAt`, `RespondedAt` | **[Partial]** — renamed + not computed |
| **Deals** | DealID `PK`, InquiryID `FK`, ClientID `FK`, Stage, Revenue, OwnerUID `FK` | `Deal`: identical shape, plus **[extra]** `Probability` (0–100, default 25), `ExpectedCloseDate`, `Notes` | **[Built]** + extended |
| **Quotations** | ID `PK`, DealID `FK`, DateGenerated, TotalAmount, LineItems | `Quotation` with `QuoteReference`, `ClientId`, `ValidUntil`, `Subtotal`, `VatRate`, `VatAmount`, `GrandTotal`, `Terms`, `Status`. `LineItems` are a **separate `QuotationLineItem` table with cascade delete**, not an embedded array — the exact opposite of the ERD's stated modelling decision ("stored as an embedded array rather than a subcollection") | **[Partial]** — normalised instead of embedded |
| **Products** | SKU `PK`, ProductName, Category, Price, StockQty | `Product` — same, plus `LowStockThreshold`, `ImageUrl` | **[Built]** + extended |
| **Tickets** | TicketID `PK`, ClientID `FK`, EquipmentID `FK`, CreatedByUID `FK`, Description, **PhotoURLs**, SlaDeadline, Status, AssignedToUID `FK` | `Ticket` — all present, plus **[extra]** `TrackingNumber`, `Title`, `Priority`, `Category`, `IsSlaBreached`, `EscalationLevel`, `EscalatedAt`, `ResolvedAt`, `ResolutionNotes`, and owned `TicketActivity` history | **[Built]** + substantially extended |
| **Equipment** | EquipID `PK`, ClientID `FK`, SerialNumber, Model, WarrantyExpiry, LastServiceDate, ServiceLogs | `Asset` (**renamed** from Equipment) + `AssetServiceLog`; **[extra]** `Manufacturer`, `Location`, `NextServiceDue`, `Status` | **[Built]** — renamed + extended |
| **Users** | UID `PK`, DisplayName, Role, Email, Active | `User`: same, plus **[extra]** `PasswordHash`, `LastLoginAt`, `PasswordResetToken/Expiry`, `DeviceToken` | **[Built]** + extended |

### 12.3 The 18 entities nobody proposed

The delivered schema has **27 `DbSet`s** against the ERD's nine. The additions cluster into four unplanned themes:

**Theme A — a telehealth/wellness product line (10 tables), built and then orphaned:**
`VitalReading`, `WellnessGoal`, `WeeklyExerciseLog`, `MobileDevice`, `HealthMetric`, `TelehealthAppointment`, `LabResultRecord`, `MobilePrescription`, `MobileSettings`, `MobileProfile`.

This was the largest unplanned investment in the project — 10 of 27 tables, plus a dedicated `MobileVitalsController`, a fifth of `MobileService`, and the `AddMobileCareTables` migration — and it originated in the **Prototype** document's mobile section, not in Task 1's scope statement at all. It was fully built into the Expo app, and then the mobile screens were **deleted** (see §8.2). What remains is a server-side island: `GET /api/mobile/vitals`, `POST /api/mobile/vitals`, `GET /api/mobile/goals`, `PATCH /api/mobile/goals/{goalId}`, `POST /api/mobile/goals/exercise`, `POST /api/healthmetrics`, `GET /api/mobile/lab-results`, `GET /api/mobile/prescriptions` and `GET/POST /api/mobile/devices` all still compile and respond — with **no caller anywhere in the shipped clients**. `MobileService.GetLabResultsAsync` returns `LimsConnected = true` as a hard-coded constant, and no code path creates a `LabResultRecord` or `MobilePrescription` row at all.

Only `MobileProfile` and `MobileSettings` retain a live consumer (the profile and settings screens).

**Theme B — fulfilment and procurement (5 tables):**
`Order` + owned `OrderLineItem`, `PurchaseOrder` + owned `PurchaseOrderLineItem`, `DeliveryLog` + owned `DeliveryStatusUpdate`, `Inventory`, `WarehouseAlert`.
Task 1's ERD had **no order entity at all** — orders were implied by "self-service reordering" but never modelled. This is a genuine modelling omission in the proposal that the build corrected.

**Theme C — sales operations (3 tables):**
`SalesGoal` (target/achieved per period), `Appointment` (B2B meetings, distinct from telehealth), `ClientTimelineNote`.

**Theme D — governance (1 table):**
`AuditLog` — entity, entity id, action, user, email, JSON old/new values, **IP address**, timestamp, written automatically by `AuditSaveChangesInterceptor` and redacting `PasswordHash`, `PasswordResetToken` and `PasswordResetExpiry`. Neither proposal mentioned auditing.

### 12.4 Schema-fidelity defects

These are worth flagging because they are the kind of thing an ERD review is supposed to prevent:

1. **All seven `Data/Configurations/*` fluent mapping classes are dead code.** There is no `ApplyConfigurationsFromAssembly` call and no manual `.Configure()` anywhere, so *none* of their mappings reach the model. Only the inline `OnModelCreating` block applies. Consequences:
   - The **unique index on `Users.Email` is not applied** (migration `InitialCreateupgrade` added length limits only). `AuthService` guards duplicates in application code, so the hole is a race rather than an everyday bug. *(Still open — see §20 action 9.)*
   - Unique indexes on `Ticket.TrackingNumber`, `DeliveryLog.TrackingNumber`, `Asset.SerialNumber` and `Quotation.QuoteReference` are **not applied** either — and `TicketService.GenerateTrackingNumber` uses `Random.Shared.Next(1000,9999)`, so collisions are possible and unenforced. *(Still open; the deferred schema migration is what would carry them.)*
   - The one index this pass did add is `IX_Inquiries_Status_Score` (`Status`, `Score`) in `20261031160000_AddInquiryScoringInputs`, which required narrowing `Inquiries.Status` from `nvarchar(max)` first — SQL Server cannot index that type.
   - `TicketConfiguration`'s `PhotoUrls` value converter is never applied.
   - `DealConfiguration` specifies `Restrict` where the applied inline mapping says `Cascade` — the dead file actively contradicts live behaviour.
2. **`Inventory` is a migrated, modelled table with zero API surface.** Nothing reads or writes it. Stock lives entirely on `Product.StockQty`.
3. **`TicketComment` was created by `InitialCreate`** but the model uses owned `TicketActivity` instead — an orphan table left in the schema history.
4. **Seven migrations are hand-written with no `.Designer.cs`** (`AddHealthMetricTable`, `AddClientTimelineNotes`, `AddB2BAppointments`, `AddSalesGoals`, `AddUserPasswordResetFields`, `AddAuditLogs`, `AddUserDeviceToken`) — they will not regenerate cleanly through tooling. An eighth, `AddInquiryScoringInputs`, was added by hand on purpose for the same reason: the snapshot it would have been generated against was not trustworthy. `AppDbContextModelSnapshot.cs` in `HEAD` does not even compile (unbalanced braces, several `modelBuilder.Entity` blocks never closed), and because those seven Designer files are stale, **no** Designer-derived baseline in this repository can be trusted — which is exactly why the broader index/precision migration was set aside instead of scaffolded and shipped. *(Nine of the nineteen migrations are hand-written now: `AddInquiryScoringInputs` and `AddTicketActivityAndDeliveryStatusHistory` were both added by hand in this pass, the second for the reason in item 9 — the tables it creates are already in the snapshot, so no scaffold will ever emit them.)*
5. **The upstream migration chain is broken on arrival.** `20261025111000_AddSalesGoals` and `20261030204733_AddTask3Features` both call `CreateTable("SalesGoals")` with different primary keys (`GoalId` + a composite index vs `Id`), so no database can have applied both. `SalesGoal` itself existed twice in one file after the merge, with those two incompatible shapes side by side. `20261025111000_AddSalesGoals` should be treated as the schema of record and `AddTask3Features`' SalesGoals block as the error; until someone reconciles them against the deployed `db70543`, `dotnet ef migrations add` will keep proposing to rebuild the table. *(Closed by `113d04d`: that block is deleted from `Up()` and `Down()`, and the snapshot and Designer now carry the `Id`/`Period` entity that actually exists. The same commit removed three `HasMaxLength(64)` declarations on foreign keys to `nvarchar(450)` principals — SQL Server rejects the width mismatch with Msg 1753, which is why `AddTask3Features` had never successfully run anywhere.)*
6. **Category taxonomies.** Two different lists were being called "the categories". The *product* one is now owned by `Core/Entities/ProductCategories.cs` (Task 1's five values), referenced by the validators — which, since action 10, actually execute — instead of being typed out three times. The *client sector* one is still free of any single owner: `Clients/Create.cshtml` offers `Mining / Clinical / Education / Rehab / Manufacturing / Waste Management`, the analytics and map demo data use `Mine / Hospital / Education / Clinic / Other`, and `InquiryScoring`'s rank table recognises `Clinical / Hospital / Mining / Manufacturing / Pharmaceutical / Waste Management / Logistics / Retail / Education`. Three consequences worth knowing: the same account is labelled differently on the client form and on the analytics screen; `Rehab` — a value the form can save — is the only sector the score cannot rank, so it silently takes the mid-tier weight; and `Pharmaceutical`, `Hospital`, `Logistics` and `Retail` are rankable but cannot be chosen from the form at all. Nothing but a shared enum fixes this, and the enquiry queue's ordering depends on it.
7. **`VatRate` precision** is `decimal(18,2)` in the applied model versus `5,4` in the dead `QuotationConfiguration` — which is why sending `vatRate: 15` for 15 % reached SQL Server as `'15,0000'` and returned HTTP 500. The validator now rejects it before binding, with a message that says to send `0.15`.
8. **`Quotation.Status`** gained an undocumented `"Reviewed"` value in the mobile flow, outside the entity's stated Draft/Sent/Accepted/Rejected set.
9. **The model snapshot and the database disagree in both directions**, and that — not the duplicate `SalesGoals` — is the defect that actually disables the tooling.
   - Four mapped entity types (`AuditLog`, `HealthMetric`, `ClientTimelineNote`, `Appointment`) have tables in every migrated database and no entry in `AppDbContextModelSnapshot.cs`, because the migrations that created them were hand-written. EF checks the live model against the snapshot *before* it touches the database and throws `PendingModelChangesWarning` on a mismatch, so `MigrateAsync()` — the call the API makes at startup — aborted before applying any migration, on every environment, including a fresh deployment with no tables yet. The `catch` logged it and continued, which is why the deployed API could look healthy and still have no schema. Downgraded to a log line by `c909aa6`. *(Snapshot repair: still open, and deliberately not done by scaffolding — a snapshot generated from today's model would absorb the unique indexes and the `decimal(5,4)` `VatRate` from item 1 as though they had been applied, which is what would make the migration in DEPLOYMENT.md §8.1 impossible to write.)*
   - Two entity types the snapshot *does* describe (`TicketActivity`, `DeliveryStatusUpdate`) had no `CREATE TABLE` in any migration, so the differ compares the model against a snapshot that already agrees with them, finds nothing, and never emits them. Both tables are written to by the application: creating a support ticket died with `Invalid object name 'TicketActivity'`, and so did every delivery status write. Added by hand in `f327007`, which is the only route open for a table the snapshot already claims exists.
   - The practical rules are in DEPLOYMENT.md §8: read a scaffolded migration line by line, never run `dotnet ef migrations remove --force` on this chain (it rebuilds the snapshot from the previous migration's Designer, nine of them have none, and the file came out as a 21-line stub), and rebuild before regenerating a script (`--no-build` reuses stale assemblies, which makes a fixed migration look unfixed).

### 12.5 JSON schema documentation

Task 1 §4.3 required documenting "database structure and JSON schemas if a NoSQL solution is used". NoSQL was not used, so the conditional does not bind — but the equivalent artefact (a data-dictionary of the SQL schema) was also never produced. The closest thing is the machine-readable endpoint list at `GET /api/` and `/api/info`, which enumerates routes and whether each requires auth, and the OpenAPI document at `/openapi/v1.json`. **There is no Swagger UI** — `AddOpenApi()`/`MapOpenApi()` serve JSON only; Swashbuckle was never added.

---

## 13. Costs and financial projections (Task 1 §7) — **re-costed against the actual host**

The proposal's three-year model priced a stack that was not used. This section replaces that model with the plan the project actually runs on — **Monster ASP** shared Windows hosting, whose hosted sites appear as `*.runasp.net` (`verichat-crm-web.runasp.net`, `verichat-crm-api.runasp.net`) and whose shared SQL Server is reached at `databaseasp.net` — and retires the Firebase tier table.

### 13.1 What the proposal priced, and why each line is gone

| Proposed cost line (10 000 users) | Replaced by |
| --- | --- |
| Firestore reads/writes ~R2 374,84/month | Nothing. EF Core + SQL Server on a shared plan: the bill does not move with query volume. |
| Firebase Hosting bandwidth ~R655,13/month | Included in the shared plan's transfer allowance. |
| Firebase Auth ~R0,090/user >10k | `JwtHelper` + the `Users` table — zero marginal cost per user. |
| Cloud Functions ~R245,67/month | The API process itself (also where `pdfGenerator.js` became `PdfGenerator.cs`). |
| Domain / SSL R20,47/month | **[Partial]** — still a real line, but see §13.3: on the plan in use today it is not a cost, it is a **missing TLS certificate**. |
| **Total ~R3 296,11/month at 10 000 users** | **[Retired]** |
| Two-year projection R12 480,19 – R19 359,03 | **[Retired]** |

### 13.2 The actual plans

Prices as published by Monster ASP, in **US dollars**; the ZAR column uses the ECB reference rate captured for this document, **US$1 = R16,6895 on 2026-10-30** (Frankfurter), and is indicative — the invoice is in dollars and the rate moves.

| Plan | Published price | Sites | Disk | RAM per worker | MSSQL | Automatic HTTPS | ZAR/month |
| --- | --- | --- | --- | --- | --- | --- | --- |
| **Free** (what both apps are on today) | $0 | 1 | 5 GB | 256 MB | 1 database, 1 GB | **[Not built]** — subdomain only | **R0,00** |
| **Premium Single** | $1,95/month, billed annually | 1 | 25 GB | 512 MB | 2 databases | **[Built]** — Let's Encrypt | **R32,55** |
| **Multi** | $6,95/month | 10 | 10× | 10× | 10× | **[Built]** | **R116,00** |

Derived figures worth putting in a report:

| Scenario | Monthly | 36 months |
| --- | --- | --- |
| Today (two Free sites + the Free SQL allowance) | R0,00 | R0,00 |
| One **Multi** account hosting web + API + a staging slot | R116,00 | R4 175,71 |
| Two **Premium Single** accounts (one per app, as today but paid) | R65,09 | R2 343,43 |
| The proposal's Firebase model, unchanged | R3 296,11 | R118 659,96 |

So the infrastructure the project actually runs on is **96,5 % cheaper than the infrastructure it proposed** at the same user count, and the 10 000-user assumption no longer drives the number at all.

### 13.3 The honest caveat

A flat monthly fee is cheaper, but it is *flat in a way the proposal's model never was*: the cost stops scaling and the **capacity ceiling stops scaling with it**.

- **RAM, not rand, is now the constraint.** 256 MB (Free) / 512 MB (Premium) per worker process has to hold the Kestrel app, EF Core, QuestPDF document generation and the SignalR hub connections at once. At the proposal's 10 000-user scale this is the line item that fails first, and — unlike a Firestore read — it fails as an app-pool recycle or a 503, not as an invoice. Nothing in the repository measures it.
- **1 GB of MSSQL is the real database budget.** It covers the CRM tables plus `AuditLogs`, which grows with every saved change, and the QA junk rows still in the deployed database (see DEPLOYMENT.md §8). A per-operation model would have charged for this; shared hosting just silently fills up.
- **The absence of staging was a plan-shape decision, not a money one.** The proposal asked for three isolated environments; the Free plan is one site per account, which is a plausible reason the project shipped with Development + base config only. Multi's ten sites remove that excuse for R116,00/month.
- **The `R20,47/month` "Domain / SSL" line is the one the proposal got right in kind and wrong in cause.** The proposal assumed TLS would be bought; the deployed reality is that automatic certificates come with a paid plan, and the repository's own mobile `.env` comment records port 443 accepting TCP while the handshake fails. That is not a cost that was skipped, it is a feature the Free plan does not include — which is also why item 18 ("bind TLS to the API host") is a plan upgrade and a DNS name, not a code change.

### 13.4 The cost-benefit analysis is unchanged

The cost-benefit analysis is independent of the stack and remains valid as a business argument, though none of its claims are instrumented in the product:

| CBA claim | Can the system measure it today? |
| --- | --- |
| 3–5 hours/rep/week saved on quotations ≈ R15 776–R22 655/month | **[Built]** — now measurable end-to-end: the quotation PDF button works and a quotation can be saved (item 3), but there is still no time-on-task telemetry |
| 15–20 % conversion increase from ranked inquiries | **[Built]** — the rules engine exists (item 11) — the baseline-vs-hand-ranked comparison the proposal wanted can now actually be run |
| 10 % reduction in stock-out lost sales | **[Partial]** — `WarehouseAlert` data exists to support the measure; no report computes it |
| 30 % admin overhead reduction from self-service | **[Not built]** |
| Ticket resolution 5 days → 1.5 days | **[Built]** — **directly measurable** — `ResolvedAt − CreatedAt` feeds the "Avg Resolution (7d)" KPI already on the support dashboard |

**Named KPIs:** average ticket resolution time and enquiry conversion rate are the two figures the product can already produce; they should be reported as the actual KPIs instead of the proposal's uninstrumented savings.

---

## 14. Security comparison (Task 1 §6.1)

| Proposed control | Delivered | Evidence |
| --- | --- | --- |
| **Broken Access Control** mitigation via strict RBAC creating definitive trust boundaries so "users cannot view or modify cross-account records" | **[Built]** (was **[Partial]**) | RBAC is real and enforced on both tiers. The three endpoints that lacked a row-ownership check — `DeliveryController.GetByClient`, `DeliveryController.Track` and `SupportTicketController.GetById` — now have one, and `NotificationHub.Subscribe` no longer trusts a role string sent by the browser. The JWT is no longer written into page source: it stays in session, and the only copy that reaches the browser is an HttpOnly cookie carried by the SignalR handshake (`AccountController.IssueHubCookie`, which also withdraws it at logout). **Residual:** the check is per-endpoint and hand-written, so the boundary depends on every future developer remembering it — `WebApplicationFactory` integration tests across the real pipeline would be the durable guard, and there are still none. |
| **Cryptographic Failures** — encrypt at rest, never hardcode weak keys | **[Partial]** (was **[Not built]**) | `PasswordHasher` is now genuine PBKDF2 (HMAC-SHA512, per-user random salt) and old single-round SHA-256 hashes are rewritten on the next successful login, so accounts upgrade themselves without a reset. `VerichatCRM.API/appsettings.json` no longer contains the production database password — the connection string is empty and the value is expected from the host's secret store, and `Microsoft.Data.SqlClient` encrypts by default so only an explicit `Encrypt=False` is refused. **Still open, and it is the reason this row is not **built**:** the leaked password is *still the live one*. It has not been rotated, it remains in git history, and neither action is possible from a working copy — see DEPLOYMENT.md §1, §3 and §6. A secret that was committed and is merely moved out of the file is not a fixed secret. *(Password value deliberately not reproduced in this document.)* |
| **XSS** — context-aware output encoding, allow-list input validation, sanitisation library, CSP with `unsafe-inline` disabled | **[Partial]** | Razor's automatic HTML encoding gives context-aware output encoding, and `HtmlSanitizer` 9.2.1039 is referenced in all four .NET projects. Two of the three gaps are now closed: FluentValidation is registered (`AddFluentValidationAutoValidation` + `AddValidatorsFromAssembly`), so the anti-`<`/`>`/`javascript:` validators actually run, and the never-registered `XssSanitizationMiddleware` — which sanitised nothing by its own admission — is deleted and replaced by `RequestBodySanitisationMiddleware` on the inbound JSON. **Open:** there is still **no Content-Security-Policy header**, and adding one is not a one-liner because the layout pulls five CDN scripts (Bootstrap, jQuery, SignalR, Chart.js, Leaflet) and several views use inline `<script>` blocks — a working policy needs nonces, which means touching `_Layout` and every inline script. One ordering consequence worth recording: because FluentValidation runs *during model binding*, it sees the raw payload; the sanitiser runs later in the pipeline, so a validator rejecting `<script>` is doing the work the sanitiser would otherwise have done. |
| **TLS 1.3 with forward secrecy** for all external and internal traffic | **[Partial]** | `UseHttpsRedirection()` on both apps; HSTS outside Development on Web. No protocol/version pinning. The database connection no longer disables encryption, but the deployed sites are on a hosting plan that does not provision certificates automatically (§13.3), so the mobile app's own `.env` comment reports port 443 accepting TCP while the TLS handshake fails — the transport is still cleartext in practice. |
| **NIST CSF — Identify & Protect** | **[Built]** | Encryption at rest inherited from SQL Server, access controls, audit interceptor |
| **NIST CSF — Detect & Respond** (continuous monitoring, logging for unusual activity) | **[Partial]** | `AuditSaveChangesInterceptor` gives a genuine data-change trail with IP; `ILogger` throughout. **No log aggregation, no alerting, no anomaly detection.** |
| **NIST CSF — Recover** (structured backups and restoration protocols) | **[Not built]** | No backup or restore documentation, scripts or runbook in the repository. |
| Rate limiting / automated data-scraping defence | **[Not built]** | Explicitly named in the proposal's threat framing ("automated data scraping"); `AddRateLimiting` appears nowhere in the API. |
| Password reset flow | **[Partial]** | The whole scaffold exists — `PasswordResetToken`/`Expiry` columns, `GetByResetTokenAsync`, `EmailService.SendPasswordResetEmailAsync` with 1-hour-expiry copy, `ForgotPasswordRequest`/`ResetPasswordRequest` DTOs — but **no controller exposes any of it**. There is no forgot-password endpoint and no UI link, so the flow is unreachable. |
| Email notifications generally | **[Not built]** | `EmailService` is registered in DI and **injected by nobody**. Every notification path falls back to SignalR. Outbound email never sends, and no `Email` configuration section exists in `appsettings.json`, so even if wired it would only log "would send". |

---

## 15. Testing comparison

Task 1 WBS items 8.7 (Back-End Testing, 3 days), 9.4 (Implement Automated Testing, 3 days) and 10.2 (System Testing & Bug Fixing, 4 days) all promised test coverage in a pipeline.

**Actual:** `VerichatCRM.Tests` — xUnit 2.9.3, `Microsoft.EntityFrameworkCore.InMemory` 10.0.12, no Moq, no `WebApplicationFactory`. **Seven files, 71 passing cases** (the count is what `dotnet test` reports, not what the source declares: `[Theory]` attributes expand into several cases each):

| File | Subject |
| --- | --- |
| `AuthServiceTests.cs` | registration refuses a public administrator claim, and normalises the e-mail before persisting a client role |
| `ClientOwnershipControllerTests.cs` | client-scoped data isolation |
| `DashboardAnalyticsControllerTests.cs` | role-shaped analytics payloads |
| `DesignTimeAppDbContextFactoryTests.cs` | the EF design-time factory |
| `InquiryScoringTests.cs` | **two groups under one name** — eight cases for the weighted scoring engine (the four factors sum to exactly 100, nothing-stated lands mid-queue rather than last, larger and more urgent outrank smaller, an unrecognised sector or urgency is not treated as worthless, contract beats prospect beats inactive), then six for the quotation validator (fraction rates accepted, a 15 instead of 0.15 rejected with the message that fixes it, negative rates rejected, terms over the column length rejected, and a line needing a SKU and something to charge for) |
| `OrderServiceTests.cs` | order validation and stock |
| `PermissionTests.cs` | the web permission matrix (newest) |

**Note:** What this suite does not cover, stated plainly because §20 item 11 was signed off on it: the *override* rule — that a score typed by a human is never recalculated away — lives in `InquiryService.UpdateAsync` and has **no test**. The scoring function is proven; the service decision to leave its output alone is not. That is the difference between a tested calculator and a tested behaviour, and it is the first thing to add if this queue ever matters.

**Note:** And the more serious hole: `PasswordHasher` — PBKDF2/HMAC-SHA512 with a per-user salt and the rewrite-on-next-login migration for the old SHA-256 hashes, the change §20 rates as Critical — **is not tested at all**. Grep the suite for `PasswordHash`, `NeedsUpgrade`, `PBKDF2` or `Verify` and it returns nothing. The two `AuthServiceTests` cases cover role refusal and e-mail normalisation only. A credential store whose correctness is asserted by reading the code, with no test that a wrong password fails, a legacy hash upgrades, or a re-hash is idempotent, is the kind of change that silently breaks in someone's first refactor. `git log` will not catch it and neither will `ci.yml`.

All are **unit tests with in-memory fakes**. There is **no integration test, no end-to-end test, no API contract test and no performance test**. The README is honest here: *"It is not a complete integration or end-to-end suite."*

**The mobile app has zero automated tests.** `VerichatMobile/package.json` declares no `test` script and installs no test runner — there is no `jest`, `jest-expo`, `@testing-library/react-native`, `detox`, or `maestro` anywhere in the 37 runtime dependencies, and no `__tests__` directory or `*.test.ts`/`*.test.tsx` file exists under `VerichatMobile/src`. This is what made the phantom `npm test -- --watchAll=false` step in the deleted `android-build.yml` (§11.2) unrunnable: the step was copied from Create React App boilerplate and never matched this project. Mobile correctness has been verified exclusively by hand, which explains why several of the mobile defects in §18 (the plural `/api/supporttickets` 404, the three import-less service files, the `Logistics` role missing from the mobile allow-list) shipped undetected.

The count drift in the README is closed: it claimed *"eleven authentication, ownership, order, stock, role-dashboard and EF design-time context cases"*, which understated the suite even before this pass (`PermissionTests.cs` alone holds 18 cases). It now says **71 cases across seven files**, which is what `dotnet test` prints.

The UAT plan in Task 1 §8 gave six scripted scenarios. Their current status:

| UAT ID | Scenario | Status |
| --- | --- | --- |
| UAT-01 | Sales rep views ranked inquiry list in priority order | **[Passes]** — in the strong sense now — the queue sorts on a score the server **computes** from deal value, sector, contract status and urgency, and a rep's typed score overrides it (§8). "Ranked" no longer means "someone typed a number" |
| UAT-02 | Generate a one-click PDF quotation with correct line items, pricing and branding | **[Passes]** — from the staff UI — the Generate-PDF button is enabled and reaches `GET /api/Quotation/{id}/pdf`, with a Save action beside it and totals printed in rand. Closed by §20 item 3 |
| UAT-03 | Procurement manager repeats a past order; confirmation email sent; inventory warning triggered if stock low | **[Partial]** — reorder **works**; **confirmation email missing**; stock warning **works** for catalogue changes, **fails** on the order path (which rejects rather than warns) |
| UAT-04 | Client logs a ticket for a broken ventilator; auto tracking number; routed to tech support; SLA clock starts | **[Partial]** — tracking number **works**, SLA clock **works**, **automatic routing missing** (manual assignment) |
| UAT-05 | Admin views revenue forecasting dashboard with real pipeline data and projections | **[Built]** |
| UAT-06 | System alerts sales rep when a bulk order exceeds warehouse stock; banner on quotation screen; procurement notified | **[Not built]** — **cannot pass** — no quotation-screen banner; the order is rejected instead |

**2 of 6 UAT scenarios cannot pass against the delivered system, and 3 more only partially.**

---

## 16. Change management, onboarding and handover (Task 1 §8)

| Proposed | Delivered |
| --- | --- |
| Executive sponsorship (Mrs. J. Maredza as internal champion) | **[Documented only]** |
| Business-case communication, SOP alignment, bi-weekly reviews for 8 weeks | **[Documented only]** |
| **Role-based training — "no user will be trained on the full system, only what they need"** | **[Not built]** — **No training material exists in the repository.** This is the direct justification for `userguide.md` (companion file). Note the delivered permission matrix *does* make role-scoped training feasible: each role genuinely sees a different sidebar. |
| Hands-on walkthroughs with real scenarios | **[None prepared]** |
| **User Guide distribution** — "annotated screenshots, step-by-step instructions, FAQ" (Task 3 deliverable 11.6, 4 days, Rai) | **[Never produced]** — WBS item 11.6 "Create User Guide" and 11.7 "User Guide Review" are planned and unexecuted. This is the gap `userguide.md` closes. |
| Buddy system, in-system feedback form | **[Not built]** — **No feedback mechanism exists in the application** — no form, no endpoint, no table |
| Go-live plan: pre-launch data migration, soft launch to staff only, 48-hour validation, client portal launch, hypercare, formal handover | **[Partial]** — `MigrateAsync()` at API startup is the only migration mechanism, and there is **no data-migration path from Verichat's legacy spreadsheets** — the proposal assumed "data migration from existing records to Firestore" that was never built. The staged soft-launch/portal/hypercare sequence is undocumented in the repo. |
| Handover pack: source on GitHub, database access transferred, admin credentials documented | **[Partly closed]** — GitHub **[works]**; the README's secrets section is a reasonable handover instruction; **no handover pack document exists** |

---

## 17. What was built that was never proposed

Roughly a third of the delivered system has no origin in either Task 1 document. This is scope the team added, and it should be recognised as such rather than mistaken for proposal compliance.

| Unplanned capability | Where |
| --- | --- |
| **A telehealth/wellness product line — built, then retired to the server** — vitals logging, four auto-created daily goals, weekly exercise logs, device registry, lab results, QR prescriptions, appointment booking | 10 Core entities, `MobileVitalsController`, `MobileTelehealthController`, `MobileController`, migration `AddMobileCareTables`. **The mobile screens were later deleted**; only the server half remains (§12.3 Theme A). Task 1's scope statement never mentions health tracking; only the *prototype* document's mobile section did. |
| **Offline-first API layer** — the most substantial unplanned engineering in the project | `src/constants/api.ts`: per-account token-scoped GET cache in AsyncStorage (`verichat.api.cache.<hash>.<path>`), a replayable write queue (`verichat.api.queue.*`) with `Idempotency-Key` headers, NetInfo-triggered `flushQueuedActions()`, deliberate exclusion of endpoints needing server-generated IDs, and `components/offline-status-banner.tsx` with a pending-action counter |
| **Biometric authentication** in the mobile app | `expo-local-authentication`, `constants/biometricAuth.ts`, auto sign-in on `index.tsx`, opt-in flag in SecureStore |
| **Background refresh** | `expo-background-task` + `expo-task-manager`, task `verichat-assignment-sync-v2` at a 60-second minimum interval, registered from `_layout.tsx` |
| **Brand-level mobile polish** | animated splash overlay, `BrandLoader`, `AnimatedButton`, `FloatingInput`, `SkeletonCard` shimmer states, `FadeInDown` staggered card entrances, `useKeyboardAutoScroll`, ZAR `en-ZA` currency formatting |
| **Barcode scanning** | `components/barcode-scanner-modal.tsx` + `expo-camera` |
| **Interactive client map with sector pins and visit logging** | `Sales/Map.cshtml` (Leaflet + OSM), `POST /api/client/{id}/log-visit`, `MapClientDto` |
| **Lead tracker with follow-up scheduling and due/overdue logic** | `Sales/Leads.cshtml`, `FollowUpHelper.cs`, `NextFollowUpDate`, `POST /api/inquiry/{id}/follow-up` |
| **Sales goals and monthly target progress** | `SalesGoal` entity, `SalesGoalsController`, `_GoalProgress.cshtml`, mobile `SalesGoalCard` with an SVG progress ring |
| **B2B appointments with meeting-link issuance and staff notification groups** | `Appointment` entity, `AppointmentController`, `Views/Sales/Appointments.cshtml`, mobile `BookAppointmentScreen.tsx` |
| **Automatic audit trail with IP capture and secret redaction** | `AuditSaveChangesInterceptor`, `AuditLog` |
| **Purchase orders and goods-receipt stock increment** | `PurchaseOrder` + `PurchaseOrderLineItem`, `PATCH /{id}/receive` |
| **Delivery tracking with status history and carriers** | `DeliveryLog` + `DeliveryStatusUpdate` |
| **SLA auto-escalation background service** | `SlaEscalationService` (5-minute sweep) |
| **Client rep-assignment with history** | `AssignedRepId`, `AssignmentHistoryItem`, `GET /api/client/{id}/assignment-history` |
| **A chat-style client communication timeline** | `ClientTimelineScreen.tsx`, `GET /api/client/{id}/timeline`, `POST /api/client/{id}/notes` |
| **Client-initiated quote requests with a staff review queue** | `POST /api/Quotation/request`, `PATCH /api/mobile/quotations/{id}/response` (Accept/Decline in `cartScreen.tsx`), the Pipeline "Mobile quote requests" panel, and the `Terms.StartsWith("Mobile quote request")` convention tying them together |
| **Branded HTTP status pages** (404/403/500) via status-code re-execute | `UseStatusCodePagesWithReExecute`, `_StatusCard.cshtml`, `RoleHome.cs` |
| **Readiness endpoint reporting pending migrations and JWT-key presence** | `HealthController` `/api/health` |
| **A machine-readable API index** | `GET /api/` and `/api/info` enumerate routes from `EndpointDataSource` with a `requiresAuth` flag |
| **A custom Android Gradle config plugin** capping build concurrency for a low-memory host | `VerichatMobile/plugins/withAndroidGradleLimits.js` |

---

## 18. Consolidated defect register

Findings surfaced during the audit, ordered by severity. These are code facts, not proposal gaps. Items 1–34 cover the solution as a whole; items 35–52 are the mobile-client audit, added after the first draft of this document understated what shipped in Expo.

**Status marks were added when §20 was carried out.** **[Closed]** = closed in this working copy (built, 71 tests green, not yet committed or deployed). **[Partly closed]** = partly closed, with what remains stated. **[Open]** = still open. Anything requiring the hosting control panel, the database or the shared remote stays **[Partly closed]** or **[Open]** by definition, because a working copy cannot do it.

### Critical
1. **[Partly closed]** **Committed production database credential.** `VerichatCRM.API/appsettings.json` contained a live SQL Server connection string with an embedded password and `Encrypt=False`, immediately below a comment asserting the password was no longer committed. **Closed in code:** the value is gone from the file, the connection string is empty with a comment naming the secret store as its source, and encryption is on by default. **Open:** the credential itself has not been rotated and still sits in git history — both need the Monster ASP panel and a decision about rewriting 11 shared branches (DEPLOYMENT.md §1, §3 and §6).
2. **[Closed]** **No CI/CD runs today**, yet the README documented `.github/workflows/ci.yml` in two places. The file is restored from `7adceef` and both README claims are now true.
3. **[Closed]** **Password hashing was unsalted single-round SHA-256** with a hard-coded global salt behind a "PBKDF2" comment. Now PBKDF2/HMAC-SHA512 with a per-user salt and rewrite-on-next-login. **Untested** — see §15; this is the change most in need of a test and the one with none.
4. **[Closed]** **Unauthenticated-by-policy data exposure:** `DeliveryController.GetByClient`/`Track` and `SupportTicketController.GetById` lacked ownership checks; `NotificationHub.Subscribe(role)` let any user join any role group. All four fixed, with `ClientOwnershipControllerTests` covering the pattern.

### High
5. **[Closed]** **JWT leaked to client-side script** — non-HttpOnly, non-Secure cookie plus `ViewBag.ApiToken` JSON-embedded in page source (`PipelineController.cs:32`, `TelehealthController.cs:24`). The token now stays in session and only the SignalR handshake carries it, in an HttpOnly cookie that logout withdraws.
6. **[Closed]** **No CSRF protection** on `Pipeline/MoveDeal` and `Quotation/Save` (POST-JSON without `[ValidateAntiForgeryToken]`), while every sibling action had it.
7. **[Closed]** **Quotation PDF unreachable from the staff UI** — the core promised deliverable was disabled. Wired to `GET /api/Quotation/{id}/pdf`, with a save action; closes UAT-02.
8. **[Closed]** **Warehouse "Reorder Now" threw `ReferenceError`** — `js/warehouse-alerts.js` needed `api` from `js/api.js`, which the page never loaded. Converted to a form POST against the real purchase-order endpoint; `js/api.js` was never referenced by anything and is deleted.
9. **[Partly closed]** **Global notification bell connected to the wrong host** — `notifications.js` fell back to `window.location.origin` because `window.verichatApiUrl` was never emitted by `_Layout`, and the Web app hosts no hub. The layout emits it now (`_Layout.cshtml:46`), the script builds `${API_BASE}/hubs/notifications` and requests it with credentials (`notifications.js:15,54`), and the API accepts the token from the `JwtToken` cookie for any `/hubs` path (`Program.cs:145-149`) — which is what made it possible to take the JWT out of page script in defect 5 without losing the hub. **The origin bug is fixed; the authentication is not, and the reason is the cookie's scope.** A cookie set by the web app is only ever sent to the host that set it. On a developer machine that holds: `localhost:5000` and `localhost:5001` are the same host, cookies ignore ports, and the hub connects. In production the web app and the API are two different `*.runasp.net` subdomains, so the browser never carries `JwtToken` to the API and every hub handshake arrives anonymous against a `[Authorize]` hub — **the bell works in Development and 401s on the deployed pair**, before CORS (defect 13) even gets a chance to refuse it. The fix is not a cookie tweak: either a short-lived hub token minted by the web app and passed as the `access_token` query parameter the API already reads, or the API and web app fronted on one host. Until then this row should be read as "points at the right place, still silent in production".
10. **[Closed]** **`PipelineController` had two public constructors**; whichever DI resolved, one dependency stayed null and `Index` threw. Merged into one three-parameter constructor. This is worth reading together with defect 5's page: whichever candidate the container picked, the sales pipeline board was a non-deterministic 500, so a developer who saw it working once would not have believed the report.
11. **[Closed]** **Procurement managers were locked out of Warehouse Alerts** — the sidebar OPERATIONS section was keyed on `procurement.dashboard`, but the action requires `warehouse.alerts`, which only Admin held. Click → `/Account/NoAccess`. The permission is granted to the role now **and** the sidebar gate reads the permission rather than the role, so the two can no longer drift apart. See also defect 53: the link they were being sent to could not have rendered anyway.
12. **[Open]** **No CSP header**, while loading five CDN scripts and inline scripts — the exact risk the proposal's XSS section addressed. Still open; needs a nonce scheme through `_Layout` and every inline `<script>`, not a static header.
13. **[Partly closed]** **`Cors:AllowedOrigins` is `[]` in committed config** and the fallback list applies only in Development, so `WithOrigins([])` admits **no** browser origin in production. The list must be populated at deploy time — the value depends on the real host name, which is why the committed file stays empty. DEPLOYMENT.md §3 carries it as an operator step.

### Medium
14. **[Split]** **All seven `Data/Configurations/*` classes were dead.** They are applied now (`ApplyConfigurationsFromAssembly`), and `InquiryConfiguration` ships the `IX_Inquiries_Status_Score` index. **Still open:** unique indexes on `Users.Email`, the two tracking numbers, `Asset.SerialNumber` and `Quotation.QuoteReference` — the scaffolded migration that would have carried them proposed to rebuild `SalesGoals` and re-add dropped columns instead, because the chain is broken on arrival (§12.4), so it was set aside rather than shipped. That specific reason is closed (`113d04d` deleted the duplicate `SalesGoals` block), and a new one replaced it: a scaffold from today's model opens with `CREATE TABLE` for the four hand-written tables the snapshot has never seen and would abort on "already an object named" (§12.4 item 9). Re-confirming the deferred set against a scratch database built by the app itself: 187 unique indexes, all of them keys, no `UI_…`, and `Quotations.VatRate` still `decimal(18,2)`.
15. **[Closed]** **`FluentValidation` never registered**; the five validators (including the anti-XSS whitelist) never executed. Registered, plus a quotation validator that catches the `vatRate: 15` vs `0.15` bug at the boundary instead of in SQL Server. `HtmlSanitizer` is now genuinely used by `RequestBodySanitisationMiddleware`.
16. **[Closed]** **`ExceptionHandlingMiddleware`, `JwtMiddleware`, `Filters/AuthorizeAttribute`, both `Extensions/*` classes, `PipelineService`/`IPipelineService`, `Core/Enums/DealStage`, `OrderStatus`, `UserRole`, `IRepository`** — all empty or unregistered stubs, deleted. Worth singling out two: `Filters/AuthorizeAttribute.cs` was an empty class that would have *silently satisfied* any `[Authorize]` in a file that missed the ASP.NET Core using — a security landmine shaped like dead code — and `Core/Enums/UserRole` was a fourth, stale role vocabulary (4 values, `SupportAndLogistics` merged) beside the canonical 7-role `UserRoles`. **Note:** deleting the exception-handling stub leaves the API with **no** global exception handler; that gap is recorded as defect 56 rather than papered over.
17. **[Open]** **`DataSeeder` contradicts three separate "NO demo data" comments** and injects 4 clients + 6 products into every fresh database.
18. **[Open]** **Password-reset and email infrastructure complete but unwired** — no endpoints, no UI, no `Email` config section, no injectors.
19. **[Open]** **`MobilePrescription` and `LabResultRecord` are read-only with no writer path at all**; `LimsConnected` is hard-coded `true`.
20. **[Open]** **`MobileService.GetDashboardAsync` hard-codes** `RecurringCount = 0` and `NextRecurringOrder = null`.
21. **[Closed]** **Currency inconsistency** — quotation totals rendered `toLocaleString('en-US', {currency:'USD'})` while the rest of the application is in ZAR. Pinned on the PDF and the totals.
22. **[Partly closed]** **Three sector taxonomies in active use** (§12.4 item 6). The *product* categories now have one owner (`ProductCategories.cs`) used by the validators; the *client sector* vocabulary is still spread across the client form, the analytics demo data and the scoring rank table, with `Rehab` unrankable and four rankable sectors unselectable.
23. **[Open]** **`OrderController.PATCH status`** accepts an arbitrary string with no whitelist.
24. **[Open]** **`TicketService` escalation activity** records `FromValue` after the status is overwritten, so both values read "Escalated".
25. **[Open]** **`QuotationRepository.GenerateNextReferenceAsync`** counts by year (`count+1`) rather than taking the per-year maximum — a collision after any deletion.
26. **[Open]** **`ClientService.DeleteAsync`** always returns `true` (`ContinueWith(_ => true)`).
27. **[Open]** **Performance:** `ClientService.GetAllAsync` N+1 per client; `DeliveryLogService.UpdateStatusAsync` loads the whole table to change one row; `UsersController.GetAll` wraps sync EF in `Task.Run`.
28. **[Partly closed]** **Seven hand-written migrations lacked `.Designer.cs`.** They now carry the `[DbContext]` + `[Migration]` pair that makes a migration *discoverable* by tooling at all — `AddHealthMetricTable` was previously invisible to `dotnet ef`, which is how a table can be "migrated" and yet not exist on a fresh database. Regenerating the Designer files is still open, and trusting any Designer-derived baseline is not possible while the snapshot story in §12.4 stands.
29. **[Closed]** **Orphaned views.** The accurate picture, after checking every explicit `View("…")` and `View("~/…")` path in `Controllers/` against the files on disk: what was genuinely unreachable and is now deleted was `Views/View.cshtml` (the Visual Studio scaffold placeholder), `Views/Procurement/LogisticsTracker.cshtml` (no action named `LogisticsTracker` exists in `ProcurementController`) and `Views/Telehealth/Appointments.cshtml` — a read-only list of the same model that `Views/Sales/Appointments.cshtml` renders with assignment, approval and meeting-link forms, so it was the superseded earlier draft of that screen. Orphaned view-models deleted: `AdminDashboardViewModel`, `DashboardViewModel`, `ErrorViewModel`, `LogisticsTrackerViewModel`. **Two orphaned views were kept** — `Views/Procurement/Dashboard.cshtml` and `Views/Support/Dashboard.cshtml` — see 29b for why they are the un-shipped per-role dashboard designs rather than clutter. `ProcurementDashboardViewModel`, `SupportDashboardViewModel` and `QuotationViewModel` stay, because those views reference them.
29b. **The "role-based dashboards" are one shared view, not three.** `AdminController.Dashboard` (`AdminController.cs:26`), `ProcurementController.Dashboard` (`ProcurementController.cs:47`) and `SupportController.Dashboard` (`SupportController.cs:28`) each `return View("~/Views/Dashboard/Index.cshtml", dashboard)` with the same `DashboardAnalyticsResponse` from `GET /api/dashboard/analytics`. The only per-role difference is that payload — chiefly `RoleLabel` — plus the `SetHeader` subtitle. This is why `Views/Procurement/Dashboard.cshtml` and `Views/Support/Dashboard.cshtml` look like oversights: each is bound to its own typed view-model (`ProcurementDashboardViewModel.ManagerName`, `SupportDashboardViewModel.OpenTickets`) and each was written as the role-specific screen the proposal describes, then bypassed when every role was pointed at the generic one. Deleting them would be a product decision — it discards the only concrete design for the split the proposal asked for — so they are kept, unused, and named as such. Making them live means building the per-role payloads behind them, which is §20 item 11's dashboard half and is not a cleanup.

**Correction to this register, from the audit itself.** An earlier revision of defect 29 asserted that all three dashboard views *were* returned by their actions and that the first two were login-redirect targets, and separately asserted that `Views/Sales/Appointments.cshtml` was a duplicate safe to delete. Both statements were wrong, and the second was acted on: deleting it left `TelehealthController.cs:27` — which renders it by explicit path, and which the sidebar links to at `_Sidebar.cshtml:95` — pointing at a missing view, so **Client Appointments would have thrown "The view '~/Views/Sales/Appointments.cshtml' was not found" for every role that can see the menu item**. `RedirectToAction("Dashboard", "Procurement")` redirects to an *action*, not to a view file, and that action renders the shared dashboard; deleting the role-specific view file would not have broken sign-in. The file is restored and the mistake is recorded here rather than quietly unwound.
30. **[Closed]** **Twelve (actually eleven) `*.cshtml.cs` empty Razor-Page `PageModel` code-behinds in an MVC app**; `Views/View.cshtml` scaffold leftover; `Areas/**` folders empty with no `MapAreaControllerRoute`. All deleted, along with the `<Folder Include>` entries that were keeping the empty areas alive in the project file.
31. **[Closed]** **Six unreferenced CSS files, two `.backup.css` copies, dead `ApiSettings` config section, `js/api.js` never loaded, `Roles.SelfRegisterable` never read.** Deleted — including the Web app's own `SelfRegisterable`, which was a second copy of the list and had already drifted from the one the API enforces (no Medical Student), so the form and the server were validating against different sets. Also deleted: `wwwroot/js/logistics-tracker.js`, `wwwroot/pages/home.backup.css` and `Views/Shared/_Layout.cshtml.css` (scoped CSS with no `@scope` block and no `*.styles.css` bundle linked anywhere, so it never reached a browser). `wwwroot/images/logo.png` is **not** referenced by any view, stylesheet or script — the UI uses the text brand mark instead — but at 3.4 KB it is a brand asset rather than clutter and is left in place, deliberately.
32. **[Closed]** **Filenames with embedded spaces:** `ClientConfiguration .cs`, `SupportApiService .cs`, `PurchaseOrderService .cs`, `WarehouseAlertService .cs` — all four renamed with `git mv`, so the rename is recorded rather than appearing as a delete plus an add.
33. **[Closed]** **Oversized artefacts at the repository root — and a correction to how they were described.** This register originally said a 556 MB `VerichatCRM.Infrastructure.zip` was **committed** at the root alongside `_build.log`, `build-out.txt`, `signalr-install.log`, `signalr-remove.log`, `obj/`, `tmp/` and `.vs/`. Only two of those were ever in git: `git ls-tree HEAD` shows `build-out.txt` and `tmp/`, and `git log --all --diff-filter=A` across every branch finds no zip and no log file — the zip, the three logs, `obj/` and `.vs/` are local build residue on the developer machine, already matched by ignore rules. That distinction matters, because the only history-purge obligation in this repository is the `appsettings.json` password (defect 1); nothing else needs rewriting. Work done: `build-out.txt` (which contained nothing but an MSBuild error about a solution file that does not exist), `tmp/pdfs/task1.txt` and `Core/Entities/User.cs.bak` are deleted, the zip and the local logs are off disk, and `.gitignore` now additionally matches `*.zip`, `tmp/`, `build-out.txt`, `*.bak` and `*.orig` so the residue cannot come back through a careless `git add`.
34. **[Partly closed]** **`VerichatCRM_FeatureWalkthrough.md` is substantially inaccurate** — it describes the project as "appears to be a .NET-based application", claims only Infrastructure/Tests/Mobile exist, states real-time sync and offline caching are implemented "React Native ↔ C# bridge", and lists "next steps: explore full project structure". §20 item 13 asked for it to be deleted; it has been **kept**, because it is authored prose the team restored deliberately rather than generated dead code, and it is now accurate in neither respect. It needs a rewrite or an owner's decision, not an agent's deletion.

### Late server-side findings — discovered while carrying out §20

These are new to this document; none of them were in the original register.

53. **[Closed]** **`Views/Account/NoAccess.cshtml` never existed**, while `RequirePermissionAttribute.cs:44` and `AccountController` both redirect to `Account/NoAccess`. Every permission denial in the application therefore ended in an unhandled "view not found" — a 500, not the 403 it was written to produce. That includes the *correct* enforcement: a user with no rights did not see "you don't have access", they saw a crash. `NoAccess()` now renders the branded `Views/Shared/Unauthorized.cshtml` that the status-code pipeline already uses for 403, so the denial path and the pipeline path show the same page.
54. **[Closed]** **A web-registered or admin-reassigned client account had no `Clients` row.** Only `MobileService.EnsureClientAccountAsync` created one, and only from `MobileAccountController`. Orders, quotations and the enquiry queue are all keyed on `Clients.ClientId`, so the first write failed. Now the admin role change creates it (`UsersController.UpdateRole`), and `UserRoles.ClientAudience` names the two roles that need it.
55. **[Closed]** **`decimal` model binding is culture-sensitive and the host is `en-ZA`.** A comma is that culture's decimal separator, but `input[type=number]` only accepts a dot, so a value rendered from `Model.EstimatedValue` came back unreadable on post. Web now has `InvariantDecimalModelBinderProvider`, and Razor renders numeric inputs with `CultureInfo.InvariantCulture`.
56. **[Open]** **The API has no global exception handler** and returns exception text to callers. Deleting the empty `ExceptionHandlingMiddleware` (defect 16) removed the stub but never created the behaviour; the project needs a real handler, which is why §20 item 13 says "delete or implement" and this stays open rather than being logged as fixed.
57. **[Closed]** **Two endpoints were registered for `GET /health`** — the controller's liveness action and a `MapGet("/health")` in `Program.cs` — so every request to it raised `AmbiguousMatchException`, i.e. **HTTP 500 on the one URL a hosting provider's uptime monitor calls**. The duplicate registration is removed and the comment left in its place explains why: `/health` and `/api/health/live` are liveness, `/api/health` is readiness and does check the database. The remaining wrinkle is naming — `/api/health` answering the readiness probe while `/health` answers liveness is easy to misconfigure in a control panel — so DEPLOYMENT.md §7 states which URL to point a monitor at.
58. **[Open]** **`HealthMetricsController` and the mobile wellness surface are unreachable as a group** — the tables exist, the endpoints exist, the Expo screens were deleted (§17/§19.2b). `POST /api/healthmetrics` has no caller anywhere in the repository.
59. **[Partly closed]** **Razor HTML-encodes JSON placed in a `<script>` block**, so `@Json.Serialize(...)` inside an inline script silently turns `"` into `&quot;` and the script fails to parse. Anything passing structured data to page script must use `<script type="application/json">` plus `JSON.parse`, or `Json.Serialize(...).ToString()` with explicit `Html.Raw` on a validated payload. This is why the JWT-in-page-source fix (defect 5) could not simply be moved to a different page variable.
60. **[Open]** **`NU1902` / `NU1903` on `System.Security.Cryptography.Pkcs` 9.0.11** — a moderate and a *high* severity advisory, reported on restore in all six .NET projects and therefore in the CI log. It comes in transitively, so the fix is a `PackageReference` override on the 10.0.x line, and it should be taken before this ships, not after.
61. **[Closed]** **`SalesGoal` existed twice in one file** with two incompatible shapes (`GoalId`/`TargetAmount`/`Type`/`StartDate`/`EndDate` beside `Id`/`Target`/`Achieved`/`Period`), and `AppDbContext` declared the `SalesGoals` `DbSet` twice — the file did not compile. Resolved to the `Period`/`PeriodLabel` shape, the duplicate `Entity<SalesGoal>` mapping and `DbSet` removed, with a comment naming the migration that still describes the other one. See §12.4 item 5 for why that is a schema decision and not just a compile fix.
62. **[Closed]** **`AppDbContextModelSnapshot.cs` in `HEAD` does not compile** — unbalanced braces and several `modelBuilder.Entity` blocks left unclosed, which meant `dotnet ef` had no readable baseline and any migration added against it would have been generated from a broken model. Rebuilt mechanically from `20261030204733_AddTask3Features.Designer.cs`, then hand-extended for the inquiry columns.
63. **[Partly closed]** **Explicit-path views are invisible to both directions of a reachability check**, which is how defect 29 ended up wrong in two places at once. `return View("~/Views/Sales/Appointments.cshtml", …)` resolves at render time only: the compiler never sees it, no test touches it, and a survey that walks *actions → convention view names* reports the file as unused while a survey that walks *views → action names* reports the action as covered. The only safe procedure is to grep every `View("…")` string literal, confirm each literal path exists on disk, and then confirm each `.cshtml` is named by at least one action or one literal. Of `VerichatCRM.Web`'s 26 reachable controller-folder views, **3 are reachable only through an explicit view name or path, never through an action of the same name** — `Sales/Appointments.cshtml` (literal path from `TelehealthController.cs:27`; `SalesController` has no `Appointments` action), `Inquiry/Form.cshtml` (`View("Form", …)` from a *private* `Form` helper, so no action is named `Form`) and `Procurement/ProductForm.cshtml` (same shape, via `View("ProductForm", …)`). `Dashboard/Index.cshtml` is reached both ways — by convention from `DashboardController.Index` and by literal path from three other dashboards — which is why it survives either survey. So roughly one view in eight is a blind spot for anything reasoning from conventions. **Not fixed:** moving each view into its own controller's folder would change paths that work today, and that is not a cleanup.

### Mobile client (Expo) — found during the mobile audit

These are the defects the absence of any mobile test runner (§15) allowed through.

**High**

35. **Three service files call endpoints that do not exist.** `src/services/NotificationsService.ts`, `src/services/PushNotifications.ts` and `src/services/RealTimeTracker.ts` all target `/api/notifications`, `/api/notifications/device-token`, `/api/notifications/send` and `/api/notifications/{id}/read`. **There is no `NotificationController`** — the 21 controllers are listed in Appendix A; notifications are served by `MobileController` and the SignalR `NotificationHub` at `/hubs/notifications`. Every one of these calls 404s.
36. **`RealTimeTracker.ts:32,53` uses the plural `/api/supporttickets`** while `SupportTicketController` is routed `api/[controller]` → **singular `/api/supportticket`**. A second, independent 404. (The working call site in `EquipmentStatusScreen.tsx:39` correctly uses the singular form, which is how the divergence was missed.)
37. **All three files are import-less orphans** — nothing in `src/app/` imports `NotificationsService`, `PushNotifications` or `RealTimeTracker`, so the 404s have never been observed at runtime. They are dead code that *looks* like a live feature, which is the more dangerous version.
38. **The SignalR client is entirely commented out and references a third, non-existent host.** `RealTimeTracker.ts:84` reads `// .withUrl('https://api.verichat-crm.com/hub/notificationHub')`. That domain is not the deployed API (`verichat-crm-api.runasp.net`) nor the Web app, the hub path is wrong (`/hubs/notifications`), and no `@microsoft/signalr` client is active anywhere in the mobile app. **The mobile client therefore receives no real-time updates at all** — it relies on the offline-cache polling layer in `api.ts`.
39. **`Logistics` is absent from `STAFF_MOBILE_ROLES`.** `src/constants/api.ts:310-315` whitelists four roles — System Administrator, Sales Representative, Procurement Manager, Field Support Engineer — while the server defines seven (`UserRoles.cs:13` and `Roles.cs:14` both declare `Logistics`). A real Logistics user authenticates successfully against `/api/auth`, is then **rejected by `isStaffMobileRole()`** on the staff path. The role exists in the web UI, is assignable in `Users & Roles`, and cannot use the app.
40. **No token refresh anywhere.** `api.ts:383` and `api.ts:141` both respond to HTTP 401 with `setAuthToken(null)` — a hard sign-out. JWTs are the only credential and there is no refresh-token endpoint, so **session length is fixed by the JWT expiry** and a client who leaves the cart open while idle loses it (compounding defect 41).
41. **The quote cart is memory-only and silently lost.** `src/constants/cartStore.ts:16-17` holds `let lines: CartLine[] = []` and `let placedOrders: PlacedCartOrder[] = []` as module-level variables — no AsyncStorage, no SecureStore, no server-side draft. Force-close the app, or hit defect 40, and **a half-built quotation request disappears with no warning**. For a client assembling a multi-item order this is the most user-visible failure in the mobile app.
42. **Committed `VerichatMobile/.env` sets cleartext HTTP:** `EXPO_PUBLIC_API_URL=http://verichat-crm-api.runasp.net`, contradicting the `https://` fallback hard-coded in `api.ts`. The file's own comment is candid that the host "currently only serves cleartext HTTP (port 443 accepts TCP but the TLS handshake fails)" and warns that *"Android and iOS release builds block cleartext traffic"* — so **a release build as configured today cannot reach the API at all**, and the development build transmits JWTs and credentials over an unencrypted link. This is also the direct cause of the proposal's TLS 1.3 requirement being unmet on the mobile side.

**Medium**

43. **A visible "Log in with SSO" button that does nothing.** `adminLogin.tsx:38-42` defines `handleSSOLogin` as `Alert.alert('SSO', 'Single sign-on is not configured yet.')`, and line 107 renders it as a real `TouchableOpacity` alongside the working login. Users will click a primary-looking control and be told the feature is missing. Either implement `expo-auth-session` or remove the button.
44. **Push notifications are configured against a fictional project.** `PushNotifications.ts:54,146` pass `projectId: '12345678-1234-1234-1234-123456789012'`, while `app.json:63` carries the real EAS id `c238e4c2-2c5f-4492-abd9-07ff2d3a1302`. Independently, **`expo-notifications` is not listed in `app.json.plugins`** (the nine configured plugins are router, the Gradle-limits plugin, splash-screen, camera, secure-store, local-authentication, background-task), so notification permissions are never requested at build time.
45. **Privacy / Terms / About are dead rows.** `settingsScreen.tsx:49-54` registers `privacy`, `terms` and `about` in `MORE_ROWS`, rendered at lines 152-159, with **no content behind any of them**. For an app handling client health and equipment data, an inert Privacy entry is a compliance gap as well as a UX one.
46. **A second, dead login form** survives in `src/app/explore.tsx` with the comment `//TODO: call your authentication endpoint here`, next to the real `clientLogin`/`adminLogin` screens — a route an Expo Router link-map can reach but which cannot authenticate.
47. **`EquipmentStatusScreen.tsx:39` hard-codes `priority: 'Medium', category: 'Technical'`** into every ticket a client raises from an asset. The client cannot choose severity, so **a failed ventilator and a cosmetic issue both enter the SLA queue as Medium**, undermining the escalation logic `SlaEscalationService` depends on. The proposal's dynamic priority handling is lost at the point of data entry.
48. **Dead routes and components** shipped in the bundle: `Dashboard.tsx`, `ClientCommunicationScreen.tsx`, `ClientTimelineScreen.tsx`; components `web-badge`, `hint-row`, `external-link`, `ui/collapsible`, `AnimatedIcon`.
49. **Dead exports** never called from any screen: `registerStaff`, `quickReorder`, `submitCartOrder`, `recordPlacedOrder`, `getPlacedCartOrders`, `clearCart`.
50. **`GoalsScreen.tsx` is a component filed under `src/app/`.** It exports `SalesGoalCard` and is imported by `DashboardScreen.tsx:8`, but because Expo Router treats every file in `src/app/` as a route, it needed a `href: null` suppression entry in `app-tabs.tsx:42`. Naming it `…Screen` and leaving it in the routes directory after the wellness module it belonged to was retired (§19.2b) is a structural smell; it belongs in `src/components/`.
51. **Native/web divergence:** `app-tabs.web.tsx` does **not** implement the staff-role tab hiding that `app-tabs.tsx` does, so the web export shows clients the staff tabs. Combined with an incomplete `dist/` web export committed to the repo, the web build is not a usable fallback.
52. **Licence and hygiene:** the unmodified Expo MIT licence header (`Copyright (c) 2015-present 650 Industries`) is carried in generated files; `src.zip` sits untracked in the mobile folder; and the custom Gradle plugin `./plugins/withAndroidGradleLimits` exists to raise build memory, which signals the Android build is at its limits without that being documented.

---

## 19. Summary of the three "what did we end up using" answers

### 19.1 Features we ended up using
Client directory and unified profiles with key personnel and a merged timeline; the inquiry queue with rule-derived scoring plus a manual override, filters, follow-ups and lead tracking; the drag-and-drop Kanban pipeline with weighted forecast and monthly sales-goal progress; the product catalogue with CRUD, images, low-stock thresholds and warehouse alerts; a mobile quote-request cart with server-side pricing and a staff review queue; client Accept/Decline on issued quotations; **quotation PDF generation and saving from the staff UI**; multi-line ordering with server-validated stock deduction and one-tap reorder on the API side; equipment/asset tracking with warranty and calibration expiry and service logs; a ticket hub with SLA deadlines, automatic escalation, activity timelines and photo evidence; delivery tracking with status history; purchase orders with goods receipt; role-based dashboards and a four-chart BI analytics screen; an interactive Leaflet client map; B2B appointment booking with meeting links; a staff-facing mobile operations dashboard (assets, tickets, deliveries, sales goal); a client-facing mobile app with catalogue, barcode search, quote cart, equipment status, appointment booking, profile and settings; biometric sign-in; an offline cache-and-replay queue; background refresh; automatic audit logging; health and readiness endpoints.

### 19.2 Features we did **not** end up using (proposed but absent)
AI or learned inquiry scoring (the weights in `InquiryScoring.cs` are hand-tuned constants and nothing is fed back from outcomes); student discounts; secure checkout and payment processing; digital invoicing; SLA-based *automatic* routing to a team; the enterprise/employer wellness tracker; the telemedicine video console and in-app chat; HMS/LIMS integration (both faked); Bluetooth device syncing; the logistics live vehicle map; a real rules engine behind the Admin "Rules Engine" screen, which is still a static table; bulk-order stock *warning* on the quotation screen; error-rate and system-load analytics; rate limiting; a Content-Security-Policy header; a feedback form; a password-reset user journey; outbound email of any kind; push notifications (no endpoint exists); order confirmation and delivery-tracking screens for the client; a data migration from legacy records.

### 19.2b Features we built and then removed
Uniquely worth recording, because the server still carries them: the **entire mobile wellness and telehealth surface** — vitals logging, the four auto-created daily goals, weekly exercise logs, BLE device sync, the lab-result viewer and the QR prescription screen — was implemented in Expo and then **deleted** (`feature.md:109`). `react-native-ble-plx` was removed from dependencies. The ten supporting tables, four controllers' worth of endpoints and one migration remain, unreachable. `Dashboard.tsx` and `ClientCommunicationScreen.tsx` are also dead routes still on disk.

### 19.3 Technology we ended up using vs. discarding

**Adopted:** .NET 10 · ASP.NET Core MVC + Razor · ASP.NET Core Web API · JWT bearer (HS256) · Entity Framework Core 10.0.12 · SQL Server · SignalR · QuestPDF 2026.9.1 · Expo SDK 57 · React 19.2.3 · React Native 0.86.3 · TypeScript 6.0.3 · Expo Router (with a custom tab component, not a `(tabs)` group) · Expo SecureStore · AsyncStorage · NetInfo · expo-camera · expo-local-authentication · expo-background-task + expo-task-manager · expo-linear-gradient · expo-image · expo-splash-screen · expo-glass-effect · react-native-reanimated 4.5.1 + worklets · react-native-svg · react-native-gesture-handler · react-native-safe-area-context · react-native-screens · react-native-web · Bootstrap 5 · jQuery · Chart.js · Leaflet · xUnit · EF Core InMemory · EAS Build profiles (development / preview / production) · runasp.net Web Deploy · GitHub (PRs only).

**Discarded from the proposal:** Firebase Authentication · Cloud Firestore · Firebase Hosting · Cloud Functions · Firebase Security Rules · the Firestore client SDK and gRPC channel · the React SPA · Firestore's pay-per-operation cost model · Gitflow · GitHub Actions CI/CD · Azure · TLS 1.3 pinning · BCrypt/Argon2 (mentioned, not adopted) · `react-native-ble-plx` (added, then removed).

**Referenced but never actually used — .NET:** FluentValidation.AspNetCore (all four projects; validators written but never registered) · HtmlSanitizer (all four; middleware sanitises nothing and is never registered) · Entity Framework Core packages in `VerichatCRM.Core` · `Microsoft.AspNetCore.OpenApi` in Core and Infrastructure · all seven `IEntityTypeConfiguration` classes · `IDatabaseInitializer` · `IEmailService`'s implementation · the Observer and Facade patterns as formally named artefacts. **Four of these were resolved by §20:** FluentValidation is registered and its validators run, `HtmlSanitizer` is used by the new `RequestBodySanitisationMiddleware`, and the seven configuration classes are applied via `ApplyConfigurationsFromAssembly`. `IDatabaseInitializer` is **not**: the stand-alone `VerichatCRM.DatabaseInitializer` project was deleted, but the interface and its implementation survive in `VerichatCRM.Infrastructure/Services/DatabaseInitializer.cs`, are still registered at `VerichatCRM.API/Program.cs:93`, and are still never requested by anything — `MigrateAsync` is reachable only if a constructor asks for it, and no constructor does.

**Referenced but never actually used — mobile (10 of 37 runtime dependencies):** `@expo/ui` · `@react-native-community/datetimepicker` (the calendar is hand-built) · `expo-constants` · `expo-device` · `expo-font` · `expo-glass-effect` · `expo-haptics` · `expo-status-bar` · `expo-system-ui` · `react-native-qrcode-svg`. `expo-notifications` is referenced only by `PushNotifications.ts`, **which no screen imports**.

**Declared but non-functional:** `expo-notifications` push delivery (no `expo-notifications` entry in `app.json.plugins`, a placeholder `projectId` of `12345678-…` in code that contradicts the real EAS project id, and **no `/api/notifications` controller on the server**) · the mobile SSO button · the Settings "Privacy / Terms / About" rows · the "Schedule Service" button on the assets screen · the catalogue sort dropdown · the warehouse stock-level chart. **Two entries from the original list are no longer non-functional:** the warehouse "Reorder Now" button (was a `ReferenceError`; it is now a form POST that raises a real purchase order — defect 8) and the decorative "Live Map View" (the view that contained it was deleted as unreachable — defect 29).

---

## 20. Recommended next actions

Ordered by the ratio of risk removed to effort.

**Status column added after the §20 work was carried out.** "Closed in code" means the change is in this working copy, built with 0 errors and covered by 71 passing tests; none of it is committed, and none of it has been deployed. Anything an operator with control-panel or remote access must do is listed separately, because it cannot be done from a working copy.

| # | Action | Status after this pass |
| --- | --- | --- |
| 1 | **Rotate the committed database credential** and move it to the host's secret store; add `Encrypt=True`; scrub git history. *(Critical, hours.)* | **[Partial]** — **Code half closed** — `appsettings.json` now carries an empty connection string plus a comment saying where the secret lives, and the SQL driver encrypts by default (only an explicit `Encrypt=False` is rejected). **Open:** the password itself is still valid and still in git history. Rotation on `databaseasp.net`, the API's environment-variable binding and the history rewrite are operator actions; the rewrite touches 11 shared branches and must not be done casually. See DEPLOYMENT.md §1, §3 and §6. |
| 2 | **Restore `.github/workflows/ci.yml` from git**, do **not** resurrect `android-build.yml`, delete the README's false claims. *(High, under an hour.)* | **[Closed]** — `ci.yml` restored from `7adceef`, `android-build.yml` left deleted, README corrected. `npx tsc --noEmit` on the Expo app is clean, so the feared dead-export errors no longer block the pipeline job. |
| 3 | **Enable the Generate-PDF button** and add a save action. *(High, hours.)* | **[Closed]** — the button posts to the working `GET /api/Quotation/{id}/pdf` and the quotation is saved; closes UAT-02. The ZAR currency formatting on the PDF was pinned at the same time. |
| 4 | **Fix the ownership checks** on `DeliveryController.GetByClient`/`Track` and `SupportTicketController.GetById`; authorise `NotificationHub.Subscribe`. *(Critical, hours.)* | **[Closed]** — ownership is enforced on all three and `Subscribe` no longer trusts a client-supplied role string. Covered by `ClientOwnershipControllerTests`. |
| 5 | **Stop leaking the JWT to page script**; HttpOnly Secure cookie; `[ValidateAntiForgeryToken]` on the two JSON POSTs. *(High.)* | **[Closed]** — the token is in session and in an HttpOnly cookie for the SignalR handshake only; `ViewBag.ApiToken` is gone. The cookie rules for a cross-port localhost hub and the logout-withdrawal case are documented at `AccountController.IssueHubCookie`. |
| 6 | **Replace `PasswordHasher`** with PBKDF2/BCrypt/Argon2 and migrate on next login. *(Critical, days.)* | **[Closed]** — **in code, with no test** — PBKDF2 (HMAC-SHA512, per-user salt) with `NeedsUpgrade`/rewrite-on-first-successful-login for the old single-round SHA-256 hashes. Grep the suite for `PasswordHash`, `NeedsUpgrade` or `Verify` and it returns nothing (§15), so this Critical change is verified by reading it rather than by running it; a test that a wrong password fails and a legacy hash upgrades is the highest-value two cases left in this repository. |
| 7 | **Repair the reorder path.** *(High.)* | **[Closed]** — converted to a form POST against a real purchase-order endpoint, with `Supplier` sourced; `js/api.js` was unreferenced dead code and is deleted (see 13). |
| 8 | **Fix `notifications.js`** to target the API origin; emit `window.verichatApiUrl` from `_Layout`. *(High.)* | **[Partial]** — **Origin closed, authentication is not.** The layout emits the API base and the script now reaches `/hubs/notifications` on the right host, with a cookie fallback in the API's bearer events. But a cookie only travels to the host that set it: that works on `localhost` with two ports and **fails between the two separate `*.runasp.net` subdomains the deployed pair uses**, so the bell is still silent in production. See defect 9 for the two ways to close it properly. |
| 9 | **Apply the configuration classes**, add the missing unique indexes in a migration, regenerate the `.Designer.cs` files. *(Medium.)* | **[Partly closed]**, deliberately. `ApplyConfigurationsFromAssembly` is live, `InquiryConfiguration` is applied and its `IX_Inquiries_Status_Score` index ships in the hand-written `20261031160000_AddInquiryScoringInputs`. **Deferred:** the broader index/precision migration. Scaffolding it produced a schema delta that tried to rebuild `SalesGoals` and re-add dropped columns, because the upstream chain is broken-on-arrival — `20261025111000_AddSalesGoals` and `20261030204733_AddTask3Features` both `CreateTable("SalesGoals")` with different primary keys, so no database can have applied both. That migration was set aside rather than shipped blind, and `HEAD`'s `AppDbContextModelSnapshot.cs` was mechanically corrupt (unbalanced braces, several blocks never closed) and had to be rebuilt from a Designer file before anything could be generated at all. **Two of those blockers are now closed:** `113d04d` deleted the duplicate `SalesGoals` block and repaired the widths that had stopped `AddTask3Features` ever applying, and `c909aa6` downgraded the `PendingModelChangesWarning` that had aborted `MigrateAsync()` before it ran a single migration. **Still open:** the unique indexes and the `decimal(5,4)` `VatRate`, and the snapshot repair itself — which must be done by hand, because a scaffolded snapshot would silently claim the indexes exist (§12.4 item 9). Regenerating the nine hand-written migrations' Designer files is likewise still open and is now known to be the *cause* of the drift, not just untidiness. See DEPLOYMENT.md §8. |
| 10 | **Register FluentValidation and a real sanitiser**; add a CSP header. *(Medium.)* | **[Partial]** — Validators registered (`AddFluentValidationAutoValidation` + `AddValidatorsFromAssembly`), `RequestBodySanitisationMiddleware` replaces the never-registered `XssSanitizationMiddleware`. **Open: there is still no Content-Security-Policy header** — the layout loads five CDN scripts and several views use inline `<script>` blocks, so adding one needs a nonce scheme, not a static string. |
| 11 | **Decide the inquiry-scoring question honestly.** *(Product decision.)* | **[Closed]** — **both** halves — the weighted rules engine is implemented (§8 row, `InquiryScoring.cs`, 8 new unit tests) and a hand-typed score still overrides it, which is the "manually ranked triage" reading written into this document and `userguide.md`. |
| 12 | **Resolve the role inconsistencies.** *(Medium.)* | **[Closed]** — Medical Student can use the Quick-Cart (authorised on `QuotationController.RequestQuote`, allowed through the client-audience login gate and self-registration); Procurement Manager now actually holds `warehouse.alerts` and the sidebar gates on the permission rather than the role; the admin dropdown is driven by `UserRoles.All`, so Logistics/Client User/Medical Student are assignable, and assigning a client role now creates the `Clients` row the mobile order path needs. Also closed two bugs found on the way: `Views/Account/NoAccess.cshtml` never existed, so **every** permission denial threw instead of rendering (now uses the existing `Views/Shared/Unauthorized.cshtml`), and `PipelineController` had two competing constructors, so dependency injection could leave `_goals` or `_api` null and 500 the sales pipeline board. |
| 13 | **Delete the dead code.** *(Low risk, large readability win.)* | **[Closed]** — except two judgement calls (below) — 11 `PageModel` code-behinds, `Views/View.cshtml`, `Views/Procurement/LogisticsTracker.cshtml`, `Views/Telehealth/Appointments.cshtml`, 4 orphaned view-models, 13 empty API/Core stub files (including a do-nothing `Filters/AuthorizeAttribute.cs` that could silently shadow the real `[Authorize]`, and the stale 4-value `Core/Enums/UserRole`), 10 unreferenced CSS/JS files, the empty `Areas/` tree with its `<Folder Include>` entries, the unread `ApiSettings` config block, `build-out.txt`, `User.cs.bak`, `tmp/`, a duplicate `IEmailService`, and four source files whose names ended in a space (`ClientConfiguration .cs`, `PurchaseOrderService .cs`, `WarehouseAlertService .cs`, `SupportApiService .cs`). The 556 MB zip and the log files were already gone. **Kept:** `VerichatCRM_FeatureWalkthrough.md` — it is 5.9 KB of prose the team authored, not dead code, and it came back in the stash the team chose to restore; deleting authorial content was left to its owner. Also **kept:** `Views/Procurement/Dashboard.cshtml` and `Views/Support/Dashboard.cshtml`, orphaned but the only concrete design for the per-role dashboards the proposal asked for (29b). **One deletion here was wrong and is reversed:** `Views/Sales/Appointments.cshtml` is not a duplicate — `TelehealthController.cs:27` renders it by explicit path — so removing it would have made the Client Appointments screen a hard failure for every role that can see it. Restored; full account in defect 29. |
| 14 | **Re-cost the infrastructure** against the actual hosts and retire the Firebase tier table. *(Documentation.)* | **[Closed]** — §13 is rewritten against Monster ASP's published Free/Premium/Multi plans (the `*.runasp.net` sites and `databaseasp.net` SQL Server are that provider's), with the ZAR conversion dated and the capacity caveats named: on a flat-fee shared plan the bill stops scaling at 10 000 users but the 256–512 MB worker-process ceiling does not move. |
| 15 | **Produce and deliver the user guide and role-based training.** *(Task 3 deliverable 11.6.)* | **[Drafted]** — `userguide.md` exists with the role-by-role screens, the "what to add / what to consider / how training runs" sections and the corrections this pass forced. **Delivery and the actual training session are still open**, and the guide's own broken-path markers (static Rules screen, no password reset, no email) must be resolved or explicitly scoped out before it is handed over. |

### Mobile-specific actions

| # | Action | Status |
| --- | --- | --- |
| 16 | **Add `'Logistics'` to `STAFF_MOBILE_ROLES`** (`api.ts:310`). *(High, minutes.)* | **[Closed]** — and the refusal message is now generated from the same array, so it cannot drift out of date again. |
| 17 | **Persist the quote cart** and add a token-refresh path. *(High, a day.)* | **[Open]** — . |
| 18 | **Bind TLS to the API host and switch `.env` to `https://`.** *(Critical for shipping.)* | **[Open]** — and now known to be a **plan** decision, not a code one: certificates are provisioned automatically only on Monster ASP's paid tiers (§13.3). |
| 19 | **Delete or implement the three orphaned service files.** *(Medium.)* | **[Open]** — deliberately left out of action 13, because `NotificationsService.ts`, `PushNotifications.ts` and `RealTimeTracker.ts` are unreferenced-but-intentional scaffolding; deleting them is a product statement about push, not a cleanup. |
| 20 | **Remove the SSO button and the Privacy/Terms/About rows**; let client tickets set priority and category. *(Medium.)* | **[Open]** — . |
| 21 | **Adopt the restored `ci.yml` mobile job as the minimum bar.** *(Medium.)* | **[Partial]** — The job exists; `npx tsc --noEmit` passes today and `npx expo lint` has not been run against it. A mobile test runner remains entirely absent. |

---

## Appendix A — Verified repository facts

| Fact | Value |
| --- | --- |
| Solution file | `VerichatCRM.slnx` |
| Projects | 5 — Web, API, Core, Infrastructure, Tests (+ `VerichatMobile`, non-.NET). The `DatabaseInitializer` project is gone: no `.csproj`, and the solution never referenced it |
| Target framework | `net10.0` throughout |
| EF Core migrations | 18 (+ model snapshot); **8 lack a `.Designer.cs`** — the seven listed in §12.4 plus the hand-written `20261031160000_AddInquiryScoringInputs` |
| `DbSet`s in `AppDbContext` | 27 (a duplicated `SalesGoals` entry was removed) |
| Entity files in Core | 37 |
| API controllers | 22 |
| API services / service interfaces | 21 / 13 |
| SignalR hubs | 1 — `NotificationHub` at `/hubs/notifications` |
| Web controllers | 14 (root); `Areas/Admin` and `Areas/Sales` **deleted** — they held no files and `MapAreaControllerRoute` was never called |
| Razor views | 40 `.cshtml` — 38 outside `_ViewImports`/`_ViewStart`, of which 10 are `Views/Shared` partials, layouts and error pages (all referenced) and 28 sit in controller folders; **26 are reachable and 2 are not** (`Procurement/Dashboard.cshtml`, `Support/Dashboard.cshtml`, kept deliberately as the un-shipped per-role designs — defect 29b). Reachability was checked per file against both convention and explicit `View("~/…")` paths, which is how `Views/View.cshtml`, `Views/Procurement/LogisticsTracker.cshtml` and `Views/Telehealth/Appointments.cshtml` were found to be dead, and how the mistaken deletion of `Views/Sales/Appointments.cshtml` — live, via the explicit path at `TelehealthController.cs:27` — was found and reversed |
| Public landing assets | `pages/landing.css`, `js/landing.js` and 10 JPEGs in `wwwroot/images/landing` (1.3 MB, products 900px, sectors 840px, hero and team band 1600px). `pages/home.css` was deleted with the page it belonged to |
| Permission constants in `Perm.cs` | 27, across 7 roles |
| Test files / framework | 7 files, xUnit 2.9.3 + EF InMemory; **71 tests, all passing**; still **no Moq, no `WebApplicationFactory`** |
| Mobile automated tests | **0** — no `test` script, no runner (`jest`/`jest-expo`/`detox`/`maestro`/`@testing-library` all absent), no `*.test.tsx` or `__tests__` under `VerichatMobile/src`. `npx tsc --noEmit` is clean |
| Mobile route files | 18 in `src/app/` (flat, no `(tabs)` group); only 5 tabs are visible, via the custom `app-tabs.tsx` |
| Mobile dependencies | 37 runtime + 4 dev (`@types/react`, `eslint`, `eslint-config-expo`, `typescript`) = 41; **10 of the runtime dependencies are unused** (§19.3) |
| CI/CD workflow files present | **1** — `ci.yml`, restored from `7adceef` (was 0). `android-build.yml` was deliberately **not** restored; see §11.2 |
| CI/CD workflow files deleted | 2 historically — `ci.yml` (34 lines, added `7adceef`) and `android-build.yml` (120 lines, added `70bcc21`), both removed in `e029d66` (2026-10-25); only the second is still removed |
| Git tags | **0** |
| Commits | 169 — **the §20 work below is uncommitted in a working copy** |
| Branches | `main`, `develop`, `dev2`, `api`, `web`, `mobile`, `mutsa`, `feature-debug`, `section1---mutsa`, `section2--rai`, `feature/product-catalog-ordering` |
| Conventional-Commit-formatted subjects | 25 / 169 (~15 %) |
| Hosting provider | **Monster ASP** — `verichat-crm-web.runasp.net` and `verichat-crm-api.runasp.net` (its `*.runasp.net` subdomain scheme) plus shared SQL Server `db70543` at `databaseasp.net`. Both sites are on the provider's **Free** plan (§13.2) |

## Appendix B — Diagram inventory and disposition

| Diagram in the proposals | Page | What it showed | Disposition |
| --- | --- | --- | --- |
| UML Domain Class Diagram | T1 p28 | Client → KeyPersonnel → Inquiry → Deal → Quotation → Product, with `belongs to` / `can have` / `evolves` / `can generate` / `can include` | **[Built]** — Conceptually realised; `Inquiry → Deal` is now nullable-linked, `Deal → Quotation` is `Restrict` |
| UML Design Class Diagram | T1 p29 | User, Address, SalesRepresentative, Support, Deal, Quotation, Product with attributes, `+`/`-` access, PKs and methods `login()`, `handleTickets()`, `processDeal()`, `closeDeal()`, `createQuotation()`, `updateQuotation()`, `ManageLeads()` | **[Replaced]** — Methods became service operations, not instance methods; `Address` became flat columns on `Client`/`MobileProfile`; the `Employee → SalesRepresentative / Support` inheritance became a single `User` + `Role` string |
| Relationship table | T1 p30 | User→Address 0..1→1; SalesRep→Deal 1→0..*; SalesRep↔Support many-to-many via an intermediate table; Deal→Quotation 1→1..*; Quotation→Product 1→1..*; inheritance | **[Partial]** — All hold **except** the many-to-many rep↔support intermediate table, which became `Ticket.AssignedToUserId` + `TicketActivity` |
| UML Sequence Diagram (sales flow) | T1 p31 | Sales Rep → Verichat system → Client: select inquiry / advance deal stage / select catalogue items / `checkStock()` / PDF quotation ready / email quotation | **[Partial]** — Three of six steps work; `checkStock()` rejects rather than warns, and **email quotation is never sent** |
| UML State Machine Diagram (ticket) | T1 p31 | Received → Under review → Action taken → Resolved, with an SLA-Breach auto-escalation loop and a "client rejects proposed resolution" return edge | **[Partial]** — `TicketStatuses` are exactly Received / Under Review / Action Taken / Resolved (+ Escalated, Closed) and the SLA escalation loop is implemented by `SlaEscalationService`. **The "client rejects proposed resolution" return edge is not implemented** — there is no client-facing ticket interaction surface |
| UML Deployment Diagram | T1 p32 | Client (Verichat SPA React) → Firebase Hosting / Firebase Auth / Cloud Functions (`api.js`, `pdfGenerator.js`) → Firestore | **[Replaced]** — Fully replaced; `pdfGenerator.js` → `PdfGenerator.cs` is the one faithful node |
| ERD / collections map | T1 p34 | Nine Firestore collections with PK/FK annotations | **[Built]** — All nine as SQL tables; 18 added — see §12 |
| UML Object Diagrams (Observer, Facade) | T1 p35 | Pattern instances | **[Replaced]** — Both realised under different mechanisms — see §10.2 |
| Layered topology diagram | T1 p36 | Presentation / business logic / data | **[Built]** — The proposal's most accurately realised artefact |
| Cloud topology diagram | T1 p37 | Client apps → Hosting / Auth / Functions / Firestore | **[Not built]** — see §10.3 |
| Gitflow branching model | T1 p40 | main / develop / feature/* / release/* / hotfix/* with PR gates | **[Not followed]** — see §11.1 |
| CI/CD pipeline architecture | T1 p41 | Nine stages Push → Lint & Test → Build → PR Review → Merge → Staging Deploy → UAT → release/* → Manual Approval → Deploy | **[Built, then deleted]** — `ci.yml` implemented 3 of the 9 stages against the real .NET/Expo stack and was removed in `e029d66`; `android-build.yml` pointed at a fictional `monsterasp.example.com` deploy endpoint. Nothing runs today — see §11.2 |
| UX journey maps (4) | T1 p8–10 | Image-only, no extractable text | **[Partial]** — 2 of 4 fully delivered |
| Change-management diagram | T1 p48 | Image-only | **[Not implemented]** — as product functionality |
| Prototype architectural navigation & interface map | PT p2 | Figma link, mobile + web views | **[Superseded]** — by the shipped navigation |
| Prototype screen mock-ups (15 web, 5 mobile) | PT p8–31 | Per-screen layout, KPIs, information flow | See §8 — 6 fully, 11 partial, several absent |
| Mobile-vs-web responsive design tables | PT p6, p33 | Sidebar vs bottom tabs, tables vs cards, 4-col vs 2-col grid | **[Built]** — Web sidebar and mobile tab bar both delivered — the 5 visible tabs are Home / Catalogue / Consult / My Assets / Profile via the custom `app-tabs.tsx` (staff see only Home + Profile). Tables-vs-cards **as proposed** |
| Core palette architecture | PT p7 | 7 colour tokens | **[Implemented]** — approximately; PDF uses different hexes |
| EXPO exhibit layout & judging flow | EXPO p1–5 | Station setup, visitor flow, 5–10 min slot, backup demo video | **[Doc only]** — Event logistics — informs the demo/training plan in `userguide.md` |

---

*End of gap analysis. Companion file: `userguide.md`.*
