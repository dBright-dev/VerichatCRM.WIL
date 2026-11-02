# Verichat CRM — feature walkthrough

What the delivered system does, in the order you would click through it, and what was
fixed underneath to make each of those clicks work. Read [userguide.md](userguide.md) for
day-to-day operations per role, [documentation.md](documentation.md) for the audit against
the two project proposals, and [DEPLOYMENT.md](DEPLOYMENT.md) for hosting and secrets.

Everything below describes the code on `dev2` as of `4091143`. Route paths, form fields,
button labels and the messages quoted verbatim are taken from the source, not paraphrased;
§11 lists the commands that produced each number.

## 1. What the delivered system is

| Tier | Project | What it is |
| --- | --- | --- |
| Web | `VerichatCRM.Web` | ASP.NET Core 10 MVC. 15 controllers. Staff screens **and** the client self-service portal. Cookie session; the JWT never leaves this server. |
| API | `VerichatCRM.API` | ASP.NET Core 10 Web API, 22 controllers, JWT bearer auth, SignalR hub, OpenAPI JSON at `/openapi/v1.json`. |
| Domain | `VerichatCRM.Core` | Entities, enums, DTOs, the enquiry scoring rules (`Core/Models/Inquiry/InquiryScoring.cs`). |
| Data | `VerichatCRM.Infrastructure` | `AppDbContext`, 19 migrations → 38 tables, repositories, `AuditSaveChangesInterceptor`. |
| Mobile | `VerichatMobile` | Expo (router) app, 17 screens, offline queue + retry. |
| Tests | `VerichatCRM.Tests` | 71 tests, all passing. |

Seven roles: System Administrator, Sales Representative, Procurement Manager, Field
Support Engineer, Logistics, Client User, Medical Student. The last two are the client
audience — they buy from Verichat instead of working inside it.

## 2. The access model — read this before the walkthroughs

One table decides every screen in the web app: `Perm.ByRole` in
[VerichatCRM.Web/Authorization/Perm.cs](VerichatCRM.Web/Authorization/Perm.cs). A role
holds a set of permission strings; a permission string gates both the sidebar link and the
URL, because `_Sidebar.cshtml` asks `Can(Perm.X)` and the controller carries
`[RequirePermission(Perm.X)]`. Hiding a link is therefore never the security boundary —
`RequirePermissionAttribute` runs on the request, and a refusal renders
`Account/NoAccess`, which offers a way home via `RoleHome.UrlFor(role)`.

Three role sets in [Core/Entities/UserRoles.cs](VerichatCRM.Core/Entities/UserRoles.cs)
decide the rest:

- `All` — what an administrator can assign from Users &amp; Roles.
- `SelfRegisterable` — the two roles the public sign-up form offers: Client User and Medical
  Student. Every staff role is excluded on purpose, because nothing reviews a self-claimed
  account any more; a staff login is created by an administrator from Users &amp; Roles, and the
  first System Administrator comes from the one-time configuration bootstrap.
- `ClientAudience` — Client User and Medical Student. Both need a row in `Clients`,
  because orders, quotations and the enquiry queue are keyed on `Clients.ClientId`, and
  that row uses the `UserId` as its key. So the row is created wherever the account
  becomes one of these roles: at self-registration
  ([API/Services/AuthService.cs](VerichatCRM.API/Services/AuthService.cs#L90)) and when an
  administrator changes someone into it
  ([API/Controllers/UsersController.cs](VerichatCRM.API/Controllers/UsersController.cs#L51)).

| Role | Lands on | Reachable screens |
| --- | --- | --- |
| System Administrator | `/admin/dashboard` | everything, including Users &amp; Roles, SLA control, Rules Engine, analytics |
| Sales Representative | `/pipeline` | inquiry queue, leads, client map, quotation builder, catalog (edit), analytics, appointments, client directory |
| Procurement Manager | `/procurement/dashboard` | product catalog, warehouse alerts, purchase orders, ticket hub, logistics tracker, client directory |
| Field Support Engineer | `/support/dashboard` | ticket hub + resolution, asset tracking, calibration, appointments |
| Logistics | `/support/dashboard` | logistics tracker, asset tracking, ticket hub |
| Client User | `/Portal` | My Verichat, Catalogue, My Quotations, Support — and the accept/decline on a sent quote |
| Medical Student | `/Portal` | the same four, without accept/decline |

The generic `/Dashboard` workspace is a staff screen too: it is gated on
`Perm.Workspace`, which every staff role holds and neither client role does.

## 3. Walkthrough A — a client signs up on the web and buys something

This vertical is new in this pass: the client audience previously had mobile screens only,
and on the web a Client User who signed in reached a staff shell with zeros in it.

1. **`/Account/Register`.** The role dropdown offers exactly two choices — `Client User — buy,
   request quotes, raise support` and `Medical Student — browse and request, no purchasing` —
   and a note under it says why nothing else is listed. There is no staff option to mis-click.
2. **Submit.** The controller validates the role against `UserRoles.SelfRegisterable`
   before posting, so a hand-typed `System Administrator` arrives as a field error
   ("Select a valid role.") rather than as an opaque server refusal. The API creates the
   user, hashes the password, and opens the `Clients` row keyed to that `UserId`.
3. **You are signed in and sent to `/Portal`** — "My Verichat", with the quotations,
   tickets and deliveries for your own account. A staff role still goes to its own
   dashboard; the branch is `RedirectToRoleDashboard` in
   [Web/Controllers/AccountController.cs](VerichatCRM.Web/Controllers/AccountController.cs).
4. **Catalogue (`/Portal/Catalogue`).** Category chips are built from what is actually in
   the database, so a category appears only once a product exists in it. Search matches
   product name or SKU. Each card carries a quantity box.
5. **Set quantities, add a note, Request quote.** Only `qty-<SKU>` fields with a positive
   whole number become lines — the SKU is the field name, so a catalogue of
   `VC-CM-003` posts `qty-VC-CM-003=5`. Text that is not a number names the product
   ("… is not a whole number of units."), and a single line is capped at 10,000 units with
   a message that says so. Nothing is priced from the browser: the form posts SKUs and
   quantities to `/api/quotation/request`, and the API reads prices from the catalogue and
   raises a **draft** quotation on your own account. Success reads
   *"Quote VC-QT-20261031-248836 requested. A sales representative will confirm pricing and
   availability."*
6. **My Quotations (`/Portal/Quotations`)** — status chips (All, Draft, Reviewed, Sent,
   Accepted, Rejected, Expired), newest first, with a **PDF** link. A `Sent` quotation is
   rendered open and, for a Client User, answerable; a `Draft` or `Reviewed` one is shown
   as not yet actionable.
7. **Download the PDF (`/Portal/Pdf/{id}`).** The browser cannot call the API for the file
   itself, because only the web server holds a token; `PortalController` pulls the bytes
   and hands them over with the original filename.
8. **A sales representative works it** (Walkthrough B): prices, saves, sends. The quote
   status becomes `Sent`.
9. **Back on the portal, accept or decline** (`/Portal/Respond`). Both buttons post with
   the antiforgery token; anything other than `Accepted`/`Rejected` is rejected before the
   API call. Answering a draft is refused by the API and its message is what the client
   sees — on the walkthrough quotation, *"Only a sent quotation can be accepted or
   declined."* — because the server, not the view, decides whether a response is possible.
10. **Support (`/Portal/Tickets`).** Raise a fault with subject, description, priority
    (Low/Medium/High/Critical from `TicketPriorities`), category (General, Equipment fault,
    Delivery, Billing, Training query) and an optional equipment id. `form.ClientId` is
    overwritten from the session before the post, so a tampered body cannot raise a ticket
    against another organisation. Success reads *"Ticket VC-TCK-261031-6414 was raised. We
    respond within the SLA for its priority."*

**The Medical Student difference.** `Perm.ByRole` gives Client User
`{ClientPortal, ClientQuoteRespond}` and Medical Student `{ClientPortal}`. The
accept/decline controls are not rendered, and `Respond` carries its own
`[RequirePermission(Perm.ClientQuoteRespond)]`, so posting the URL directly reaches
`Account/NoAccess` rather than the quotation. `Quotations` asks `Perm.Has(role, …)`
instead of inferring from the role name, which is why the view cannot drift out of sync
with the gate.

**Cross-account isolation, and one false alarm.** Reads go to routes already scoped to the
caller (`/api/mobile/quotations`, `/api/supportticket/client/{id}`,
`/api/delivery/client/{id}`) and every one of those controllers compares the requested
client id with the token's user id and refuses otherwise
(`SupportTicketController.cs:40`, `QuotationController.cs:56/73/86`). While walking this
through, a student account fetching a client's PDF looked like a leak: `curl -i` showed
`302 → /Portal/Quotations`, and the "200 with 5,268 bytes" was `-L` following that redirect
onto the portal's own HTML. No file crossed.

## 4. Walkthrough B — an enquiry becomes revenue

1. **`/Inquiry` (Inquiry Queue).** The list sorts on the score. Recording an enquiry
   captures sector, urgency and estimated value; `InquiryScoring` produces the rank from
   the rules table rather than from a guess, and the composite index
   `IX_Inquiries_Status_Score` (migration `20261031160000`) backs the ordering. Existing
   rows are not silently re-scored — re-ranking is a human action in the form.
2. **`/Sales/Leads`** and **`/Sales/Map`** for the territory view.
3. **`/Quotation` (Quotation Builder).** Pick the client, add lines, set VAT, save, send.
   VAT is entered as `0.15`, and the validator rejects `15` with a message that says to
   send `0.15` — the earlier behaviour reached SQL Server as `'15,0000'` and returned 500.
4. **`Generate PDF`** streams the branded document; the client sees it on the portal
   (step 7 above) and answers it there. The status flip fires a SignalR notification, so
   the sales rep's screen raises a live toast without a refresh.
5. **`/Pipeline`** is where the accepted quote becomes a deal you can see.

## 5. Walkthrough C — a fault becomes a resolution

1. A client raises a ticket (portal, step 10) or a client does it on the phone
   (Walkthrough E). `SupportTicketController.Create` forces `ClientId` to the caller for
   the client audience, and generates the tracking number.
2. **`/Support/TicketHub`** for engineers: filter, open, assign, update status, record
   resolution notes. `Perm.TicketUpdate` is what allows the write, so Logistics can see a
   ticket and cannot resolve it.
3. **Every status change is written to `TicketActivity`** — action, who performed it,
   from/to values, notes, timestamp — and that table only came into existence in this
   pass. Before `20261031210000_AddTicketActivityAndDeliveryStatusHistory` no migration
   had ever emitted it, so *creating a ticket threw* `Invalid object name 'TicketActivity'`
   on a migrated database.
4. **`/Logistics/Assets` (Asset Tracking)** holds the installed equipment and its
   calibration schedule (`CalibrationSchedules`), and **`/Logistics/Tracker`** follows
   shipments. Each delivery status movement is appended to `DeliveryStatusUpdate` — the
   other table the same migration added — so a shipment keeps its trail; the link is
   optional and `NoAction`, which is why deleting a delivery does not take its history
   with it.
5. **SLA.** Deadlines come from the admin-managed SLA control, and the hub's filters sort
   on breach risk. Assignments and resolutions push to `NotificationHub`, so the affected
   user's toast fires.

## 6. Walkthrough D — low stock becomes a purchase order

1. **`/Procurement/WarehouseAlerts`** lists products under their `LowStockThreshold`. This
   is the Procurement Manager's main working screen and it is gated on `warehouse.alerts`,
   a permission that role now actually holds (it did not, and the sidebar link walked
   straight into a 403).
2. **Reorder** posts a form — not a JSON body from a link — so `ValidateAntiForgeryToken`
   applies, and the supplier named on the order is a real supplier record, not a typed
   string. The order appears in `PurchaseOrders` and the stock movement is audited.
3. **`/Procurement/ProductCatalog`** for creating and editing catalogue items; product
   categories come from `Core/Entities/ProductCategories.cs`, so the validators and the
   form agree.

## 7. Walkthrough E — the mobile app

`VerichatMobile` is a three-door sign-in: **Client**, **Staff / Admin**, **New User**, each
accepting only its own roles, with plain messages when someone uses the wrong door. Client
screens: `clientHomescreen`, `medicalCatalogueShop`, `cartScreen` (the quick cart that
becomes a quote request), `BookAppointmentScreen`, `EquipmentStatusScreen`,
`ClientCommunicationScreen`, `ClientTimelineScreen`, `profileScreen`, `settingsScreen`.
Staff screens: `Dashboard`, `GoalsScreen` (sales targets with an SVG progress ring),
`adminLogin`, `explore`. Health data (`VitalReading`, `WellnessGoal`, `WeeklyExerciseLog`,
`HealthMetric`, `TelehealthAppointment`, `LabResultRecord`, `MobilePrescription`) is the
client's own record, synced through `MobileService`. Writes made offline are queued and
replayed, which is the feature to show first in a demo.

## 8. The public landing page and the auth screens

- **Logo.** The real Verichat identity, extracted from the company profile and
  committed in `4e36b22`, which replaced a generated "V" placeholder. Two lock-ups
  are in play and the split is deliberate — see `DESIGN-SYSTEM.md` §1.3. The square
  **mark** (`wwwroot/images/logo.png`, byte-identical to
  `images/brand/verichat-mark.png`) is the chrome: the landing header
  (`_LandingLayout.cshtml:31`), the staff/portal sidebar (`_Sidebar.cshtml:3`) and
  both auth panels (`Login.cshtml:10`, `Register.cshtml:10`). The wide **wordmark**
  is the statement: the marketing footer
  (`_LandingLayout.cshtml:69` → `brand/verichat-logo-light.png`) and every mobile
  sign-in and home header. Two surfaces still show somebody else's logo — the iOS
  app icon is the Expo default and the web cold-start splash renders
  `expo-logo.png` — recorded as open items 10 and 11 in that register; the native
  splash now uses the Verichat wordmark.
- **Team section.** "The people behind it" lists **company roles, not developers**: Chief
  Executive Officer (executive sponsor), Procurement &amp; Logistics Manager (supply chain),
  Client Support Manager (client services), each with what that role owns once the system is
  live — catalogue and supplier records; the ticket queue, SLAs and calibration calendar;
  sector choice and client contracts. The roster lives in
  [Web/ViewModels/Home/LandingContent.cs](VerichatCRM.Web/ViewModels/Home/LandingContent.cs)
  as `TeamMember(Initial, Name, Role, Focus)`, so the tiles render from data, and the
  initials come from the job title, not a person's name.
- **Sign in / sign up.** Both use `_AuthLayout` + `wwwroot/pages/auth.css`: a two-panel
  screen — brand, headline, three product promises on the left; a single card with labelled
  fields, one primary button and a switch link on the right. `autocomplete` attributes are
  set (`email`, `current-password`, `new-password`), the form has an antiforgery token,
  validation messages render under the field that failed, and the layout collapses to one
  column on narrow screens.

## 9. What was fixed underneath

**Authentication and secrets.** Passwords are PBKDF2 (`API/Helpers/PasswordHasher.cs`,
stored as `pbkdf2-sha256$…`) with legacy hashes upgraded on first successful sign-in. The
database credential and the JWT key left `appsettings.json` and are supplied out of band;
the API refuses to start without them, and refuses to start outside Development with
`Encrypt=False`.

**Nothing leaks to the browser.** The JWT is held in the server session; no token appears
in markup or in `localStorage`. Each page carries one
`<meta name="request-verification-token">` and every mutating form posts the antiforgery
token — including the two JSON POSTs that previously did not.

**Ownership.** Every client-scoped API route compares the requested id with the token's
user id (see Walkthrough A), and `NotificationHub.Subscribe` authorises the connection
before it maps a user to a live socket.

**Input.** FluentValidation is registered so the validators actually run, and
`HtmlSanitizer` strips markup on free-text fields before it reaches the database.

**Audit.** `AuditSaveChangesInterceptor` records entity, id, action, user, timestamp, IP
and JSON old/new values for every `SaveChanges`, redacting `PasswordHash`,
`PasswordResetToken` and `PasswordResetExpiry`.

**The schema chain — three commits, because the database was the deployment blocker.**

| Commit | Defect | Symptom |
| --- | --- | --- |
| `113d04d` | `AddTask3Features` declared three FK columns `nvarchar(64)` against `nvarchar(450)` principals, and emitted a second `CREATE TABLE SalesGoals` with a shape belonging to no entity. | Msg 1753, whole migration rolled back; the migration had never run anywhere. |
| `f327007` | `TicketActivity` and `DeliveryStatusUpdate` are in the model snapshot but were in no migration's SQL, so the differ found nothing to do and never emitted them. | `Invalid object name 'TicketActivity'` on ticket creation; every delivery status write failed. |
| `c909aa6` | Four mapped entities (`AuditLog`, `HealthMetric`, `ClientTimelineNote`, `Appointment`) have tables only from hand-written migrations, so they are absent from the snapshot; EF validates the model against the snapshot before touching the database. | `MigrateAsync()` threw `PendingModelChangesWarning` **before applying any migration**, on every environment. The startup catch logged it, so a correctly configured deployment came up over an empty schema and `/api/health` still reported Healthy. Now logged, not fatal. |

The snapshot drift itself is still open and deliberately not "fixed" by scaffolding —
a snapshot generated from today's model would absorb the unique indexes and the
`decimal(5,4)` `VatRate` as though they had been applied. See DEPLOYMENT.md §8 and §8.1.

**The deployed HTTP 500.** 500.30 means `Program.cs` threw before the app could serve a
request. It is not a missing runtime and not an IIS misconfiguration: the credential and
signing key were removed from git, the shipped `appsettings.json` carries both empty, and
the API fails its own configuration check. Two application settings in the hosting panel
fix that, and `c909aa6` is what makes the app usable after them. DEPLOYMENT.md §7.1 is the
runbook.

## 10. Deliberate gaps

- The unique indexes on `Users.Email`, both tracking numbers, `Assets.SerialNumber` and
  `Quotations.QuoteReference`, and `VatRate`'s `decimal(5,4)`, are declared in code and
  exist in no database. They need the deferred migration in DEPLOYMENT.md §8.1, which has
  to be written by hand and pre-flighted against live rows.
- Self-registration cannot grant System Administrator, and cannot grant a staff role to an
  existing person — that is Users &amp; Roles, and assigning a client role there opens the
  `Clients` row for you.
- No password policy is enforced anywhere but length-by-hint; SSO on mobile is a stub.
- `Inventory` is a modelled, migrated table with no API surface; stock lives on
  `Product.StockQty`. `TicketComment` from `InitialCreate` is an orphan table.
- Client-sector category lists still disagree between the client form, the analytics demo
  data and the scorer's rank table (documentation.md §12.4 item 6).

## 11. How the claims above were verified

| Claim | Command / action | Result |
| --- | --- | --- |
| 71 tests pass | `dotnet test VerichatCRM.Tests -c Release` | `Passed! - Failed: 0, Passed: 71` |
| The whole chain builds a database from nothing | Release output, `ASPNETCORE_ENVIRONMENT=Production`, connection string pointed at `VerichatCRM_DeployTest`, a database that did not exist | `/api/health` Healthy, environment=Production; startup logged the pending-model warning once and then "Database migrations applied." |
| 19 migrations, 38 tables | `SELECT COUNT(*) FROM sys.tables` / `__EFMigrationsHistory` on that database | 38 and 19 |
| The two new tables exist | `SELECT name FROM sys.tables WHERE name IN ('TicketActivity','DeliveryStatusUpdate','Appointments','AuditLogs','ClientTimelineNotes','HealthMetrics')` | all six returned |
| `SalesGoals` created once, correct shape | `INFORMATION_SCHEMA.COLUMNS` for `SalesGoals` | one table, 9 columns (`Id`, `UserId`, `Title`, `Target`, `Achieved`, `Period`, `PeriodLabel`, `CreatedAt`, `UpdatedAt`) |
| The deferred constraints really are absent | `sys.indexes WHERE is_unique = 1 AND is_primary_key = 0` | zero rows (187 unique indexes, every one a primary key); `Quotations.VatRate` is `decimal(18,2)` |
| The client portal works end to end | register → log in → catalogue → request quote → PDF → ticket → respond, as a real Client User and a real Medical Student against a running API + web pair | see the two rows below |
| Registration opens the client's `Clients` row | `/Account/Register` posting `Role=Client User`, then `SELECT c.ClientId, c.ClientName, u.Role FROM Clients c JOIN Users u ON u.UserId = c.ClientId` | both new accounts have a `Clients` row keyed to their `UserId`; the POST returned `302 → /Portal` |
| Each role lands on its own screen, and is refused the others' | `curl` with one cookie jar per account | student: `login=302 → /Portal`, `/Portal`, `/Portal/Catalogue`, `/Portal/Quotations`, `/Portal/Tickets` all `200`; `/Dashboard`, `/Portal/Respond` and `/Admin/Dashboard` all `302 → /Account/NoAccess`. Client User: same, except `/Portal/Quotations` renders the Accept control |
| A quote request and a ticket really persist | portal forms, then read the pages back | quotation `VC-QT-20261031-248836` listed as `Draft` with a PDF link; `GET /Portal/Pdf/{id}` returned `application/pdf`, 56,034 bytes, beginning `%PDF-1.4`; ticket `VC-TCK-261031-6414` listed with the SLA message; accepting the draft left it `Draft` and surfaced the API's own refusal |
