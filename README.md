# Verichat CRM and Medical Logistics System

Verichat is a role-based CRM and medical-supplies workflow with a browser-based staff application, an ASP.NET Core API, a SQL Server database, and an Expo mobile client. This README describes the code that is in this repository.

A recorded walkthrough of the running system is at [https://youtu.be/sKW-5iuRPyI](https://youtu.be/sKW-5iuRPyI).

## Documentation in this repository

Seven documents are part of the repository and refer to each other. If you are looking for one question, this table says which file answers it.

| Document | Answers | Length |
| --- | --- | --- |
| `README.md` | What the system is, how to run it, what is live and what is not. | this file |
| [`userguide.md`](userguide.md) | How each of the seven roles works the system, screen by screen. | ~1,300 lines |
| [`documentation.md`](documentation.md) | Every requirement in the two project proposals, against the code that meets it or does not. | ~900 lines |
| [`VerichatCRM_FeatureWalkthrough.md`](VerichatCRM_FeatureWalkthrough.md) | What the delivered features do, in the order a person clicks through them. | ~300 lines |
| [`DEPLOYMENT.md`](DEPLOYMENT.md) | Hosting, where each secret belongs, credential rotation, the migration runbook. | ~1,300 lines |
| [`TRAINING-PLAN.md`](TRAINING-PLAN.md) | Rollout, training sequence, risks and mitigations. | ~350 lines |
| [`DESIGN-SYSTEM.md`](DESIGN-SYSTEM.md) | The brand mark, the palette and the rule that decides whether a screen belongs to it. | ~280 lines |

What is *not* here: the Final Report and the EXPO pack are submitted through the assessment portal rather than versioned with the code, so a reader who looks for them in this repository will not find them and should not assume they were skipped.

## Hosted applications

Four deployment targets are configured. They are not in the same state, so each was requested over HTTP rather than described from the publish profile — results below are from **2 November 2026**.

| Surface | URL | What it answers |
| --- | --- | --- |
| Staff web application | [http://verichat-crm-web.runasp.net/](http://verichat-crm-web.runasp.net/) | **200.** The landing pages, `/Account/Login` and `/privacy` render. |
| API on Monster ASP | [http://api-site.runasp.net/health](http://api-site.runasp.net/health) | **200 `Alive`.** The process starts; `/` redirects. |
| API readiness, Monster ASP | [http://api-site.runasp.net/api/health](http://api-site.runasp.net/api/health) | **503 `Degraded`** — `database: Unhealthy`, `jwt: Healthy`. |
| API readiness, Azure App Service | [https://verichat-api-aqcbgydhhbgbgha5.eastus-01.azurewebsites.net/api/health](https://verichat-api-aqcbgydhhbgbgha5.eastus-01.azurewebsites.net/api/health) | **503 `Degraded`** — same shape, `jwt: Healthy`. Served over HTTPS with a valid certificate. |
| API, second Monster name | [http://verichat-crm-api.runasp.net/](http://verichat-crm-api.runasp.net/) | **`HTTP Error 500.30 - ASP.NET Core app failed to start`.** |

**The database is the only failing check, and it fails on both hosts that start, which means hosted sign-in does not work.** Sign-in is a call to the API, and the API cannot read the `Users` table, so a correct password still ends as a failure — `DEPLOYMENT.md` §7 records the HTTP 500 the web tier returns and why. Everything that does not touch SQL (the landing pages, the privacy notice, the sign-in form itself, `/health`) works as designed. **The whole system, with its data, runs locally** — see [Start both tiers with one command](#start-both-tiers-with-one-command).

The two causes are different and both are already known:

- **Monster ASP** reaches the SQL Server that came with the hosting plan. That server accepts connections from the host's own private network only, and its `sa`-equivalent credential has been rotated once since the connection string was written, so the deployed string and the server no longer agree. `DEPLOYMENT.md` §9 has the rotation runbook.
- **Azure** reaches `verichat-server-2026` / `VerichatDB` with an Entra token minted by the App Service's managed identity. The database exists and the token is valid, but that identity has never been added as a user inside the database, so the server has nothing to authorise it against. `Database/azure-managed-identity-user.sql` is the script that creates that user; it needs to be run by someone who can connect to the server as its Entra administrator.

`18456` (login rejected) and `4060` (cannot open this database) are different failures with different fixes, and the payload quoted above cannot tell them apart because both hosts still run the build that answers `CanConnectAsync` with one sentence for all of them. `HealthController` now opens the connection itself and classifies the SQL error number, so the next deployed build names which of the two each host needs. Push to `dev2` deploys the Azure copy automatically (see [GitHub Actions](#github-actions)).

`api-site.runasp.net` and `verichat-crm-api.runasp.net` are two separate sites, not two names for one: they now answer differently, which they could not do if they shared a process. The second has no application settings at all — it is the deployment the publish profile was generated for and never completed — and `DEPLOYMENT.md` §7.1 is the runbook for finishing it.

Only the Azure host serves HTTPS; an `https://` request to any `*.runasp.net` name fails at the TLS layer, because no certificate is bound there. `VerichatCRM.Web/appsettings.json` and the mobile `API_BASE_URL` fallback therefore carry the `http://api-site.runasp.net` origin (`VerichatMobile/.env` is git-ignored, so it matches but is not part of the repository), and a signed mobile release build needs the Azure origin instead, because Android and iOS block cleartext traffic — that repoint is the remaining step in `DEPLOYMENT.md` §7.3.

One consequence of the hosted state is worth checking deliberately: the deployed sign-in page contains no **Demo: quick login** panel and no `.test.local` address in its markup, verified against `http://verichat-crm-web.runasp.net/Account/Login` on the same date. The development accounts, the one-click buttons, and the list they read are gated to `IsDevelopment()` / `__DEV__` at three separate layers, so a deployment has neither the panel nor the accounts.


## Architecture

```mermaid
flowchart LR
    Staff[Staff browser] --> Web[ASP.NET Core MVC and Razor]
    Client[Expo mobile app<br/>iOS Android Web] --> API[ASP.NET Core Web API]
    Web -->|JSON over HTTP client| API
    API --> Services[Application services]
    Services --> Data[EF Core and repositories]
    Data --> SQL[(SQL Server)]
```

| Project | Responsibility |
| --- | --- |
| `VerichatCRM.Web` | Razor MVC staff UI, role-based navigation, API client, and session-backed web sign-in. |
| `VerichatCRM.API` | JWT authentication, authorization, REST controllers, request validation, application services, and health endpoint. |
| `VerichatCRM.Core` | Domain entities, request/response models, role and status constants, and repository contracts. |
| `VerichatCRM.Infrastructure` | EF Core `AppDbContext`, SQL Server mappings and migrations, repositories, and startup catalogue seeding. |
| `VerichatMobile` | Expo Router application written in TypeScript and React Native, with shared iOS, Android, and web screens. |
| `VerichatCRM.Tests` | xUnit tests for authentication, role-owned data, order validation and stock, and dashboard analytics. |

The API uses .NET 10, ASP.NET Core controllers, EF Core, and SQL Server. The mobile project uses Expo SDK 57 and React Native 0.86. The Task 1 proposal's Firebase Authentication, Firestore, Firebase Hosting, and Cloud Functions are not used by this implementation.

## Implemented workflows

- Staff authenticate through the API; the web app stores the JWT in its server-side session. Mobile stores its token in SecureStore on native platforms and browser local storage on web.
- Role permissions are checked in the API and the web application. Self-registration cannot create an administrator. Client mobile registration creates a corresponding CRM directory record.
- Staff dashboards and analytics query current clients, inquiries, orders, tickets, products, assets, and deliveries. Order totals are labelled as order value; the database does not contain payment or recognized-revenue records.
- The mobile catalogue loads products from the API. Checkout sends a multi-line order; the server uses database prices, validates quantities and stock, updates stock, and associates the order with the signed-in client.
- Staff can issue quotations through the existing sales workflow. The client can view its own issued quotation and line items in the mobile app; staff can view the client's mobile orders and quotations from the web client profile.
- Ticket, asset, delivery, inquiry, pipeline, purchase-order, and warehouse-alert API areas are present. A warranty-expiry query can find assets within a requested number of days.

## Repository map

```text
VerichatCRM.API/Controllers/       HTTP endpoints and access policies
VerichatCRM.API/Services/          Business operations
VerichatCRM.API/DTOs/              API request and response contracts
VerichatCRM.API/Validators/        FluentValidation rules for request bodies
VerichatCRM.API/Middleware/        Request body sanitisation
VerichatCRM.API/Hubs/              SignalR hub for live staff notifications
VerichatCRM.Core/Entities/         Persisted domain entities, and the role constants
VerichatCRM.Core/Enums/            Asset, delivery and ticket status vocabularies
VerichatCRM.Core/Interfaces/       Repository and service contracts
VerichatCRM.Core/Models/           Per-area view models, and the shared demo account list
VerichatCRM.Infrastructure/Data/   EF Core context, migrations, repositories
VerichatCRM.Web/Controllers/       Staff MVC actions
VerichatCRM.Web/Views/             Razor screens
VerichatCRM.Web/wwwroot/css/       The design tokens every stylesheet reads
VerichatCRM.Web/wwwroot/pages/     The public landing pages' stylesheets
VerichatMobile/src/app/            Expo Router screens
VerichatMobile/src/constants/      API clients, mobile state, the mirrored demo list
VerichatCRM.Tests/                 xUnit automated tests
Database/                          Hosted migration and schema scripts, applied by hand
.github/workflows/                 CI, plus the two Azure publish workflows
start-local.ps1                    Starts both tiers in the right order, and stops them
```

The six documents the map does not list - `documentation.md`, `userguide.md`, `DEPLOYMENT.md`, `TRAINING-PLAN.md`, `DESIGN-SYSTEM.md` and `VerichatCRM_FeatureWalkthrough.md` - are described in [Documentation in this repository](#documentation-in-this-repository) above, and the scripts in `Database/` are the ones [Database and schema](#database-and-schema) explains.

### Projects and dependencies

```text
VerichatCRM.Web ──HTTP/JSON──> VerichatCRM.API ──> VerichatCRM.Core
                                      │                    ▲
                                      └──> VerichatCRM.Infrastructure
                                                │
                                                └── SQL Server

VerichatMobile ──HTTP/JSON──────────> VerichatCRM.API
VerichatCRM.Tests ──tests───────────> API/Core/Infrastructure behavior
```

`Core` contains the domain and contracts; `Infrastructure` implements persistence using EF Core; `API` exposes business operations and authentication; `Web` is the staff-facing MVC application; `Mobile` is the Expo client application. The solution file is `VerichatCRM.slnx`.

## Local development

### Prerequisites

- .NET 10 SDK
- SQL Server; on Windows, the checked-in Development connection string uses LocalDB
- Node.js 22 and npm
- A supported Expo device, emulator, or browser

### Clone the repository

```powershell
git clone https://github.com/dBright-dev/VerichatCRM.WIL.git
Set-Location VerichatCRM.WIL
```

### Configure secrets

The JWT signing key is intentionally blank in checked-in settings; supply it outside source control. The production database connection string is too, and it does not need to be set locally at all — `VerichatCRM.API/appsettings.Development.json` carries a LocalDB string that works on its own. Set only the key:

```powershell
dotnet user-secrets init --project VerichatCRM.API
dotnet user-secrets set "Jwt:Key" "<a newly generated random signing key of at least 32 bytes>" --project VerichatCRM.API
```

> If you have ever run `dotnet user-secrets set "ConnectionStrings:DefaultConnection" ...` for a hosted rehearsal, that value outranks the Development file. It no longer breaks the two `dotnet run` commands below — both API launch profiles pin the LocalDB string as an environment variable, which outranks user secrets — but it does silently redirect every `dotnet ef` command, because the EF tools apply only their own environment default and not the profile's variables. The API then spends a minute or more trying to reach that server before it binds a port (`Program.cs:266` runs the migration at startup), and sign-in reports *"The request failed (HTTP 500)"*. Remove it (`dotnet user-secrets remove "ConnectionStrings:DefaultConnection" --project VerichatCRM.API`) unless you are deliberately pointing a local run at another server. `DEPLOYMENT.md`'s Secrets inventory and section 9 explain the precedence.

For a hosted API, set `Jwt__Key`, `ConnectionStrings__DefaultConnection`, and `Cors__AllowedOrigins__0` in the hosting provider's secret/configuration store. The CORS origin must be the exact HTTPS origin serving a browser client; do not add a trailing path. Outside Development the API also refuses to start when the connection string says `Encrypt=False`. The web app's API target is configured by `ApiBaseUrl`. The mobile app accepts `EXPO_PUBLIC_API_URL` and otherwise uses the configured Verichat API host.

> A database password and two JWT signing keys were previously committed across several revisions of `VerichatCRM.API/appsettings.json`. Removing them from current files does not remove them from Git history. Rotate the database credential, replace the signing key in every running environment, and invalidate tokens signed with the old key before deployment. `DEPLOYMENT.md` is the runbook for that, including the history-scrub commands (which are destructive and deliberately not run).

EF Core migration-script generation during publish uses `DesignTimeAppDbContextFactory` and does not start the API host, so publish can build its database script without a JWT secret. The running API still requires both the JWT key and a real database connection string from its environment or secret store.

### Start both tiers with one command

```powershell
.\start-local.ps1
```

This is the recommended way to run the project locally. It starts the API, waits until `/api/health` actually reports `Healthy`, and only then starts the web app — so the ordering mistake that produces `No connection could be made because the target machine actively refused it. (localhost:5184)` cannot happen. Both tiers write to `logs\api.log` and `logs\web.log` (gitignored), and if the API fails to come up the script prints the last lines of its own output instead of leaving you to find a window. Ctrl+C in that terminal stops both tiers.

Add `-Lan` to also bind the API to `0.0.0.0:5184` so a phone or emulator on your network can reach it. That exposes the development API to your local network; use it only while testing, on a network you trust.

Add `-Stop` from any other terminal to shut both tiers down. Use it when a previous run was killed hard enough to skip its own cleanup and an orphaned app host is still holding `:5184` — the next start then fails to bind a port, which looks exactly like a broken database or credential and is neither.

The two sections below describe the same two processes started by hand, which is still the right thing to do in Visual Studio or when you want the tiers in separate terminals.

### Start the API

In PowerShell window 1:

```powershell
dotnet run --project VerichatCRM.API --launch-profile https
```

The Development HTTPS URL is `https://localhost:7076`. On startup the API applies EF Core migrations, seeds a small sample client/product catalogue when those tables are empty, creates the first administrator only when bootstrap settings are supplied, and — on a Development host that asks for it — the per-role sign-in accounts described below.

### Start the staff web application

In PowerShell window 2:

```powershell
dotnet run --project VerichatCRM.Web --launch-profile https
```

The Development HTTPS URL is `https://localhost:7266`. No `$env:ApiBaseUrl` is needed: `VerichatCRM.Web/appsettings.Development.json` already points the web tier at `http://localhost:5184`, which every API launch profile binds, and it blanks `BackupApiBaseUrl` so a local run cannot reach the deployed API by accident.

The web app is a proxy, so **it needs the API window running**. If it is not, sign-in fails with `HttpRequestException: No connection could be made because the target machine actively refused it. (localhost:5184)` — that is not a database or credential problem, it is the other project. In Visual Studio, set both `VerichatCRM.API` and `VerichatCRM.Web` as Multiple startup projects before pressing F5.

The first administrator comes from the API bootstrap configuration (`Bootstrap__AdminEmail`, `Bootstrap__AdminPassword`, and `Bootstrap__AdminName`), or from the development sign-in accounts listed below. Self-registration is not a way to obtain a staff login: the public `/Account/Register` form offers **only `Client User` and `Medical Student`** — exactly `UserRoles.SelfRegisterable` — and any other role in the posted form is refused before a row is written. Every staff account, System Administrator included, is created by an authenticated administrator on `/Admin/Users`. A client or student account is live the moment it is created and signs in immediately; there is no review step, and `Access` on the users screen is the only thing that can turn it off again.

### Start the mobile application

In PowerShell window 3:

```powershell
cd VerichatMobile
npm ci
$env:EXPO_PUBLIC_API_URL = "https://localhost:7076"
npx expo start
```

Physical devices need an API URL they can reach and a trusted HTTPS certificate. A device cannot use its own `localhost` to reach the development computer.

### Sign in as any role

A fresh development database is given one account per role, so every screen in the system can be opened without typing seven accounts into the users screen by hand. `DemoUserSeeder` writes them at API startup, and only while **both** conditions hold: the host is running in the Development environment, and `Bootstrap:DemoUsers` reads `true` — set in `VerichatCRM.API/appsettings.Development.json`. Turn either off and no such account is created on that machine; a deployment would have to be handed the Development settings file *and* `ASPNETCORE_ENVIRONMENT=Development` before a single row appeared.

Sign in at `https://localhost:7266/Account/Login`, or point the mobile app at the API. Each role is dropped on its own board:

| Role | Email | Password | Opens |
| --- | --- | --- | --- |
| System Administrator | `admin@verichat.test.local` | `Demo!Admin2026` | `/Admin/Dashboard` |
| Sales Representative | `sales@verichat.test.local` | `Demo!Sales2026` | `/Pipeline` |
| Procurement Manager | `procurement@verichat.test.local` | `Demo!Stock2026` | `/Procurement/Dashboard` |
| Field Support Engineer | `engineer@verichat.test.local` | `Demo!Fix2026` | `/Support/Dashboard` |
| Logistics | `logistics@verichat.test.local` | `Demo!Move2026` | `/Logistics/Dashboard` |
| Client User | `client@verichat.test.local` | `Demo!Client2026` | `/Portal` |
| Medical Student | `student@verichat.test.local` | `Demo!Study2026` | `/Portal` |

The two client roles share `/Portal` because they share what they are allowed to see; the mobile app gives each of them its own screens.

Typing those is the slow way to use them. On a Development machine `/Account/Login` shows a **Demo: quick login** panel under the sign-in form with one button per role, and the mobile app's two sign-in screens show the chips for the audience they can serve — five staff roles on Staff sign-in, Client User and Medical Student on Client sign-in. A button posts a short code, not a credential: the controller looks the account up in `DemoAccounts` (the same list `DemoUserSeeder` writes, so a button cannot name an address that does not exist), signs in through the ordinary `/api/auth/login` call, and the role's own board opens. The password never reaches the page, and both clients hide the panel outside Development — a deployed site renders neither the buttons nor the accounts.

**These are development credentials and nothing more.** They are not passwords for the hosted system and cannot be: the hosted API does not run in Development, so the accounts are never created there, and `Bootstrap__AdminEmail` / `Bootstrap__AdminPassword` remain the only way an administrator is born. The `.test.local` domain is deliberately not routable — email-verification links and password resets addressed to these accounts can never reach a real mailbox.

The `Client User` and `Medical Student` accounts also receive the CRM client record that orders, quotations and the enquiry queue are keyed on. That is the same call `AuthService.RegisterAsync` now makes for anybody who signs up through the public form, and without it the portal and the mobile app would open on an empty list rather than a usable account.

The administrator account is how a staff login is created: `/Admin/Users` → **+ Add user** (`/Admin/CreateUser`) → choose one of the five staff roles. There is no queue to approve anything in, because the sign-up form cannot ask for a staff role — an unapproved self-registration is no longer a state the system can be in.

## Database and schema

The source of truth is SQL Server, not Firestore. EF Core entities and mappings live in Core and Infrastructure; migrations are in `VerichatCRM.Infrastructure/Migrations`. API startup applies migrations. Do not use production data when demonstrating or testing locally.

The startup seeder adds example clients and products only when the relevant tables are empty, and stops there — tickets, quotations and anything the business actually records are entered by a user. It does not create ordinary user accounts on a deployment; the development sign-in accounts above are a separate, doubly guarded step. Configure the bootstrap administrator credentials as secrets and clear them after initial provisioning.

## Tests and code checks

Run from the repository root:

```powershell
dotnet restore VerichatCRM.slnx
dotnet test VerichatCRM.Tests/VerichatCRM.Tests.csproj --configuration Release
dotnet build VerichatCRM.slnx --configuration Release --no-restore
```

Run mobile checks from `VerichatMobile`:

```powershell
npm ci
npx expo lint
npx tsc --noEmit
```

The current xUnit suite covers 71 authentication, ownership, order, stock, permission-matrix, inquiry-scoring, role-dashboard, and EF design-time context cases across seven test files. It is not a complete integration or end-to-end suite: there are no API contract tests, and the Expo client has no test runner configured (`VerichatMobile/package.json` defines no `test` script), so mobile behaviour is verified by `expo lint` and `tsc` only.

## GitHub Actions

`.github/workflows/ci.yml` restores, tests, and builds the .NET solution and runs Expo lint and TypeScript checks on pull requests and pushes to `main` or `master`. This workflow is CI only: it does not deploy the API, web app, or database. GitHub branch protection, reviewer requirements, deployment environments, and production secrets must be configured in the repository settings separately.


