# Commit plan — 55 commits, one concern each

Working artifact for the Task 3 close-out. **Not a submission deliverable** — delete it once the commits exist, or keep it as the change log the Final Report cites.

Derived from `git status --porcelain` + `git diff HEAD` against `dev2` at `b61c567`: **143 changed paths, ~5,300 insertions, ~5,900 deletions**. Nothing in this plan is committed yet, and no credential value is reproduced here.

## How to use it

- The heading after the number is the **commit subject**; the fenced block is the **commit body**. Copy both: `git commit -m "<subject>" -m "<body>"`, or put the body in a file and use `-F`. Every subject is under 72 characters and in Conventional Commits form, so `git log --oneline` reads as a feature list.
- Ordered so dependencies land first (hygiene → API security → Core → Infrastructure → API features → Web → mobile → dead code → docs), so `dotnet build` and `dotnet test` stay green at each step except where noted below.
- Each entry ends with a `Stage:` line. Most files go in whole. **Files carrying changes for two different commits are marked `(hunk: …)` — stage them with `git add -p <path>` and pick only the described hunks.** Shared files: `VerichatCRM.API/Program.cs` (commits 5, 6, 7, 14, 15), `AccountController.cs` (7, 46), `PipelineController.cs` (40, 41), API `QuotationController.cs` (20, 21), Web `QuotationController.cs` (39, 40), `Views/Shared/_Layout.cshtml` (40, 43), `Views/Pipeline/Index.cshtml` (40, 43), the Infrastructure configuration classes (26, 27).
- Three pairs must land together or the tree is briefly inconsistent: **21 + 39** (API resolves the deal the web client stopped sending as null), **25 + 30 + 31** (duplicate model, migration, snapshot), **14 + 16** (sanitising middleware and the validator rules it makes redundant).
- Do not use `git add -A`: it mixes concerns, re-adds the scratch files, and would stage the 143 paths into whichever commit is checked out at the time.

---

## Repository hygiene and CI

### 1. `chore(repo): ignore secrets, scratch output and machine-local state`

```
Two halves of the same tidy-up.

The tree had no rule keeping .env, secrets.json or appsettings.Production.json
out of a commit, and the upstream Visual Studio template's `*.env` pattern does
not match the dotfile `.env` at all — so the file most likely to hold a live
credential was ignored only by accident. Adds .env/.env.*, secrets.json,
appsettings.Production.json, archive formats, tmp/, build-out.txt, *_build.log,
*.bak and *.orig, and untracks both projects' .csproj.user files, which recorded
the last publish profile path and dialog widths on one developer's machine and
churned a diff for everyone else who opened the solution.

Removes two artifacts that were never product: build-out.txt, two lines of
MSBuild output from a failed `dotnet build VerichatCRM.sln` (the solution is
VerichatCRM.slnx), and tmp/pdfs/task1.txt, a 1,373-line text dump of the Task 1
brief that is the source material for documentation.md rather than code.
```

Stage: `.gitignore`, `build-out.txt`, `tmp/pdfs/task1.txt` · `git rm --cached VerichatCRM.API/VerichatCRM.API.csproj.user VerichatCRM.Web/VerichatCRM.Web.csproj.user` (the template's `*.user` rule at line 9 already covers them; they are tracked only because they were committed before it)

### 2. `chore(api): retire the standalone database initialiser`

```
The console app built its own DI container, registered AppDbContext on a
connection string it also printed in a startup banner, resolved
IDatabaseInitializer and ran InitializeAsync().

The API now migrates, seeds and bootstraps the first administrator at startup by
itself, so this was a second path to the same schema that CI never exercised and
that could quietly drift — plus a second place the hosted connection string sat
in plaintext. Its absence is also why "how do I create the database" now has one
answer instead of two competing ones.
```

Stage: `VerichatCRM.DatabaseInitializer/Program.cs`

### 3. `ci: restore the .NET and Expo validation pipeline`

```
Two jobs, on every push to main/master and every pull request:

  dotnet  — setup-dotnet 10.0.x, restore VerichatCRM.slnx, dotnet test in
            Release, dotnet build of the solution.
  mobile  — node 22 with an npm cache keyed on VerichatMobile/package-lock.json,
            npm ci, npx expo lint, npx tsc --noEmit.

The dotnet job compiles only once commit 35 is in, which repairs the test
project's constructor call; before that, expect it to fail on the test project
and not on your change.
```

Stage: `.github/workflows/ci.yml`

---

## Secrets, configuration and startup guards

### 4. `fix(api): remove the committed production database credential`

```
VerichatCRM.API/appsettings.json carried the live SQL Server password for the
hosted database, with Encrypt=False, across seven revisions. The value is not
reproduced here and is deliberately never written into another tracked file.

ConnectionStrings:DefaultConnection is blanked and the deployed value comes from
ConnectionStrings__DefaultConnection or the hosting environment's own settings.
Adds <UserSecretsId> so a developer can hold Jwt:Key and a local connection
string without touching a tracked file at all — the mechanism is what makes this
change comfortable to run on a fresh clone rather than a burden.

Also fills Cors:AllowedOrigins with the deployed web origin (an empty list
silently blocks every browser call) and adds the //ConnectionStrings, //Jwt and
//Cors documentation keys so the next reader knows what belongs in this file and
what never should.

Rotation and history rewriting are operator actions, in DEPLOYMENT.md §1 and §6:
deleting the string from the file does not un-expose it.
```

Stage: `VerichatCRM.API/appsettings.json`, `VerichatCRM.API/VerichatCRM.API.csproj`

### 5. `fix(api): refuse to start production with an unusable configuration`

```
Three guards, each throwing with a message that names the key to set instead of
an exception nobody can act on:

  - ConnectionStrings:DefaultConnection missing;
  - a connection string that disables encryption outside Development
    (RequestsNoEncryption parses both Encrypt=False and Encrypt=no);
  - Jwt:Key blank, which would otherwise mint tokens nobody can verify.

Migration and seeding failures are caught so an unreachable database degrades to
a diagnosable start rather than an HTTP 500.30 on the first request — the
difference between a support ticket that says what is wrong and one that says
"it's down". Also deletes the informal note left in the connection-string guard
comment.
```

Stage: `VerichatCRM.API/Program.cs` *(hunk: startup guards + comment deletion)*

### 6. `fix(api): stop Development CORS from discarding configured origins`

```
The Development branch replaced Cors:AllowedOrigins with a localhost pair, so a
local web app pointed at the deployed API was blocked by its own configuration.
It now concatenates the localhost origins onto whatever is configured, and
production logs a warning when the list is empty.

Separately, two app.MapGet("/health") registrations made /health throw
AmbiguousMatchException while /api/health — the route the web app, the mobile app
and the deployment checks all use — kept working. The duplicate is removed rather
than the broken one fixed, so there is exactly one health contract.
```

Stage: `VerichatCRM.API/Program.cs` *(hunk: CORS + duplicate /health)*

---

## Token handling and SignalR authorization

### 7. `security(api): resolve the hub token from an HttpOnly cookie`

```
SignalR's WebSocket and negotiate requests cannot carry an Authorization header
written by page script, so the JWT used to be handed to JavaScript on every staff
page — readable by any injected script, and by anyone looking over a shoulder in a
shared office.

AccountController now issues the JwtToken cookie with HttpOnly=true, SameSite=None
plus Secure when the hub is cross-origin, and deletes it on logout. The hub's
OnMessageReceived accepts that cookie, or an access_token query value, on every
/hubs path — it previously matched one literal path and silently fell back to
anonymous for the rest — and maps the JWT claims onto Context.User so anything
authorizing inside the hub has a user to work with.
```

Stage: `VerichatCRM.API/Program.cs` *(hunk: OnMessageReceived)* · `VerichatCRM.Web/Controllers/AccountController.cs` *(hunk: IssueHubCookie, Logout)*

### 8. `fix(api): authorize SignalR group subscriptions by claim`

```
Subscribe took any group string a client typed. A signed-in Client User could join
the "System Administrator" group and receive staff notifications, including
escalations that name another practice.

user:<id> groups are now limited to the caller's own NameIdentifier / `sub` claim
and role groups require Context.User.IsInRole(group); anything else throws
HubException, so a client that asks for a feed it cannot have fails visibly
instead of listening quietly.
```

Stage: `VerichatCRM.API/Hubs/NotificationHub.cs`

---

## Credential storage

### 9. `security(api): hash passwords with PBKDF2-SHA256 and per-user salts`

```
The replacement scheme was unsalted SHA-256 with one hard-coded pepper: a single
round, a salt shared by every account, and an offline crack of the whole table
from one leaked constant.

Now PBKDF2-SHA256 with a 16-byte random salt per user, 32-byte output and 210 000
iterations, stored self-describing as `pbkdf2-sha256$iterations$salt$hash`. Verify
compares with CryptographicOperations.FixedTimeEquals, falls back to VerifyLegacy
for rows written before this change, and NeedsUpgrade reports which rows are still
legacy — so the format can move again without another data migration.
```

Stage: `VerichatCRM.API/Helpers/PasswordHasher.cs`

### 10. `feat(api): retire legacy hashes on the next successful sign-in`

```
A migration cannot re-hash what it cannot reverse, and forcing every account
through a password reset would be a hostile first day. AuthService re-hashers with
the current scheme whenever NeedsUpgrade is true, so each legacy row disappears the
moment its owner signs in successfully, and the population of legacy rows shrinks
on its own without a flag day.
```

Stage: `VerichatCRM.API/Services/AuthService.cs`

---

## Roles, audiences and shared vocabulary

### 11. `feat(core): name the roles that belong to the client audience`

```
UserRoles adds the MedicalStudent role and the ClientAudience array (ClientUser,
MedicalStudent). Three places — the API's authorization attributes, the web
registration page and the mobile sign-in chooser — each carried their own copy of
"which roles are customers", and they disagreed: one accepted a single name, another
accepted two, and the third accepted a different pair.
```

Stage: `VerichatCRM.Core/Entities/UserRoles.cs`

### 12. `fix(mobile): stop the audience gate locking out two working roles`

```
STAFF_MOBILE_ROLES did not include "Logistics", so a Logistician was refused by the
staff flow and by the client flow as well, and could not enter the app at all from
the account the web system gave them. The client-audience check only accepted
"Client User", so a Medical Student could register and then be rejected at sign-in.

Both now follow UserRoles.ClientAudience's shape (commit 11), and the staff failure
message is derived from STAFF_MOBILE_ROLES rather than a hand-typed list — so the
error a user reads cannot drift from the roles that actually work.
```

Stage: `VerichatMobile/src/constants/api.ts`, `VerichatMobile/src/constants/authService.ts`

### 13. `feat(core): give the product catalogue one category vocabulary`

```
ProductCategories: PPE, Laboratory, Hospital Infrastructure, Home Care, Infant Care,
with All and a case-insensitive IsValid. The five strings were duplicated across the
API validator, the web pick-list and the seed data, which is how a product could be
saved under a category nothing filtered on and then vanish from the catalogue chips
while still being ordered.
```

Stage: `VerichatCRM.Core/Entities/ProductCategories.cs`

---

## Validation and request sanitisation

### 14. `security(api): sanitise JSON request bodies before model binding`

```
New RequestBodySanitisationMiddleware, registered after authentication and before
MapControllers, runs an HtmlSanitizer with every allowed tag, attribute, CSS scheme
and protocol removed and KeepChildNodes on, then rewires the stream so binding sees
the cleaned value. Free text is where the exposure actually is — ticket
descriptions, enquiry notes, product names — and all of it arrives as JSON.

The guard rails are the substance: 512 KB body limit, JSON depth 8, values
containing no "<" passed through untouched, credential-shaped properties (Password,
PasswordHash, Token, AccessToken, RefreshToken, Secret, Signature, DeviceToken,
ApiKey) never rewritten, and a bare [FromBody] string handled instead of skipped.

Deletes XssSanitizationMiddleware, which buffered the request body and then
sanitised nothing — a stub that made the pipeline look protected while providing no
protection.
```

Stage: `VerichatCRM.API/Middleware/RequestBodySanitisationMiddleware.cs`, delete `VerichatCRM.API/Middleware/XssSanitizationMiddleware.cs`, `VerichatCRM.API/Program.cs` *(hunk: UseMiddleware)*

### 15. `fix(api): register FluentValidation so the validators run at all`

```
The validator classes were written and were mostly correct, but
AddValidatorsFromAssembly and AddFluentValidationAutoValidation were never called,
so nothing executed them. Invalid requests reached the services and surfaced as an
HTTP 500 from a database constraint instead of a 400 carrying field errors — which
is why a mistyped VAT rate looked like a server fault to the person who made it.
```

Stage: `VerichatCRM.API/Program.cs` *(hunk: FluentValidation registration)*

### 16. `feat(api): validate quotations and product updates against the columns`

```
- VatRate must be a fraction: the column is decimal(5,4), so the natural-looking
  "15" overflowed and 500-ed every quotation save. Now >= 0 and < 1, with a message
  that says what to type.
- Terms capped at 4000 to match the column rather than discover the constraint.
- Quotation line items require an SKU and a chargeable quantity.
- New UpdateProductRequestValidator with null-conditional rules, so a partial edit
  is not rejected for fields it did not touch.
- Category validated by ProductCategories.IsValid (commit 13) instead of an inline
  array that had already fallen out of step with the seed data.
- ImageUrl capped at 500 and required to be absolute http(s).
- The angle-bracket rules on ProductName and SupplierName removed: the sanitising
  middleware runs before binding (commit 14), so a "<" reaching a validator is
  prose, and the old rule rejected legitimate product names for the wrong reason.
- CreateOrderRequest.ClientId NotEmpty removed — that value is only known after
  the JWT is bound, so the check belongs in the controller (commit 17).
```

Stage: `VerichatCRM.API/Validators/CreateProductRequestValidator.cs`

### 17. `fix(api): reject an order with no client instead of writing an orphan`

```
CreateOrder returns 400 "Client ID is required." before the service is called. The
check lives in the controller and not in CreateOrderRequestValidator because
validators run during model binding, before the client id derived from the JWT has
been assigned — a rule in the validator would reject valid requests. The comment in
the file records that, so the next person does not tidy it back into the wrong place.
```

Stage: `VerichatCRM.API/Controllers/OrderController.cs`

---

## Resource-level authorization

### 18. `fix(api): return delivery data only to the client that owns it`

```
GetByClient and Track gain a CanAccessClient guard. SysAdmin, Procurement Manager,
Logistics, Field Support Engineer and Sales Representative pass; Client User and
Medical Student only for their own id.

A tracking number is deliberately not treated as a capability: it is short,
guessable, printed on paperwork, and the response it unlocked disclosed the
destination address.
```

Stage: `VerichatCRM.API/Controllers/DeliveryController.cs`

### 19. `fix(api): stop ticket detail crossing client boundaries`

```
/api/supportticket/{id} returned any ticket to any authenticated caller. A client
that guessed an id could read another practice's fault description, the photographs
attached to it and the asset serial number. GetById now applies the same ownership
rule as deliveries.
```

Stage: `VerichatCRM.API/Controllers/SupportTicketController.cs`

### 20. `fix(api): let both client audiences request a quote`

```
RequestQuote's [Authorize(Roles)] named only "Client User". A Medical Student is
accepted by the mobile account chooser, can browse the catalogue and fill a cart,
and then hit 401 at the final action. The endpoint now names both roles from the
audience list the rest of the system uses.
```

Stage: `VerichatCRM.API/Controllers/QuotationController.cs` *(hunk: RequestQuote)*

### 21. `fix(api): attach a quotation to a deal instead of failing the FK`

```
DealId is a required non-nullable foreign key and the web client sent null, so
saving a quotation died on the constraint and nothing was written. Create now
validates before touching the database (blank client id → 400, no line items → 400,
unknown client → 404) and resolves the deal: reuse the client's most recent one by
CreatedAt, otherwise open an "Initial Inquiry" deal valued at the line subtotal.

This is also what makes the Pipeline Board usable rather than empty: deal cards are
created by saving a quotation or by a mobile quote request, so making the FK
satisfiable is what turns the board into a working screen instead of a blank one
with a legend.
```

Stage: `VerichatCRM.API/Controllers/QuotationController.cs` *(hunk: Create + deal resolution)*

### 22. `fix(api): create the client row when an account joins the audience`

```
Orders, quotations and enquiries all key on Clients.ClientId. An account created as
staff and later re-roled to Client User had no Clients row, so it could sign in to
the mobile app and then fail on every write with an error that named neither the
client nor the cause.

UpdateRole takes IMobileService via [FromServices] and calls
EnsureClientAccountAsync whenever the new role is in UserRoles.ClientAudience.
```

Stage: `VerichatCRM.API/Controllers/UsersController.cs`

---


## Enquiry scoring: the model

### 23. `feat(core): store the enquiry's urgency and estimated value`

```
Inquiry gains Urgency (string?) and EstimatedValue (decimal?) — the two inputs the
qualification score is derived from, and which the entity previously lacked, so
scoring had nothing to work with but a hand-typed guess. The Score doc comment now
records the rule that matters when support asks: computed at capture when absent,
editable afterwards, and never recalculated once a human has set it.
```

Stage: `VerichatCRM.Core/Entities/Inquiry.cs`

### 24. `feat(core): encode the enquiry qualification score`

```
InquiryScoring weights sector 25, contract status 20, rand value 35 and urgency 20,
banded (value thresholds at R500k / R150k / R50k / R10k) and clamped to 0–100.

Two deliberate judgement calls, stated in the file:
  - unrecognised values earn mid-band credit, so a sparsely recorded enquiry sits in
    the middle of the queue instead of at the bottom, where nobody would open it;
  - Active > Prospect > Inactive, because a lapsed account has trust to win back
    that a cold lead does not.

This replaces the constant 50 the API used to default into every new record, which
made the whole queue indistinguishable and gave representatives no ordering to work.
```

Stage: `VerichatCRM.Core/Models/Inquiry/InquiryScoring.cs`

### 25. `fix(core): remove the duplicate SalesGoal shape left by the merge`

```
The 2026-10-30 merge left a second SalesGoal class and SalesGoalTypes enum in the
same file — int GoalId, TargetAmount, Type, Start/EndDate. Nothing mapped to it, but
the EF snapshot carried it, and that mismatch is exactly what made scaffolding a
migration after the merge try to drop and recreate tables. The Guid-Id, Title,
Target, Achieved, Period shape is the one that survives.
```

Stage: `VerichatCRM.Core/Entities/SalesGoal.cs`

---

## Persistence and migrations

### 26. `refactor(infra): let the configuration classes own entity mapping`

```
ApplyConfigurationsFromAssembly now runs before the inline entity blocks, with the
ordering rule written down: last call wins, so relationships and delete behaviour
stay in the context while lengths, precision and indexes live in the configurations.
Before this the split was decided differently per entity, and the same property was
configured twice with two different results.

OwnsMany() dropped for ServiceLogs, KeyPersonnel, DeliveryStatusUpdate,
TicketActivity and QuotationLineItem: those are independent tables with their own
keys, and mapping them as owned collections exposed the same rows through two paths
and made deletes unreliable.

Also: removes the PhotoUrls semicolon value converter because deployed rows hold JSON
arrays; adds Address max length 500, Deal Notes 4000, Quotation Status and Terms
4000 with an index on ClientId, Ticket ResolutionNotes 4000; and renames
"ClientConfiguration .cs" to ClientConfiguration.cs, the trailing space having
already broken every tool that globs filenames.
```

Stage: `VerichatCRM.Infrastructure/Data/AppDbContext.cs`, `Data/Configurations/{Asset,Client,Deal,DeliveryLog,Quotation,Ticket}Configuration.cs`

### 27. `fix(infra): filter unique indexes so blank alternates cannot collide`

```
SerialNumber, TrackingNumber and QuoteReference each gain a `<> N''` filter.
Uniqueness is right for a real reference, but an empty string is what is stored when
a field is legitimately not known yet — so the second delivery saved without a
tracking number violated a constraint the first one already occupied, and neither
record was named in the failure.
```

Stage: `Data/Configurations/{Asset,DeliveryLog,Quotation,Ticket}Configuration.cs` *(hunk: filtered index calls)*

### 28. `feat(infra): map the enquiry scoring inputs`

```
New InquiryConfiguration: InquiryId key, Urgency nvarchar(30), EstimatedValue
decimal(18,2). Status is narrowed from nvarchar(max) to nvarchar(30) because SQL
Server cannot index an (max) column at all, and the queue's ordinary read is "open
enquiries, best first" — the composite (Status, Score) index turns that from a scan
into a seek as the table grows.
```

Stage: `VerichatCRM.Infrastructure/Data/Configurations/InquiryConfiguration.cs`

### 29. `fix(database): restore the attributes that left HealthMetrics unmigrated`

```
The hand-written AddHealthMetricTable migration lacked
[DbContext(typeof(AppDbContext))] and [Migration("20261023000000_…")], so EF's
attribute scan never collected it. dbo.HealthMetrics was never created on any
environment built from migrations, and POST /api/healthmetrics failed there while
working on machines whose database had been produced by the older initialiser — the
classic shape of a bug that only ever appears on the deployed system.
```

Stage: `VerichatCRM.Infrastructure/Migrations/20261023000000_AddHealthMetricTable.cs`

### 30. `feat(database): add the enquiry scoring columns and the queue index`

```
Narrows Inquiries.Status to nvarchar(30), adds Urgency nvarchar(30) null and
EstimatedValue decimal(18,2) null, creates IX_Inquiries_Status_Score, and provides a
complete Down.

Written by hand rather than scaffolded: scaffolding after the 2026-10-31 merge
collided with the duplicate SalesGoal entity (commit 25) and a snapshot that no
longer described the model (commit 31). The migration's remarks say so and point at
DEPLOYMENT.md §8 for the pre-flight checks to run before applying this to a database
that already holds rows — narrowing a column that contains longer text is the one
step here that can fail on live data.
```

Stage: `VerichatCRM.Infrastructure/Migrations/20261031160000_AddInquiryScoringInputs.cs`

### 31. `chore(database): realign the model snapshot with the mapped model`

```
Adds the two scoring columns and the composite index, drops the duplicate SalesGoal
entity and its User navigation, removes the Appointment, AuditLog, ClientTimelineNote
and HealthMetric blocks the snapshot no longer matched, and normalises the ToTable
calls to the overload EF 10.0.12 writes. 56 entity blocks to 52.

Land this with commits 26–30: a snapshot that disagrees with the configurations is
what makes the next scaffolded migration drop real tables with complete confidence.
```

Stage: `VerichatCRM.Infrastructure/Migrations/AppDbContextModelSnapshot.cs`

### 32. `refactor(infra): drop null checks the compiler already settles`

```
The `eventData is not null` guards in AuditSaveChangesInterceptor's
SavedChanges/SavedChangesAsync were unreachable — only Context is nullable on the
event data, and that is already handled where it is read.
```

Stage: `VerichatCRM.Infrastructure/Data/Interceptors/AuditSaveChangesInterceptor.cs`

---

## Enquiry scoring: service, tests and form

### 33. `feat(api): score an enquiry when the field is left blank`

```
CreateInquiryRequest and UpdateInquiryRequest add Urgency (max 30) and
EstimatedValue, and Score becomes int? instead of `int = 50`, so "not stated" is
representable and stops being silently recorded as a mediocre guess.

InquiryService calls InquiryScoring.Calculate(sector, contractStatus, value,
urgency) when no score is supplied. An explicit score always wins and is then never
recalculated — a representative who overrode the machine must not lose that
judgement the next time someone edits the record. On update the score is recomputed
only when one of the four factors actually changed (factorsChanged). KnownUrgency
maps unrecognised strings to null so they behave as not-stated, and
LatestDealValueAsync falls back to the client's most recent Deal.Revenue so a known
account is scored on real money instead of an empty field.
```

Stage: `VerichatCRM.API/Services/InquiryService.cs`, `DTOs/Requests/CreateInquiryRequest.cs`, `DTOs/Requests/UpdateInquiryRequest.cs`, `DTOs/Responses/InquiryResponse.cs`

### 34. `test(core): pin the scoring bands and the quotation validator rules`

```
Fourteen tests (13 facts, 1 theory) over behaviour that is otherwise invisible until
someone complains about queue order:

  - the maximum is exactly 100, and nothing-stated scores 46 — mid-queue;
  - a larger enquiry outranks a smaller one, a more urgent outranks a less urgent one;
  - unrecognised urgency behaves exactly as not-stated;
  - Active > Prospect > Inactive;
  - sector matching is case-insensitive against stored casing, and an unknown sector
    is not worthless.

Plus the rules from commit 16: fractional VAT accepted, 15 rejected with the
corrective message, negative rejected, Terms longer than the column rejected, absent
Terms rejected against a non-null column, and a line item needing both SKU and
chargeable quantity.

That is 71 tests across 7 files, up from the 11 the README claimed — the correction
is in commit 55.
```

Stage: `VerichatCRM.Tests/InquiryScoringTests.cs`

### 35. `test(api): build the quotation controller the way the application does`

```
ClientOwnershipControllerTests updated for the constructor introduced by commit 21:
an in-memory AppDbContext factory, a null! hub with a comment explaining that a
read-only ownership check emits no notification, and
NullLogger<QuotationController>. Without this the test project does not compile,
which is what keeps commit 3's dotnet job from ever going green.
```

Stage: `VerichatCRM.Tests/ClientOwnershipControllerTests.cs`

### 36. `feat(web): capture urgency and estimated value on the enquiry form`

```
InquiryController feeds its pick-lists from InquiryStatuses.All, InquirySources.All
and the new InquiryUrgencies.All instead of arrays typed into the action, so a status
added to the model can no longer be forgotten on the form. Urgency and EstimatedValue
flow through the view model and InquiryApiService on both create and edit; the form
gains the urgency select and the decimal value field.

The score input's placeholder now reads "leave blank to score automatically". That
one string is the feature: the scoring rule is worthless unless the person entering
the enquiry knows to leave the box empty.
```

Stage: `VerichatCRM.Web/Controllers/InquiryController.cs`, `Models/Inquiry/InquiryViewModel.cs`, `Services/InquiryApiService.cs`, `Views/Inquiry/Form.cshtml`

---

## Web front end: binding, errors and the two money workflows

### 37. `fix(web): bind decimal input with the invariant culture`

```
New InvariantDecimalModelBinderProvider, registered at index 0 of
ModelBinderProviders. On a machine set to en-ZA the default binder read "2400.00"
against a comma decimal separator and bound it as 0 — so an estimated value of R2 400
quietly became R0, and the score derived from it was wrong in a way that is invisible
on screen.

Applies to decimal and double?, accepts only the invariant form, and rejects
"1.234,5" with a model error instead of guessing which convention the operator meant.
```

Stage: `VerichatCRM.Web/ModelBinding/InvariantDecimalModelBinderProvider.cs`, `VerichatCRM.Web/Program.cs`

### 38. `feat(web): tell the operator why the API refused a save`

```
ApiClient gains LastError, populated by MessageFrom(), which reads the
FluentValidation `errors[0]` shape first and then message / detail / title — so a 400
from the validator work in commit 16 arrives on screen as the sentence the validator
wrote, not as a generic failure. ReadAsync became an instance method so it can
populate it. GetFileAsync returns an ApiFile (content, content type, file name), which
is the piece the quotation PDF needs.
```

Stage: `VerichatCRM.Web/Services/ApiClient.cs`, `Services/Interfaces/IApiClient.cs`

### 39. `feat(web): save a quotation and open its generated PDF`

```
The form could compose a quotation but never send it: the PDF button was disabled and
there was no POST at all. Save now posts the line items, and Pdf(id) streams
/api/quotation/{id}/pdf through GetFileAsync, so the document is generated
server-side from the stored reference — the browser never assembles a price that a
client might act on.

Index.cshtml carries the quotation as seed JSON (culture, currency, VAT rate), computes
totals and VAT client-side for display only, renders every amount with the invariant
separator so what is typed is what binds (commit 37), and shows a status line carrying
the API's own reason if the save fails (commit 38). The PDF link is enabled only after
a first successful save. QuotationApiService stops sending DealId=null, which commit 21
now resolves.

Two inherited behaviours worth knowing, both recorded in the user guide: each save
creates a new draft rather than updating the previous one, and saving for a client with
no deal creates that deal card.
```

Stage: `VerichatCRM.Web/Controllers/QuotationController.cs` *(hunk: Save, Pdf)*, `Services/QuotationApiService.cs`, `Views/Quotation/Index.cshtml`

### 40. `fix(web): send an antiforgery token with the JSON posts`

```
The pipeline drag-and-drop posted JSON to MoveDeal with nothing but a cookie session,
which is precisely the case antiforgery protects: any page in another origin's tab
could move a deal by making the victim's browser send the credential.

_Layout emits a request-verification-token meta tag, site.js exposes vcRequestToken()
reading it, and callers set a RequestVerificationToken header. MoveDeal carries
[ValidateAntiForgeryToken], as does the web QuotationController's Save — a POST that
produces a priced document should not be the exception.

_Layout, PipelineController, Web QuotationController and Views/Pipeline/Index are
shared with commits 39, 41 and 43; stage the token hunks here.
```

Stage: `Views/Shared/_Layout.cshtml` *(hunk: meta tag + IAntiforgery)*, `wwwroot/js/site.js`, `Views/Pipeline/Index.cshtml` *(hunk: request header)*, `Controllers/PipelineController.cs` *(hunk: MoveDeal)*, `Controllers/QuotationController.cs` *(hunk: Save attribute)*

### 41. `fix(web): merge the pipeline constructors that left a dependency null`

```
PipelineController had two constructors. Which one the framework resolved depended on
registration order, and one of them left a service field null, so /Pipeline was a
500 that came and went with unrelated DI changes — the least reproducible class of bug
and the most demoralising to demo. Merged into a single constructor taking the
pipeline service, goals and the API client; the antiforgery attribute from commit 40
stays on MoveDeal.
```

Stage: `Controllers/PipelineController.cs` *(hunk: constructor)*

### 42. `feat(web): reorder against a real purchase order record`

```
The Reorder button called an endpoint that created nothing, so the low-stock screen
could not actually reorder. Reorder is now a form POST: [ValidateAntiForgeryToken],
[RequirePermission(Perm.WarehouseAlerts)], a validation pass, a recompute of the
suggested quantity from the live stock figure rather than the copy in the page, then
POST /api/purchaseorder and a TempData confirmation the screen renders as a banner.

Supplier is pre-filled from the last purchase order for that SKU (LastSupplierFor) and
the unit cost from the product price, because a record with no supplier is not useful
to anyone; CanReorder and SuggestedUnitCost on the view model keep the form off rows
already above their reorder point. warehouse-alerts.js loses the client-side reorder
fetch and keeps the stock chart.

Honest scope, and it is written into the guide: this is a record of intent. Nothing is
sent to a supplier, and the alert does not clear itself when the goods arrive.
```

Stage: `Controllers/ProcurementController.cs`, `ViewModels/Procurement/WarehouseAlertsViewModel.cs`, `Views/Procurement/WarehouseAlerts.cshtml`, `wwwroot/js/warehouse-alerts.js`

---

## Web authorization, session hygiene and on-screen accuracy

### 43. `security(web): keep the JWT and the API origin out of page script`

```
Completes commit 7 on the client side. Controllers stopped handing
ViewBag.ApiToken and ViewBag.ApiBaseUrl to the view; _Layout publishes only the API
base URL as window.verichatApiUrl, which is not a secret and was the only legitimate
reason the page needed any of it.

notifications.js and signalr.js stop reading a token out of localStorage and cookies
and rely on the HttpOnly cookie with withCredentials, and reconnection is capped at
five attempts with backoff. Previously it retried every five seconds forever, so an
API restart at lunchtime left every open tab pinning a socket against the hub for the
rest of the afternoon.

Also removes the redundant ApiSettings block from the web appsettings.json: the value
was readable from two keys and only ApiBaseUrl was ever used, which invited exactly
one of them being updated.
```

Stage: `Controllers/TelehealthController.cs`, `Views/Sales/Appointments.cshtml`, `Views/Shared/_Layout.cshtml` *(hunk: verichatApiUrl)*, `Views/Pipeline/Index.cshtml` *(hunk: drop the signalr bootstrap)*, `wwwroot/js/notifications.js`, `wwwroot/js/signalr.js`, `VerichatCRM.Web/appsettings.json`

### 44. `fix(web): gate every sidebar link on the permission its screen demands`

```
Three links were guaranteed 403s for one shared reason: the sidebar consulted a coarse
role boolean where the action consults a permission.

  - Logistics Tracker appeared for Field Support Engineer, who holds support.dashboard
    (which is what put the section on their screen) but not logistics.tracker;
  - Warehouse Alerts appeared for Procurement Manager while the permission was granted
    to nobody but the administrator — so the screen the role exists to work in was
    unreachable from its own navigation;
  - Asset Tracking and Product Catalog keyed off "is admin" / "is sales" rather than
    their own permissions.

Each link now calls Can(Perm.…) with the constant the action requires. Perm.Has accepts
a nullable role because the sidebar renders before anyone signs in and a missing
UserRole means "no permissions", not an error; the avatar block tests empty as well as
null, since Substring on the "" defaults threw on every anonymous page.
```

Stage: `VerichatCRM.Web/Authorization/Perm.cs`, `Views/Shared/_Sidebar.cshtml`

### 45. `fix(web): show an action only where the role can perform it`

```
The same defect class as commit 44, one level down. Clients/Index offered "+ Add
Client" to Sales Representative, who holds clients.directory and clients.profile but
not clients.create, and the server sent anyone who used it to /Account/NoAccess.
Clients/Profile offered Start a Quotation and View in Pipeline to roles lacking
quotation.builder and pipeline.board.

Both now ask Perm.Has for the permission the target action demands, and the
catalogue's empty-state copy branches on the same test so a role without create rights
is never told to create the first product.
```

Stage: `Views/Clients/Index.cshtml`, `Views/Clients/Profile.cshtml`

### 46. `fix(web): render the 403 page and limit registration to real roles`

```
NoAccess was a bare View() for a view that does not exist, so every permission refusal
in the product produced an unhandled 500 instead of the page that explains what
happened. It now renders the shared Unauthorized view with the caller's role.

Register validated against UserRoles.All, which includes System Administrator: anyone
could sign up as the role that owns the Users & Roles screen. It now validates against
UserRoles.SelfRegisterable, and the Roles.cs copy of that list is deleted because it
disagreed with the Core one and nothing read it. The admin role dropdown iterates
UserRoles.All instead of four hand-typed names, so a role added to the system appears
in the one screen that assigns roles rather than being silently unassignable.
```

Stage: `Controllers/AccountController.cs` *(hunk: NoAccess, Register)*, `Authorization/Roles.cs`, `Views/Admin/Users.cshtml`

### 47. `docs(web): correct the on-screen claims the code does not support`

```
Three sentences inside the product were false, and a user guide that described the
screens faithfully would have documented a fiction:

  - Rules Engine said SLA escalation runs every 15 minutes; both hosted services are
    configured at 5 (SlaEscalationService, FollowUpBackgroundService);
  - Rules Engine claimed the pipeline is filtered per representative; enforcement is
    screen-level only, so the scope column now says that;
  - Admin/Users was subtitled "View, create, edit, deactivate all users" while the
    screen creates nobody and cannot reactivate a deactivated account.

The UI and the guide now agree, which is the rubric's "documentation perfectly matches
the system" requirement at its smallest and most checkable scale.
```

Stage: `Views/Admin/Rules.cshtml`, `Controllers/AdminController.cs`

---

## Public landing page

### 48. `feat(web): give the landing page its own content model`

```
The public page rendered QuickAction and RoleCard lists assembled in HomeController —
the same staff-shaped data as the authenticated workspace, shown to a first-time
visitor who is deciding whether the product is for them at all.

LandingContent puts the marketing copy and its imagery in one static, reviewable place:
product showcase, workflow steps, sector cards and the team block. HomeViewModel
carries those five collections; HomeController loses the two builders and the models
they produced. Deliberately not database-backed: a page an anonymous visitor opens
should not be able to fail because the API is unreachable.
```

Stage: `ViewModels/Home/LandingContent.cs`, `ViewModels/Home/HomeViewModel.cs`, `Controllers/HomeController.cs`, delete `ViewModels/Home/QuickAction.cs`, `ViewModels/Home/RoleCard.cs`

### 49. `feat(web): rebuild the landing page on its own public shell`

```
_LandingLayout is a separate shell for a separate audience. It reads the session token
only to decide which call to action to show, and carries no sidebar, no topbar, no
notification bell and no SignalR connection — a visitor has no business opening a
staff socket, and there is no reason to give them the opportunity.

Index.cshtml is the new page: hero over a real product photograph, an eyebrow and two
calls to action that resolve to the workspace when signed in and to sign-in when not;
a product showcase that is explicitly browse-only, with no cart and no order because a
visitor has no account to order into; sector cards; the workflow strip; and the team
section.
```

Stage: `Views/Shared/_LandingLayout.cshtml`, `Views/Home/Index.cshtml`

### 50. `style(web): add the landing stylesheet, interactions and photography`

```
landing.css (936 lines, `lnd-` prefixed tokens, no selector shared with the staff
theme) leaves the workspace stylesheet untouched: a marketing page and an operator
screen have different jobs and must not be able to regress each other. Dark, minimal,
generous whitespace, reusing the theme's accent vocabulary rather than redefining it.

landing.js handles nav and the mobile menu, scroll reveal, the workflow tabs, the
lightbox zoom on product and sector imagery and the pointer highlight — each behaviour
gated on a `.lnd-js` class so the page still reads correctly with JavaScript disabled,
and it honours prefers-reduced-motion.

Ten photographs produced for this release: hero, cold-chain dispatch, four product
shots, three sector shots and the team image. The team block is placeholder until real
headshots exist; nothing here asserts a real person's likeness.
```

Stage: `wwwroot/pages/landing.css`, `wwwroot/js/landing.js`, `wwwroot/images/landing/`

---

## Dead code

### 51. `refactor: delete the unreferenced scaffolding in the API and Core`

```
Every type below was an empty namespace and class shell left by a Visual Studio
template, and a grep for its type name came back empty. They cost a reader time and
imply capabilities that do not exist.

  - API: PipelineService + IPipelineService, UpdatePipelineRequest, ClientResponse and
    PipelineResponse (the pipeline lives in the Web project against /api/deal),
    ExceptionHandlingMiddleware, JwtMiddleware, ApplicationBuilderExtensions,
    ServiceCollectionExtensions, Filters/AuthorizeAttribute.
  - Core: the superseded four-member UserRole enum (the UserRoles string constants won),
    the empty DealStage and OrderStatus enums, the IRepository stub, and a tracked
    UTF-16 User.cs.bak backup of the entity.
  - Infrastructure: IEmailService, which declared password-reset and notification mail
    nothing ever sent — a file that made the system look like it had notifications it
    did not have.

Verified by grep before deletion and by a clean Release build after.
```

Stage: API `Extensions/`, `Filters/AuthorizeAttribute.cs`, `Middleware/{ExceptionHandlingMiddleware,JwtMiddleware}.cs`, `Services/PipelineService.cs`, `Services/Interfaces/IPipelineService.cs`, `DTOs/Requests/UpdatePipelineRequest.cs`, `DTOs/Responses/{ClientResponse,PipelineResponse}.cs`; Core `Enums/{DealStage,OrderStatus,UserRole}.cs`, `Interfaces/IRepository.cs`, `Entities/User.cs.bak`; Infra `Data/External/Interfaces/IEmailService.cs`

### 52. `refactor(web): delete dead views, code-behind stubs, styles and scripts`

```
  - Eleven `*.cshtml.cs` PageModel classes attached to MVC Razor views. MVC does not use
    them; each was an empty OnGet() that never ran, and their presence sent people
    looking for Razor Pages binding that was not there.
  - Views/View.cshtml (empty stub), Views/Telehealth/Appointments.cshtml and
    Views/Procurement/LogisticsTracker.cshtml with its code-behind — the board the live
    Logistics/Tracker screen replaced.
  - Unused view models: DashboardViewModel, Admin/AdminDashboardViewModel,
    Procurement/LogisticsTrackerViewModel, Models/ErrorViewModel.cs. The real dashboards
    render Views/Dashboard/Index.cshtml from their own controllers.
  - Orphaned styles and scripts, grep-verified against every layout, view and
    _ViewImports: wwwroot/css/{brand,components,layout,pages,site,verichat-theme.backup}.css,
    wwwroot/pages/{home,home.backup}.css, _Layout.cshtml.css,
    wwwroot/js/{api.js,logistics-tracker.js}. api.js deserves naming: it read a JWT from
    localStorage against a hardcoded URL, and deleting it is what leaves commit 43's
    cookie-based transport as the only path in the product.
  - The csproj's empty <Folder> placeholders for Areas and Views directories that do not
    exist.
```

Stage: the eleven code-behind deletions `Views/Account/{Login,Register}.cshtml.cs`,
`Views/Clients/Create.cshtml.cs`, `Views/Logistics/Tracker.cshtml.cs`,
`Views/Procurement/{Dashboard,LogisticsTracker,ProductCatalog,WarehouseAlerts}.cshtml.cs`,
`Views/Shared/{_Sidebar,_Topbar}.cshtml.cs`, `Views/Support/TicketHub.cshtml.cs`;
`Views/View.cshtml`, `Views/Telehealth/Appointments.cshtml`,
`Views/Procurement/LogisticsTracker.cshtml`, `ViewModels/DashboardViewModel.cs`,
`ViewModels/Admin/AdminDashboardViewModel.cs`,
`ViewModels/Procurement/LogisticsTrackerViewModel.cs`, `Models/ErrorViewModel.cs`,
`wwwroot/css/{brand,components,layout,pages,site,verichat-theme.backup}.css`,
`wwwroot/pages/{home,home.backup}.css`, `Views/Shared/_Layout.cshtml.css`,
`wwwroot/js/{api,logistics-tracker}.js`, `VerichatCRM.Web/VerichatCRM.Web.csproj`

### 53. `chore: normalise the filenames that ended in a space`

```
"PurchaseOrderService .cs", "WarehouseAlertService .cs" and "SupportApiService .cs" each
had a space before the extension. They compiled, because the type name comes from the
class declaration, but any tooling that globs or splits on the filename misses them —
including some checkout and test-report paths.

Content is byte-identical: rename only, kept away from every logic change so
`git log --follow` still traces each file's real history. "ClientConfiguration .cs" was
renamed inside commit 26, where it also had content changes.
```

Stage: `VerichatCRM.API/Services/{PurchaseOrderService,WarehouseAlertService}.cs`, `VerichatCRM.Web/Services/SupportApiService.cs` (as renames)

---

## Documentation

### 54. `docs: rewrite the user guide as the final submission document`

```
userguide.md is rewritten from the Task 2 draft into a 22-section operator guide
(1,150 lines): quick start; what the system is and what it deliberately does not do;
technical prerequisites including the absence of an `engines` field and a `.nvmrc`; the
ports and URLs table with a warning about the two stale addresses in
VerichatMobile/.env; access and sign-in per role; a self-host setup that names the four
configuration values and the log lines that mean each step worked; the navigation shell
with the per-role sidebar matrix; a guide per role; the mobile guide; four end-to-end
workflows; a form-by-form data reference quoting the validation strings verbatim; 32
FAQs; a troubleshooting table mapping every startup guard to the symptom a user actually
sees; a 30-item known-limitations list; a glossary; Appendix A, the role-by-screen matrix
read from Perm.cs; and Appendix B, the annotated screenshot inventory for every major step.

Every statement was re-derived from source instead of inherited from the draft. That is
what surfaced commits 41 and 44–47: the draft had been faithful to a document that drifted
from the product — a disabled PDF button, "no CI currently running", manual-only scoring,
a reorder that created nothing, sidebar links that ended in a 403.

TRAINING-PLAN.md preserves the internal training material lifted out of that draft —
role cohorts, the phased plan, UAT exercises and the EXPO station script — under a header
naming it as planning material and listing which of its items this release fixed. It stays
out of the guide because it is not an operator document.
```

Stage: `userguide.md`, `TRAINING-PLAN.md`

### 55. `docs: publish the deployment runbook and align the remaining documents`

```
DEPLOYMENT.md (249 lines) is new: what runs where against Monster ASP shared hosting (web,
API, hosted SQL Server), the secrets inventory showing where each configuration key must
live now and what rotating it costs, credential rotation, how the API receives its
configuration on shared hosting, the encryption requirement commit 5 enforces, TLS as a
prerequisite for the live notification feed, the git-history rewrite written as
operator-only instructions because it is destructive and was not run here, post-deployment
confirmation, and §8's pre-flight checks that commit 30's migration needs before touching a
populated database.

README.md corrected: the new documents listed, secrets described as blank in tracked
settings rather than quietly placeholder-shaped, the Encrypt=False warning, and the test
count 11 → 71 cases across 7 files with the honest note that there is no mobile test runner.

documentation.md (903 lines) is the proposal-to-implementation gap analysis: requirement and
user-story traceability against the Task 1 brief, UX journeys, architecture and ERD versus
the tables actually mapped, the Gitflow and CI/CD story, the hosting re-cost, security,
testing, the defect register and next actions. VerichatCRM_FeatureWalkthrough.md (157 lines)
carries the feature tour and the file-relationship map.
```

Stage: `DEPLOYMENT.md`, `README.md`, `documentation.md`, `VerichatCRM_FeatureWalkthrough.md`

---

## Coverage check

All 143 changed paths are assigned exactly once, except the eight files deliberately split
by hunk (listed in *How to use it*) and the `XssSanitizationMiddleware` deletion, which
belongs to commit 14 because that is the change that replaces it.

Verification after the sequence completes:

```bash
git status --porcelain          # only COMMIT-PLAN.md, if you have not deleted it
dotnet build VerichatCRM.slnx -c Release   # 0 errors, the 20 known NU1902/NU1903 advisories
dotnet test VerichatCRM.Tests/VerichatCRM.Tests.csproj -c Release  # Passed: 71, Failed: 0
git log --oneline -55 | cat
```
