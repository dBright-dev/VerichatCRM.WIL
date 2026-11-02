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

The two hosts are not failing for the same reason, and the reasons are documented separately — `DEPLOYMENT.md` §7.1 and §7.2:

- **Monster ASP** and its SQL Server sit in the same hosting family: the published server name `db70543.databaseasp.net` resolves to the private address `10.0.0.23`, which only a machine already inside that network can route to. An API colocated there ought to connect, and `DEPLOYMENT.md` §7.1 records it doing exactly that on 2 November — the database answering in single-digit milliseconds, with pending migrations reported rather than a connection failure. **Today the same host reports that no connection opens at all.** That difference is the finding: something changed between those two probes — the credential the panel holds, the SQL server's state, or the settings on the published build — and it is not something the repository can see. `DEPLOYMENT.md` §1 is the credential-rotation runbook, and the classified health check names which one it is once that build is deployed.
- **Azure** is a different problem and a known one. Its deployed connection string must name a server that is reachable from Azure; the Monster SQL server never is, by construction (§7.2), which is why `verichat-server-2026` / `VerichatDB` was created. The remaining gap is that the App Service's managed identity has never been added as a user inside that database, so the server has nothing to authorise the token against. `Database/azure-managed-identity-user.sql` is the script that creates the user, and it has to be run by the server's Entra administrator — it cannot be run by the identity it creates. The shape of the failure supports that reading: this host used to take 13.9 seconds to report the same problem, which is what a connect timeout looks like, and now takes 1.5 — a refusal this fast was answered by a server, not swallowed by a network. `DEPLOYMENT.md` §7.4 records the measurement and what would falsify it.

`18456` (login rejected) and `4060` (cannot open this database) are different failures with different fixes, and the payload quoted above cannot tell them apart because both hosts still run the build that answers `CanConnectAsync` with one sentence for all of them. `HealthController` now opens the connection itself and classifies the SQL error number, so the next deployed build names which of the two each host needs; `dev2` is the branch the Azure publish workflows watch (see [GitHub Actions](#github-actions)).

`api-site.runasp.net` and `verichat-crm-api.runasp.net` are two separate sites, not two names for one: they now answer differently, which they could not do if they shared a process. The second has no application settings at all — it is the deployment the publish profile was generated for and never completed — and `DEPLOYMENT.md` §7.1 is the runbook for finishing it.

Only the Azure host serves HTTPS; an `https://` request to any `*.runasp.net` name fails at the TLS layer, because no certificate is bound there. `VerichatCRM.Web/appsettings.json` and the mobile `API_BASE_URL` fallback therefore carry the `http://api-site.runasp.net` origin (`VerichatMobile/.env` is git-ignored, so it matches but is not part of the repository), and a signed mobile release build needs the Azure origin instead, because Android and iOS block cleartext traffic — that repoint is the remaining step in `DEPLOYMENT.md` §7.3.

One hosted detail changes with this build, so it is worth stating on both sides of the publish. The deployed sign-in page checked on 2026-11-02 carried no **Demo: quick login** panel and no `.test.local` address in its markup (`http://verichat-crm-web.runasp.net/Account/Login`) — the gate then was `IsDevelopment()` / `__DEV__`, and a published host is not Development. That gate is now the same `Bootstrap:DemoUsers` key the API seeds from, and it ships `true`, so publishing this build puts seven buttons on the deployed page and, once the deployed API can reach its database, seven accounts behind them. See [Sign in as any role](#sign-in-as-any-role) for what that exposes and how to switch it off.


## UI/UX Web Application


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

- Staff authenticate through the API; the web app stores the JWT in its server-side session. Mobile stores its token in SecureStore on native platforms and browser local storage on the web.
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

The Development HTTPS URL is `https://localhost:7076`. On startup the API applies EF Core migrations, seeds a small sample client/product catalogue when those tables are empty, creates the first administrator only when bootstrap settings are supplied, and — wherever `Bootstrap:DemoUsers` reads true — the per-role sign-in accounts described below.

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

The system is given one account per role, so every screen can be opened without typing seven accounts into the users screen by hand. `DemoUserSeeder` writes them at API startup, and **one setting** decides: `Bootstrap:DemoUsers` has to read `true`. It used to demand the Development environment as well, which meant the shortcut could never exist on the deployed site a marker actually visits — a published host is not in Development. The Development requirement is gone, and the key now ships `true` in `VerichatCRM.API/appsettings.json` (as well as `appsettings.Development.json`), so publishing carries the accounts with it. Any host that must not have them sets `Bootstrap__DemoUsers=false` in its own app settings, which outranks the file; `Bootstrap__AdminEmail` / `Bootstrap__AdminPassword` remain the only way a *real* administrator is born.

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

Typing those is the slow way to use them. `/Account/Login` shows a **Demo: quick login** panel under the sign-in form with one button per role, and the mobile app's two sign-in screens show the chips for the audience they can serve — five staff roles on Staff sign-in, Client User and Medical Student on Client sign-in. A button posts a short code, not a credential: the controller looks the account up in `DemoAccounts` (the same list `DemoUserSeeder` writes, so a button cannot name an address that does not exist), signs in through the ordinary `/api/auth/login` call, and the role's own board opens. The password never reaches the page.

Both sides of that are gated by the one setting, deliberately the same key name on the web tier as on the API so the two cannot drift into buttons with nothing behind them. `VerichatCRM.Web.Services.DemoAccess` answers yes in Development or whenever `Bootstrap:DemoUsers` reads `true`, and the panel is rendered on the published site because that is where the work is demonstrated. The chips in the native app follow a build flag instead — `__DEV__`, or `EXPO_PUBLIC_DEMO_LOGIN=true`, which `eas.json` sets in the `preview` and `production` profiles; a bundle built without it has no way to turn them on afterwards, because that value is inlined at build time.

**What this exposes, stated plainly.** These seven passwords are in this file on purpose — a credential nobody can reproduce is a locked door, not a test account — and because the setting is on in the checked-in settings, anyone who can reach the deployed sign-in page can open any role in it, including System Administrator. That is the accepted cost of demonstrating a role-separated system to a marker over a URL rather than over a laptop. It is only safe while the databases behind it hold demonstration rows, which is what they hold; a deployment that ever carries real client data sets `Bootstrap__DemoUsers=false` and deletes these seven users from the `Users` table.

One caveat about the published site today: the panel renders there, but the two deployed APIs cannot reach their database (see [Hosted applications](#hosted-applications)), so a chip on the deployed page is refused the same way a mistyped password would be. The buttons are waiting on the hosted migration, not on their own gate. The `.test.local` domain is deliberately not routable — email-verification links and password resets addressed to these accounts can never reach a real mailbox.

The `Client User` and `Medical Student` accounts also receive the CRM client record that orders, quotations and the enquiry queue are keyed on. That is the same call `AuthService.RegisterAsync` now makes for anybody who signs up through the public form, and without it the portal and the mobile app would open on an empty list rather than a usable account.

The administrator account is how a staff login is created: `/Admin/Users` → **+ Add user** (`/Admin/CreateUser`) → choose one of the five staff roles. There is no queue to approve anything in, because the sign-up form cannot ask for a staff role — an unapproved self-registration is no longer a state the system can be in.

## Database and schema

The source of truth is SQL Server, not Firestore. EF Core entities and mappings live in Core and Infrastructure; the migration chain is in `VerichatCRM.Infrastructure/Migrations` and runs to 22 migrations. On a machine that can reach its database, API startup applies that chain before it binds a port, which is why a local clone needs nothing but LocalDB and a JWT key. Do not use production data when demonstrating or testing locally.

**The hosted database is not migrated, and it is a separate step.** Because both deployed API copies fail on the connection itself (see [Hosted applications](#hosted-applications)), the startup migration never runs there — the hosted schema is whatever was last applied by hand, and nothing in the code can tell you which that was. The scripts in `Database/` are how it is brought forward, in this order:

| Script | What it does |
| --- | --- |
| `hosted-migrations/verify-migration-state.sql` | Read-only. Prints the migrations the target database has recorded and the tables it has, so you know what is pending before you run anything. |
| `preflight-migration-checks.sql` | Read-only. Whether the columns that `SyncSchemaAndAccountLifecycle` and its successors widen currently hold data that would not fit, and whether any e-mail, tracking number, quote reference or serial is duplicated where a unique index is about to be created. |
| `apply-pending-migrations.sql` | Applies the whole chain idempotently. Every statement is wrapped in `IF NOT EXISTS (… [MigrationId] …)`, so it is safe to run twice and it skips what is already there. Its opening `SET QUOTED_IDENTIFIER ON` is not decoration: without it the batch stops at migration 19 of 22 with a `CREATE INDEX` failure and the earlier transactions already committed. Regenerate it with the command in its header rather than hand-editing it. |
| `hosted-migrations/16-…sql` … `22-…sql` | The same chain one migration at a time, for a database that should be moved step by step and checked between steps. |
| `azure-sql-full-schema.sql` | The entire schema as one script, for a database with nothing in it yet. |
| `seed-catalogue.sql` | The public product catalogue, applied after the schema exists. |
| `azure-managed-identity-user.sql` | Not a schema change: creates the database user for the Azure App Service's managed identity. It must be run by the server's Entra administrator, because it cannot be run by the identity it creates. |

`apply-pending-migrations.sql` was executed twice against the same database to prove the second run is a no-op, and the result was 22 migrations and 39 tables with no errors.

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

The xUnit suite is **140 test cases in ten test files**, all passing (measured on 2 Nov 2026: `Failed: 0, Passed: 140, Skipped: 0` in 8 seconds). It covers authentication and PBKDF2 password migration, account recovery and the login-help path, the permission matrix for all seven roles, client ownership on the controller boundary, order validation and stock movement, inquiry scoring, badge counting, dashboard analytics, and the EF design-time context that lets migrations be scripted without starting the host. Three further files in that project are fakes (`FakeEmailService`, `FakeMobileService`, `FakeUserRepository`), not tests.

It is not a complete integration or end-to-end suite: there are no API contract tests, no test exercises the Razor views or the web tier's proxy, and the Expo client has no test runner configured (`VerichatMobile/package.json` defines no `test` script), so mobile behaviour is verified by `expo lint` and `tsc` only. The screens a reviewer actually judges - the role dashboards, the portal, the quick-login panel - are checked by running them, not by this suite, and that difference should be read as a limitation rather than as coverage.

## GitHub Actions

Three workflows are checked in, and only one of them is CI.

`.github/workflows/ci.yml` runs on pull requests and on pushes to `main` or `master`, in two jobs: `dotnet` restores `VerichatCRM.slnx`, runs the test project and builds the solution in Release; `mobile` runs `npm ci`, `npx expo lint` and `npx tsc --noEmit` in `VerichatMobile`. It does not deploy anything.

The other two are Azure's own generated publish workflows, and they **run on every push to `dev2`**:

| Workflow | Deploys | To |
| --- | --- | --- |
| `dev2_verichat-api.yml` | `dotnet publish` of the repository | Azure Web App `verichat-api` (Production slot) |
| `dev2_verichatcrm.yml` | `dotnet publish` of the repository | Azure Web App `VerichatCRM` (Production slot) |

`dev2` is therefore not a quiet integration branch: pushing it queues a rebuild and a republish of both Azure apps without anyone pressing a button, so a push that breaks the build is also a deployment attempt. Whether the deployment then succeeds is the question below it.

Neither publish workflow names a project. Both run bare `dotnet build --configuration Release` and `dotnet publish -c Release` at the repository root, where the file is `VerichatCRM.slnx` — a solution with six projects and no designated startup project — so each workflow publishes whatever the root resolves to rather than the one assembly that app needs, and both apps receive the same artifact. The fix is `dotnet publish VerichatCRM.API` in the API workflow and `dotnet publish VerichatCRM.Web` in the web one, and it has not been applied.

Whether those pushes actually republished either Azure app could not be confirmed from the repository. Nine minutes after the health-check change landed on `dev2`, the Azure endpoint was still answering with the sentence the previous build produces; the run log, which needs repository settings access to read, is the only thing that distinguishes a job that failed from a job that is still in the queue.

GitHub branch protection, reviewer requirements, deployment environments and production secrets are configured in the repository and portal settings, not here; no workflow holds a credential, since the Azure jobs authenticate with the GitHub connection's service principal.



