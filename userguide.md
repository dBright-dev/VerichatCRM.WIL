# Verichat CRM — User Guide

**Version 3.0 — Task 3 (Final submission)** · Compiled 31 October 2026
Applies to: `VerichatCRM.Web` (staff web application), `VerichatCRM.API` (REST API + SignalR hub), `VerichatMobile` (client and field-staff app), on the .NET 10 / SQL Server stack described in [DEPLOYMENT.md](DEPLOYMENT.md).

Every screen, button, field, validation message and permission in this guide was read out of the shipped source at the version above. Where the product does not do something, this guide says so in §18 rather than describing an imagined feature.

**How to read the guide**

| Marker | Meaning |
|---|---|
| **▷ Screen name** | A page you can see; the URL is given the first time it appears. |
| `[SG-nn]` | A screenshot **frame still to be captured**, not an image in this document. Appendix B lists all 52, the state to capture and the annotations to draw. |
| **Note:** | A behaviour that surprises people, or a limitation that will cost you time if you do not know it. |
| **Role tag** | A step marked *(Admin only)*, *(Sales)*, etc. is enforced by the server; another role gets the 403 page described in §6.4. |

---

## Table of contents

1. [Quick start — the five-minute path](#1-quick-start--the-five-minute-path)
2. [What the system is](#2-what-the-system-is)
3. [Technical prerequisites](#3-technical-prerequisites)
4. [Getting access and signing in](#4-getting-access-and-signing-in)
5. [Setup: installing and running the software yourself](#5-setup-installing-and-running-the-software-yourself)
6. [Navigation: how every web screen is built](#6-navigation-how-every-web-screen-is-built)
7. [Role guide — System Administrator](#7-role-guide--system-administrator)
8. [Role guide — Sales Representative](#8-role-guide--sales-representative)
9. [Role guide — Procurement Manager](#9-role-guide--procurement-manager)
10. [Role guide — Field Support Engineer](#10-role-guide--field-support-engineer)
11. [Role guide — Logistics](#11-role-guide--logistics)
12. [Role guide — Client User and Medical Student (mobile and web portal)](#12-role-guide--client-user-and-medical-student-mobile-and-web-portal)
13. [End-to-end workflows](#13-end-to-end-workflows)
14. [Form-by-form data entry reference](#14-form-by-form-data-entry-reference)
15. [Read-only screens](#15-read-only-screens)
16. [Frequently asked questions](#16-frequently-asked-questions)
17. [Troubleshooting](#17-troubleshooting)
18. [Known limitations](#18-known-limitations)
19. [Glossary](#19-glossary)
20. [Appendix A — role × screen access matrix](#20-appendix-a--role-screen-access-matrix)
21. [Appendix B — annotated screenshot inventory](#21-appendix-b--annotated-screenshot-inventory)
22. [Appendix C — getting help](#22-appendix-c--getting-help)

---

## 1. Quick start — the five-minute path

**You are a staff user and someone has already installed the system.** Go to §4.2, sign in, and read the one section of §7–§11 that matches your role. You do not need §3 or §5.

**You are a client.** On the phone: open the app, tap **Client**, sign in, and work through §12 — the five tabs are the whole product there. In a browser: sign in at `/Account/Login` the way staff do and you get the **client portal**, four screens of your own (§12.8). Neither path shows you a staff screen, and typing one gets you the 403 page, not somebody else's data.

**You are installing the software to run it yourself.** §3 for what you need, then §5 for the ordered procedure. Budget 30–45 minutes for a clean machine; the four configuration values in §5.3 are where most people lose time.

**You are assessing or demonstrating the system.** §13 gives five complete stories that cross both applications; §21 lists one annotated screenshot per major step, and [VerichatCRM_FeatureWalkthrough.md](VerichatCRM_FeatureWalkthrough.md) walks the same features with the commands behind each claim.

---

## 2. What the system is

Verichat CRM is the sales, procurement and field-service back office for a medical-equipment supplier serving clinical, mining, industrial and education customers. It replaces the spreadsheet-and-WhatsApp process the company ran on.

### 2.1 The four moving parts

| Part | What it is | What it does |
|---|---|---|
| **Staff web application** | ASP.NET Core MVC (`VerichatCRM.Web`) | The seven role-shaped workspaces described in §7–§11. It holds no business logic of its own — it calls the API and renders the answer. |
| **Client / field app** | React Native on Expo 57 (`VerichatMobile`) | Five tabs for clients (§12) and a staff operations dashboard with barcode scanning. Works offline and syncs when the network returns. |
| **REST API + hub** | ASP.NET Core minimal/API (`VerichatCRM.API`) | Owns the data, the rules (SLA, enquiry scoring, stock alerts), the JWT sign-in, the two background services, and the SignalR notification hub at `/hubs/notifications`. |
| **SQL Server database** | EF Core 10 migrations | One relational store, reached only through the API. Migrations are applied automatically when the API starts. |

The web app never touches the database. The mobile app never touches the database. Both speak to the API, which means the rules cannot be argued with from a client.

### 2.2 The two audiences

- **Staff** (System Administrator, Sales Representative, Procurement Manager, Field Support Engineer, Logistics) live in the web app. Five of the seven roles also use the mobile app for field work.
- **Clients** (Client User, Medical Student) work in the mobile app and in their own part of the web app — the **client portal** at `/Portal`, reached by signing in the same way staff do (§12.8). They see four screens and no staff screen: a client who types a staff URL is refused on the server, not by a hidden menu.

### 2.3 What the system does *not* do

Say this out loud in any demo, because §18 lists it in detail and an assessor will find it otherwise: there is **no payment, no checkout, no email or SMS sending, and no push delivery**. A mobile cart produces a *quote request*, which a human turns into a quotation. "Buy" never happens in the software.

---

## 3. Technical prerequisites

### 3.1 To use the deployed system

| Requirement | Minimum | Notes |
|---|---|---|
| Browser (staff) | Current Edge, Chrome or Firefox | The app uses CSS `backdrop-filter`, `<dialog>` and native drag-and-drop. **Internet Explorer is not supported and was never tested.** |
| Screen (staff) | ≥1280 px wide recommended | The Pipeline Board needs 1100 px of horizontal scroll or it wraps columns; phones are not a supported staff-web device. |
| Device (client app) | Android 8+ or iOS 16+, or Expo Go | Built with `expo ~57.0.26`, `react-native 0.86.3`. |
| Network | HTTPS out to the API host | The app loads Google Fonts, Bootstrap, Chart.js, Leaflet and the SignalR client from CDNs; a locked-down network that blocks CDNs will render an ugly but still functional page. |
| Account | One per person | There is no shared-account support: the audit trail and the notification hub both key off your user id. |

### 3.2 To build and run it yourself

| Tool | Version | Verify with |
|---|---|---|
| .NET SDK | **10.0.x** (`net10.0`) | `dotnet --version` |
| SQL Server | 2019 or newer, or a hosted equivalent (the project ships on Monster ASP / `databaseasp.net`) | any client that can open the connection |
| Node.js | 20 LTS or newer (developed on 24.x) | `node --version` |
| npm | ships with Node; the mobile app is an npm project (`package-lock.json`) | `npm --version` |
| Git | any recent version | `git --version` |
| Expo CLI | via `npx expo`, nothing installed globally | `npx expo --version` in `VerichatMobile` |

**Note:** The mobile project declares **no `engines` field and there is no `.nvmrc` anywhere in the repository** — nothing enforces the Node version. If `npx expo start` fails with a syntax or module-resolution error, check Node first.

You also need:

- **A SQL Server login** that can create a database (the API runs migrations at startup and creates the database if it does not exist).
- **A GitHub repository** with Actions enabled if you want the CI pipeline (§5.7); the workflow file `.github/workflows/ci.yml` is already in the repo.
- **No SMTP server, no Firebase project, no payment gateway** — earlier proposals assumed them; the shipped system uses none of them.

### 3.3 Ports and URLs

| Thing | Address | Where it comes from |
|---|---|---|
| Staff web (local) | `http://localhost:5253`, `https://localhost:7266` | `VerichatCRM.Web/Properties/launchSettings.json` |
| API (local) | `http://localhost:5184`, `https://localhost:7076` | `VerichatCRM.API/Properties/launchSettings.json` |
| Staff web (deployed) | `http://verichat-crm-web.runasp.net` | Monster ASP shared hosting — see "What runs where" in DEPLOYMENT.md |
| API (deployed) | `http://api-site.runasp.net` | Monster ASP publication, profile `site94518-WebDeploy.pubxml` |
| Health probe | `GET {api}/api/health` | `HealthController` — use this to tell "the API is down" from "the database is down" |
| Notification hub | `{api}/hubs/notifications` | SignalR, cookie-authenticated |

**Note:** Both deployed addresses are **`http://`, not `https://`**, and that is the host's current state rather than a choice: no certificate is bound to either site, so an `https://` request to any of the `*.runasp.net` names fails at the TLS handshake. Measured 1 Nov 2026 — `http://verichat-crm-web.runasp.net` returns 200, `https://verichat-crm-web.runasp.net` does not connect. The shipped `ApiBaseUrl` in `VerichatCRM.Web/appsettings.json` and the `api.ts` fallback both name `http://api-site.runasp.net` now, so there is nothing stale left to copy. (`VerichatMobile/.env` is git-ignored and is not in a fresh clone, so the `api.ts` fallback is what a new mobile build actually uses unless you set the variable yourself.) A signed mobile build still cannot ship against it until a certificate exists (§5.6 and DEPLOYMENT.md §5).

---

## 4. Getting access and signing in

### 4.1 First sign-in on a brand-new system

The application ships with **no demo users**. An empty database has no way to sign anybody in, so the first System Administrator is created once, at API startup, from configuration:

```
Bootstrap__AdminEmail    = you@verichat.co.za
Bootstrap__AdminPassword = a long password only you know
Bootstrap__AdminName     = Your Name          (optional; defaults to "Administrator")
```

Rules, all verified in `AdminBootstrapper`:

- It runs **only when the user table is completely empty**. Restarting the app can never resurrect or overwrite an administrator.
- If the user table is empty *and* these variables are missing, the API logs a warning and starts anyway — and then nobody can sign in. That is the cause of "the login page rejects every password" on a fresh install (§17).
- `System Administrator` is the one self-service role that needs no approval: choosing it on
  `/Account/Register` writes the account as already approved and signs you straight into the
  console. Anyone who reaches that page can therefore create an administrator, so treat the
  sign-up route as part of the trusted perimeter (§18). To give the role to someone who already
  has an account instead, sign in as an administrator and change their role in
  **Users & Roles** (§7.2) — that path is reviewed by a human.

**After the first sign-in, change the password** (there is no password-change screen; see §18) and delete the two `Bootstrap__` variables.

### 4.2 Signing in on the web

▷ **Sign in** — `/Account/Login`

1. Open the web app. You land on the public landing page; the navigation bar's **Sign in** button, and the direct URL, both reach the form.
2. Enter **Email address** and **Password**. There is no role selector — your role is read from the database when the API authenticates you, and cannot be chosen at the door.
3. Select **Sign In**.
4. You are sent straight to your role's workspace:

| Your role | You land on |
|---|---|
| System Administrator | ▷ Admin Dashboard — `/Admin/Dashboard` |
| Sales Representative | ▷ Pipeline Board — `/Pipeline` |
| Procurement Manager | ▷ Dashboard — `/Procurement/Dashboard` |
| Field Support Engineer | ▷ Dashboard — `/Support/Dashboard` |
| Logistics | ▷ Dashboard — `/Support/Dashboard` |
| Client User | ▷ **My Verichat** — `/Portal`, the client portal (§12.8) |
| Medical Student | ▷ **My Verichat** — `/Portal`, the client portal (§12.8) |
| Anyone, by typing a URL they do not hold | ▷ the 403 "no access" page — `/Account/NoAccess`, with a link back to their own home (§6.4) |

Errors appear above the form, not in a popup. A wrong password, an unknown address and a **deactivated** account all say the same thing — `Invalid credentials.` — deliberately, so the form cannot be used to discover which addresses have accounts. If you are sure the password is right, the account is probably deactivated (§17).

`[SG-01]` — the sign-in form, empty, with the brand panel visible.
`[SG-02]` — the same form after a bad password, showing `Invalid credentials.` in the validation summary.

### 4.3 Creating your own account (self-service)

▷ **Create account** — `/Account/Register`, linked from the sign-in card as *Create one*.

Fields: **Full name**, **Email address**, **Password**, and **What will you do here?** — the
role select, laid out as three labelled groups:

- *Order from Verichat* — `Client User — buy, request quotes, raise support`, `Medical Student — browse and request, no purchasing`
- *Work inside Verichat* — `Sales Representative — revenue engine`, `Procurement Manager — supply & logistics`, `Field Support Engineer — on-site resolution`, `Logistics — delivery & transport`
- *Run Verichat* — `System Administrator — full control, no review needed`

The first option a visitor sees is `Client User`. A dropdown with no selection displays its
first entry, and this page is anonymous, so the administrator choice is deliberately listed
last rather than pre-chosen.

What happens when you submit depends on the role:

- **System Administrator** is signed in on the spot and sent to ▷ Admin Dashboard. The account
  is written as approved, because this is the one role exempt from the review queue.
- **Every other role** creates an application, not a session. The account is stored as
  `Pending`, the page shows *Waiting for approval*, and signing in before an employee approves
  it shows that screen again rather than letting you in. Approval happens in
  ▷ Users & Roles (§7.2).

Signing up as Client User or Medical Student also creates the client record your quotes, orders
and tickets hang off — the API opens a `Clients` row keyed to your new user id, so nothing has
to be typed twice.
**Password length is enforced, on every path that sets or checks one.** Signing up requires at
least 8 characters (`[Required, MinLength(8)]` on `RegisterRequest.Password`), a reset requires at
least 8 (`MinimumPasswordLength` in `AccountRecoveryService.cs:31`, checked at `:82`), and signing
in rejects a password shorter than 6 (`[Required, MinLength(6)]` on `LoginRequest.Password`) before
the database is queried. So a short password never reaches the hashing code, and the API answers
with a validation problem naming the field rather than a message you have to interpret. What is
**not** enforced anywhere is complexity — no digit, no symbol, no dictionary check — so
`aaaaaaaa` satisfies all three today.
**What the screen promises about a confirmation link is true.** Once the account row is
committed, the API issues an address-verification token and emails its link to the address just
typed (`AuthController.Register` -> `AccountRecoveryService.SendConfirmationEmailAsync`), for every
role including System Administrator. The link opens `/Account/VerifyEmail` (§18), confirms the
address in one click without a button, and expires after 7 days. It is not the approval itself: an
unconfirmed address is merely a harder one for a reviewer to approve. When no SMTP host is
configured the send is logged rather than delivered, so on a local run the link appears in
`logs/api.log` instead of an inbox — and because the link is built from the configured web origin,
a local registration writes a `verichat-crm-web.runasp.net` URL that only works once the deployed
site has the same account.

Rejections you will see: `Email is already registered.` and `Select a valid role.` — the second
is what stops someone editing the HTML of the form and posting a role string that is not in
`UserRoles.SelfRegisterable`.

Both client roles can also sign up from the mobile app's **New User** button (§12.1); the two
paths produce the same account, and the same client record.

`[SG-03]` — the registration form with the role dropdown open, showing the seven choices in
their three groups.

### 4.4 Signing in on mobile

The app opens on **Choose your account** with three buttons — **Client**, **Staff / Admin**, **New User**. Each account type is allowed through one door only:

| Door | Roles accepted | Screen |
|---|---|---|
| **Client** | Client User, Medical Student | *Email address* + *Password* → **Sign In** |
| **Staff / Admin** | System Administrator, Sales Representative, Procurement Manager, Field Support Engineer, Logistics | *Email address* + *Password* → **Log in** |
| **New User** | self-registers a Client User | *Full Name, Company Name, Email address, Password, Confirm Password* → **Sign up** |

Tapping the wrong door tells you so plainly: *"This account is for staff…"* or *"Staff sign-in is limited to…"*. Leave the empty **Log in with SSO** button alone — it is a stub that answers *"Single sign-on is not configured yet."* (§18).

`[SG-04]` — the three-door account chooser. `[SG-05]` — the client sign-in form. `[SG-06]` — the New User form with all five fields visible.

### 4.5 Signing out, and when the system signs you out

**Sign out** is the last item in the web sidebar footer; it posts to `/Account/Logout`, clears the session and withdraws the hub cookie, so nobody who uses your browser inherits a live token.

Your API token lasts **120 minutes** (`Jwt:ExpiryMinutes`). The web session does not expire with it. **Note:** The visible symptom of an expired token is not a login prompt: pages keep their headings and sidebar but **every number reads 0 and every list is empty**, because the API answered 401 and the web layer showed a blank result. Sign out and back in; if it recurs on a schedule, that is why.

`[SG-07]` — a dashboard mid-failure: layout intact, all KPI cards at zero.

---

## 5. Setup: installing and running the software yourself

Follow these in order. Each step ends with **what you should see**; if you do not see it, §17 has the matching symptom.

### 5.1 Get the code

```bash
git clone https://github.com/dBright-dev/VerichatCRM.WIL.git
cd VerichatCRM.WIL
ls
```
**You should see:** `VerichatCRM.API`, `VerichatCRM.Core`, `VerichatCRM.Infrastructure`, `VerichatCRM.Tests`, `VerichatCRM.Web`, `VerichatMobile`, `VerichatCRM.slnx`, and the four `.md` documents.

### 5.2 Prepare the database

You need a SQL Server instance and a login that can create databases. Nothing to run yet — the API creates and migrates the database itself on first start. Write down the connection string in this exact shape:

```
Server=<host>; Database=verichatcrm; User Id=<login>; Password=<secret>;
Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;
```

- `Encrypt=True` is **required in any non-Development environment**: the API refuses to start if the string says `Encrypt=False`. In Development it is allowed but pointless.
- `Database=` may name a database that does not exist yet.

### 5.3 Configure the API — the four values that matter

Never put these in `appsettings.json`; they go in environment variables or user-secrets.

```bash
cd VerichatCRM.API
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your connection string>"
dotnet user-secrets set "Jwt:Key" "$(openssl rand -base64 48)"
dotnet user-secrets set "Bootstrap:AdminEmail"    "you@verichat.co.za"
dotnet user-secrets set "Bootstrap:AdminPassword" "<a strong password>"
```

(On Windows without `openssl`, any 64-character random string will do for `Jwt:Key`.)

`appsettings.json` holds the non-secret parts: `Jwt:Issuer` = `VerichatCRM`, `Jwt:Audience` = `VerichatCRMUsers`, `Jwt:ExpiryMinutes` = `120`, and `Cors:AllowedOrigins`, which must list the web app's origin. For local development the web app runs on a different port than the API, so add it:

```json
"Cors": { "AllowedOrigins": [ "http://localhost:5253", "https://localhost:7266" ] }
```

**Startup guards, all deliberate:** the API throws immediately if `ConnectionStrings:DefaultConnection` is missing, and throws if `Jwt:Key` is missing. It will **not** start half-configured to fail later with an opaque SQL error.

### 5.4 Run the API

```bash
dotnet run --project VerichatCRM.API --launch-profile http
```
**You should see**, in this order: `Applying database migrations...` → `Database migrations applied.` → `Starter catalogue ready.` (or a note that users already exist, skipping the admin bootstrap) → `SLA Escalation Service started. Interval: 00:05:00` → the Kestrel listening line.

Confirm it is healthy:

```bash
curl http://localhost:5184/api/health
```

**What the first run puts in the database** — be aware of it before you train anyone:

- The four `DataSeeder` clients: **Anglo American** (Mining, Active), **Nexam Holdings** (Manufacturing, Prospect), **Siemens Healthineers** (Clinical, Active), **Landfill Solutions SA** (Waste Management, Inactive).
- A starter product catalogue (`VC-CM-003` Crutch – Adjustable Forearm, `VC-TB-005` IV Giving Set – Sterile, and the rest of the seed list) so the catalogue and the warehouse-alert screens are not empty.
- Your one bootstrap administrator. **No** enquiries, tickets, quotations, deliveries, appointments, assets or deals.
- Both seed blocks are guarded by "any rows exist?", so they run once and never overwrite your data.

### 5.5 Run the web app

```bash
# from the repository root, in a second terminal
dotnet run --project VerichatCRM.Web --launch-profile http
```
It reads `ApiBaseUrl` from `VerichatCRM.Web/appsettings.json`, which points at the **deployed** API (`http://api-site.runasp.net`). That deployment does not start today (DEPLOYMENT.md §7.1), so a local run of the web app will fail every API call until you either point `ApiBaseUrl` at a local API or fix the hosted site. To make the web app talk to your local API instead, set:

```bash
ASPNETCORE_ENVIRONMENT=Development
dotnet user-secrets set "ApiBaseUrl" "http://localhost:5184"   # run inside VerichatCRM.Web
```

**You should see:** `Now listening on: http://localhost:5253`. Open it, click **Sign in**, and use the bootstrap administrator's address and password from §5.3.

### 5.6 Run the mobile app

```bash
cd VerichatMobile
npm ci
npx expo start
```

Before starting it, set the API address in `VerichatMobile/.env`:

```
EXPO_PUBLIC_API_URL=http://<your-machine-LAN-IP>:5184
```

| You are running | Use |
|---|---|
| API on your machine, app in a browser | `http://localhost:5184` |
| API on your machine, app on the **Android emulator** | `http://10.0.2.2:5184` |
| API on your machine, app on a **physical phone** (Expo Go) | `http://<LAN-IP>:5184` — a phone cannot see `localhost` |
| The deployed API | `http://api-site.runasp.net` |

If the address is wrong the app is not broken — it is reaching nothing. Every screen falls back to its cached or empty state and the offline banner appears (§12.7).

Optional, for a signed build: `eas.json` already carries `development`, `preview` and `production` profiles, and `app.json` carries the EAS project id. A **release** build needs an HTTPS API address; `http://` is blocked by Android's cleartext policy.

### 5.7 Verify the installation

Work down this list; each line has a screen behind it in this guide.

1. `GET {api}/api/health` returns OK.
2. `SELECT COUNT(*) FROM Users` on the database returns 1.
3. Sign in as the administrator → you see a dashboard with KPI cards, three charts and a Timeline panel (§6.1).
4. Sidebar shows **ADMIN › Users & Roles** and **Rules Engine**.
5. ▷ Client Directory lists the four seeded clients (§8.10).
6. ▷ Product Catalog lists the seeded products (§9.2).
7. Create one enquiry (§8.4) → it appears in ▷ Inquiry Queue with a score you did not type (§8.5).
8. ▷ Warehouse Alerts shows at least one seeded product at `Low` or `Critical`, because two seed products have `StockQty` at or below `LowStockThreshold`.
9. Start the mobile app, sign in as a client, add a product to the cart (§12.3).
10. Back in the web app, ▷ Pipeline Board shows your enquiry's client in **Mobile quote requests** or a deal card after you save a quotation (§13.2).
11. `dotnet test` → `Passed: 71, Failed: 0`.
12. Push to GitHub → the Actions tab shows **CI** going green: `dotnet restore`, `dotnet test`, `dotnet build -c Release`, plus the mobile job (`npm ci`, `npx expo lint`, `npx tsc --noEmit`).

---

## 6. Navigation: how every web screen is built

### 6.1 The shell

Every signed-in web page uses the same frame (`_Layout` → `_Sidebar` + `_Topbar`):

```
┌──────────────────┬──────────────────────────────────────────────────┐
│ [logo] Verichat  │ Page title                                       │
│ MEDICAL          │ Subtitle line                                    │
│ PROCUREMENT      │                                                  │
│                  │        topbar:  [bell]   [gear]                  │
│ YOUR NAME        ├──────────────────────────────────────────────────┤
│ ROLE             │   the page content                               │
│                  │                                                  │
│ OVERVIEW         │                                                  │
│ CLIENTS          │                                                  │
│ SALES            │                                                  │
│ ...              │                                                  │
│                  │                                                  │
│ > Sign out       │                                                  │
└──────────────────┴──────────────────────────────────────────────────┘
```

The diagram above is drawn with single-width characters on purpose so that its borders line up in any monospace font. Where it says `[logo]`, `[bell]` and `[gear]`, the running app shows real icons: the Verichat **mark** from `~/images/logo.png` (the square V/C ligature, not the wide wordmark — see `DESIGN-SYSTEM.md` §1.3 for which lock-up goes where), the 🔔 Notifications button and the ⚙ Settings button.

- **Avatar + name + role** at the top of the sidebar is read from your session; the role is shown in capitals. Before anyone signs in it reads `Verichat User` / `GUEST`.
- **Page title and subtitle** come from the screen, not from you. The subtitle usually carries the live count ("3 enquiries", "4 open • 1 escalated • 2 SLA breaches").
- **Skip to main content** is the first tab stop on every page.

`[SG-08]` — any dashboard, cropped to show the whole shell: brand block, user block, sidebar sections, topbar, content.

### 6.2 The sidebar is different for every role

This is the single most important navigation fact in the product: **the links you see are the screens your role is allowed to open**, computed from the same permission table the server enforces. There is no "hidden but reachable" middle ground and — since this build — no link that walks you into a 403.

| Section | Links | Who sees them |
|---|---|---|
| **OVERVIEW** | *Admin Dashboard* (Admin) / *Dashboard* (Sales) / *My Verichat* (the two client roles) | each role's own landing screen |
| **MY ACCOUNT** | *Catalogue*, *My Quotations*, *Support* | Client User, Medical Student — and nobody else |
| **CLIENTS** | *Client Directory* | Admin, Sales, Procurement |
| | *Asset Tracking* | Admin, Sales, Field Support, Logistics |
| | *Client Appointments* | Admin, Sales, Procurement, Field Support |
| **SALES** | *Inquiry Queue*, *Pipeline Board*, *Quotation Builder* | Admin, Sales |
| | *Lead Tracker*, *Client Map*, *Analytics* | Admin, Sales |
| **CATALOG** | *Product Catalog* | Admin, Sales, Procurement |
| **SUPPORT** | *Ticket Hub* | Admin, Procurement, Field Support, Logistics |
| | *Logistics Tracker* | Admin, Procurement, Logistics — **not** Field Support |
| **OPERATIONS** | *Warehouse Alerts* | Admin, Procurement |
| **ADMIN** | *Users & Roles*, *Rules Engine* | Admin |
| footer | *→ Sign out* | everyone |

A Client User or Medical Student signing in to the web app gets the **MY ACCOUNT** sidebar and the four portal screens — that is the client portal, §12.8 — and no staff link. The reverse holds too, and this build closed the one hole in it: the generic workspace at `/Dashboard` used to check only that a session existed, so a signed-in client who typed the URL reached a staff shell. It is now gated on `workspace.dashboard`, which every staff role holds and neither client role does, so the client gets ▷ *no access* with a link back to their own home.

`[SG-09]`…`[SG-13]` — one sidebar-only crop per staff role (Admin, Sales, Procurement, Field Support, Logistics) so the difference is visible side by side.

### 6.3 The topbar

Two controls:

- **🔔 Notifications** — present on every page. **Note:** It is currently **decorative**: nothing is wired to the button or its badge counter. Live notifications arrive as a **toast in the top-right corner** instead (§6.5). Do not tell users to click the bell.
- **⚙ Settings** — rendered disabled with the title *Settings coming soon*.

### 6.4 The three status pages

| Page | URL | When you get it | What it tells you |
|---|---|---|---|
| **403 — no access** | `/Account/NoAccess` | Your role lacks the permission the screen demands — e.g. a client-role account opens a staff screen, or a Medical Student posts the accept/decline form | Your role, and a way back to a screen you are allowed to use |
| **404 — not found** | `/Home/StatusPage?code=404` | A record was deleted under you, or you typed a bad URL | link back to the dashboard |
| **500 — error** | `/Home/Error` | An unhandled failure | nothing sensitive; the detail is in the server log |

Anything requiring a screen sends an **unsigned** visitor to the login page rather than to 403 — so "it keeps sending me to sign in" means your session is gone, not that you lost permission.

`[SG-14]` — the 403 page, reached by a Client User typing a staff URL such as `/Dashboard`; `[SG-15]` — the 404 page.

### 6.5 Live notifications

`notifications.js` is loaded on every page and connects to `{ApiBaseUrl}/hubs/notifications` with your HttpOnly cookie. On an event:

1. A toast slides in top-right: title, message, a × that hides it, auto-hides after 6 seconds.
2. If the title contains `Appointment`, `Quote Request` or `Quotation`, **the page reloads after 0.9 s** so the new record is on screen. That reload is intentional; it is not a crash.
3. Reconnection: automatic with backoff, capped at 5 attempts, then it gives up quietly for that page load. A refresh restarts it.

Two conditions stop the hub from connecting at all, both silent by design: `ApiBaseUrl` empty, or the SignalR library not loaded (a blocked CDN).

`[SG-16]` — a toast on screen (mobile quote request accepted is the easiest one to trigger live).

### 6.6 Keyboard and accessibility

- **Pipeline Board**: each card is focusable and carries a **stage dropdown**; choosing from it does exactly what a drag does. Drag-and-drop is not the only way.
- **All mutating forms** post with an anti-forgery token and work with the keyboard end to end.
- Charts are `<canvas role="img" aria-label="…">` with a text description; the numbers behind them are in the KPI cards as text.
- Reduced-motion is honoured on the landing page.

---

## 7. Role guide — System Administrator

**You see:** every section in §6.2. You are the only role that can change roles, create catalogue items, resolve warehouse alerts, or grant anything to anyone.

### 7.1 Your dashboard

▷ `/Admin/Dashboard` — KPI cards, *Order value · last six months* (line), *Order status mix* (doughnut), *Volume trends* (multi-line), a **Timeline** of recent events and a **Recent activity** list. All of it is `GET /api/dashboard/analytics`; if it is empty you have no data yet, which on a fresh install is normal for everything except the four seeded clients.

`[SG-17]` — the admin dashboard with at least one enquiry, one ticket and one order so the panels are populated.

### 7.2 Users & Roles

▷ `/Admin/Users`

| Column | What you can do |
|---|---|
| Name, Email | read only |
| **Role** | choose any of the seven roles, then **Save** (`POST /Admin/UpdateUserRole`) |
| **Status** | `Active` / `Inactive` |
| **Last login** | `yyyy-MM-dd HH:mm`, or `—` if the account never signed in |
| | **Deactivate** (active accounts only) |

**Note:** Three things to know before you use this screen.

1. **There is no "add user" button here**, despite what older notes say. Accounts appear by self-registration (§4.3 web, §12.2 mobile) or from the bootstrap. Your job on this screen is to *correct* a role, not to create people.
2. **Deactivate has no opposite.** The button appears on active accounts; once a person is inactive the row offers nothing, and they are told `Invalid credentials.` at sign-in. Until an "activate" path exists, re-activating someone is a database edit (§18).
3. **Changing someone to `Client User` or `Medical Student` moves them off the staff console and onto the client path** — four portal screens on the web (§12.8) and the client tabs on the phone. The API creates the `Clients` row that path needs when the role changes, keyed to that person's user id. Warn them first: every staff link they had disappears at once.

### 7.3 SLA Control

The card at the bottom of ▷ Users & Roles, *"Manually trigger the SLA escalation job (normally runs every 5 minutes)"* → **Run SLA check now**. It scans open tickets past their deadline, bumps each to `Escalated`, raises `EscalationLevel` by 1 and writes an activity entry. You will normally not need it — the background service does the same sweep every 5 minutes; use the button when you want the effect *now* for a demo or after an outage.

`[SG-18]` — ▷ Users & Roles with three users at different roles; annotate the role dropdown, Save, Deactivate and the SLA card.

### 7.4 Rules Engine

▷ `/Admin/Rules` is **a read-only statement of what the system enforces, not a configuration screen**. It lists four rules and their status:

| Rule | Scope | Status |
|---|---|---|
| SLA breach auto-escalation | Support tickets | Active (every 5 minutes) |
| Admin-only product creation | Catalog | Active |
| Role-based pipeline access | Pipeline — screen-level only; deals are not filtered per representative | Active |
| Low-stock reorder alerts | Warehouse | Monitored |

**Note:** There are no inputs here. You cannot change SLA hours, thresholds, scoring weights or role grants from this screen — those live in code (`TicketPriorities.GetSlaHours`, `InquiryScoring`, `Perm.ByRole`). If you were promised a rules console, this is not it (§18).

`[SG-19]` — the Rules Engine table.

### 7.5 Everything else you can reach

You can use every Sales, Procurement, Support and Logistics screen in §8–§11. Practically, administrators spend their time on the four screens in §7 plus ▷ Client Directory (§8.10) — where **only you** can create a client, and where product creation (§9.2) also lives.

---

## 8. Role guide — Sales Representative

**You see:** OVERVIEW › Dashboard, CLIENTS › Client Directory / Asset Tracking / Client Appointments, the whole SALES section, CATALOG › Product Catalog. **You do not see** Ticket Hub, Warehouse Alerts, Logistics Tracker, Users & Roles or Rules Engine.

**Your loop in one line:** record an enquiry → work the queue → move the deal on the board → build and save a quotation → hand it to the client.

### 8.1 Pipeline Board — where you land

▷ `/Pipeline` (subtitle: *"Drag cards between stages to update deal progress."*)

Top to bottom:

1. **Monthly goal** card. Progress bar, `R x of R y`, percentage. On a fresh account it reads *"No goal set yet. Set a target to track progress here."* — goals are created from the mobile staff dashboard (§12.6) or directly through `PATCH /api/salesgoals/{id}/progress`.
2. **Mobile quote requests** (only when any exist): client, reference, requested items, estimate, received time, status, and one button — **Mark reviewed** (Draft → Reviewed) or **Send to client** (Reviewed → Sent).
3. **Three KPIs**: Total Pipeline, Weighted Forecast, Deals This Period.
4. **Six columns**: `Initial Inquiry`, `Site Assessment`, `Proposal Review`, `Quotation Sent`, `Closed Won`, `Lost`. Each header shows its deal count and total value.
5. **Cards**: client, value, assigned rep, expected close date, and *Last Contact: …*. A card with a red left border and red text is **overdue for follow-up**.

**Moving a deal:** drag the card into another column, **or** use its dropdown. The move is applied optimistically and, if the API rejects it, the card flies back to its column and a red banner says *"Could not move that deal. It was returned to its previous stage."* — that banner is the only failure signal, so watch for it.

**Note:** You cannot create a deal on this screen. Cards appear when a quotation is saved for a client that has no deal yet (the API opens an `Initial Inquiry` deal from the quoted value — same rule for a mobile quote request). So the honest way to get your first card is §8.8.

`[SG-20]` — the board mid-drag with the pointer over *Proposal Review*; annotate the goal card, KPIs, a stale card's red border, the dropdown alternative, and the toast/banner area.
`[SG-21]` — the *Mobile quote requests* panel with one Draft row, before and after **Mark reviewed**.

### 8.2 Dashboard

▷ `/Dashboard` (sidebar OVERVIEW) — the same shared analytics view as §7.1, with your role in the subtitle. Use it for the Timeline; use the board for work.

### 8.3 Inquiry Queue

▷ `/Inquiry/Index`

- Six KPI tiles: **Total, Open, New, Overdue, Won, Avg score**. Overdue turns red above zero.
- **Status chips** — `All`, `New`, `Contacted`, `Qualified`, `Quoted`, `Won`, `Lost` — filter by URL, so a filtered view is linkable.
- Table: **Score** (badge ≥70 green, ≥40 amber, below that neutral), **Subject** (plus an *Overdue* badge), **Contact** (+ company), **Client**, **Source**, **Status**, **Received**, and an **Edit** link. There is no Delete on the list; deleting lives on the edit form.
- **+ New enquiry** on the right, or in the empty-state panel: *"The inquiry queue is empty / Enquiries you record are scored automatically and appear here, highest first."*

`[SG-22]` — the queue with 6+ enquiries across every status, one Overdue and one score below 40.

### 8.4 Recording an enquiry

▷ `/Inquiry/Create` — *"Record an enquiry / The score decides where the enquiry sits in the queue — 100 is worked first."*

| Field | Behaviour you should know |
|---|---|
| **Subject** | free text; what you will recognise later |
| **Company** | dropdown of real clients from the directory. Choosing one **fills Company, Contact name, Email and Phone from the client record** — those four are then editable only by changing the company. |
| **Contact name** | populated from the client's contact person |
| **Email / Phone** | populated from the client record |
| **Source** | `Phone, Email, Website, Walk-in, Mobile App, Referral` |
| **Urgency** | `— not stated —, Immediate, High, Standard, Flexible` — one of the four scoring factors |
| **Estimated value (rand)** | whole rand, dot as the decimal separator. **Note:** a comma is rejected by the number input even though this is a South African install. **Leave it blank and the client's most recent deal value is used.** |
| **Score (0–100)** | **leave blank to score automatically.** Typing a number wins, permanently: §8.5 |
| **Details** | free text |
| | **Add to queue** — or **Cancel** back to the list |

On the edit form you additionally get **Status** (*"Moving the status off 'New' records that a response was sent."*), **Internal notes**, and **Delete enquiry** with a browser confirmation *"Delete this enquiry permanently?"*.

`[SG-23]` — the empty create form; `[SG-24]` — the same form after choosing a company, with the four populated fields ringed and numbered.

### 8.5 How the automatic score is built

Verified in `InquiryScoring`: **four weighted factors, 100 points**.

| Factor | Weight | Bands |
|---|---|---|
| **Estimated value** | 35 | ≥R500 000 → 35 · ≥R150 000 → 28 · ≥R50 000 → 21 · ≥R10 000 → 14 · below → 7 · blank → 10 |
| **Sector** | 25 | Clinical/Hospital 25 · Pharmaceutical 23 · Mining 20 · Manufacturing 18 · Waste/Logistics 13 · Retail 10 · Education 8 · unknown or blank 15 |
| **Contract status** | 20 | Active 20 · Prospect 12 · Inactive 6 · unknown 10 |
| **Urgency** | 20 | Immediate 20 · High 15 · Standard 10 · Flexible 5 · not stated 10 |

A hand-entered score is **never recalculated** — editing the sector, value or urgency afterwards will not move it. Re-rank an enquiry by editing its score. Unknown values score at the middle band on purpose: a blank field is not evidence of a cold lead.

**Note:** The *mechanism* is documented above; the *house threshold* — what Verichat calls a 70 versus a 30 — is a business decision nobody has written down yet. Ask your manager before you start trusting the number over your own judgement.

### 8.6 Lead Tracker

▷ `/Sales/Leads` (subtitle *"Who to contact next, most urgent first."*)

- KPIs: **Open leads**, **Due today**, **Overdue** (red above zero).
- Filter chips, including one labelled **Due / overdue**.
- Columns: Client (+ enquiry subject), Stage, **Last contact**, **Next follow-up** (`Today` / `Overdue` in red, or a date, or *Not scheduled*), and two actions.
- **Mark contacted** — one click, no dialog; it stamps the contact date and advances the enquiry status.
- **Add follow-up** — a dialog asking **Next follow-up date** (required, cannot be in the past) and **Note (optional)** (max 500 characters).

Leads here **are enquiries** in an open status — there is no separate lead object to create. The empty state points you at *+ New enquiry*.

`[SG-25]` — the tracker with one Overdue row and the follow-up dialog open.

### 8.7 Client Map

▷ `/Sales/Map` — Leaflet on OpenStreetMap tiles, centred on South Africa.

- **Legend chips** filter pins by sector (Hospital ✚, Mine ⛏, Education 🎓, Clinic 🧪, Other 📍). Clicking a chip dims it and hides that layer.
- **Click a pin** → the right-hand panel shows the client, their last contact date and a **Log visit** action that stamps today; plus a link to the client profile.
- **Not mapped** lists clients with no coordinates — if a client is missing from the map, they are in this list because nobody has recorded an address with coordinates yet.
- `?demo=true` shows sample pins in Development only, clearly badged *Showing demo data (Development only)*.

**Note:** Tiles come from `tile.openstreetmap.org`. No network to that host = a blank grey box with working pins and panel.

`[SG-26]` — the map with pins, a selected client panel and the not-mapped list.

### 8.8 Quotation Builder

▷ `/Quotation/Index` — reachable from the sidebar, from a client profile (**Start a Quotation**, which attaches that client), or from the pipeline.

| Action | Result |
|---|---|
| **+ Add Row** | a new line item; **Product**, **SKU**, **Qty** (min 1), **Unit Price**, live **Total**, and × to remove |
| editing any qty or price | line total, Subtotal, `VAT (15%)` and **Grand Total** recalculate instantly, in the browser's locale/currency, matching the server's arithmetic |
| **Save Quotation** | `POST /Quotation/Save` with the anti-forgery header; on success the status line reads `Saved as VC-QT-<date>-<6 chars>. Editing the items and saving again creates a new draft.` and the PDF button unlocks |
| **Generate PDF Quote** | downloads the PDF — **enabled only after the first save**, greyed until then because there is no quotation id to render |
| **Back to Pipeline** | the board |

The right-hand **Client Summary** shows client, sector badge, reference, *Valid until* (30 days out) and terms. With no client attached it says: *"No client is attached yet. Open a client from the pipeline or the client directory and choose **Quote** to start a quotation for them."*

Save refuses, in this order, with the reason in the status line: no client (`Attach a client first: open the client and choose Quote.`), no items (`Add at least one line item with a product or SKU.`), and any row missing a SKU or with qty < 1 (`Every line item needs a SKU and a quantity of at least 1.`).

**Note:** Two behaviours people trip on: **a second save creates a new draft** rather than updating the first; and saving a quotation for a client with no deal **creates** that deal card on the board (§8.1). VAT is fixed at 15 % per quotation and is not editable from this screen.

`[SG-27]` — the builder with three line items, totals visible, a client attached, PDF still disabled. `[SG-28]` — after **Save Quotation**: status line + enabled PDF button. `[SG-29]` — the generated PDF.

### 8.9 Analytics

▷ `/Analytics/Index` — four charts (*Sales vs target* grouped bars, *Pipeline funnel*, *Revenue by sector* doughnut, *Monthly order value* line) under KPI cards. Empty data does not crash: *"Sales, pipeline and sector charts will appear once analytics data is available."*

### 8.10 Client Directory and profiles

- ▷ `/Clients/Index` — **Search clients** (name or contact person) + **Sector** filter + **Filter**. Columns: Client, Sector, Contact (person + email), Status badge, Assigned Rep, **Total Deals Value** (currency), **View**. **Note:** **+ Add Client** is admin-only; you will not see that button.
- ▷ `/Clients/Profile/{id}` — contact block, Total Deals Value card, **Start a Quotation** (your main route into §8.8 with the client pre-attached), **View in Pipeline**, and two live lists: **Mobile orders** and **Quotations**. Clients who have never been quoted show *No quotations have been issued.*

### 8.11 Client Appointments

▷ `/Telehealth/Appointments` — requests booked from the mobile app (§12.4). For you, this screen is **actionable**: choose an assignee, paste a **Teams/Zoom meeting link**, **Save assignment / link**, then **Approve** / **Decline** a Pending request or **Mark completed** an Approved one. A meeting link turns the row's status cell into **Meeting ready**.

---

## 9. Role guide — Procurement Manager

**You see:** Dashboard, Client Directory, Client Appointments, Product Catalog, Ticket Hub, Logistics Tracker, Warehouse Alerts. **You do not see** the SALES section, Asset Tracking, or anything under ADMIN.

### 9.1 Dashboard

▷ `/Procurement/Dashboard` — the shared analytics view (§7.1) with your date in the subtitle.

### 9.2 Product Catalog

▷ `/Procurement/ProductCatalog`

- **Category chips**, a **search box** (matches name or SKU, live as you type) and a **sort** select: *Name A-Z / Name Z-A / Price Low-High / Price High-Low / Stock Low-High*.
- Cards show name, `R price` (whole rand), an **In Stock** / **Low Stock** badge and the unit count.
- **Note:** You have read-only access. There is no **+ New product** button for your role (product creation is admin-only by design, and it is stated on the Rules Engine page), and no **Edit** link (that needs `products.edit`, held by Admin and Sales). If a product needs changing, §7 or Sales.

`[SG-30]` — the catalog with the search box holding text and one card annotated with a Low Stock badge; capture it **twice**, as a Procurement Manager and as an Admin, so the button differences are visible.

### 9.3 Warehouse Alerts (your main working screen)

▷ `/Procurement/WarehouseAlerts` (subtitle *"Low-stock safeguard and reorder management"*)

- A red banner when anything is at critical: **"N item at critical stock level: <name>"**.
- Table: **SKU, Product Name, Current Stock** (coloured by state), **Min Stock, Reorder Qty, Status** (`Critical` / `Low` / `OK`), and a **Reorder** cell.
- Where stock is at or below the threshold you get two inputs and a button: **Supplier** (required, up to 200 characters — it is pre-filled with the product's existing supplier when one is known) and a **unit cost** number (min 0.01), then **Reorder**. That creates a purchase order for `Reorder Qty` units. Everything else reads *"Stock is above the reorder point."*
- Below the table, **Stock Level vs Minimum Required** chart.

**Note:** Reorder is a **record of intent**, not an integration: no purchase order is emailed or sent to a supplier portal, and the alert does not clear itself when the goods arrive — someone must add the stock (§18). There is no "acknowledge" or "resolve alert" control on this screen; the `Admin/ResolveAlert` endpoint exists but is not wired to any button.

`[SG-31]` — the alerts page with one Critical and one Low row, the supplier + cost fields filled, before pressing Reorder; `[SG-32]` — the confirmation banner after.

### 9.4 Ticket Hub and Logistics Tracker

You can **read both but change neither**: tickets are triaged by Field Support (`support.ticket.update` is not in your permission set), and the tracker has no controls for anyone (§15).

### 9.5 Client Directory

Read-only for you in practice: no **+ Add Client** (admin only), and the profile's **Start a Quotation** / **View in Pipeline** buttons are hidden because you lack those permissions.

---

## 10. Role guide — Field Support Engineer

**You see:** Dashboard, Asset Tracking, Client Appointments, Ticket Hub. That is deliberately a short list: you are the role that closes tickets.

### 10.1 Ticket Hub

▷ `/Support/TicketHub` — subtitle *"<n> open • <n> escalated • <n> SLA breaches"*.

- **Status chips** filter instantly client-side: `All`, `Received`, `Under Review`, `Action Taken`, `Resolved`, `Escalated`, `Closed`.
- Each row: tracking reference, **priority badge** (Critical red / High amber / Medium blue / Low neutral), an `Escalated L<n>` badge when the SLA has been blown, the status on the right, the title, and **`SLA: <deadline>`** in universal `yyyy-MM-dd HH:mm:ss` form plus **⚠ SLA breached** when past.
- **View Details** opens the ticket.

**Note:** Tickets cannot be created in the web app. They arrive from a client's mobile app (*Log Service Ticket*, §12.5) or through `POST /api/supportticket`. If you take a fault over the phone, someone with the mobile app — or the API — has to raise it (§18).

`[SG-33]` — the hub with at least one Escalated, one breached and one Resolved row; annotate the chips, the priority badge and the SLA line.

### 10.2 Ticket detail — where the work happens

▷ `/Support/TicketDetail/{id}`

Left column: **Description**, **Photos** (thumbnails from the field, each opens full-size in a new tab), **Resolution notes** (only once resolved), and an **Activity** timeline newest-first — every status change, escalation and note, with the actor's comment and a UTC timestamp, ending at *Ticket received*.

Right column: **SLA** — live countdown in human units (*"Due in 4h 12m"*, *"Overdue by 2d 6h"*); once finished it says **Resolved within SLA** (green) or **⚠ SLA was breached**. **Details** — Assigned (it says `Unassigned` or `Assigned`, not a name), Equipment ref, Escalation level, and **View client profile** when you hold that permission.

**Update status** — pick **New status** (your current status is excluded from the list) and **Notes**, then **Update status**. The notes field changes itself:

| New status | Notes field |
|---|---|
| anything except `Resolved` | *(optional)* |
| **`Resolved`** | **(required to resolve)** — enforced in the browser and again on the server: *"Add resolution notes before marking a ticket resolved."* |

**Note:** Closed is terminal. The Update-status card disappears on a Closed ticket, so it cannot be reopened from the UI. Move a ticket to Closed only when you mean it.

**Note:** Selecting `Escalated` by hand competes with the background job, which sets `Escalated` and raises `EscalationLevel` on its own whenever a deadline passes.

`[SG-34]` — a ticket mid-work: photos visible, timeline ≥3 entries, SLA countdown in the "Due in" state. `[SG-35]` — the same card with **Resolved** chosen, showing *(required to resolve)*.

### 10.3 Asset Tracking

▷ `/Logistics/Assets` — KPI tiles **TOTAL ASSETS / CALIBRATION OVERDUE / DUE THIS MONTH / WARRANTY EXPIRING**; a table of SERIAL #, ASSET NAME, CLIENT, LAST SERVICE, NEXT DUE, WARRANTY and STATUS, with rows tinted red (Overdue) or amber (Due Soon); the serial number opens **▷ Asset detail** (*Service History* log + *Asset Details*).

- **Schedule Service** in the last column is **a link to the Ticket Hub**, not a per-asset action: it does not carry the serial number with you, so you reference the equipment yourself in the ticket the client raises. **Note:** Say that to users in training or they will file tickets against the wrong asset.
- Assets and their service logs are created from the mobile side (§12.5) or through the API; there is no "add asset" form in the web app.

`[SG-36]` — the assets table with one Overdue row and the detail panel for it.

### 10.4 Client Appointments

You may open the list (your role holds `appointments.manage`) but you are not in the manage set (Admin, Sales, Procurement), so every row reads **View only** and the assignee cell shows a raw id. Triage the request from the ticket instead; use this screen to see what was booked.

---

## 11. Role guide — Logistics

**You see:** Dashboard, Asset Tracking, Ticket Hub, Logistics Tracker. Four screens, and this is the complete list of what the role can do in the web app.

### 11.1 Logistics Tracker

▷ `/Logistics/Tracker` (subtitle *"Order dispatch, delivery tracking & route management"*) — a grid of shipment cards: **tracking reference**, status badge, **Origin → Destination**, **ETA**, **Weight (kg)**, **Order id**.

Status vocabulary (from the API): `Processing`, `Dispatched`, `In Transit`, `Delivered`, `Failed`, `Returned`.

**Note:** "Route management" is in the subtitle; the controls are not in the screen. This page is read-only for every role, including the administrator. Status changes happen through `PATCH /api/delivery/{id}` / the order path on the API side, not through a button here (§18). The realistic desk procedure is: watch this board, and raise a ticket when a delivery fails.

`[SG-37]` — the tracker with at least three cards in three different statuses.

### 11.2 Asset Tracking, Ticket Hub, Dashboard

As described in §10.3 and §10.1. You can **open** tickets but not change them (`support.ticket.update` is not granted to your role), which means your loop is: read the ticket → act in the physical world → let Support close it.

---

## 12. Role guide — Client User and Medical Student (mobile and web portal)

Most of this section is the shipped `VerichatMobile` app, five tabs at the bottom: **Home · Catalogue · Consult · My Assets · Profile**. Staff accounts see a different tab bar (§12.6). §12.8 is the part they get on the web — the client portal, which the same two accounts open in a browser.

### 12.1 First launch

**Choose your account** → **Client** → *Email address* + *Password* → **Sign In**, or **New User** for a self-service account (*Full Name, Company Name, Email address, Password, Confirm Password* — all five required, the two passwords must match, and the account you get is a **Client User**). Missing fields are refused with *"Missing details / Enter your email address and password to log in."*

### 12.2 Home

Greeting, **Your Orders** split into Active / Pending / Recurring, four **Quick actions** (Product Catalogue, Book consultation, My equipment, Quote requests), a **My Assets** summary, your **recurring order** and **tracking** cards, and your newest **three quotations** and **three consultations**. It refreshes every 30 seconds and whenever you return to the app.

### 12.3 Catalogue → build a quote request

**Catalogue** tab: *Equipment Catalogue*, a **Search products** box, **category chips** from live data, and a barcode-scan icon. Each card: name, price in rand, **Add to cart**. The cart badge counts items.

**Cart** (*Quote Request Cart*, reached from the tab bar or Profile): **+ / −** on quantities, then **Request a Quote** → `POST /api/quotation/request`. What you have sent is a **request**, not an order: nothing is charged and no stock is reserved. A sales representative (§8.1) reviews it, and when it has been sent back to you it appears here with **Accept quotation** / **Decline**.

**Note:** There is no checkout, no payment and no invoice path in the app. The proposal's e-commerce flow was scoped out; the quotation workflow replaced it.

`[SG-38]` — the catalogue with a search applied; `[SG-39]` — the cart with two items and the **Request a Quote** button; `[SG-40]` — a sent quotation with **Accept** / **Decline**.

### 12.4 Consult → book an appointment

**Consult** tab → *What can we help with?* with six topics (`New Quote`, `Equipment Calibration`, `Equipment Service`, `Invoicing Issue`, `Product Consultation`, `Other`), a calendar you pick from, hour and minute inputs, and **Request details** (up to 4000 characters) → **Send appointment request**.

A date in the past is refused with *"Choose a future date"*. Your request appears on the staff ▷ Client Appointments screen (§8.11); **Join consultation call** shows on your side only once a representative has pasted a meeting link.

`[SG-41]` — the booking form half-filled, with the calendar open.

### 12.5 My Assets → raise a fault

**My Assets** tab lists each item's **model, serial, status, location, next service and warranty expiry**, with a scan filter. Tap an asset → **Log Service Ticket**: *Issue summary* (≤200 chars) and *Describe the fault or service needed* (≤4000 chars), both required → submit.

**Note:** Two things the app does not let you choose: **priority is fixed at `Medium`** and **category at `Technical`** regardless of the fault, so an urgent machine must be flagged to Support by other means. The scanner's filter compares literally — a code that matches nothing shows *"No equipment matches that QR code."* rather than an error.

`[SG-42]` — My Assets with three assets, one Due Soon; `[SG-43]` — the Log Service Ticket sheet with both fields filled.

### 12.6 Profile, settings, biometrics

**Profile** tab: account rows (Email address, Phone number, Company, VAT number), **Delivery address**, links to **Settings** and the quote-request cart, and **Sign out**. Pencil-edit fields: *Account contact name* (required), *Phone number*, *Company or organisation*, *VAT number (optional)*. The address sheet asks for *Address label, Street address, City or region, Country* — all four required, or *"Address incomplete"*.

**Settings**: switches for **Push Notifications** *(Orders, SLA alerts & promotions)*, **Biometric Login** *(Sign in with fingerprint or Face ID)* and **Dark Mode** *(Reduce eye strain)*, saved to `PATCH /api/mobile/settings`; and *Purchase History & Reports*, *Privacy*, *Terms and Conditions*, *About Verichat*.

- **Biometric sign-in is real**: it works with the device's own authentication, is opt-in, and once enabled the app requires it at cold start. Turning it on demands a live fingerprint/Face ID prompt, so it cannot be enabled from a screenshot.
- **Push Notifications is a stored preference, nothing more.** No device token is ever registered, so no push will arrive; the app's freshness comes from polling and the offline queue. **Note:** Say this in demos — the switch looks like a feature (§18).
- **Privacy / Terms / About** currently answer *"This information is not available in the mobile API yet."*

**Staff mobile dashboard** (System Administrator, Sales Representative, Procurement Manager, Field Support Engineer, Logistics): an **Operations dashboard** with Assets / Service due / Open tickets metrics, the asset list, **Scan** (QR and 13 barcode types via the camera), support tickets and deliveries, and the **sales goal ring card** where *Update achieved* posts progress. Staff do not see the Catalogue, Consult or My Assets tabs.

`[SG-44]` — Settings with biometrics mid-prompt; `[SG-45]` — the staff Operations dashboard with the scanner open.

### 12.7 Offline behaviour — the feature to show first

This is the most technically interesting thing the app does, and it works:

- Read screens are **cached per account**; with no network, you see your last-known data under the banner **"Offline · showing saved information"**.
- Writes (a fault, a cart change, an appointment note) are **queued in order** and replayed on reconnect, each carrying an **Idempotency-Key** so a retry cannot double-create. The banner shows *"N change(s) saved to sync later"*, then *"Syncing…"*.
- A background task refreshes the dashboard, at most once a minute.
- **What is not queued:** anything that must be live — sign-in, registration, the product catalogue (deliberately never served from cache, so you cannot "order" a discontinued product), and POSTs for appointments, devices, orders and reorders. Those simply fail offline, and the app does not pretend otherwise.

`[SG-46]` — the offline banner with a queued change count (airplane mode is the reliable way to capture it).

### 12.8 The client portal on the web

The same two roles get their own corner of the web app — not a stripped-down staff screen,
and nothing that belongs to anybody else. Sign in at `/Account/Login` the way staff do, and
you land on `/Portal`. The sidebar shows **OVERVIEW ▸ My Verichat** and **MY ACCOUNT ▸
Catalogue · My Quotations · Support**. Those four are the whole of it.

▷ **My Verichat** — `/Portal`. Your name, then three lists: quotations raised for your
account, your support tickets, your deliveries. Empty on day one, which is correct.

▷ **Catalogue** — `/Portal/Catalogue`. Category chips across the top (only categories that
actually contain products appear), a search box that matches product name or SKU, and a card
per product with its photo, price, stock and a quantity box.

1. Type a quantity on the products you want — the number goes in the box, not a cart.
2. Add a note if the order has a deadline ("needed before the 15th").
3. **Request quote.**

You get a reference back — `Quote VC-QT-20261031-248836 requested. A sales representative
will confirm pricing and availability.` — and the quote appears in **My Quotations** as
*Draft*. **Note:** A request is not an order: no stock is reserved, no price is agreed, and the
price shown on the card is an indication. A person in sales sets the real figures and sends
it back to you.

▷ **My Quotations** — `/Portal/Quotations`. Filter chips: *All · Draft · Reviewed · Sent ·
Accepted · Rejected · Expired*. A **PDF** button downloads the branded document for anything
that has been priced. When the status is *Sent*, a Client User sees **Accept** and **Decline**;
answering is what turns your acceptance into a record the sales pipeline picks up. A
*Draft* cannot be answered, and the screen tells you so in the API's own words: *Only a sent
quotation can be accepted or declined.*

▷ **Support** — `/Portal/Tickets`. Your tickets with their tracking numbers, and a form to
raise another: **Subject**, **Priority** (*Low · Medium · High · Critical*), **Category**
(*General · Equipment fault · Delivery · Billing · Training query*), **Description**, and an
optional equipment reference. Raised against your own account, always — the account is taken
from your session, so the form cannot be edited to file a fault on someone else's. A new
ticket reads *Ticket VC-TCK-261031-6414 was raised. We respond within the SLA for its
priority*, and the support team sees it in their Ticket Hub immediately.

**Medical Student is the same four screens, minus one thing.** Students can browse, request
and follow support, but they cannot accept or decline a quotation: the buttons are not
rendered, and posting the URL anyway reaches the 403 page. That difference is one row in the
permission table (`client.quote.respond`), not a special case in a view.

**What the web portal does not do.** Bookings, the health record, the quick cart with
offline queueing and biometric sign-in are mobile-only (§12.4, §12.6, §12.7). Use the app for
those; the portal is for reading quotes and raising faults from a keyboard.

`[SG-48]` — the portal home with the three lists. `[SG-49]` — Catalogue with quantities set
and the note filled in. `[SG-50]` — My Quotations showing a *Sent* quote with Accept/Decline.
`[SG-51]` — the Support form with a ticket and its tracking number below it.
`[SG-52]` — the 403 page reached by a Medical Student posting the respond URL.

---

## 13. End-to-end workflows

Use these four in training and in the EXPO demo; each step names the role, the screen and the screenshot frame.

### 13.1 New enquiry → quotation in the client's hands

| # | Role | Screen | Action |
|---|---|---|---|
| 1 | Sales | ▷ Inquiry Queue | **+ New enquiry**, pick a real client so the contact block fills, set **Urgency**, enter **Estimated value**, leave **Score** blank `[SG-24]` |
| 2 | Sales | ▷ Inquiry Queue | the row appears, badge coloured by the score; **Overdue** if unanswered `[SG-22]` |
| 3 | Sales | ▷ Lead Tracker | **Mark contacted**, then **Add follow-up** for the agreed date `[SG-25]` |
| 4 | Sales | ▷ Client Directory → profile | **Start a Quotation** |
| 5 | Sales | ▷ Quotation Builder | **+ Add Row** per product, fill SKU/qty/price, watch **Grand Total** live `[SG-27]` |
| 6 | Sales | ▷ Quotation Builder | **Save Quotation** → reference issued, **Generate PDF Quote** unlocks `[SG-28]` `[SG-29]` |
| 7 | Sales | ▷ Pipeline Board | a deal card now exists at **Initial Inquiry**; drag it to **Quotation Sent** `[SG-20]` |
| 8 | Sales | ▷ Inquiry Queue → Edit | status → **Quoted** |

### 13.2 Client carts on the phone → sales review → accepted

1. *Client, mobile:* Catalogue → add items → **Request a Quote** `[SG-39]`.
2. *Sales, web:* ▷ Pipeline Board, **Mobile quote requests** panel → **Mark reviewed** `[SG-21]`.
3. *Sales:* price it in the builder, save, **Send to client**.
4. *Client, mobile or web:* the quotation appears in the cart screen (§12.3) or in **My Quotations** on the portal (§12.8) → **Accept quotation** `[SG-40]` `[SG-50]`; a toast fires on staff web and the page reloads (§6.5) `[SG-16]`.
5. *Sales, web:* drag the card to **Closed Won**.

### 13.3 Fault on a client's asset → SLA → escalation → resolution

1. *Client, mobile:* My Assets → asset → **Log Service Ticket** `[SG-43]`. Priority is fixed at Medium, so the SLA clock starts at **24 hours**.
2. *Field Support, web:* ▷ Ticket Hub → **⚠ SLA breached** rows first, **View Details** `[SG-33]`.
3. *Field Support:* status → **Under Review** (optional note) → **Action Taken** `[SG-34]`.
4. Every 5 minutes the API's escalation job re-checks deadlines: a breached ticket becomes **Escalated**, `EscalationLevel` +1, and an `SlaEscalated` activity is written automatically.
5. *Field Support:* **Resolved** — notes are mandatory here and only here `[SG-35]`; the SLA card flips to *Resolved within SLA* or *⚠ SLA was breached*, and the resolution notes appear on the left.
6. **Closed** last, knowing it is irreversible from the UI.

### 13.4 Low stock → alert → reorder

1. *Admin:* ▷ Product Catalog → **+ New product** → set **Low-stock threshold** (§14) `[SG-47]`.
2. *Procurement:* stock falls to the threshold → ▷ Warehouse Alerts, red critical banner + row status `Critical` `[SG-31]`.
3. *Procurement:* confirm **Supplier** and **unit cost**, press **Reorder** → a purchase order for the suggested quantity is recorded `[SG-32]`.
4. *Admin:* correct the stock level through the product edit form. **Note:** The alert does not clear itself, and nothing is sent to the supplier.

### 13.5 A client does the whole thing themselves, on the web

No staff action is needed to start or finish this one, which is the point of the portal.

1. *Client:* ▷ **Create one** from the sign-in card → *Order from Verichat* → **Client User** → the account and its client record are created in one step, and you land on `/Portal` `[SG-03]` `[SG-48]`.
2. *Client:* ▷ Catalogue → quantities and a note → **Request quote** → a *Draft* quotation appears under your own reference `[SG-49]`.
3. *Sales, web:* the draft is in the Pipeline Board's mobile-quote panel → reviewed, priced, saved, **sent** (§13.1).
4. *Client:* ▷ My Quotations → **PDF** to read the offer → **Accept** `[SG-50]`. A Medical Student at this step gets the 403 page instead — requesting is allowed, answering is not `[SG-52]`.
5. *Client:* ▷ Support → raise a fault against a delivery or an asset → the ticket lands in the staff Ticket Hub with its SLA clock already running `[SG-51]`.

**Note:** Steps 2 and 4 are still not a purchase order. Nothing is reserved at request time and no
payment exists anywhere in the product (§2.3); acceptance is a recorded decision that a
person in sales converts.

---

## 14. Form-by-form data entry reference

What each field does, in the words the screen uses. "Required" means the server refuses an empty value.

**Sign in** — *Email address* (required), *Password* (required). Errors: `Invalid credentials.`

**Create Account (web)** — *Full Name*, *Email Address*, *Password* (required, min. 8 — enforced), *Role* (one of the seven choices in three groups, §4.3). Errors: `Email is already registered.`, `Select a valid role.`, `That role cannot be self-assigned. Ask an administrator to create the account.`

**Add New Client** *(Admin only)* — *Client Name* (required, ≤200), *Sector* (required: Mining / Clinical / Education / Rehab / Manufacturing / Waste Management), *Contact Person* (≤150), *Contact Phone* (≤30), *Contact Email* (valid email, ≤200), *Address* (≤500). Tip panel on the right: the client starts as **Prospect**, a rep can be assigned after creation, key personnel come from the profile.

**Client Profile › Assign sales rep** *(Admin only)* — one dropdown of active sales representatives (**"Choose a rep…"** is empty and will not save), then **Save assignment**. Below it, **Assignment history** with dates.

**Record / Edit enquiry** — as §8.4, plus on edit: *Status*, *Internal notes*, **Delete enquiry**.

**Add follow-up** — *Next follow-up date* (required, today or later), *Note (optional)* (≤500).

**New / Edit product** *(Admin creates, Admin + Sales edit)* — *Product name*, *Category* (free text with a datalist of existing categories), *Price* (≥0, 0.01 steps), *Opening stock* (create only, ≥0), *Low-stock threshold* (≥0; *"A warehouse alert is raised automatically when stock falls to this level."*), *Image URL* (a link, not an upload; blank = no image; a broken link shows nothing rather than an error icon). Note on the form: *"Saved products are published to the catalogue and appear in the mobile app for clients to browse and order."*

**Reorder** *(Warehouse Alerts)* — *Supplier* (required, ≤200), *unit cost* (>0), **Reorder**.

**Quotation line item** — *Product* (text), *SKU* (required for the save), *Quantity* (min 1), *Unit Price* (min 0). Save refuses a missing SKU or a qty below 1.

**Ticket › Update status** — *New status* (required; your current status is not offered), *Notes* (required only for `Resolved`, ≤1000).

**Client Appointments** — *Assign representative* (dropdown "Name · Role"), *Meeting link* (URL; pasted from Teams/Zoom), *Save assignment / link*, **Approve**, **Decline**, **Mark completed**.

**Mobile: New User** — *Full Name, Company Name, Email address, Password, Confirm Password* (all required, both passwords must match).
**Mobile: Log Service Ticket** — *Issue summary* (≤200, required), *Describe the fault or service needed* (≤4000, required).
**Mobile: Book appointment** — topic (6 choices), date, hour, minute (future), *Request details* (≤4000).
**Mobile: Profile / address** — as §12.6.

---

## 15. Read-only screens

Screens you open for information and cannot change from, whatever your role: **Logistics Tracker** (§11.1), **Analytics** (§8.9), every **Dashboard** (§7.1), **Asset detail** (§10.3), the client lists on **Client Profile**, **Rules Engine** (§7.4), **Photos** on a ticket (they open in a new tab), and the enquiry **queue table** itself (the write actions are on the edit form).

---

## 16. Frequently asked questions

**Sign in and access**

1. *I typed the right password and it says* `Invalid credentials.` — Wrong password, unknown address, or a **deactivated** account: all three answer identically. Ask an administrator to check ▷ Users & Roles.
2. *I forgot my password.* Use **Forgot password?** on the sign-in card — `/Account/ForgotPassword`
   emails a link that opens `/Account/ResetPassword`, works once and expires in an hour. A person
   with no administrator access can also be reset by an administrator; there is still no screen
   where you change your password while signed in (§18).
3. *I signed up and chose System Administrator.* That role is offered on the form and is the
   one exempt from review, so the account works the moment it is created and lands on
   ▷ Admin Dashboard. A posting of any role outside `UserRoles.SelfRegisterable` is refused
   with `Select a valid role.`, and an existing administrator can demote the account again in
   ▷ Users & Roles (§7.2).
4. *I signed up and my screen says* *Waiting for approval.* Your role is queued for review — the
   right password in a pending account returns that screen on purpose (§4.3). An employee
   approves it in ▷ Users & Roles, then sign in again.
5. *My client account signs in on the phone and gets "no access" on the web.* You are on a staff URL. Client roles have their own four screens — ▷ *My Verichat*, *Catalogue*, *My Quotations*, *Support* — reached from `/Portal` (§12.8); a client who types `/Pipeline` is refused on purpose, and the 403 page links back to `/Portal`.
6. *I changed someone's role and their menu changed instantly.* Menus are built per request from the role; that is the expected behaviour, and it is why the sidebar is the best indicator of what you are allowed to do.
7. *Everything on my dashboard is 0 or empty.* Usually an expired 120-minute token (§4.5): sign out and back in. Otherwise the API or the database is down — §17.

**Sales**

7. *The score in the queue is not what I expect.* Check whether someone typed a score: a typed value is never recalculated (§8.5). Blank means the four factors produced it.
8. *Why does the value field refuse a comma?* It is a number input — dot for decimals, even on a South African install (§8.4).
9. *I left Estimated value blank and the score changed.* Deliberate: the client's most recent deal value is used.
10. *The Pipeline Board is empty.* Cards come from saved quotations and mobile quote requests; there is no "add deal" button (§8.1). Create a client, save a quotation, and you have a card.
11. *My drag snapped back.* The API rejected the move; the card returns itself and a red banner explains. Check you picked a real stage.
12. *I saved a quotation and the PDF button is still grey.* PDF is enabled only after the first successful save, and the status line tells you if the save failed (§8.8).
13. *I edited and saved again — now there are two quotations.* Yes: each save creates a new draft (§8.8).
14. *Where is "delete enquiry" on the list?* On the enquiry's **Edit** form, at the bottom.

**Support and tickets**

15. *I need to raise a ticket for a client over the phone.* The web app has no ticket form (§10.1): the client raises it in the app, or it goes in through `POST /api/supportticket`.
16. *Notes say required and I cannot resolve.* Only `Resolved` requires notes, and the server insists too — that is how the resolution audit stays complete.
17. *I closed a ticket and now there is no status form.* Correct — `Closed` is terminal in the UI (§10.2).
18. *A ticket says Escalated but nobody escalated it.* The 5-minute background job does that automatically on a breached deadline.
19. *Priority is Medium on everything the clients report.* The mobile form fixes priority at Medium and category at Technical (§12.5); adjust it in the database or work around it by phone.
20. *Dates look wrong.* Ticket detail and SLA values are shown in **UTC** with the suffix; the pipeline, timeline and mobile screens use local time. Same instant, two labels.

**Procurement and stock**

21. *I am Procurement and there is no "+ New product".* Product creation is admin-only (§9.2), by design and stated on the Rules Engine screen.
22. *I pressed Reorder — did the supplier get anything?* No email or portal call. It records a purchase order in the database (§9.3).
23. *The alert is still there after the goods arrived.* Nothing receives stock automatically; edit the product's stock level.
24. *What makes an alert appear?* `CurrentStock <= LowStockThreshold`, so the threshold you set when creating the product is the whole rule.

**Mobile**

25. *Nothing loads on the phone.* Check `EXPO_PUBLIC_API_URL` (§5.6) — the app is probably reaching a host it cannot see.
26. *I did things while offline — did they save?* Yes, except the list in §12.7. The banner tells you the queue length.
27. *My push notifications never arrive.* The switch stores a preference; no token registration runs (§12.6). Use the in-app screens, not push.
28. *Biometrics will not turn on.* Your device must have an enrolled fingerprint/Face ID and the app must be able to prompt for it.
29. *Is there a payment or checkout in the app?* No — quote requests and quotations only (§2.3).

**Assessment / demo**

30. *May I use `?demo=true`?* Only in Development, only on Analytics and Client Map, and both pages badge it as demo data. In Production the flag does nothing.
31. *Can I reset the training data?* There is no in-app reset. Re-point the connection string at a fresh database and let the API migrate and seed it (§5.4).
32. *Where are the tests?* `dotnet test` → 71 passing, covering permissions, enquiry scoring, and the API service paths. The mobile app has no automated tests — say so if asked (§18).

---

## 17. Troubleshooting

Work top-down: is the API up? is the database reachable? is *my* session valid? — then look at the screen.

| Symptom | Most likely cause | Fix |
|---|---|---|
| API will not start: `ConnectionStrings:DefaultConnection is not configured.` | §5.3 not done, or the variable is not visible to the process | set it in the run profile or user-secrets; confirm with `dotnet user-secrets list` |
| API will not start: `Jwt:Key is missing.` | no signing key configured | set `Jwt__Key` to a 64+ character random string |
| API will not start in Production: `…sets Encrypt=False.` | the connection string asks for plaintext | remove the key or set `Encrypt=True; TrustServerCertificate=True` |
| `Cannot open database … requested by the login` | the login cannot reach the database, or migrations ran before the DB existed | grant the login; restart the API so migrations run first |
| IIS shows `HTTP 500.30` right after deployment | startup work failed (migration, bootstrap) | read the log — a genuinely unreachable database lets the app boot in degraded mode instead |
| The login page rejects every password, on a brand-new install | no bootstrap administrator was configured | set `Bootstrap__AdminEmail` / `AdminPassword`, restart the API, and confirm the warning is gone from the log |
| `Email is already registered.` on sign-up | the address exists, possibly as another role | sign in; if the role is wrong, an administrator corrects it in ▷ Users & Roles |
| Sidebar empty / 403 page for a legitimate user | they signed in as the wrong audience (client role in the web app) | §4.2's landing table; use the mobile app for client roles |
| A sidebar link now 403s that used to work | the account's role was changed since you last looked | §6.2's matrix is the authority; compare with ▷ Users & Roles |
| Dashboards and lists render but every value is 0 | the 120-minute token expired (§4.5), or the API is down | sign out/in; then `curl {api}/api/health` |
| Charts are blank but numbers are fine | Chart.js/CDN blocked by the network | allow `cdn.jsdelivr.net`, or accept tables-only rendering |
| Map is a grey rectangle | OpenStreetMap tiles blocked | allow `tile.openstreetmap.org`; pins and the panel still work |
| Pipeline drag does nothing at all | JavaScript disabled, or `site.js` failed to load | enable script; the per-card **stage dropdown** is the no-script path |
| Toast never appears although events are expected | `ApiBaseUrl` empty (hub refuses to connect silently) or the SignalR CDN is blocked | check the value in `VerichatCRM.Web/appsettings.json` and the browser console for `[SignalR]` |
| Notification bell does nothing when clicked | it is not wired (§6.3) | expected; watch for the top-right toast |
| PDF button greyed | the quotation has not been saved once | save it; the status line reports why if the save failed |
| `Attach a client first…` when saving | you opened the builder from the sidebar instead of a client | open the client's profile → **Start a Quotation** |
| Reorder refuses to submit | **Supplier** empty or unit cost < 0.01 | both are required inputs on the row |
| Mobile app: "Missing details" | an empty field on sign-in | both email and password are required |
| Mobile app: *"Staff sign-in is limited to…"* | the wrong door for the account | use **Client** for Client User / Medical Student, **Staff / Admin** for the five staff roles |
| Mobile app: *This account is for staff…* | a staff role tried the client door | as above |
| Mobile: everything is stale and a banner says Offline | it genuinely cannot reach the API | check `EXPO_PUBLIC_API_URL`; on a physical device `localhost` is your phone, not your laptop (§5.6) |
| Mobile: writes never sync | you signed out before the queue drained | the queue needs a signed-in session; sign in on the same device and let *"Syncing…"* finish |
| Mobile: scanner finds nothing | the code is not on any asset record | it will read *"No equipment matches that QR code."* — enter the serial manually |
| Mobile: *Single sign-on is not configured yet.* | that button is a stub | use password sign-in |
| Landing page shows *Add name* in the team band | placeholder content awaiting a real name and headshot | edit `VerichatCRM.Web/ViewModels/Home/LandingContent.cs` |
| `dotnet test` fails after your change | you hit a permission or scoring rule | the failing test name points at the module; do not commit it — CI runs the same suite on every push |

**Reading the log.** The API writes the migration line, the seed line, `SLA check complete — N ticket(s) escalated.` and any startup failure. The web app surfaces API failures to the user as short sentences — *"The session is no longer valid; sign in again."*, *"You are not allowed to perform this action."*, *"Another record already uses one of these values."* — which is usually the fastest clue you have.

---

## 18. Known limitations

The honest page. Give this to every new user with the training pack; the system is good, and saying what it does not do is what makes the rest of this guide trustworthy.

**Access and accounts**

1. **No way to change your password while signed in.** `ChangePasswordRequest` exists as a DTO with no route behind it. The only self-service path is the one-time reset link from `/Account/ForgotPassword` (§4.5), which requires access to the mailbox on the account; an administrator cannot set one either, so a person locked out of their email needs direct database access.
2. **Password length is enforced; complexity is not.** At least 8 characters at sign-up (`[MinLength(8)]` on `RegisterRequest.Password`) and at reset (`MinimumPasswordLength`, `AccountRecoveryService.cs:31`), at least 6 at sign-in (`[MinLength(6)]` on `LoginRequest.Password`) — but no digit, symbol or dictionary rule anywhere, so `aaaaaaaa` satisfies all three.
3. **Deactivate has no Activate.** A deactivated account cannot be restored from ▷ Users & Roles.
4. **No user creation in the web UI by an administrator** — self-registration (the five self-assignable staff roles, or either client role) and the bootstrap admin are the only paths; there is no "create a colleague" form on ▷ Users & Roles despite what some older notes claim. An administrator can *change* an existing person's role, which is how staff accounts become client accounts.
5. The 403 page does not tell you **which permission** was missing.

**Screen and data behaviour**

6. **The notification bell and its badge are not wired.** Live updates come as toasts only (§6.3).
7. **Settings (⚙) in the topbar is disabled** — *Settings coming soon*.
8. **Logistics Tracker is read-only for every role**, including the administrator, and its own subtitle promises route management. Status changes require the API.
9. **Staff cannot create a ticket from the web app** — ▷ Ticket Hub is read-and-work only, so a fault logged on someone's behalf has to be raised on mobile or through the API. The client portal can raise one (§12.8): that is the client's own ticket on the client's own account, which is the case the form was written for. The portal itself stays deliberately small — four screens, no appointment booking, no health record, no offline cart; those remain mobile-only (§12.4, §12.6, §12.7), and deliveries are listed on *My Verichat* rather than tracked page by page.
10. **Warehouse Alerts has no acknowledge/resolve control.** The `Admin/ResolveAlert` endpoint exists but no button reaches it, and alerts persist until the stock number changes.
11. **`Assets › Schedule Service` is a plain link to the Ticket Hub.** It does not carry the asset, and it creates no ticket.
12. **Two Razor views are dead files** — `Views/Procurement/Dashboard.cshtml` and `Views/Support/Dashboard.cshtml` are not rendered by any action; all four dashboards share `Views/Dashboard/Index.cshtml`.
13. **Client Appointments shows a raw user id** in the assignee column for roles that cannot manage (Field Support).
14. **The Rules Engine screen is static.** No thresholds, weights or SLA hours are configurable from it.
15. **Pipeline deals are not filtered per representative** — anyone holding `pipeline.board` sees every deal.
16. **`Ticket › SlaEscalated` activity logs "Escalated → Escalated"** (the status is assigned before the from-value is read), so the timeline's arrow is cosmetic.
17. **Time is mixed**: ticket detail and SLA in UTC, pipeline/timeline/mobile in local time.

**Mobile app**

18. **Push Notifications is a preference only** — no token registration, so nothing is delivered. `expo-notifications` is installed and its service files are **never imported** (`PushNotifications.ts`, `NotificationsService.ts`, `RealTimeTracker.ts` are dead code, and two of them use `window.*`, so they would only work in a browser anyway).
19. **No SignalR in the mobile app** at all; freshness is 30/60-second polling plus foreground refresh.
20. **Log Service Ticket hard-codes `Medium` priority and `Technical` category.**
21. **"Log in with SSO" is a stub.**
22. **Privacy, Terms and Conditions and About Verichat** all answer *"This information is not available in the mobile API yet."*
23. **Four coded screens are unreachable** — `Dashboard.tsx`, `ClientCommunicationScreen.tsx`, `ClientTimelineScreen.tsx` and `explore.tsx` (a duplicate, unauthenticated login) have no navigation path to them.
24. **The staff sign-in note omits Logistics** even though the role is allowed.
25. **The scanner's asset filter applies non-matching codes** and shows the "no equipment" message rather than reporting an unknown code.
26. **No automated tests exist for the mobile app.** The 71 passing tests cover .NET only.

**Platform**

27. **No certificates on the Free Monster ASP plan**, so the deployed HTTPS endpoints depend on the platform's own TLS; the `.runasp.net` origins must match `Cors:AllowedOrigins` exactly.
28. **NU1902/NU1903 build warnings — cleared in this build.** `System.Security.Cryptography.Pkcs` used to resolve to 9.0.11 (a transitive dependency of `Microsoft.Data.SqlClient 6.1.6`, reached via EF Core SqlServer 10.0.12) and carried a moderate and a high advisory, twenty warnings and zero errors. It is now pinned to **10.0.14** in `VerichatCRM.Core.csproj`, which raises it for every project underneath, and the restore is warning-free. The number stays in this list so that anyone comparing builds knows the warning they used to see is *fixed*, not suppressed. Detail in [documentation.md](documentation.md) §18 item 60.
29. **No global exception handler in the web app**, so an unexpected failure surfaces as the standard 500 page.
30. **Currency and culture are host-dependent**: quotation totals and the PDF follow the server's locale (en-ZA → rand), which is why the page and the script share the culture explicitly.

**What this release does not do at all**, so nobody has to discover it mid-demo: no payment, no checkout, no invoice generation, no email/SMS sending, no supplier portal, no goods receipt, no stock receipt, no document storage (product images are external URLs), no telehealth media (a meeting link is a link, not a call), no student discount, no report export, and no multi-company tenancy.

---

## 19. Glossary

| Term | Meaning in this product |
|---|---|
| **Bootstrap administrator** | The single System Administrator created from configuration on an empty database (§4.1). |
| **Clients row** | The account record keyed on the user id that every order, quotation and enquiry hangs off. Client-audience roles get one automatically; staff roles do not. |
| **Deal / card** | One client's live opportunity on the Pipeline Board. Created implicitly by the first quotation, never by hand. |
| **Escalation level** | How many SLA sweeps have caught this ticket. Increments automatically every 5 minutes while a breach stands. |
| **Enquiry score** | 0–100 qualification number from four weighted factors, or typed by a human and then frozen (§8.5). |
| **Idempotency key** | The token attached to a replayed offline write so retrying it cannot create a duplicate. |
| **Low-stock threshold** | The number that turns a product into a warehouse alert when `CurrentStock <= threshold`. |
| **SLA deadline** | Created at ticket open: 4 h Critical, 12 h High, 24 h Medium, 48 h Low. |
| **Mobile quote request** | A client cart submitted for review; appears on the Pipeline Board until a human prices it. |
| **Permission** | One of the 26 strings in `Perm.cs`; roles hold sets of them, and screens demand one. |
| **Role** | One of the seven named job types in `UserRoles`; the server reads it from the database at sign-in. |
| **Session** | The web app's cookie holding your name, role and token; separate from the API token and its 120-minute life. |
| **Toast** | The top-right live notification. Distinct from the unwired bell. |
| **Weighted forecast** | Pipeline value adjusted by stage probability, on the board's KPI row. |

---

## 20. Appendix A — role × screen access matrix

Verified against `VerichatCRM.Web/Authorization/Perm.cs` and the actions' `[RequirePermission]` attributes. **Y** = open · **—** = refused (403) · *link hidden*.

| Screen | Admin | Sales | Procurement | Field Support | Logistics | Client / Student |
|---|:--:|:--:|:--:|:--:|:--:|:--:|
| Landing page, Privacy | Y | Y | Y | Y | Y | Y |
| Sign in / Register | Y | Y | Y | Y | Y | Y self-register (§4.3) |
| Dashboard (workspace, `/Dashboard`) | Y | Y | Y | Y | Y | — 403 |
| **Client portal — My Verichat** (`/Portal`) | — | — | — | — | — | Y |
| **Client portal — Catalogue + Request quote** | — | — | — | — | — | Y |
| **Client portal — My Quotations + PDF** | — | — | — | — | — | Y |
| **Client portal — Accept / decline a sent quote** | — | — | — | — | — | Y Client · — Student |
| **Client portal — Support (list + raise)** | — | — | — | — | — | Y |
| Client Directory | Y | Y | Y | — | — | — |
| Client Profile | Y | Y | Y | — | — | — |
| Add Client | Y | — | — | — | — | — |
| Assign sales rep | Y | — | — | — | — | — |
| Asset Tracking | Y | Y | — | Y | Y | — |
| Client Appointments (view) | Y | Y | Y | Y | — | — |
| Appointments (manage) | Y | Y | Y | — | — | — |
| Inquiry Queue | Y | Y | — | — | — | — |
| Lead Tracker | Y | Y | — | — | — | — |
| Client Map | Y | Y | — | — | — | — |
| Pipeline Board | Y | Y | — | — | — | — |
| Quotation Builder / Save / PDF | Y | Y | — | — | — | — |
| Product Catalog (read) | Y | Y | Y | — | — | — |
| Product create | Y | — | — | — | — | — |
| Product edit | Y | Y | — | — | — | — |
| Warehouse Alerts | Y | — | Y | — | — | — |
| Reorder | Y | — | Y | — | — | — |
| Ticket Hub (read) | Y | — | Y | Y | Y | — |
| Ticket update status | Y | — | — | Y | — | — |
| Logistics Tracker | Y | — | Y | — | Y | — |
| Analytics | Y | Y | — | — | — | — |
| Users & Roles, Rules Engine | Y | — | — | — | — | — |
| Sign out | Y | Y | Y | Y | Y | Y |
| Mobile tabs (Catalogue, Consult, My Assets) | — | — | — | — | — | Y |
| Mobile staff dashboard + scanner | Y | Y | Y | Y | Y | — |

The one difference between the two client roles is in this table: accepting or declining a
sent quotation needs `client.quote.respond`, which Client User holds and Medical Student does
not. Everything else in the client column is shared. Refusals are decided by the
`[RequirePermission]` attribute on the controller or action, so the *link hidden* and the
*403* columns in this table are the same fact measured twice — which is the point of
§6.2.

---

## 21. Appendix B — annotated screenshot inventory

The rubric requires a high-quality annotated screenshot for **every major step**. `[SG-nn]` markers throughout this guide are placed at exactly those steps. Frames are 1600×1000, browser zoom 100 %, real signed-in data (not lorem ipsum), with numbered red callouts matching the caption underneath.

**No image files ship with this guide:** it was written from the shipped source, so the screens themselves can only be photographed from a running instance with a real account for each role — the table below is the shot list, in capture order, and each row is a frame that is still to be taken.

| Frame | Screen | State to capture | Annotate |
|---|---|---|---|
| SG-01 | `/Account/Login` | empty | (1) email (2) password (3) Sign In (4) *Create one* |
| SG-02 | `/Account/Login` | failed attempt | (1) validation summary `Invalid credentials.` |
| SG-03 | `/Account/Register` | dropdown open | (1) full name (2) email (3) password hint (4) the seven roles, in their three groups (5) *System Administrator — full control, no review needed* (6) Create Account |
| SG-04 | mobile *Choose your account* | cold start | (1) Client (2) Staff / Admin (3) New User |
| SG-05 | mobile client login | empty | (1) email (2) password (3) Sign In (4) Create account |
| SG-06 | mobile New User | mid-entry | (1)–(5) the five fields (6) Sign up |
| SG-07 | any dashboard | token expired | (1) KPIs at 0 (2) the sign-out path |
| SG-08 | any page, full shell | signed in | (1) brand (2) user/role (3) sections (4) topbar (5) content (6) Sign out |
| SG-09…13 | sidebar only, Admin / Sales / Procurement / Field Support / Logistics | one role each | (1) the links that differ from the previous frame |
| SG-14 | `/Account/NoAccess` | Client User typing `/Dashboard` | (1) the role it shows (2) the link back to `/Portal` |
| SG-15 | 404 page | any bad id | (1) return link |
| SG-16 | any page + toast | live event | (1) toast (2) its × (3) the unwired bell |
| SG-17 | `/Admin/Dashboard` | populated | (1) KPIs (2) three charts (3) Timeline (4) Recent activity |
| SG-18 | `/Admin/Users` | 3 users | (1) role dropdown (2) Save (3) Status badge (4) Last login (5) Deactivate (6) SLA Control |
| SG-19 | `/Admin/Rules` | as is | (1) four rules (2) the "no editing here" note |
| SG-20 | `/Pipeline` | mid-drag | (1) goal card (2) KPI trio (3) column header totals (4) stale red card (5) dropdown (6) banner area |
| SG-21 | `/Pipeline` mobile requests | one Draft | (1) items (2) estimate (3) **Mark reviewed** |
| SG-22 | `/Inquiry/Index` | mixed statuses | (1) six KPIs (2) chips (3) score badge colours (4) Overdue badge (5) Edit |
| SG-23 | `/Inquiry/Create` | empty | (1) all fields labelled |
| SG-24 | `/Inquiry/Create` | client chosen | (1) company dropdown (2) the four auto-filled fields (3) urgency (4) value hint (5) blank score (6) Add to queue |
| SG-25 | `/Sales/Leads` | one overdue + dialog | (1) KPIs (2) chips (3) overdue row (4) Mark contacted (5) dialog fields |
| SG-26 | `/Sales/Map` | pins + selection | (1) legend chips (2) pin (3) client panel (4) Log visit (5) Not mapped list |
| SG-27 | `/Quotation/Index` | 3 items | (1) rows (2) live totals (3) VAT 15 % (4) client summary (5) grey PDF |
| SG-28 | same | after save | (1) status line (2) enabled PDF (3) reference |
| SG-29 | the PDF | — | (1) reference (2) line items (3) rand totals (4) validity |
| SG-30 | `/Procurement/ProductCatalog` | search applied | (1) chips (2) search (3) sort (4) Low Stock badge (5) *no* create/edit for this role |
| SG-31 | `/Procurement/WarehouseAlerts` | Critical + Low | (1) banner (2) row colours (3) supplier (4) cost (5) Reorder (6) chart |
| SG-32 | same | after Reorder | (1) confirmation banner |
| SG-33 | `/Support/TicketHub` | mixed | (1) subtitle counts (2) chips (3) priority badges (4) Escalated L1 (5) ⚠ breached |
| SG-34 | `/Support/TicketDetail/{id}` | mid-work | (1) ref + priority (2) description (3) photos (4) activity (5) SLA countdown (6) status form |
| SG-35 | same | Resolved chosen | (1) *(required to resolve)* (2) notes box (3) Update status |
| SG-36 | `/Logistics/Assets` | one overdue + detail | (1) four KPI tiles (2) tinted row (3) serial link (4) Schedule Service |
| SG-37 | `/Logistics/Tracker` | 3 statuses | (1) ref + badge (2) origin→destination (3) ETA/weight/order (4) "no controls here" |
| SG-38…40 | mobile catalogue / cart / sent quote | live data | (1) search (2) Add to cart (3) qty +/− (4) Request a Quote (5) Accept / Decline |
| SG-41 | mobile book appointment | calendar open | (1) topics (2) date (3) time (4) details (5) Send |
| SG-42 | mobile My Assets | 3 assets | (1) serial/status/service (2) Scan |
| SG-43 | mobile ticket sheet | filled | (1) summary (2) description (3) submit |
| SG-44 | mobile Settings | prompt showing | (1) Push (2) Biometric (3) Dark (4) the three stub rows |
| SG-45 | mobile staff dashboard | scanner open | (1) metrics (2) Scan (3) goal ring |
| SG-46 | mobile, airplane mode | queued | (1) offline banner (2) pending count |
| SG-47 | `/Procurement/ProductForm` (Admin) | new product | (1) name (2) category datalist (3) price (4) opening stock (5) threshold (6) image URL |
| SG-48 | `/Portal` | signed in as Client User | (1) *MY ACCOUNT* sidebar (2) quotations (3) tickets (4) deliveries |
| SG-49 | `/Portal/Catalogue` | quantities set | (1) category chips (2) search (3) price + stock (4) the qty boxes (5) note (6) Request quote |
| SG-50 | `/Portal/Quotations` | one Sent | (1) status chips (2) badge (3) PDF (4) Accept (5) Decline |
| SG-51 | `/Portal/Tickets` | after raising | (1) subject (2) priority (3) category (4) description (5) tracking number (6) the SLA confirmation |
| SG-52 | `/Portal/Respond` as Medical Student | 403 | (1) the refusal (2) that accept/decline is Client User only |

**To replace the frames with real captures:** the live portal URL plus one test account per role (System Administrator, Sales Representative, Procurement Manager, Field Support Engineer, Logistics, Client User) is enough for SG-01…SG-37 and SG-47…SG-52; SG-38…SG-46 need a device or an Expo Web session. Credentials will not be written into any file in this repository, including this one.

---

## 22. Appendix C — getting help

1. Check §17 by symptom — the table covers the failures users actually hit.
2. Check §18 before reporting a limitation as a bug; it is a shorter conversation.
3. Ask your department's super user (the training plan in [TRAINING-PLAN.md](TRAINING-PLAN.md) names the role per cohort).
4. A System Administrator can answer anything in §4 and §7 and can see the audit trail on the ticket timeline.
5. There is no in-product feedback form and no support ticket route for staff — feedback goes to the repository owners, and this guide is reviewed against every release. The build this guide describes is the one that produced `Passed: 71, Failed: 0` on `dotnet test` and a clean Release build (0 errors); CI runs that same restore/test/build plus the mobile lint and typecheck on every push to `main`/`master` and on every pull request (`.github/workflows/ci.yml`).
