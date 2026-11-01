# Verichat CRM — Training Plan, Risks and Rollout Notes

> Internal planning material extracted from the Task 2 user-guide draft when that document was rewritten as the
> submission-grade **User Guide** (Task 3). It is not part of the user-facing manual.

> **Staleness warning:** several line-level claims here predate the final build — the quotation PDF
> button is now wired and working, CI runs on every push, enquiry scoring is automatic with manual
> override, and reorder posts a real purchase order. Where this file and
> [userguide.md](userguide.md) / [documentation.md](documentation.md) disagree, the other two are
> correct; the verified position of each item is listed in documentation.md.

## Part 5 — Broken paths the guide must warn about

Consolidated from the gap analysis so that the guide writer never has to rediscover them. **Every row here is a candidate for a "Please note" box in the published manual.**

| Path | Status | What to tell the user | Fix ref |
|---|---|---|---|
| Quotation → **Generate PDF** | **[Works]** — button enabled, Save added, totals in rand | teach the download; UAT-02 closes | was D7, §20-3 |
| Warehouse Alerts → **Reorder** | **[Works]** — form POST raises a real purchase order; confirm or correct the prefilled supplier and unit cost | "the quantity is recalculated by the server from the rule shown on the row" | was D8 |
| Warehouse Alerts as **Procurement** | **[Built]** — permission granted; sidebar reads the permission | normal navigation | was D11 |
| Warehouse Alerts → stock chart | **[Broken]** — renders empty; the series it expects is `Month / Desktop / Mobile`, which is meaningless for consumables | say nothing; don't demo | — |
| Inquiry **priority score** | **[Built]** — computed from value/sector/contract/urgency when left blank; a typed score overrides it and is never recalculated | "the number is a starting position — overrule it and say why in a follow-up" | §20-11 |
| **Over-stock order** | **[Broken]** — rejected outright, not warned about | "split or reduce the order; the system will not let you oversell" | §8 |
| Catalogue **sort dropdown** | **[Broken]** — decorative, does nothing | "Sort is not available" | §19.3 |
| Assets → **Schedule Service** | **[Broken]** — only links to Ticket Hub | "Use Consult to book" | §19.3 |
| Ticket from an **asset** | **[Broken]** — always Medium/Technical | "If critical, phone us" | D47 |
| In-app **push / live updates** (mobile) | **[Broken]** — none | "Pull to refresh" | D35-D38, D44 |
| **SSO** button on staff login | **[Broken]** — alert stub | "Not configured" | D43 |
| Settings → **Privacy / Terms / About** | **[Broken]** — empty rows | do not reference | D45 |
| **Client User / Medical Student** on web | **[Broken]** — by design → proper 403 page now, not a crash | "Clients use the app" | `Perm.cs`, was D53 |
| **Logistics** on mobile | **[Built]** — staff dashboard, as of this pass | train it — their job is in the field | was D39 |
| **Idle session** in mobile | **[Broken]** — hard sign-out, cart lost | "Don't leave the app open mid-quote" | D40, D41 |
| Global **notification bell** (web) | **[Partial]** — points at the right hub now, but still silent on the deployed pair: the session cookie cannot cross from the web host to the API host | "Alerts appear on the ticket and dashboard screens, not as a popup" | D9, defect 9 |
| **Password reset** | **[Broken]** — no UI, no endpoint | "Ask Admin" | D18 |
| Outbound **email** of any kind | **[Broken]** — `EmailService` is registered but injected by nobody | "No confirmation emails are sent" | D18, UAT-03 |
| Any **permission denial** | **[Built]** — was a server error; now the branded 403 page | trainees will see a real message, so teach what it means | was D53 |

**Note:** A guide that silently omits these is a guide that generates support tickets. My recommendation is an explicit, up-front "Known limitations in this release" page — better to lose a little polish than to have ten users discover the same dead button.

---

<a name="part-6"></a>
## Part 6 — What we should ADD to the product before training

Not a wish list — the minimum set where absence of the feature makes the system untrainable or the guide unwritable.

**Before any client touches it**
1. **Persist the quote cart** (AsyncStorage or a server-side draft) and add token refresh so a 401 does not destroy it. *(D40 + D41, ~a day.)* **Still open**, and it is still the worst client-facing flaw.
2. **Bind TLS to the API host, then switch every origin to `https://`.** Release builds are otherwise impossible. *(D42.)* **Still open, and now it has a named owner and a named cost:** the API sits on Monster ASP's **Free** shared plan (`api-site.runasp.net`), and that plan does not issue certificates automatically — the wildcard is on `*.runasp.net`, not on your hostname, so a Let's Encrypt HTTP-01 challenge has nowhere to land (see the technical report §13.3). Either pay for the Premium plan that includes certificate management or drive DNS-01 validation by hand. This is a control-panel task, not a code change. The code half is finished: `VerichatCRM.Web/appsettings.json`, the `api.ts` fallback and the (git-ignored) `VerichatMobile/.env` all name `http://api-site.runasp.net`, so when the certificate lands the change is those same values with `http` → `https`, in one commit — do all three together, because an `https` page calling an `http` API is blocked as mixed content.
3. **Real Privacy and Terms content**, shown in Settings. *(D45.)* **Still open.**
4. **An order/quote confirmation and status screen for clients** — today a client submits a request and then sees nothing until someone tells them. This is the biggest source of "did it work?" anxiety and it does not exist. *(§19.2.)* **Still open.**
5. **Priority and category selection when raising a fault from an asset.** *(D47.)* **Still open.**

**Before staff rely on it for real work**
6. **Done — Enable Generate-PDF.** The button reaches the working endpoint, a Save action sits beside it, totals print in rand and UAT-02 closes. *(was D7, §20-3.)*
7. **Done — the reorder path.** `Warehouse Alerts → Reorder` is now a form POST that raises a real purchase order, Procurement Manager holds `warehouse.alerts`, and the sidebar gates on the permission rather than the role. *(was D8 + D11.)* **Note:** The stock-level chart next to it is still empty scaffolding and the order path still rejects an over-stock order instead of warning.
8. **A password-reset journey** (endpoint + UI), or Admin is a permanent bottleneck. *(D18.)* **Still open** — and the hashing underneath it changed (§20 item 6), so the eventual reset flow has to write PBKDF2 hashes, not the old format.
9. **Wire `EmailService`** so confirmations and SLA notices actually go out, or delete it and stop saying the system emails. *(D18.)* **Still open.**
10. **Done — `'Logistics'` is in `STAFF_MOBILE_ROLES`.** The role signs in on the phone and reaches the staff operations dashboard. *(was D39.)*

**Structural, so the guide stops going stale**
11. **Done — CI restored.** `.github/workflows/ci.yml` is back from `7adceef` and the README now describes one workflow that exists, with the true test count. **What it does not do is deploy**: no staging or release environment, no approval gate, no branch protection, and its first run on the remote is still unwitnessed. *(§11.2, §20-2.)*
12. **A machine-readable API index** and per-screen in-app help (`Help` drawer / tooltips). **Note:** Still open. The gap analysis found the API surface hard enough to map that three mobile service files were written against endpoints that never existed.
13. **Note:** Half done — `VerichatCRM_FeatureWalkthrough.md`. §20 item 13 asked for it to be deleted; it was **kept**, because it is prose the team authored and restored deliberately, and deleting someone's writing is their call. It is still substantially wrong (it describes the project as "appears to be a .NET-based application", claims only Infrastructure/Tests/Mobile exist, and says real-time sync is implemented through a "React Native ↔ C# bridge"). **Someone must either rewrite it or mark it superseded by this guide** — an unread accurate document is worth more than a deleted inaccurate one, but a third document that contradicts the other two is worth nothing.
14. **Rotate the committed database credential** before anyone external is onboarded onto this system. *(D1 — password value deliberately not reproduced anywhere in these documents.)* **The code half is done** — `appsettings.json` no longer carries a secret and the driver encrypts by default. **The password itself is still the live one and still in git history**, which needs the hosting control panel and an explicit decision about rewriting 11 shared branches.

---

<a name="part-7"></a>
## Part 7 — What we should ADD to this guide before publishing

The finished artefacts I would want in the pack:

- [ ] **Two separate documents, not one.** A 12–16 page **Client Handbook** (mobile, screenshots-first, almost no jargon) and a **Staff Reference** organised by role. Mixing them is why this draft is long.
- [ ] **Annotated screenshots** for every screen in Appendix B. Currently there are none in the repo. Prefer real captures from the deployed build, taken after §6 items 1-7 land, or the screenshots will document broken states.
- [ ] **A short GIF/video per core task** (submit a quote request, raise a fault, advance a deal, find an alert). 15–30 seconds each, embedded or on a shared drive.
- [ ] **Quick-reference cards** — one page per role (drafts in Appendix C).
- [ ] **FAQ / troubleshooting page** seeded from the broken-paths table in §5: "I clicked Reorder Now and nothing happened", "Where's my PDF?", "Why can't I sign in on my phone?", "My cart disappeared", "Do I get an email? (No.)"
- [ ] **A glossary** — Inquiry vs Lead vs Deal vs Quotation vs Order vs Ticket vs Asset, plus SLA, tracking number, quote reference, warranty vs calibration expiry. Half of the support load in a CRM is vocabulary.
- [ ] **"Known limitations in this release"** page (from §5) with a *target fix version* per line, and a date.
- [ ] **Roles & access request form** — who can approve which role, and what a user does when a link is missing.
- [ ] **A feedback route.** The proposal specified a feedback form; none exists (§19.2). Without it we get no signal from training.
- [ ] **Version and date on every page**, plus an owner. The gap analysis found three documents in this repo already stale in different ways (§11.2 README, D34 walkthrough, the eleven-case test count) — the guide must be dated or it will join them.
- [ ] **Accessibility + language pass.** Verichat's clients are South African clinics and institutions; consider plain-language English and a second language for the client handbook.

---

<a name="part-8"></a>
## Part 8 — Training plan

Adapted from Task 1 §8.2's six-phase schedule, keeping its central rule:

> **"No user will be trained on the full system — only what they need."**

Everything below honours that: each session is scoped to one role's sidebar.

### 8.1 Phases and timeline

| Phase | When | Audience | Duration | Format | Content |
|---|---|---|---|---|---|
| **P0 Executive demo** | T−2 weeks | Verichat leadership | 45 min | Demo, live | End-to-end walkthrough (§4.6 story), the broken-paths honesty page, sign-off on go-live date |
| **P1 Super-user / Train-the-Trainer** | T−2 weeks | 1 nominated person per department + Admin | 2 × half-day | Hands-on lab | Full system for the super user's own role, plus how to answer FAQ, how to escalate, how to add/reset a user |
| **P2 Sales cohort** | T−1 week | Sales reps | 3 hours | Workshop | Inquiry Queue scoring rules → Lead Tracker → Pipeline Board (drag) → Quotation Builder → *the PDF caveat* → Analytics. Exercise: Appendix D UAT-01 + UAT-02 |
| **P3 Procurement + Logistics cohort** | T−1 week | Procurement, Logistics, warehouse | 3 hours | Workshop | Catalogue & thresholds → Warehouse Alerts (*what works vs what doesn't*) → Purchase orders & goods receipt → Logistics Tracker → Asset Tracking. Exercise: UAT-03 |
| **P4 Support cohort** | T−1 week | Field support engineers | 2.5 hours | Workshop | Ticket Hub, the five states, SLA clock & escalation → photo evidence → asset service history → Client Appointments. Exercise: UAT-04 |
| **P5 Admin** | T−1 week | System Administrator(s) | 2 hours | Workshop | Users & Roles (**including which roles can log in where** — §1), Rules Engine-is-static, Analytics (UAT-05), and the **help-desk duty** created by no self-service reset |
| **P6 Go-live standby** | T day → T+2 | All staff | 3 days, rostered | Floor-walking / Teams channel | Named person per department answering "what do I click now?" |
| **P7 Buddy period** | T+1 → T+2 weeks | All staff | — | Pairing | Each new user paired with their super user; daily 15-minute huddle for the first week |
| **P8 Client onboarding** | T+1 → T+4 weeks, rolling | Each client account | 30–45 min per site | Remote or on-site | Install/build handout → the 5 tabs → scan a product → build a quote request → raise a fault → biometrics → **honest "no payment, no email, pull-to-refresh" framing** |
| **P9 Post-adoption review** | T+4 weeks | Super users + Admin | 90 min | Retro | What people stopped doing in the system, which broken paths were hit, defect triage into §6 |

**Note:** Decide T (go-live date) — nothing above can be booked without it.

### 8.2 Competency checks (not grades — a go/no-go per person)

Each cohort must demonstrate, unaided, on a training database:

- **Sales:** score an inquiry, move a deal two stages, build a quotation with three line items and issue it.
- **Procurement:** create a product, set a low-stock threshold, read an alert and place/acknowledge an order.
- **Support:** open a ticket, assign it, take it through Received → Under Review → Action Taken → Resolved, upload evidence.
- **Admin:** create a user of each of the five usable roles and state which client each can open.
- **Client:** install, sign in, scan a barcode, add two items, submit a quote request, accept or decline a quotation, raise a fault on an asset, enable biometrics.

Anything a person cannot do unaided goes into P6/P7 as a targeted repeat rather than a full re-run.

### 8.3 Materials required per cohort

Role-tailored quick-reference card (Appendix C) · annotated screenshots for that role's screens only · a live training account · the "known limitations" page · the FAQ · the 5-minute "what the system does not do" talk track.

### 8.4 Delivery mechanics

- **Train-the-trainer over external trainers.** Task 1 §8 assumed departmental super users; keep that — the gap analysis shows this project's documentation drifts when it is written by whoever built it.
- **Sessions of ≤3 hours**, split by role, because the sidebars genuinely differ.
- **Record every session** for absentees and for P8; a recorded library is also the cheapest fix for the "guide goes stale" problem — re-record one clip, not one document.
- **One shared channel** (WhatsApp group per department was the proposal's choice and suits this user base) staffed during P6/P7.

### 8.5 Change management framing

Task 1 §8 used a Kotter-style sequence; the parts still worth honouring, given what actually shipped:

1. **Urgency** — the current process is spreadsheets and WhatsApp; say what breaks today.
2. **Guiding coalition** — one super user per department is exactly this.
3. **Vision** — Part 0's paragraph, verbatim, repeated in every session.
4. **Empowerment** — remove the broken-path blockers *before* the cohort that would hit them. Training someone to work around a dead button teaches them the tool is broken.
5. **Short-term wins** — pick visible ones that exist: the Pipeline Board, the offline mobile cart, the SLA countdown.
6. **Don't claim the anchor** — the system does not yet hold the flagship PDF or email flows; overclaiming here is how adoption dies at week four.

---

<a name="part-9"></a>
## Part 9 — Training considerations and risks

| Risk | Why it is real here | Mitigation |
|---|---|---|
| **Training on broken paths** | Seven of the §5 broken paths are on primary journeys (PDF, reorder, alerts access) | Fix §6 items 6-7 before P3/P2; otherwise teach the workaround explicitly and label it a workaround |
| **Clients can't install it** | Cleartext HTTP blocks release builds (D42) | §6-2, or limit P8 to a demo device in-house |
| **Over-promising** | Proposal advertised AI ranking, push, email, checkout, telehealth, BLE, payments | Every session opens with the honest "what this release does not do" page |
| **Admin becomes the bottleneck** | No self-service password reset (D18) | §6-8; until then, publish a named reset contact and a 4-hour SLA |
| **Role confusion / dead accounts** | 7 roles, **all 7 can now sign in somewhere** (5 on mobile); the two client roles have deliberately empty web permissions | §1's table as a laminated card for Admin; P5 competency check |
| **Data quality at the door** | `DataSeeder` injects 4 clients + 6 products into every fresh DB (D17) | Clean the training database before sessions so nobody learns on seed data |
| **Mobile-only staff expectation** | Logistics now signs in on mobile **[works]**; staff mobile still overlaps the web app | State device policy per role explicitly |
| **Silent failures** | Dead service files 404 with no visible error; no mobile tests at all (§15) | P6 floor-walking catches what testing did not |
| **Documentation drift** | README, walkthrough and gap analysis already disagree | Date + owner on every artefact; record sessions (8.4) |
| ~~Currency confusion~~ | **[Closed]** — quotations and the PDF are in rand like everything else | State it once in the Sales module so nobody assumes a dollar price |
| **Security behaviour** | JWT in page script (D5) closed **[yes]**; committed credential still live (D1) and cleartext still blocks release builds (D42) | Train on the *fixed* build; include a "never email your password, we don't email anything" line |
| **Language & accessibility** | Clinic and institutional clients | Plain-language client handbook, screenshots-first, optional second language |
| **No feedback loop** | Proposal's feedback form was never built | §7 feedback route; P9 retro as the interim channel |

---

<a name="part-10"></a>
## Part 10 — Decisions needed from the team

**Blocking publication of a real guide:**

1. **Go-live date (T).** P0-P9 are relative to it.
2. **Which URL is authoritative** for web and API — one string in the guide.
3. **How clients obtain the app**: TestFlight, sideloaded APK, Expo Go, or demo-device-only until TLS is fixed.
4. **Is `Medical Student` a real role?** **Answered: yes, and it is self-registerable.** The Quick-Cart half is built — `POST /api/Quotation/request` authorises `"Client User,Medical Student"` and role assignment now creates the `Clients` row that path needs. **Still open: the student discount**, which no part of the pricing code implements, and whether the two client roles should ever look different to the user.
5. **Is Logistics mobile-supported?** **Answered: yes** — added to `STAFF_MOBILE_ROLES`, so this is now a training promise rather than a caveat.
6. **Manual vs automatic inquiry scoring** **Answered: automatic default, manual override** (`InquiryScoring`, weights 35/25/20/20; a typed score is never recalculated). **Still open: the house scoring rule itself** — what Verichat calls a 70 versus a 30 is a business judgement nobody has written down, and §4.1 can teach the mechanism but not the thresholds.
7. **Currency**: **Answered: rand, everywhere** — quotation totals and the PDF no longer format in USD.
8. **Rules Engine screen**: **Still unanswered.** The enquiry scoring engine is real code but this screen does not show it, so Admin is looking at a static table and calling it the rules engine. Build it as the scoring/SLA configuration surface it implies, or take it out of Admin's view and out of the guide.
9. **Who owns the guide**, and the review cadence.
10. **Client data-protection position** — real Privacy/Terms text is required before D45's empty rows are shown to a customer.
11. **Training environment**: is there a non-production database we can safely seed, or do we train on live data?

---

<a name="appendix-a"></a>
## Appendix A — Role × screen access matrix (verified from `Perm.cs`)

Legend: **F** full · **R** read-only · **—** not permitted (link hidden, URL gives `/Account/NoAccess`) · **†** the sidebar link for that role was missing until this build and is now rendered.

| Screen (permission) | Admin | Sales | Procurement | Support | Logistics | Client | Student |
|---|---|---|---|---|---|---|---|
| Admin Dashboard (`admin.dashboard`) | F | — | — | — | — | — | — |
| Procurement Dashboard (`procurement.dashboard`) | F | — | F | — | — | — | — |
| Support Dashboard (`support.dashboard`) | F | — | — | F | F | — | — |
| Sales Dashboard (`sales.dashboard`) | F | F | — | — | — | — | — |
| Client Appointments (`appointments.manage`) | F | F | F | F | — | — | — |
| Client Directory (`clients.directory`) | F | F | F | — | — | — | — |
| Client Profile (`clients.profile`) | F | F | F | — | — | — | — |
| **Create Client** (`clients.create`) | F | — | — | — | — | — | — |
| Assign rep (`clients.assign`) | F | — | — | — | — | — | — |
| Asset Tracking (`assets.tracking`) | F | F | — | F† | F† | — | — |
| Logistics Tracker (`logistics.tracker`) | F | — | F | F | F | — | — |
| Client Map (`sales.map`) | F | F | — | — | — | — | — |
| Lead Tracker (`sales.leads`) | F | F | — | — | — | — | — |
| Pipeline Board (`pipeline.board`) | F | F | — | — | — | — | — |
| Inquiry Queue (`inquiries.queue`) | F | F | — | — | — | — | — |
| Quotation Builder (`quotation.builder`) | F | F | — | — | — | — | — |
| Product Catalog (`products.catalog`) | F | F | F | — | — | — | — |
| Create product (`products.create`) | F | — | F | — | — | — | — |
| Edit product (`products.edit`) | F | F | F | — | — | — | — |
| Ticket Hub (`tickets.hub`) | F | — | F | F | F | — | — |
| Update ticket (`support.ticket.update`) | F | — | — | F | — | — | — |
| **Warehouse Alerts** (`warehouse.alerts`) | F | — | **F†** | — | — | — | — |
| Analytics (`dashboard.analytics`) | F | F | — | — | — | — | — |
| Admin Analytics (`admin.analytics`) | F | — | — | — | — | — | — |
| Rules Engine (`admin.rules`) | F | — | — | — | — | — | — |
| Users & Roles (`admin.roles`) | F | — | — | — | — | — | — |
| **Mobile app sign-in** | **[Built]** | **[Built]** | **[Built]** | **[Built]** | **[Built]** | **[Built]** | **yes** * |

\* Client User and Medical Student both reach the client journey and the mobile app still shows them the same tabs — but the path itself now works for students: `POST /api/Quotation/request` authorises `"Client User,Medical Student"`, so the quick cart can raise a real quote request. What remains unbuilt is the **discount** half of the proposal's student offer, and whether the two roles should diverge at all is still an open product question (§10 item 4).

**Fixed since the first draft of this guide:** the Asset Tracking link used to render only under `@if (isSales)`, so Field Support and Logistics — both of whom *hold* `assets.tracking` — had the permission and no link, and could only reach the screen by typing the URL. The sidebar now gates that link on `Can(Perm.AssetTracking)` (`_Sidebar.cshtml:89`), the same class of fix as Warehouse Alerts: **the menu must ask the permission the action asks.**

---

<a name="appendix-b"></a>
## Appendix B — Screen inventory (what the guide has to cover)

**Web — 14 controllers, 38 Razor views** (28 in controller folders, 10 shared partials, layouts and status pages). Sidebar-reachable: Admin Dashboard, Procurement Dashboard, Support Dashboard, Sales Dashboard, Client Directory, Client Profile, Client Appointments, Asset Tracking, Logistics Tracker, Inquiry Queue, Lead Tracker, Client Map, Pipeline Board, Analytics, Quotation Builder, Product Catalog, Ticket Hub, Ticket Detail, Warehouse Alerts, Users & Roles, Rules Engine.
**The public landing page is a separate shell.** `/` (`Home/Index.cshtml`) renders in
`_LandingLayout.cshtml` — its own navigation bar with **Sign in** and **Client portal**, a
hero, a three-step workflow, a reference-only product gallery with click-to-zoom, the sector
cards and a meet-the-team band — and it deliberately does **not** load the sidebar, the topbar
or the notification bell. Everything a trainee sees after signing in is the `_Layout` shell
described above, so screenshots of the two must not be mixed in the same page of the handbook.
The landing copy and the four showcased items live in `ViewModels/Home/LandingContent.cs`; the
product names there follow the seeded catalogue, so if the seed data changes the page needs a
manual edit. The team names come from the repository's commit authorship and are placeholders
for real names and headshots.

**Unreachable views.** `Telehealth/Appointments.cshtml` and `Procurement/LogisticsTracker.cshtml` were deleted, and one deletion was reversed: `Sales/Appointments.cshtml` is not a duplicate — `TelehealthController` renders it by explicit path, so it is the live Client Appointments screen and removing it would have broken that page. **Two views remain in the tree that no action returns** — `Procurement/Dashboard.cshtml` and `Support/Dashboard.cshtml`. They are kept on purpose: each is bound to its own role-specific view-model and is the only concrete design for the per-role dashboards the proposal described, which every role was later redirected away from. Making them live is a feature (write the per-role payloads), deleting them is a product decision, and neither is a cleanup. See technical report §18 defect 29b.

**Note:** Important for screenshotting: there is only one dashboard, not five. `Admin/Dashboard`, `Procurement/Dashboard`, `Support/Dashboard` and `Dashboard/Index` all return the same `~/Views/Dashboard/Index.cshtml`; only the `RoleLabel` and subtitle differ (`AdminController.cs:26`, `ProcurementController.cs:47`, `SupportController.cs:28`). So the guide needs **one** dashboard screenshot plus a note that the heading changes per role — not four, and do not waste a training session pretending the layouts differ. This also means the proposal's distinct per-role dashboard designs were consolidated into a single template, and the four orphaned `*Dashboard.cshtml` files above are the drafts that were replaced.

**Mobile — 18 route files in `src/app/`, 5 visible tabs.** Live: `index` (role chooser), `clientLogin`, `adminLogin`, `newUserAccount`, `clientHomescreen`, `DashboardScreen`, `medicalCatalogueShop`, `cartScreen`, `BookAppointmentScreen`, `EquipmentStatusScreen`, `profileScreen`, `settingsScreen`. Dead/still-shipped (D46, D48): `explore`, `Dashboard`, `ClientCommunicationScreen`, `ClientTimelineScreen`, plus `GoalsScreen` (a card, D50).

---

<a name="appendix-c"></a>
## Appendix C — Draft quick-reference cards

### C.1 Sales Representative
```
MY JOB: turn inquiries into accepted quotations.
LAND ON   Pipeline Board the moment I sign in (that IS my dashboard).
DAILY     Inquiry Queue → score 0-100 (I choose; no auto-ranking) → follow up
          Pipeline Board → drag card to next stage
          Quotation Builder → add catalogue lines → issue to client
CAN'T     create a client (Admin only) · see tickets (ask Support) · download PDF yet
WATCH OUT Quotation PDF button is disabled. Sort dropdowns do nothing.
IF STUCK  #faq · my super user is ______
```

### C.2 Client User (mobile)
```
MY JOB: request equipment, know what I own, report a fault.
5 TABS    Home · Catalogue · Consult · My Assets · Profile
ORDER     Catalogue → scan or browse → add to cart → Submit request
          Verichat sends back a QUOTATION → I Accept or Decline
FAULT     My Assets → pick the machine → Report fault
          ⚠ it always says "Medium" — if it is URGENT, PHONE US: ______
BOOK      Consult tab (not the Assets button)
GOOD TO   Biometric sign-in · works offline, your edits sync when you reconnect
KNOW      You pay in the app NO. You get no email NO. Pull to refresh (no push)
          If you close the app mid-cart, the cart may be empty — don't close it.
```

### C.3 System Administrator
```
LAND ON   Admin Dashboard (all 27 permissions).
USERS     Admin → Users & Roles. 5 roles can sign in to web; 4 to mobile.
          Client User / Medical Student = MOBILE ONLY (web says "NoAccess" — correct).
          Logistics = web only TODAY.
THE ONLY ONE OF ME   Warehouse Alerts · Rules Engine · Users & Roles · Create Client
RULES ENGINE screen is READ-ONLY — there is no engine behind it.
HELP DESK no password reset exists. Forgotten password = comes to me.
BEFORE GO-LIVE rotate the DB credential; clean seeded demo data out.
```

---

<a name="appendix-d"></a>
## Appendix D — UAT scenarios repurposed as training exercises

The proposal's six scripted UAT scenarios (Task 1 §8) double as the practical lab, and running them in front of users shows them exactly where the release is honest.

| UAT | Exercise for | Expected result today |
|---|---|---|
| UAT-01 ranked inquiry list | Sales | **[Partial]** — list sorts — by **the number the rep typed** |
| UAT-02 one-click branded PDF | Sales | **[Not built]** — **button disabled**; pass only if Admin calls the API directly — run it as a "here is what lands next" demo |
| UAT-03 repeat order + email + low-stock warning | Procurement | **[Partial]** — reorder **works** *(unless via the broken button)* · **no email** · order path **rejects** instead of warning |
| UAT-04 ticket + tracking number + auto-routing + SLA | Support | **[Partial]** — tracking **works**, SLA **works** · **assignment is manual** |
| UAT-05 revenue forecast dashboard | Admin / Sales | **[Pass]** |
| UAT-06 bulk order over stock → banner + notify procurement | Procurement | **[Not built]** — **no banner**; order is rejected |

**Recommendation:** run this table as an open slide in every cohort. It converts two failures (UAT-02, UAT-06) from "the training is hiding something" into "the training told us first", which is what protects adoption at the P9 review.

---

<a name="appendix-e"></a>
## Appendix E — EXPO 2026 station demo script

Constraints from `EXPO station layout and posters.pdf`: a **5–10 minute** slot — 1 min elevator pitch, 3–4 min live demonstration, 2 min individual contribution, 2–3 min Q&A — a tri-fold board, and a **mandatory pre-recorded backup demo video**.

**1 min — pitch.** Part 0, verbatim, ending on the differentiator: *the mobile client works when the clinic's internet does not.*

**3–4 min — live demonstration (the round trip, both apps in frame).**
1. Mobile, client account: scan a product → add to cart → submit quote request. *(shows barcode + offline queue)*
2. Web, Sales: Inquiry Queue → score → drag the deal two stages → Quotation Builder → issue.
3. Mobile: the quotation arrives → **Accept**.
4. Web, Support: raise a ticket on that client's asset → point at the **SLA countdown**.

**Record the PDF download from the staff UI.** That button is now enabled and reaches the working endpoint (§6-1 above), so the clip shows the real user path — click, download, rand totals on the page. Keep the API URL visible in the frame as a fallback for the live demo, but do not present the endpoint as the product.

**2 min — individual contribution.** Each team member, 30–40 seconds, on the thing *they* built — the panel scores the individual as well as the artefact.

**2–3 min — Q&A: the five questions to expect, with honest answers.**
- *"Does the client pay in the app?"* — No; quotation workflow only, by design of this phase.
- *"Is the inquiry scoring automatic?"* — No; manual 0–100 today, automatic ranking is scoped but unbuilt.
- *"Show me the PDF."* — Server side works; the UI button is disabled pending wiring — **and say that before being asked twice**.
- *"How is it deployed / tested?"* — .NET 10 + EF Core + SQL Server on runasp.net; 6 xUnit files, no CI currently running. *(Be accurate: the README overstates this — D2.)*
- *"What about security?"* — JWT + role-based permission matrix enforced server-side; disclose honestly that TLS and credential handling are on the pre-launch fix list.

**The one message to repeat in every answer:** the proposal's Firebase design was replaced wholesale by a conventional .NET stack, and the business scope was kept — which is precisely what this project set out to prove.

---

## Draft closing notes

Three things I would want flagged to whoever picks this up next:

1. **The guide is currently blocked by the product, not by writing.** Five of the §5 broken-path items sit on primary journeys. Writing polished prose around a disabled flagship button produces a manual full of caveats. §6 items 1-7 first.
2. **Clients must not be onboarded before TLS is fixed** (§6-2) — a release build cannot reach the API as configured today.
3. **The honest-limitations page is the most valuable artefact in this pack.** The system does a great deal that was never proposed (offline queue, SLA escalation, asset lifecycle, biometrics, a four-chart BI view) and skips a handful of headline promises. Saying both, up front, is what makes the rest of the training credible.
