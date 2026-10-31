# Deployment, secrets and credential rotation

Companion to `README.md`. The README tells a developer how to run the code; this file tells whoever operates the deployed site **where each secret must live**, **what to do about the credentials that were committed to git**, and **what has to be true before the live notification feed works**.

Nothing in this file was executed against the hosting provider. The steps that touch `databaseasp.net`, the Monster ASP control panel, or the shared git remote are deliberately left for an operator with access, because they cannot be done from a working copy and two of them are destructive.

## What runs where

| Piece | Deployed target | Notes |
| --- | --- | --- |
| Staff web app (`VerichatCRM.Web`) | `verichat-crm-web.runasp.net` — Monster ASP shared hosting, publish profile `site94515-WebDeploy.pubxml` | Razor MVC. Talks to the API server-side and passes the API base URL to page script. |
| API (`VerichatCRM.API`) | `verichat-crm-api.runasp.net` — Monster ASP shared hosting, publish profiles `site94518-WebDeploy{,1,2}.pubxml` | REST + SignalR hub at `/hubs/notifications`. |
| Database | `databaseasp.net` (Monster ASP shared SQL Server) | Reached only by the API. The web app holds no connection string. |
| Mobile client (`VerichatMobile`) | EAS builds, installed on devices | Reads `EXPO_PUBLIC_API_URL` at bundle time. |

Publish profiles (`*.pubxml`) are git-ignored and none of them carries a `Password` element, so nothing new leaks there.

## Secrets inventory

| Config key | Read by | Correct home today | Cost of rotating it |
| --- | --- | --- | --- |
| `ConnectionStrings:DefaultConnection` | `VerichatCRM.API/Program.cs:25` | Host secret store / `ConnectionStrings__DefaultConnection`. **Never** in `appsettings*.json` outside Development. | API restart. Sessions are unaffected. |
| `Jwt:Key` | `Program.cs:104` | `Jwt__Key`. The app throws at startup when it is blank. | Every issued token becomes invalid — all staff and all mobile users are signed out. |
| `Bootstrap:AdminEmail` / `AdminPassword` / `AdminName` | `AdminBootstrapper` | `Bootstrap__*` env vars, **deleted after the first sign-in**. Only consulted while the user table is empty; leaving them set is a standing admin-account door. | None once accounts exist. |
| `Cors:AllowedOrigins` | `Program.cs:163` | `appsettings.json` (deployed origins are not secret) plus `Cors__AllowedOrigins__2` for extra environments. | Browsers stop being able to call the API. |
| `EXPO_PUBLIC_API_URL` | `VerichatMobile/src/constants/api.ts` | `VerichatMobile/.env` (ignored) or the EAS build environment. | Needs a new app build; it is inlined into the bundle. |

Local development uses the .NET secret manager rather than environment variables. The API project now carries a `UserSecretsId` (`VerichatCRM.API/VerichatCRM.API.csproj`), so this works out of the box:

```powershell
dotnet user-secrets set "Jwt:Key" "<64-char random string>" --project VerichatCRM.API
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\\MSSQLLocalDB;Database=VerichatCRM;Trusted_Connection=True;" --project VerichatCRM.API
```

User secrets are loaded only when `ASPNETCORE_ENVIRONMENT=Development`. They do nothing in production.

## 1. Rotate the database credential

This is the only step that actually fixes the exposure. Removing a password from files does not un-expose it — the value sat in `VerichatCRM.API/appsettings.json` across seven commits from `a53523c` to `c3f3fb5` (the last of them on 2026-10-26, on `dev2` and `section1---mutsa`, which are both pushed). Anyone with read access to the public repository, a fork, a clone, or GitHub's cached views can recover it with `git log -p`.

1. Sign in to the Monster ASP control panel and open its SQL Server manager for the `db70543` database. Change the login password there. If the panel does not expose the option, ask support to reset it, or run `ALTER LOGIN ... WITH PASSWORD = '<new>'` in the panel's query tool if it grants one. **This cannot be done from a working copy — there is no SQL Server reachable from this repository.**
2. Confirm nothing else authenticates with that same login. The `db70543` account is scoped to that one database, so no other application should be using it; verify before assuming.
3. Put the new value into the host's configuration store (see §3) with `Encrypt=True` (see §4).
4. Restart the API site, then check `http://verichat-crm-api.runasp.net/api/health`, which reports database readiness.

Old tokens stay valid until §2 is done; the password rotation alone does not sign anyone out.

## 2. Replace the JWT signing key

Two different signing keys were committed: a readable placeholder in `a53523c` and a generated 64-byte base64 key in `c751e3a` → `36b1f29`. Both must be treated as public. Anyone holding either can mint tokens for any role, including `System Administrator`, and the API will accept them.

1. Generate a fresh key: `[Convert]::ToBase64String((1..64 | ForEach-Object { Get-Random -Max 256 } | ForEach-Object { [byte]$_ }))`.
2. Set `Jwt__Key` on the API host and restart the site.
3. Expect every session to break. Tell staff to sign in again; the stale `JwtToken` cookie and the stale session value are overwritten on the next successful login (`VerichatCRM.Web/Controllers/AccountController.cs`).

## 3. Giving the API its configuration on shared hosting

ASP.NET Core reads configuration from environment variables and from `appsettings.Production.json`, in that order of precedence (env vars win). Which route Monster ASP's panel supports has to be confirmed once in the control panel — nothing in the repository tells us.

- **Preferred — environment variables.** Set `ConnectionStrings__DefaultConnection`, `Jwt__Key`, and `Cors__AllowedOrigins__0` in the panel's application-settings area, or as `<environmentVariables>` in `web.config` if the site runs out-of-process. Keep `web.config` out of git if it ever carries a value.
- **Fallback — `appsettings.Production.json` on the server.** Upload it by FTP and leave it out of git; `.gitignore` now lists `appsettings.Production.json`, so an accidental `git add -A` can no longer stage it. On shared hosting you cannot set NTFS ACLs on that file, so treat it as readable by anyone with the hosting control-panel login and prefer the env-var route when it is available.
- **Not available — user secrets.** Development-only by design.

A tracked template is the way to document the shape without the values: copy `appsettings.json`, add the real key names with empty values, and commit it as `appsettings.Production.example.json`. The ignore rule targets `appsettings.Production.json` only, so the template is not swallowed by it.

## 4. Encryption

`Encrypt=False` was committed with the password, so every production connection ran the TDS session unencrypted — including the login handshake. The API now refuses to start outside Development when the connection string asks for `Encrypt=False` or `Encrypt=no`:

```
InvalidOperationException: ConnectionStrings:DefaultConnection sets Encrypt=False.
Remove that key, or set Encrypt=True; TrustServerCertificate=True. See DEPLOYMENT.md.
```

Recommended shape:

```
Server=<sql-host>; Database=<db>; User Id=<login>; Password=<rotated>; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;
```

`TrustServerCertificate=True` keeps a shared host's certificate — issued for the `databaseasp.net` machine, not for your alias — from failing the handshake, but it means you are encrypting without authenticating the endpoint. Remove it once a certificate the web-server machine trusts is bound to the SQL endpoint. Omitting `Encrypt` entirely is safe: `Microsoft.Data.SqlClient` encrypts by default, which is why the guard rejects only an explicit `False`.

## 5. TLS is a prerequisite, not a nice-to-have

Two things now depend on the API being reachable over HTTPS with a valid certificate:

- **Live notifications.** The JWT is no longer written into page script. The browser authenticates the cross-origin SignalR handshake with the `JwtToken` cookie, which is set as `HttpOnly; Secure; SameSite=None` whenever the web app is served over HTTPS. Browsers refuse to *store* a `SameSite=None` cookie that is not `Secure`, and refuse to *send* a `Secure` cookie over `http://`. So if either site is served over cleartext, the hub cannot authenticate and the toast feed goes quiet — that is a deployment gap, not a code bug.
- **Mobile release builds.** Android and iOS block cleartext traffic, and `app.UseHttpsRedirection()` in `Program.cs` already sends `http://` API calls to a `https://` endpoint whose handshake currently fails (recorded in `VerichatMobile/.env`). Today's installs only work as development/preview builds.

Until both sites have certificates bound, the practical result is: log in, use every normal feature, and see no live notifications. Bind the certificate, then set `ApiBaseUrl` and `BaseUrl` in `VerichatCRM.Web/appsettings.json` to the `https://` API origin and switch `EXPO_PUBLIC_API_URL` to `https://`.

## 6. Scrub the git history

**Destructive and not executed.** `dev2`, `main`, `develop`, `api`, `web`, `mobile`, `mutsa`, `section1---mutsa`, `section2--rai`, `feature-debug` and `feature/product-catalog-ordering` are all on the shared remote, so a rewrite means force-pushing every branch and every contributor must discard their clone. Do §1 and §2 first; a rewrite is hygiene, rotation is the fix.

```powershell
# 0. Tooling: git filter-repo is a separate install, not a git subcommand.
pip install git-filter-repo

# 1. Work from a fresh mirror so nothing local can be lost.
git clone --mirror https://github.com/dBright-dev/VerichatCRM.WIL.git verichat-scrub
Set-Location verichat-scrub

# 2. Build the replacement rules FROM history, outside the repository, so the
#    secret is never written into a tracked file. Two distinct DB passwords and
#    two distinct Jwt:Key values are expected; the rules file will list one line
#    per distinct value.
$vals = git log -p -- VerichatCRM.API/appsettings.json |
    Select-String -Pattern 'Password=([^;]+)', '"Key": "([^"]+)"' -AllMatches |
    ForEach-Object { $_.Matches } | ForEach-Object { $_.Groups[1].Value } |
    Where-Object { $_ -and $_ -notmatch '^\s*$' } | Sort-Object -Unique
$vals | ForEach-Object { "$_,<REDACTED-CREDENTIAL>" } |
    Out-File -Encoding ascii $env:TEMP\verichat-replace.txt

# 3. Rewrite. --invert-paths also drops the large untracked-era leftovers if you
#    choose to purge paths as well as strings.
git filter-repo --replace-text $env:TEMP\verichat-replace.txt

# 4. Verify: every command must now print nothing.
git log --all -S 'databaseasp' --oneline
git grep -I -E '(Password=[^;<]|"Key": *"[^"]{20,})' $(git rev-list --all) -- VerichatCRM.API/appsettings.json

# 5. Publish. Everyone must re-clone, or fetch and reset; a normal pull re-merges
#    the old history and re-introduces the secret.
git push --force --mirror origin
```

Then: delete the rules file, ask GitHub to clear cached views and detached objects for the repository (a support request — a forced rewrite does not drop the old commits immediately, and any fork or release asset made beforehand still holds them), and confirm nobody is carrying an old clone that will push the secret back.

## 7. Confirm after deployment

| Check | Expected |
| --- | --- |
| `GET /api/health` | 200 and database ready |
| API starts with `ConnectionStrings__DefaultConnection` unset | Dies at `Program.cs:30` with the "not configured" `InvalidOperationException`. Run from the console and that message is printed; behind IIS the same crash is what produces HTTP 500.30 — see §7.1 |
| API starts with `Encrypt=False` outside Development | Throws the encryption guard from §4 |
| API starts with `Jwt__Key` unset | Throws "Jwt:Key is missing" |
| `view-source:` on any staff page | No JWT anywhere in the markup; one `<meta name="request-verification-token">` present |
| Sign in, then read cookies | `JwtToken` is `HttpOnly`; `Secure` and `SameSite=None` when the site is HTTPS |
| `GET /api/Delivery/track/<someone else's tracking number>` as a Client User | 403, not the delivery |
| Live toast | Appears when a ticket is assigned; if it does not, re-read §5 before debugging the JavaScript |
| A user who signed in before the PBKDF2 change | `Users.PasswordHash` now starts with `pbkdf2-sha256$` |
| `GET /health` | 200. It returned 500 (`AmbiguousMatchException`) while both `Program.cs` and `HealthController` claimed the route |

### 7.1 The deployed API answers HTTP 500.30

Observed on `http://verichat-crm-api.runasp.net/` (every path, including `/api/health`):

```
HTTP Error 500.30 - ASP.NET Core app failed to start
```

500.30 means the process was started and `Program.cs` threw before the app could
serve a request. It is not an IIS configuration error (that would be 500.19), and
it is not a missing runtime (500.31) — the sibling web site
`verichat-crm-web.runasp.net` is the same .NET target on the same host family and
returns 200, so the stack is installed and healthy.

Reproduced locally against the published output, with no environment variables:

```
Unhandled exception. System.InvalidOperationException:
ConnectionStrings:DefaultConnection is not configured. Set the environment variable
ConnectionStrings__DefaultConnection, or run: dotnet user-secrets set ...
   at Program.<Main>$(String[] args) in .../VerichatCRM.API/Program.cs:line 30
```

That is the whole of it. The credential and the signing key used to be read from
`appsettings.json`; they were removed from git (§1, §2) and the file now ships
both values empty. An app that is deployed without being *configured* cannot
start, and there are exactly three checks in front of the first request, in this
order:

| Order | `Program.cs` | Throws when |
| --- | --- | --- |
| 1 | line 28 | no `ConnectionStrings:DefaultConnection` from any source |
| 2 | line 40 | the string says `Encrypt=False` and the environment is not Development |
| 3 | line 115 | `Jwt:Key` is blank |

So the fix is in the hosting panel, not in the code:

1. Open the API site's application settings and add
   `ConnectionStrings__DefaultConnection` with the value from §4 — rotated
   password, `Encrypt=True; TrustServerCertificate=True`. The string that used to
   sit in `appsettings.json` will not work even if it is re-added verbatim,
   because it ends in `Encrypt=False` and check 2 now refuses it.
2. Add `Jwt__Key` with a fresh 64-character random value (§2). Do not reuse the
   one that was in git: every token signed with it must be treated as forgeable.
3. Restart the site, then `GET /api/health`.

If it still fails, read the log rather than guessing. `VerichatCRM.API/web.config`
turns `stdoutLogEnabled` on, so ANCM writes the startup exception to
`logs\stdout_<date>_<time>.txt` inside the site folder — open it in the host's file
manager. The folder is created by the module; if it is empty after a failed start,
the site is not reading this `web.config` (the deploy is publishing without it, or
the host injected its own).

Two things that are *not* the cause, because both are caught and logged and the app
continues in degraded mode: a migration that fails at startup (§8) and seeding the
starter catalogue. Those make `/api/health` report a problem; they do not produce
500.30.

## 8. Pre-flight checks before migrating an existing database

**One new migration ships in this working copy:** `20261031160000_AddInquiryScoringInputs`. It does three things and nothing else —

- narrows `Inquiries.Status` from `nvarchar(max)` to `nvarchar(30)`, because SQL Server cannot build an index on a `max` column and the queue sorts on `Status` (the values come from `InquiryStatuses`; the longest is nine characters);
- adds the two nullable inputs the automatic enquiry score reads: `Inquiries.Urgency` (`nvarchar(30)`) and `Inquiries.EstimatedValue` (`decimal(18,2)`);
- creates the composite index `IX_Inquiries_Status_Score`.

It was written by hand rather than scaffolded, and the reason is recorded in §12.4 of the technical report: `20261030204733_AddTask3Features` recreates the `SalesGoals` table that `20261025111000_AddSalesGoals` already created, and the model snapshot inherits that contradiction, so `dotnet ef migrations add` produced a migration that rebuilt `SalesGoals` and re-added columns the earlier hand-written migrations had already applied. A migration that fails on the first database it touches is worse than no migration, so only the three changes above were kept.

Pre-flight is short, because nothing is dropped and nothing is made unique:

```sql
-- The only narrowed column, narrowed to 30. Should return no rows.
SELECT InquiryId, Status, DATALENGTH(Status) AS Len FROM Inquiries WHERE DATALENGTH(Status) > 30;

-- Urgency, where present, must be one of the four the scorer recognises.
-- Anything else is not an error - it scores as "not stated" - but it is worth
-- knowing what is in the column before you train anyone on the ranking.
SELECT Urgency, COUNT(*) FROM Inquiries WHERE Urgency IS NOT NULL GROUP BY Urgency;
```

**Existing scores are not recalculated.** The new columns are nullable and no back-fill runs, deliberately: computing a score from data that was never captured would produce a confident-looking number that nobody chose. Enquiries written before this migration keep whatever `Score` their author typed. Re-ranking the queue after upgrading is a human action in the Inquiry form, not a migration step.

### 8.1 The deferred index-and-precision migration

`20261031091951_ApplyEntityConfigurationsAndIndexes` was scaffolded, reviewed and **then deleted from this working copy, because the SalesGoals problem above made it unsafe to ship.** The `IEntityTypeConfiguration` classes it was meant to apply are live — `ApplyConfigurationsFromAssembly` runs at startup — which means **the unique indexes on `Users.Email`, the two tracking numbers, `Assets.SerialNumber` and `Quotations.QuoteReference`, and the `decimal(5,4)` precision on `VatRate` are declared in code but do not exist in the database.** Nothing enforces them until someone adds that migration back.

When you do, run this checklist against the live database first. On an empty or newly created database it applies without complaint; on a database that has been used, any row that violates the new shape aborts the whole migration, and because the API calls `MigrateAsync()` at startup the app then comes up in degraded mode with the schema half-changed.

```sql
-- The unique email index rejects any address held twice.
SELECT Email, COUNT(*) FROM Users GROUP BY Email HAVING COUNT(*) > 1;

-- Narrowing fails where stored text is longer than the new maximum.
SELECT c.ClientId, c.ClientName, DATALENGTH(c.Address) AS AddressLen,
       DATALENGTH(c.ContactEmail) AS EmailLen, DATALENGTH(c.ContactPhone) AS PhoneLen
FROM Clients c
WHERE DATALENGTH(c.Address) > 500 OR DATALENGTH(c.ContactEmail) > 200
   OR DATALENGTH(c.ContactPhone) > 30 OR DATALENGTH(c.ClientName) > 200
   OR DATALENGTH(c.Sector) > 100 OR DATALENGTH(c.ContractStatus) > 50;

SELECT TicketId, DATALENGTH(Title) AS TitleLen, DATALENGTH(Description) AS DescLen,
       DATALENGTH(ResolutionNotes) AS NotesLen, DATALENGTH(TrackingNumber) AS TrackLen
FROM Tickets
WHERE DATALENGTH(Title) > 200 OR DATALENGTH(Description) > 4000
   OR DATALENGTH(ResolutionNotes) > 4000 OR DATALENGTH(TrackingNumber) > 30;

-- VatRate moves from decimal(18,2) to decimal(5,4): only 0.9999 and below fit,
-- so a rate stored as 15 (meaning 15%) rather than 0.15 cannot be converted.
SELECT QuotationId, VatRate FROM Quotations WHERE VatRate >= 10;

-- Every filtered unique index fails on duplicated non-blank values.
SELECT TrackingNumber, COUNT(*) FROM Tickets      WHERE TrackingNumber  <> '' GROUP BY TrackingNumber HAVING COUNT(*) > 1;
SELECT TrackingNumber, COUNT(*) FROM DeliveryLogs WHERE TrackingNumber  <> '' GROUP BY TrackingNumber HAVING COUNT(*) > 1;
SELECT QuoteReference, COUNT(*) FROM Quotations   WHERE QuoteReference  <> '' GROUP BY QuoteReference HAVING COUNT(*) > 1;
SELECT SerialNumber,   COUNT(*) FROM Assets       WHERE SerialNumber    <> '' GROUP BY SerialNumber   HAVING COUNT(*) > 1;
```

Blank values are excluded by the index filters on purpose, so empty strings are
safe; duplicates and over-length text are not.

### 8.2 Clear the demo and QA residue before you migrate anything real

`DataSeeder` (defect 17) puts four clients and six products into **every** freshly created database, and the QA database this pass was verified against has accumulated more on top of it. None of that is a schema problem, but all of it distorts the checks above — a duplicate tracking number found in a test row will block a unique index that real data would have satisfied — and it will show up in screenshots, training material and the analytics charts as if it were the customer base. Before pointing a real deployment at a migrated database, look for at least:

- products `70EA9682…` and `3AB2DABC…`, which are seed rows and not catalogue items;
- purchase orders raised while testing the Warehouse Alerts reorder path, each naming a supplier typed into a form and therefore meaningless;
- user accounts created to exercise role assignment, including client-role accounts whose `Clients` row was created by the new role-change path;
- at least one `HealthMetrics` row written while confirming that table actually exists after the `AddHealthMetricTable` fix below.

Delete them by key, not by pattern, and re-run the §8.1 queries after — not before — or you will be validating the seed data instead of the real data.

**One schema note specific to this upgrade.** `20261023000000_AddHealthMetricTable`
was written by hand without a `[Migration]` attribute, and EF only collects
migration classes that carry it, so the migration had never run anywhere and
`dbo.HealthMetrics` does not exist on a database built before this was fixed. The
attribute is now present and the migration applies normally, as verified on an
existing database. If a site has already created that table by hand, the migration
will fail with "There is already an object named 'HealthMetrics'"; record it as
applied instead of dropping the table:

```sql
INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
VALUES (N'20261023000000_AddHealthMetricTable', N'10.0.12');
```

**On the missing `.Designer.cs` files.** Eight of the migrations are hand-written:
the seven listed in §12.4 of the technical report (`AddHealthMetricTable`,
`AddClientTimelineNotes`, `AddB2BAppointments`, `AddSalesGoals`,
`AddUserPasswordResetFields`, `AddAuditLogs`, `AddUserDeviceToken`), plus
`20261031160000_AddInquiryScoringInputs` added in this pass. They carry
`[DbContext]` and `[Migration]` inline instead of having generated Designer files.
That is not a defect: the Designer exists to hold
those two attributes plus a `TargetModel` snapshot, and the attributes are what EF
actually needs to discover and order a migration. `dotnet ef migrations list` shows
all eighteen in timestamp order and `MigrateAsync()` applies them cleanly. A
fabricated `TargetModel` would be worse than none — it would assert a database
state nobody observed, and `dotnet ef migrations remove` would then restore the
wrong model. Leave them as they are and write new migrations with `dotnet ef` so
they arrive with a real Designer — but note that `AddInquiryScoringInputs` was
written by hand for the same reason `ApplyEntityConfigurationsAndIndexes` was
withdrawn: the snapshot in `AddTask3Features` contradicts the database it claims to
describe, so anything scaffolded from it today inherits that contradiction and has
to be checked line by line before it can ship.
