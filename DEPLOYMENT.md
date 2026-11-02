# Deployment, secrets and credential rotation

Companion to `README.md`. The README tells a developer how to run the code; this file tells whoever operates the deployed site **where each secret must live**, **what to do about the credentials that were committed to git**, and **what has to be true before the live notification feed works**.

Nothing in this file was executed against the hosting provider. The steps that touch `databaseasp.net`, the Monster ASP control panel, or the shared git remote are deliberately left for an operator with access, because they cannot be done from a working copy and two of them are destructive.

## What runs where

| Piece | Deployed target | Notes |
| --- | --- | --- |
| Staff web app (`VerichatCRM.Web`) | `verichat-crm-web.runasp.net` — Monster ASP shared hosting, publish profile `site94515-WebDeploy.pubxml` | Razor MVC. Talks to the API server-side and passes the API base URL to page script. |
| API (`VerichatCRM.API`) | **`http://api-site.runasp.net/`** — Monster ASP shared hosting, publish profile `site94518-WebDeploy.pubxml` (the duplicate `…WebDeploy1` / `…WebDeploy2` profiles were deleted on 1 Nov; `verichat-crm-api.runasp.net` answers identically, so both names reach either the same site or two copies of the same broken deployment — the panel is the only place that can tell them apart) | REST + SignalR hub at `/hubs/notifications`. **Not serving:** every path returns HTTP 500.30, including `/api/health/live` — see §7.1. `https://` on the same host does not connect at all: no certificate is bound. |
| Backup API (`VerichatCRM.API`, second deployment) | `https://verichat-api-aqcbgydhhbgbgha5.eastus-01.azurewebsites.net` — Azure App Service, the target of `BackupApiBaseUrl` | **Serving, but `Degraded`:** it starts and has its `Jwt__Key` and connection string set, then fails every data route because it has no database it can reach (§7.2). Not a working safety net yet. |
| Database | `databaseasp.net` (Monster ASP shared SQL Server) | Reached only by the API, and only by an API running *inside* that hosting family: the published name resolves to the private address `10.0.0.23`, so no connection string naming it works from Azure (§7.2). The web app holds no connection string. |
| Mobile client (`VerichatMobile`) | EAS builds, installed on devices | Reads `EXPO_PUBLIC_API_URL` at bundle time. |

Publish profiles (`*.pubxml`) are git-ignored and none of them carries a `Password` element, so nothing new leaks there.

**The shipped configuration now names the host the API is actually published to.** Until 1 Nov it did not: `VerichatCRM.Web/appsettings.json` carried `ApiBaseUrl = https://verichat-crm-api.runasp.net`, while the published site is `http://api-site.runasp.net` and `https://` connects to none of the `*.runasp.net` names — re-measured 1 Nov 2026, every `https://` probe fails at `curl` return code 35 (`api-site` and `verichat-crm-web`: handshake failure; `verichat-crm-api`: connection reset). No certificate is bound anywhere. A web app configured that way would have failed every API call even after the 500.30 below is fixed, and the mobile app contradicted itself — `VerichatMobile/.env` said `http://`, the `api.ts` fallback said `https://`. All three now read `http://api-site.runasp.net`:

| File | Value | In git? |
| --- | --- | --- |
| `VerichatCRM.Web/appsettings.json` | `"ApiBaseUrl": "http://api-site.runasp.net"` | yes — this is what the deployed web site ships with |
| `VerichatMobile/src/constants/api.ts` | fallback `'http://api-site.runasp.net'` | yes — the default for any build that sets no env var |
| `VerichatMobile/.env` | `EXPO_PUBLIC_API_URL=http://api-site.runasp.net` | **no** — `.gitignore:437` ignores `.env`, so this line only fixes the working copy on this machine |

Because `.env` is ignored and there is no checked-in `.env.example`, the `api.ts` fallback is the only committed source of the mobile origin; a fresh clone that never creates `.env` still talks to the right host. A machine that does create `.env` must use the same value, or it silently overrides the fallback.

CORS needs no change: `VerichatCRM.API/appsettings.json` already allows `http://verichat-crm-web.runasp.net` and its `https` variant, and the web origin is the only browser that calls the API cross-origin. `Cors:AllowedOrigins` in `appsettings.json` is for *deployed* origins, which are not secret — that is why the pair is committed rather than being a panel variable; extra local origins go in `Cors__AllowedOrigins__2`.

Two consequences of committing an `http://` origin deliberately:

1. **It is a marker of unfinished work, not a working arrangement.** The scheme is cleartext because no certificate is bound (§5). Bind one to both sites, then change `ApiBaseUrl`, `.env` and the `api.ts` fallback to `https://` in the same commit — an `https` page calling an `http` API is blocked as mixed content, so a half-migration breaks the web app.
2. **A signed mobile release build is still impossible against it.** Android and iOS block cleartext traffic in release builds, so the EAS `production` profile needs the certificate first. Development builds and Expo Web work as configured.

These values are read at *deploy* time, not build time: after changing any of them, republish the web site (`site94515-WebDeploy.pubxml`) and rebuild the mobile bundle — `EXPO_PUBLIC_*` is inlined into the JS bundle, so a `.env` edit needs a new build to take effect.

## Secrets inventory

| Config key | Read by | Correct home today | Cost of rotating it |
| --- | --- | --- | --- |
| `ConnectionStrings:DefaultConnection` | `VerichatCRM.API/Program.cs:28` | Host secret store / `ConnectionStrings__DefaultConnection`. **Never** in `appsettings*.json` outside Development. | API restart. Sessions are unaffected. |
| `Jwt:Key` | `Program.cs:133` | `Jwt__Key`. The app throws at startup when it is blank. | Every issued token becomes invalid — all staff and all mobile users are signed out. |
| `Email:Host` / `Port` / `UseSsl` / `Username` / `Password` / `FromAddress` / `FromName` | `VerichatCRM.Infrastructure/Data/External/EmailService.cs:74` | `Email__*` env vars. `Host` is the switch: while it is blank the service logs "would send" and returns without throwing, so every mail route silently no-ops. | Password reset, address verification, account approval and the contact-page enquiry still *record* their state but nothing reaches an inbox, which turns the self-service recovery routes into admin-only ones. |
| `Email:SalesAddress` | `VerichatCRM.API/Services/PublicEnquiryService.cs:161` | `Email__SalesAddress`. Optional — the code already defaults to the published sales mailbox. | Copies of public enquiries go to the default address instead. |
| `Bootstrap:AdminEmail` / `AdminPassword` / `AdminName` | `AdminBootstrapper` | `Bootstrap__*` env vars, **deleted after the first sign-in**. Only consulted while the user table is empty; leaving them set is a standing admin-account door. | None once accounts exist. |
| `Bootstrap:DemoUsers` | `DemoUserSeeder` | `appsettings.Development.json` only. It and the Development environment are two separate conditions and both must hold, so a host that is not Development ignores the key completely. | None — it creates accounts and never changes them. Verified against a scratch database: flag off, 0 rows; flag on, one account per role; restart with the flag still on, still one per role. |
| `Cors:AllowedOrigins` | `Program.cs:198` | `appsettings.json` (deployed origins are not secret) plus `Cors__AllowedOrigins__2` for extra environments. | Browsers stop being able to call the API. |
| `EXPO_PUBLIC_API_URL` | `VerichatMobile/src/constants/api.ts` | `VerichatMobile/.env` (ignored) or the EAS build environment. | Needs a new app build; it is inlined into the bundle. |

The one combination that would make `Bootstrap:DemoUsers` live on a host is a publish that ships `appsettings.Development.json` *and* an environment set to `Development`. Azure App Service and IIS both default to `Production` when the variable is unset, so the safe state is the accidental one — but if a staging slot is ever pointed at Development to read startup logs, treat every account in README's role table as existing on that slot, and delete them before it takes traffic.

Local development needs only the JWT key. The connection string is already in `appsettings.Development.json` and points at LocalDB with `Encrypt=False;TrustServerCertificate=True`, which is what LocalDB requires — `Program.cs:41` allows that key only in Development, precisely so it can never reach a real host:

```powershell
dotnet user-secrets set "Jwt:Key" "<64-char random string>" --project VerichatCRM.API
```

**Do not set `ConnectionStrings:DefaultConnection` in user secrets for local work, and check whether you already have.** Config precedence puts user secrets *above* `appsettings.Development.json`, so a value left over from a hosted rehearsal silently outranks the checked-in LocalDB string. It no longer breaks Visual Studio F5 — both API launch profiles now carry `ConnectionStrings__DefaultConnection` pinned to LocalDB, and an environment variable outranks user secrets (verified: with the stale secret still in place, the profile-launched API reports `Healthy` / `Database reachable` against LocalDB with nothing set in the shell) — but it **does** still hijack every `dotnet ef` command, because the EF tools apply only their own `ASPNETCORE_ENVIRONMENT=Development` default and not the profile's variables (§9). The failure is easy to mistake for a code bug: the API spends its startup trying to reach that server, since the migration block at `Program.cs:266` runs before Kestrel binds and `EnableRetryOnFailure` (`Program.cs:78`) retries it, so `dotnet run` looks like it has hung for a minute or more. When it finally answers, `/api/health` reports `database: Unhealthy — CanConnectAsync returned false` and the web sign-in page says only *"The request failed (HTTP 500)"* — the failure appears three layers away from its cause. Delete the key, since nothing local needs it:

```powershell
dotnet user-secrets remove "ConnectionStrings:DefaultConnection" --project VerichatCRM.API
```

Removing it is the safer habit: the secrets store is per-machine and invisible in `git status`, so it is the one place a production connection string can sit without anyone reviewing it.

User secrets are loaded only when `ASPNETCORE_ENVIRONMENT=Development`. They do nothing in production.

The web tier has a matching Development override, and it matters for more than tidiness. `ApiBaseUrl` in `appsettings.json` is the *deployed* API, and `VerichatCRM.Web/Services/ApiFallbackHandler.cs:29` treats an HTTP 500 from the primary as a transient error and retries it against `BackupApiBaseUrl` — the live Azure host. Without the override, a local sign-in does not test your code: it posts the password you typed to production infrastructure and renders whatever error comes back. `appsettings.Development.json` for the web project therefore points `ApiBaseUrl` at `http://localhost:5184` and sets `BackupApiBaseUrl` to empty, which the handler reads as "no backup configured" (`:21`) and stops the failover from ever leaving the machine.

## 1. Rotate the database credential

This is the only step that actually fixes the exposure. Removing a password from files does not un-expose it — the value sat in `VerichatCRM.API/appsettings.json` across seven commits from `a53523c` to `c3f3fb5` (the last of them on 2026-10-26, on `dev2` and `section1---mutsa`, which are both pushed). Anyone with read access to the public repository, a fork, a clone, or GitHub's cached views can recover it with `git log -p`.

1. Sign in to the Monster ASP control panel and open its SQL Server manager for the `db70543` database. Change the login password there. If the panel does not expose the option, ask support to reset it, or run `ALTER LOGIN ... WITH PASSWORD = '<new>'` in the panel's query tool if it grants one. **This cannot be done from a working copy — there is no SQL Server reachable from this repository.**
2. Confirm nothing else authenticates with that same login. The `db70543` account is scoped to that one database, so no other application should be using it; verify before assuming.
3. Put the new value into the host's configuration store (see §3) with `Encrypt=True` (see §4).
4. Restart the API site, then check `http://api-site.runasp.net/api/health`, which reports database readiness.

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
- **Mobile release builds.** Android and iOS block cleartext traffic, so today's installs only work as development/preview builds. `Program.cs:288` calls `app.UseHttpsRedirection()`, which *would* make the `http://` origin unreachable — it answers every cleartext request with a redirect to `https://`, and the handshake there fails. Whether it fires depends on the site having a 443 binding for ANCM to discover, and that is not observable on the API today because the process dies before serving a request. The sibling web site ships the same middleware (`VerichatCRM.Web/Program.cs:57`) and demonstrates what happens on this host: measured 1 Nov 2026, `http://verichat-crm-web.runasp.net/` and `/Account/Login` both return **200 with no `Location` header**, so redirection is inert there and cleartext works. If the API site *does* have a 443 binding, expect `307 → https://api-site.runasp.net` the moment it starts, which fails the handshake and looks like "the API is unreachable over http". The panel check is: does that site list an https binding? If it does, remove it or delete `UseHttpsRedirection()` until a certificate is really bound — note also that the redirect happens ahead of `UseCors` (line 289), so a browser cross-origin call would see no CORS headers on the redirect response either.

Until both sites have certificates bound, the practical result is: log in, use every normal feature, and see no live notifications. Bind the certificate, then set `ApiBaseUrl` in `VerichatCRM.Web/appsettings.json` to the `https://` API origin and switch `EXPO_PUBLIC_API_URL` (and the `api.ts` fallback) to `https://`.

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

**Status, re-measured 2 November 2026: this one is fixed.** `http://api-site.runasp.net/api/health`
now returns HTTP 503 with a real JSON body rather than the 500.30 HTML page - the process starts,
`Jwt:Key` is present, and its database answers in 5 ms. What it reports instead is
`MigrationsPending`, six of them, so §7.1 below is history and §8 is the live problem. Note that
`/api/health` proves the panel settings are right and proves nothing about the schema; its
database check only opens a connection, which is why a reachable-but-unmigrated site still looks
almost healthy.

The original failure, kept because the reasoning is what found it:

Re-measured on 1 November 2026 after the API was published again — every path on
both API host names fails the same way, while the web site on the same hosting
family is healthy:

| Probe | Result |
| --- | --- |
| `http://api-site.runasp.net/` and `/logs/` and `/api/health/live` | **500** — `HTTP Error 500.30 - ASP.NET Core app failed to start` |
| `http://verichat-crm-api.runasp.net/` | **500** — the same page |
| `https://api-site.runasp.net/`, `https://verichat-crm-web.runasp.net/` | **does not connect** — `curl` rc 35, TLS handshake failure: no certificate bound (§5) |
| `https://verichat-crm-api.runasp.net/` | **does not connect** — `curl` rc 35, connection reset before the handshake completed |
| `http://verichat-crm-web.runasp.net/` | **200** — same .NET target, same host family |

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
| 2 | line 41 | the string says `Encrypt=False` and the environment is not Development |
| 3 | line 133 | `Jwt:Key` is blank |

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

The log is **only** reachable that way: `http://api-site.runasp.net/logs/` was
requested during this investigation and answered 500.30 like every other path,
because the `<handlers>` entry claims `path="*"`, so a dead app cannot serve its own
log. Confirmed on 1 Nov that the file does ship — `stdoutLogEnabled="true"` and
`stdoutLogFile=".\logs\stdout"` are present in the published
`obj/Release/net10.0/PubTmp/Out/web.config` — so today's failed starts should already
have written one or more files there, before any further publishing.

Two things that are *not* the cause, because both are caught and logged and the app
continues in degraded mode: a migration that fails at startup (§8) and seeding the
starter catalogue. Those make `/api/health` report a problem; they do not produce
500.30.

**One that was, until `c909aa6`, and is worth reading because it survives step 1 and
step 2 above.** `Program.cs` also calls `MigrateAsync()` at startup, and on this
repository that call threw before applying a single migration:

```
System.InvalidOperationException: An error was generated for warning
'Microsoft.EntityFrameworkCore.Migrations.PendingModelChangesWarning': The model for
context 'AppDbContext' has pending changes. Add a new migration before updating the
database.
```

EF compares the live model against `AppDbContextModelSnapshot.cs` *before* it touches
the database, and the snapshot is missing four entity types the context maps —
`AuditLog`, `HealthMetric`, `ClientTimelineNote`, `Appointment` — because their tables
came from hand-written migrations, which update nobody's snapshot (§8.2). So the
sequence on a fresh deployment was: configure the panel correctly, the app starts, the
migration throws, the catch logs it and continues, and the site comes up against an
empty database. If that database exists but holds no tables — the hosted case, where
the panel created it — `/api/health` still reports Healthy, because its database check
only proves a connection opens. The fix downgrades this one warning to a log line, so the
migrations run; the snapshot drift itself is still open and is what §8 is about.

### 7.2 The Azure copy starts, and cannot reach any database

Re-measured 1 Nov 2026 at 21:13 UTC against
`https://verichat-api-aqcbgydhhbgbgha5.eastus-01.azurewebsites.net/api/health`:

```json
{"status":"Degraded", "jwt":{"status":"Healthy","detail":"Jwt:Key present."},
 "database":{"status":"Unhealthy","detail":"CanConnectAsync returned false. Check connection string / firewall."},
 "environment":null, "latencyMs":13926}
```

Three things follow from that payload, and the first two are good news that is easy to miss
because the overall status reads as a failure.

**The Azure app is configured; the primary is not.** It answers as JSON, so `Program.cs` got
past all three startup checks (§7.1), which means `Jwt__Key` **and**
`ConnectionStrings:DefaultConnection` are both set on that App Service — a blank connection
string throws before the first request and would show 500.30, exactly like
`api-site.runasp.net`. The remaining failure is one setting's *value*, not a missing setting.

**`environment: null` is not a misconfiguration.** `HealthController.cs:89` reads the raw
`ASPNETCORE_ENVIRONMENT` variable, which Azure never sets — the framework supplies
`Production` as a default without writing the variable. So the payload shows null while the
app really is running as Production, which is why the `Encrypt=False` rejection at
`Program.cs:41` is armed on that host. Do not read that field as "no environment".

**The database cannot be reached from Azure, by construction.** The published name for the
Monster ASP SQL Server, `db70543.databaseasp.net`, resolves to **`10.0.0.23`** — an RFC 1918
private address — and TCP 1433 against it fails from outside that network (re-verified;
`Test-NetConnection` reports `TcpTestSucceeded : False`). A DNS record that returns a private
address is only meaningful to machines already inside the publisher's network. The API is
normally colocated there, which is precisely why this hosting pair was chosen — but it means
**no connection string naming that server can ever work on Azure**, so the App Service copy
needs its own database (Azure SQL plus §8's chain) or it stays `Degraded` permanently. Setting
`Jwt__Key` there was necessary and is not sufficient.

`latencyMs` is not a health signal either, and should not be compared between probes: the same
endpoint returned 54 ms and 13,926 ms within minutes of each other. What *is* informative is
which branch produced the text: `HealthController.cs:37` is the "the call completed and
answered `false`" path, while `:43` would have surfaced the base exception message instead.
So the payload tells you the check ran and was refused, not why — the reason is on the host,
where `:42` logs it as `Health database check failed.`

**The failover is therefore dead at both ends.** `ApiFallbackHandler.cs:29` treats a 500 from
the primary as transient and retries it against `BackupApiBaseUrl` — this Azure copy. Today
the primary returns 500.30 for every path and the backup has no reachable data, so the retry
cannot succeed; it only doubles the latency of a failing request and makes the web app's error
page report whichever of the two came back last. The safety net becomes a safety net when the
backup has a database. Until then, either give the App Service its own Azure SQL database, or
blank `BackupApiBaseUrl` in the deployed web configuration so there is one failure to read
instead of two — the same reasoning behind §9's warning about the design-time factory, and the
reason the web project's `appsettings.Development.json` already ships that key empty.

## 8. Pre-flight checks before migrating an existing database

**Five migrations are new or rewritten in this working copy.** Read §8.1 before
running any of them against a database that holds real data, and read §8.4 before
you let anyone sign in afterwards — two of these migrations narrow columns, add
unique indexes and put existing staff accounts back in the approval queue.

- `20261101151838_SyncSchemaAndAccountLifecycle` is the index-and-precision migration §8.1 used to describe as deferred — it exists now, and it is the heaviest thing in the chain. It adds the seven account-lifecycle columns to `Users` and the `LoginHelpRequests` table, applies the widths and the `decimal(5,4)` the configuration classes have always declared, and creates the indexes no migration ever emitted, including the unique `IX_Users_Email`. **It can fail on live data**, and `Database/preflight-migration-checks.sql` (§8.1) is the script that predicts it.
- `20261101152359_ReviewSelfClaimedStaffAccounts` runs one `UPDATE` over `Users`, and it is the migration with a business consequence rather than a schema one: every existing Sales Representative, Procurement Manager, Field Support Engineer and Logistics account becomes `Pending` and **cannot sign in** until a System Administrator approves it at `/Admin/Approvals`. That is the whole point of the migration — self-registered staff had no gate before this — but it is indistinguishable from an outage to the people affected. See §8.4.
- `20261031160000_AddInquiryScoringInputs` does three things and nothing else —
  it narrows `Inquiries.Status` from `nvarchar(max)` to `nvarchar(30)`, because SQL Server cannot build an index on a `max` column and the queue sorts on `Status` (the values come from `InquiryStatuses`; the longest is nine characters); it adds the two nullable inputs the automatic enquiry score reads, `Inquiries.Urgency` (`nvarchar(30)`) and `Inquiries.EstimatedValue` (`decimal(18,2)`); and it creates the composite index `IX_Inquiries_Status_Score`.
- `20261030204733_AddTask3Features` was **rewritten**, not added (`113d04d`). It had never run successfully anywhere: three of its foreign-key columns were declared `nvarchar(64)` against `nvarchar(450)` principals, which aborts the whole `CREATE TABLE` with Msg 1753, and its `Up()` also emitted a second `CREATE TABLE SalesGoals` with a shape belonging to no entity in the solution, five days after `20261025111000_AddSalesGoals` had already created that table. Both defects, and the stale `AppDbContextModelSnapshot.cs` / `.Designer.cs` entries that carried them, are corrected. The `HasMaxLength(64)` calls are gone from `AppDbContext` on purpose — the values stored in those columns are `Guid.ToString()` (36 characters), so nothing about the data changed; only the declaration now agrees with the principal it points at.
- `20261031210000_AddTicketActivityAndDeliveryStatusHistory` is new and hand-written (`f327007`). It creates `TicketActivity` and `DeliveryStatusUpdate` — two tables the model declares, the context maps, and the application writes to, and which **no migration in the chain had ever emitted**. Raising a support ticket therefore died on save with `Invalid object name 'TicketActivity'`, and every delivery status write with the same error. They could not be scaffolded either: both entities are already in the model snapshot, so the differ compares the current model against a snapshot that agrees with it, finds nothing, and prints nothing. A hand-written migration is the only way to add them.

`IX_ClientAssignments_ClientId_SalesRepId_AssignedDate` now emits a create-time
warning — 1808 theoretical key bytes against SQL Server's 1700 limit, because two
`nvarchar(450)` columns sit in one key. It is a warning, not an error: the index is
created, and the rows that actually go through it are GUIDs and dates, about 152 bytes.
It is listed here because the message appears in the migration output and the next
person to read it will otherwise look for a failure.

**Two tooling traps on this chain.** `dotnet ef migrations add` still starts from a
snapshot that is missing four mapped entity types (§7.1), so anything scaffolded today
begins by proposing `CREATE TABLE` for `Appointments`, `AuditLogs`,
`ClientTimelineNotes` and `HealthMetrics` — tables every migrated database already has,
which aborts the script with "There is already an object named …" — and it stays silent
about genuine gaps the snapshot already describes. Read a scaffolded migration line by
line before shipping it, or write it by hand the way the other nine were. And do not run
`dotnet ef migrations remove --force` on this chain: it rebuilds
`AppDbContextModelSnapshot.cs` from the previous migration's `.Designer.cs`, the
previous migration (`20261031160000_AddInquiryScoringInputs`) is hand-written and has
none, and the snapshot came out as a 21-line stub with an empty `BuildModel`. It was
restored from the commit.

To apply the chain to a database that already exists and already has history, generate
and run the idempotent script rather than relying on startup, and read the queries below
first:

```powershell
dotnet ef migrations script --idempotent `
  --project VerichatCRM.Infrastructure --startup-project VerichatCRM.API `
  --output ApplyAllMigrations.sql
sqlcmd -S <tcp:server> -d <database> -U <login> -I -i ApplyAllMigrations.sql
```

**The `-I` is not optional.** It sends `SET QUOTED_IDENTIFIER ON` for the session, and
`sqlcmd` leaves that option OFF by default. Measured on a fresh local database, the same
script without `-I` commits migrations 1 to 19 and then dies on:

```
Msg 1934, Level 16, State 1, Line 1
CREATE INDEX failed because the following SET options have incorrect settings:
'QUOTED_IDENTIFIER'. Verify that SET options are correct for use with indexed views
and/or indexes on computed columns and/or filtered indexes ...
```

Because each migration is its own `BEGIN TRANSACTION ... COMMIT`, everything before the
failure is already committed when the batch aborts. The result is a database that is
neither the old schema nor the new one, with 19 of 21 recorded - and the idempotent guards
mean re-running it later does not undo the mixture, it just continues. With `-I` the script
finishes: 21 migrations recorded, 39 base tables, matching the development database.

The generated body also starts with a UTF-8 BOM, which is harmless at the top of a file but
is not harmless after the header `GO`. Strip it (`-replace [char]0xFEFF,''`) or the first
token of that batch is a BOM the server will not accept.

`Database/apply-pending-migrations.sql` is this script, already generated, with the two SET
statements asserted inside the file so it is safe in a client you cannot pass flags to -
which is the case for a hosting panel's SQL window. It was verified by running that file
twice, once without `-I`, both to completion, and a second time against an already-migrated
database to confirm the guards make a re-run a no-op. Regenerate it rather than editing it;
the body is machine output.

**Why the hosted site is stuck at 15, and why that is not this trap.** The deployed API reports
six pending: `20261027100000_AddUserDeviceToken` through `20261101152359_ReviewSelfClaimedStaffAccounts`,
i.e. migrations 16 to 21 in id order, with 1 to 15 recorded. That is a clean suffix, so the chain
is not broken - it simply stopped early. And it stopped at 15, not at the 19 where
`QUOTED_IDENTIFIER` kills a hand-run script, so the two failures are unrelated. The explanation
that fits is the one in §8: the build that last migrated successfully contained 15 migrations, and
the newer build - which does contain all 21, because `/api/health` enumerates the missing six out
of its own loaded assembly - throws in `MigrateAsync` before it applies anything. `Program.cs:266`
runs that at startup and the catch keeps the site alive in degraded mode, which is exactly what a
reachable database with six pending migrations looks like. The check that settles it is the host's
own stdout log (§7.1 turned `stdoutLogEnabled` on for this reason): look for
`PendingModelChangesWarning` or any `DbMigrator` exception. If the deployed binary predates
`c909aa6`, which downgrades that one warning to a log line, publishing the current build lets
startup finish the migration by itself and no script is needed. Otherwise run
`Database/apply-pending-migrations.sql` in the panel and restart the app.

Build first — `--no-build` silently reuses the assemblies already on disk, so a fix to a
migration or to the snapshot will not appear in the regenerated script and the same
error comes back unchanged, which reads as the fix not working.

`sqlcmd` takes the password interactively when `-U` is given without `-P`, which is how
it should be run: the credential then reaches neither the script file, the command line,
nor the shell history. Run it from the directory holding `ApplyAllMigrations.sql` and
pass that plain relative name — a quoted absolute path with forward slashes is mangled
into `Error occurred while opening or operating on file C:`. `--idempotent` makes the
script safe to re-run: it reads `__EFMigrationsHistory` and emits only the migrations the
database has not recorded, so a partially applied chain finishes rather than repeats.

A database that does not exist yet needs none of this: since `c909aa6` the API creates
and migrates it at startup. Re-verified against a scratch LocalDB database pointed at by an
environment variable — **39 base tables, 21 rows in `__EFMigrationsHistory`**, and
`Appointments`, `AuditLogs`, `ClientTimelineNotes`, `HealthMetrics`, `TicketActivity`,
`DeliveryStatusUpdate` and `LoginHelpRequests` all present, with `SalesGoals` created
exactly once, `Quotations.VatRate` at `decimal(5,4)`, `Tickets.Title` at
`nvarchar(200)` and the five unique indexes built. The same script from §8.1 runs clean
against that empty database, which is the check that it is safe to point at a host whose
database the panel created but nothing has ever migrated.

**One rehearsal trap.** That run has to be in `Development`. [Program.cs:41](VerichatCRM.API/Program.cs#L41)
refuses to start outside Development when the connection string says `Encrypt=False`, and
LocalDB rejects an encrypted session, so a Production rehearsal dies with
`ConnectionStrings:DefaultConnection sets Encrypt=False` before it reaches the database.
The guard is the point — it is what stops a committed `Encrypt=False` opening a plaintext
session to production SQL — so rehearse in Development and never loosen the guard to make
a local run quieter.

Pre-flight on an existing database is **not** short any more. `AddInquiryScoringInputs`
narrows one column and adds nothing but nullables, so on its own it was safe to run
blind. `SyncSchemaAndAccountLifecycle` re-types 30 columns across seven tables — 28
string widths plus the decimal precision on `Quotations.VatRate` (`18,2` → `5,4`) and
`DeliveryLogs.WeightKg` (`18,2` → `10,2`) — creates the `LoginHelpRequests` table and 15
indexes, five of them unique, and adds seven `Users` columns. A single out-of-shape row
aborts the script partway and leaves the schema half-changed, so run the §8.1 checklist
against the live database before this one, then the two queries below:

```sql
-- The only narrowed column, narrowed to 30. Should return no rows.
SELECT InquiryId, Status, DATALENGTH(Status) AS Len FROM Inquiries WHERE DATALENGTH(Status) > 30;

-- Urgency, where present, must be one of the four the scorer recognises.
-- Anything else is not an error - it scores as "not stated" - but it is worth
-- knowing what is in the column before you train anyone on the ranking.
SELECT Urgency, COUNT(*) FROM Inquiries WHERE Urgency IS NOT NULL GROUP BY Urgency;
```

**Existing scores are not recalculated.** The new columns are nullable and no back-fill runs, deliberately: computing a score from data that was never captured would produce a confident-looking number that nobody chose. Enquiries written before this migration keep whatever `Score` their author typed. Re-ranking the queue after upgrading is a human action in the Inquiry form, not a migration step.

**The whole upgrade, in order:**

1. Confirm a **System Administrator** can sign in to the target deployment. Steps 5–8 all
   depend on it, and after step 6 no other staff account can (§8.4).
2. Clear the demo and QA residue (§8.2). Do this before step 3, or the pre-flight reports
   seed rows instead of real ones.
3. Run `Database/preflight-migration-checks.sql` against the live database (§8.1) and fix
   every row sections 1–4 return. Write down what section 5 counts.
4. Apply the chain — the idempotent script above, or a deploy that lets the API migrate at
   startup. Only after step 3.
5. Confirm the schema took: `__EFMigrationsHistory` should hold 21 rows and the database 39
   base tables. Re-running sections 1–4 now proves nothing, because the narrowed columns
   cannot physically hold an over-length value; they were the prediction, not the check.
6. Work `/Admin/Approvals` and release the staff accounts the review migration pended
   (§8.4). Until you do, the only way into the CRM is the administrator from step 1.
7. Seed the catalogue (§8.3).
8. Walk §7's post-deployment confirmations.

### 8.1 The index-and-precision migration — it exists now, and the pre-flight is a gate

`20261031091951_ApplyEntityConfigurationsAndIndexes` was scaffolded, reviewed and **then deleted from this working copy, because it proposed changes that would have failed on the first database it touched.** Two of the reasons it was written that way are now fixed — the duplicate `SalesGoals` rebuild is gone (§8), so a fresh scaffold no longer tries to recreate that table — and one is not: the snapshot still omits the four tables that came from hand-written migrations, so a scaffold today opens with `CREATE TABLE Appointments / AuditLogs / ClientTimelineNotes / HealthMetrics`.

**Its purpose is no longer outstanding.** `20261101151838_SyncSchemaAndAccountLifecycle` is that migration — scaffolded, then edited the way §8.2 now tells you to edit a scaffold: the four phantom `CREATE TABLE` lines the snapshot has never seen were stripped, and what remains is the real delta. Confirmed on the local database after it ran on 2026-11-01 — `__EFMigrationsHistory` holds 21 rows and 39 base tables, `Quotations.VatRate` is `decimal(5,4)`, `Tickets.Title` is `nvarchar(200)`, and all five unique indexes are present: `IX_Users_Email`, `IX_Tickets_TrackingNumber`, `IX_DeliveryLogs_TrackingNumber`, `IX_Quotations_QuoteReference`, `IX_Assets_SerialNumber`. The same database had reported `decimal(18,2)` and no unique index on any of those five columns before this migration existed. The `IEntityTypeConfiguration` classes it applies are the ones `ApplyConfigurationsFromAssembly` has been loading at startup all along — the code always declared these shapes; nothing in the database enforced them until now.

One `Up()` does all of it, in five parts:

- seven columns on `Users` — `ApprovalStatus` `nvarchar(20)` **defaulting to `'Approved'`**, `EmailVerified` `bit` defaulting to `false`, plus `EmailVerificationToken`/`EmailVerificationExpiry`, `RejectionReason`, `ReviewedByUserId` and `ReviewedAt`;
- the `LoginHelpRequests` table the help button writes to;
- 28 string columns narrowed from `nvarchar(max)` across `Tickets`, `Quotations`, `QuotationLineItem`, `DeliveryLogs`, `Deals`, `Clients` and `Assets` — two of them are GUID foreign keys going to `nvarchar(450)`, which widens and stores 36-character values, so they are carried here for completeness rather than risk;
- two decimals re-typed: `Quotations.VatRate` `18,2` → `5,4` and `DeliveryLogs.WeightKg` `18,2` → `10,2`;
- 15 indexes created — five of them unique — and `IX_Tickets_ClientId` dropped so it can come back as the composite `IX_Tickets_ClientId_Status`.

**Run `Database/preflight-migration-checks.sql` against the live database before applying it.** It is read-only, it prints one section per risk, and every section says what a healthy result looks like:

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -d VerichatCRM -b -i 'Database\preflight-migration-checks.sql'
sqlcmd -S tcp:<server>.database.windows.net -d <database> -U <login> -b -i 'Database\preflight-migration-checks.sql'
```

Applied and verified against the local database on 2026-11-01: sections 1–4 returned no rows, and the narrowing sweep was checked for the opposite property too — with one threshold deliberately set to zero it reported the two `Tickets` rows that have descriptions, so an empty result is the sweep finding nothing rather than the sweep doing nothing.

Three things the script encodes that are easy to get wrong by hand:

- **`DATALENGTH` counts bytes; the new maximum is characters.** A 200-character `nvarchar` column holds up to 400 bytes, so the test is `DATALENGTH(col) > @MaxLen * 2`. The earlier inline version of this checklist compared `DATALENGTH` against the character maximum directly, which over-reports by 2× — it flags a 101-character value in a 200-character column. The script does not.
- **`IX_Users_Email` has no filter.** It is the only one of the five unique indexes built over the whole column, so a second blank address — or a second `NULL`, since SQL Server allows exactly one — aborts the migration just like two real duplicates. The other four exclude `''`, so blank values are safe there and duplicates are not.
- **`VatRate` is the one that cannot be salvaged by widening.** `decimal(5,4)` fits 0.9999 and below; a rate stored as `15` meaning 15% does not fit, and no index filter can help. Either store it as `0.15` or the row blocks the migration.

On an empty or newly created database all of this applies without complaint. On a database that has been used, any row that violates the new shape aborts the script at that statement, and because the API calls `MigrateAsync()` at startup the app then comes up in degraded mode with the schema half-changed. Fix or trim the offending rows, re-run the script, then migrate.

**`ApprovalStatus` defaulting to `'Approved'` is deliberate and is the reason the migration itself is safe.** Adding a `NOT NULL` column to a populated table needs a default, and the alternative — `'Pending'` — would have locked every existing account out of the building in the same second the schema changed. The review that does lock staff accounts out is the *next* migration, and it does it on purpose: §8.4.

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

**On the missing `.Designer.cs` files.** Nine of the twenty-one migrations are
hand-written: the seven listed in §12.4 of the technical report
(`AddHealthMetricTable`, `AddClientTimelineNotes`, `AddB2BAppointments`,
`AddSalesGoals`, `AddUserPasswordResetFields`, `AddAuditLogs`,
`AddUserDeviceToken`), plus `20261031160000_AddInquiryScoringInputs` and
`20261031210000_AddTicketActivityAndDeliveryStatusHistory` added in this pass. They
carry `[DbContext]` and `[Migration]` inline instead of having generated Designer files.
That is not a defect: the Designer exists to hold those two attributes plus a
`TargetModel` snapshot, and the attributes are what EF actually needs to discover and
order a migration. `dotnet ef migrations list` shows all twenty-one in timestamp order and
`MigrateAsync()` applies them cleanly — which it only started doing at `c909aa6`; before
that the same call aborted on the pending-model-changes check without running any of
them (§7.1). A fabricated `TargetModel` would be worse than none — it would assert a
database state nobody observed, and `dotnet ef migrations remove` would then restore the
wrong model, which is precisely the trap described in §8.

The two newest — `20261101151838_SyncSchemaAndAccountLifecycle` and
`20261101152359_ReviewSelfClaimedStaffAccounts` — were scaffolded rather than written, so
they do have Designer files and the hand-written count has not grown. §8.1's migration is
the first one built the way this paragraph has always told people to build it: scaffolded,
then stripped of the phantom `CREATE TABLE` lines the stale snapshot invents, then read
line by line before it shipped.

What the nine do cost is the snapshot drift behind all of this: a hand-written migration
updates no snapshot, so `AppDbContextModelSnapshot.cs` describes a database that does not
exist. Write new migrations by hand the way those nine were, or scaffold one and delete
the `CREATE TABLE` lines for the four tables the snapshot has never seen — and check
every remaining line, because the differ's other blind spot is a table that is *in* the
snapshot but was never emitted by any migration, which is exactly how `TicketActivity`
and `DeliveryStatusUpdate` went missing for a whole release.

### 8.3 The catalogue seed

`Database/seed-catalogue.sql` is the 61-line public catalogue behind `/gallery`,
the client portal and the procurement list. It is separate from the schema chain
on purpose — migrations change the shape of `Products`, this changes its contents
— and it is upsert-by-SKU, so it is re-runnable.

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -d VerichatCRM -b -i Database\seed-catalogue.sql
```

Applied and verified against the local database on 2026-11-01: the first run
re-filed six legacy categories and upserted 57 rows to a total of 61 (40 clinical,
14 education and research, 7 mining and industrial); the second run reported
`0 rows touched`, and a price and stock quantity deliberately changed on
`VC-CN-101` between the two runs survived both. That is the property the script
is designed around: a matched row has its name and category reconciled, but never
its price, count or threshold, because those are live data once the warehouse is
transacting and a re-seed must not reset them.

**Read the header before running it anywhere real.** The rows were written from
the sectors the company publishes, not from a stock count, and the sales desk has
not signed them off. `Price` is indicative and the schema has no currency column
while the staff views print `R`, so these numbers are not a price list yet.

Two things the script assumes, both worth checking on a database you did not
create yourself:

- **Categories are validated, not free text.** `CreateProductRequestValidator`
  accepts only the eight values in `ProductCategories`, and the vocabulary grew
  from five to eight in this pass — `Clinical Consumables`, `Mining & Industrial`
  and `Teaching & Simulation` were added because the company sells into all three
  and had no name for them. Rows written under the old `Clinical` / `Rehab` /
  `Education` labels would have been unsaveable from the product form, so step 1
  re-files them. The script ends by failing loudly if any row is still outside the
  list; adding a ninth category means code, seed and pick-list together.
- **`ImageUrl` stays NULL.** Product photographs are found by filename —
  `wwwroot/images/catalogue/products/<product name, slugged>.jpg`, with the
  category image standing in — rather than stored in the row. An absolute URL in
  the database would only be true on the host it was written for, and the mobile
  app loads `ImageUrl` directly.

### 8.4 After the migration: release the staff accounts the review swept up

`20261101152359_ReviewSelfClaimedStaffAccounts` runs one statement:

```sql
UPDATE [Users] SET [ApprovalStatus] = N'Pending'
WHERE [Role] IN (N'Sales Representative', N'Procurement Manager',
                 N'Field Support Engineer', N'Logistics');
```

It has no `WHERE` on the current status, so it pends **every account in those four roles that exists at the moment it runs**, including the ones an administrator created for real employees and the ones your own staff registered years ago. That is the design, and the comment above the migration says the cost out loud: nothing in the `Users` row records who created the account, so the sweep cannot tell a genuine employee from someone who typed a staff role into the sign-up form.

**The consequence is that the morning after a migration, your staff cannot sign in.** [AuthService.cs:45](VerichatCRM.API/Services/AuthService.cs#L45) refuses a `Pending` account before issuing a token, and [AccountController.cs:52](VerichatCRM.Web/Controllers/AccountController.cs#L52) sends the right password to the `Account/Review` screen instead of a dashboard. Web and mobile both read the same flag, so the same person is locked out of both. The message is honest — *"Your account is created and waiting for a Verichat employee to approve it"* — but to anyone who has not read this section it is indistinguishable from an outage. Section 5 of `Database/preflight-migration-checks.sql` counts these accounts before you migrate; write the number down.

Do this, in this order:

1. **Before migrating**, sign in to the deployment with a **System Administrator** account and confirm it works. `System Administrator`, `Client User` and `Medical Student` are outside the sweep — the admin role because the sign-up form never offers it, the other two because they reach nothing but their own portal.
2. **Migrate.** The order in §8's numbered runbook: §8.2's cleanup, then §8.1's pre-flight, then the chain, then §8.3's catalogue.
3. **Sign in as that System Administrator** and open `/Admin/Approvals`. Three roles hold `Perm.AccountApprovals` — System Administrator, Sales Representative and Procurement Manager ([Perm.cs:82](VerichatCRM.Web/Authorization/Perm.cs#L82)) — but the latter two are themselves `Pending` after step 2, so **the administrator is the only account that can work the queue.** A `Pending` administrator would have no way back in: `AccountApprovalService.ApproveAsync` refuses self-review, so the sweep leaving that role alone is load-bearing, not an oversight.
4. **Approve the accounts you know, one at a time.** There is no bulk approve, deliberately — it is the same yes-or-no decision as a normal application, and a queue of forty is a sign the sweep ran against a database it should not have. A reviewer cannot change the role while approving: the applicant stays in the role they asked for.
5. Tell the staff who signed themselves up what happened. Anyone who cannot get in has `/Account/Help` and the help button on both the sign-in and the review screen, and their notes land next to the queue at `/Admin/HelpRequests`.

If the queue genuinely cannot be worked — a demonstration, a migration onto a database whose staff are all already vetted — this is the escape hatch, and it is exactly the migration's own `Down()`:

```sql
UPDATE [Users] SET [ApprovalStatus] = N'Approved'
WHERE [Role] IN (N'Sales Representative', N'Procurement Manager',
                 N'Field Support Engineer', N'Logistics')
  AND [ApprovalStatus] = N'Pending'
  AND [ReviewedByUserId] IS NULL;
```

`ReviewedByUserId` is only ever written by the queue, so this releases the accounts the sweep caught and leaves anything a reviewer has since decided by hand — including a rejection — exactly as they left it. Running it undoes the protection this feature exists to provide, so run it to unblock a room, then work the queue properly.

Verified end to end on the local deployment, which went through both migrations on
2026-11-01:

- the pre-flight script counted **12** staff accounts in the four swept roles (3 Sales
  Representative, 5 Procurement Manager, 2 Field Support Engineer, 2 Logistics); after
  the migrations ran, 6 of them sat `Pending` and 6 `Approved` — the pending half being
  the accounts that existed before the sweep, the approved half the per-role
  demonstration accounts `DemoUserSeeder` creates after it with `ApprovalStatus` set
  explicitly in code;
- a fresh Sales Representative registered through `/Account/Register` and signed in with
  the **right** password: held on the review screen, with the "waiting for a Verichat
  employee" wording, no session issued;
- the `admin@verichat.test.local` System Administrator reached `/Admin/Dashboard`, opened
  `/Admin/Approvals`, and found 7 applicants — the 6 above plus the new one;
- its Approve button posted `/Admin/ApproveAccount`, returned to the queue, and the
  account was no longer listed;
- the held account's same credentials then signed straight in and landed on `/Pipeline`,
  the Sales Representative board.

So the procedure in this section is the one that was actually walked, not a reconstruction
of it. The escape-hatch `UPDATE` was not run.

## 9. Running the EF commands from Visual Studio

Two things go wrong before a single `UPDATE` reaches a database, and both fail in ways that
look like the tool misbehaving rather than the tool being pointed somewhere you did not
choose.

**The Package Manager Console had four candidate owners of `AppDbContext`.**
`Update-Database -Project VerichatCRM.Infrastructure -StartupProject VerichatCRM.API` failed
with *"More than one DbContext named 'AppDbContext' was found. Specify which one by providing
its full type name or its project."* `Microsoft.EntityFrameworkCore.Tools` was referenced by
every project in the solution — API, Core, Infrastructure and Web — and each of them can
resolve `VerichatCRM.Infrastructure.Data.AppDbContext`, since three of the four reference the
project that defines it, so nothing in the repository says which project owns the command. The
same pair of projects run from the CLI (`dotnet ef migrations list --project
VerichatCRM.Infrastructure --startup-project VerichatCRM.API`) finds the context once and
without complaint, which is why the console's project selection is the layer worth fixing
rather than the code. `Tools` is now referenced by `VerichatCRM.API` alone — EF's own rule is
one project, the startup — so set **Default project to `VerichatCRM.API`** before using any of
these cmdlets. If a context is ever ambiguous again, name it in full:
`-Context VerichatCRM.Infrastructure.Data.AppDbContext`. All four references were
`PrivateAssets="all"`, so this is design-time only and nothing at runtime can notice —
verified: Release build clean with 0 warnings, `dotnet ef database update --project
VerichatCRM.Infrastructure --startup-project VerichatCRM.API` still resolves the context and
the migrations assembly, and all 141 tests pass.

**Migration commands do not inherit the Development connection string.** The design-time
factory at `VerichatCRM.API/DesignTimeAppDbContextFactory.cs` is what `Update-Database`
reaches, deliberately, so script generation works without a JWT key. It reads
`appsettings.json` and `appsettings.<Environment>.json`, then user secrets at `:23`, then
environment variables at `:24` — the same order as the running app, with the same consequence:
a `ConnectionStrings:DefaultConnection` left in user secrets from a hosted rehearsal sends the
migration tool to the **production** database rather than to LocalDB. It does not refuse. It
prints *"An error occurred while accessing the database. Continuing without the information
provided by the database"*, lists the migrations with no applied-or-pending column, and if the
host *is* reachable it applies them there. Observed on a laptop that cannot route to that
server: `migrations list` with the correct project pair failed with a SQL network error and
that exact "continuing" line, and could not report pending status at all.

Pin the target, and the ambiguity in the second problem disappears with it:

```powershell
$env:ConnectionStrings__DefaultConnection = 'Server=(localdb)\MSSQLLocalDB;Database=VerichatCRM;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True'
dotnet ef database update --project VerichatCRM.Infrastructure --startup-project VerichatCRM.API
```

**The launch profiles' LocalDB pin does not reach these commands.** Both profiles in
`VerichatCRM.API/Properties/launchSettings.json` now set `ConnectionStrings__DefaultConnection`
to LocalDB, and that is what makes Visual Studio F5 work on a machine with a stale secret — but
re-measured after that change, `dotnet ef migrations list --project VerichatCRM.Infrastructure
--startup-project VerichatCRM.API` with nothing in the shell still answers *"An error occurred
while accessing the database. Continuing without the information provided by the database"*
with no applied-or-pending column. The EF tools apply their own `ASPNETCORE_ENVIRONMENT`
default, which is how the design-time factory ends up in Development, but they do not apply the
profile's other environment variables. So the shell assignment above is the only thing that
decides where a migration command goes — set it in the same line, every time.

Two reasons not to use `Update-Database` against a database you did not create, even with the
connection string pinned. `--idempotent` script generation (§8) is reviewable before anything
executes, and `sqlcmd -U` takes the password interactively, so the credential appears in no
script file, command line or shell history — whereas the design-time factory reads it out of a
per-machine store nobody audited. And the API migrates itself at startup (`Program.cs:266`),
so a development database the API has already booted against is current without any of this:
`__EFMigrationsHistory` on the development box holds 21 applied migrations, latest
`20261101152359_ReviewSelfClaimedStaffAccounts`, which is why the command above answers *"No
migrations were applied. The database is already up to date."*


## 10. Publish fails with "An equivalent project is already present in the project collection"

Visual Studio's Publish dialog stops with:

```
Publish has encountered an error.
An equivalent project (a project with the same global properties and tools version) is
already present in the project collection, with the path
"...\VerichatCRM.API\Properties\PublishProfiles\verichat-api - Web Deploy.pubxml.user".
To load an equivalent into this project collection, unload this project first.
```

**No code was compiled and nothing was deployed when this throws.** The diagnostic log's stack
says so: the throwing frame is
`Microsoft.Publish.Framework.NewFx.Persistence.MsBuildFilePersister.UpdatePropertyAsync`, reached
from `ServerProjectProfilesManager.PersistOperationPropertiesAsync`, and underneath it
`ProjectCollection.OnAfterRenameLoadedProject` → `LoadedProjectCollection.AddProject`. That is the
publish *UI writing the "last used" properties back into the profile's `.user` file*, not the
Web Deploy build. Visual Studio loads each `.pubxml.user` into one solution-wide in-memory
`ProjectCollection`, keyed by the file's path plus its global properties and tools version, and
it does not remove the entry when an earlier publish in the same session aborts. The next publish
tries to add the same identity again and MSBuild refuses, because two projects with one identity
cannot both be loaded.

This repository makes the collision likely rather than rare: all four of the API's profiles and
all three of the web app's declare the same identity, `Release` / `Any CPU`. Any profile persist
for a project can therefore collide with any other profile of that project still sitting in the
collection - and the earlier attempts did abort, because the deployed site answered 500.30 (§7.1).

**Confirming it is session state, before you spend time on anything else.** Compare the age of the
Visual Studio process with the timestamps of the publish logs:

```powershell
(Get-Process devenv).StartTime                      # when this session began
Get-ChildItem $env:TEMP -Filter 'tmp*.tmp' |        # the publish diagnostics
  Where-Object { (Get-Content $_ -TotalCount 1) -match '^20' } |
  Sort-Object LastWriteTime -Descending | Select-Object -First 6 Name,LastWriteTime
```

Read on this machine: `devenv` started 23:50:47, and the *same* exception was logged at 00:31:50,
00:36:31 and 00:37:59 - three attempts inside one unbroken session, each adding to the leak. The
logs from the sessions before it hold different faults entirely (`FormatException` in
`ProjectProfilesManager.RunPublishStepsAsync` at 15:52 and 19:59, `WebToolsException: Build failed`
at 20:39). So this error belongs to a process, not to the repository, and the cure is to empty that
process. Note also that a retry does not recover: once the collection is dirty, every further
publish in that session fails the same way.

Fix it in this order.

1. **Close Visual Studio completely and reopen.** This is the reliable cure and the only thing
   that empties the collection. Check no build nodes survive the close:
   `Get-Process MSBuild,VBCSCompiler,devenv -ErrorAction SilentlyContinue`.
2. **Unload then reload `VerichatCRM.API`** in Solution Explorer (right-click → Unload Project,
   then Reload Project) if you cannot close the IDE. It drops that project's entries and is what
   the error text asks for, but the collection is solution-wide, so a stale entry belonging to
   `VerichatCRM.Web` can still collide - which is why it is worth trying, not why it is enough.
3. **Publish from the command line**, which never touches Visual Studio's collection:

   ```powershell
   dotnet publish VerichatCRM.API -c Release -o publish-api    # packages, contacts nobody
   ```
4. Optionally delete the profiles for hosts you no longer use - `site94518-WebDeploy`,
   `site103828-WebDeploy`, `FTPProfile` are leftovers from earlier hosting attempts. Fewer
   profiles sharing one identity means fewer things that can collide. They are the only untracked
   copies of those hosts' settings, so confirm you do not still need them first.

Never delete a `.pubxml.user` file to fix this. It holds the publish credential, encrypted with
the Windows user's DPAPI key; removing it does not fix the collection - which is in memory, not
on disk - and it destroys a secret that cannot be recovered except by downloading a fresh
publish settings file from the hosting panel. The `.user` files are ignored by `.gitignore:9`
(`*.user`) and are not tracked, which is why they do not appear in `git status`.

Step 3 is verified: `dotnet publish VerichatCRM.API -c Release -o <folder>` completes and produces
64 files including `VerichatCRM.API.dll`, `VerichatCRM.Core.dll`,
`VerichatCRM.Infrastructure.dll`, `appsettings.json` and the `web.config` that §7.1 says the panel
needs. The project and its publish targets are sound, which is what makes this a Visual Studio
session-state fault rather than a repository defect. Note that publishing to a *folder* only
packages the output; `--profile` with an MSDeploy profile contacts the live host and overwrites
what is deployed there, so run that deliberately and only when the deployment is the goal.
