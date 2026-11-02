# Verichat brand, logo and design system

Companion to `README.md` (how to run it), `DEPLOYMENT.md` (how to host it) and
`documentation.md` (what is built and what is not). This file answers a different
question: **what is the visual identity, where does each piece of it live, and what
rule decides whether a screen belongs to it.**

It exists because the codebase had a palette, three stylesheets and twenty-odd
screens, and no written statement of what the mark is or which surface gets which
lock-up. That gap is how a screen ends up rendering Expo's logo on a cold start and
how `.vc-accent` came to be a purple that is not in the palette.

Everything below is traceable to a file and a line, or it is marked as **not
derivable from the repository**.

---

## 1. The logo

### 1.1 Provenance

The mark in use is the **real Verichat identity**, extracted from the company
profile PDF and committed in `4e36b22` (*"feat(web): put the real Verichat
brand… into the app"*). Before that commit every surface used a generated
placeholder — a plain "V" monogram — and that commit message says so explicitly.

**What the letters stand for is not recorded anywhere in the repository or the
source documents, and this file will not invent it.** What can be stated is what
the artwork is:

- A **two-letter ligature of V and C**, drawn in a high-contrast serif (Didone-ish:
  thick stems, hairline serifs), with the two glyphs split by a **diagonal rule**
  running bottom-left to top-right. The diagonal reads as a fraction bar, which is
  why the mark works at small sizes as a shape before it works as letters.
- A **full lock-up** that stacks the mark above the name set in tracked
  sans-serif capitals — `VERICHAT` — and a serif italic tagline beneath it:
  **"Quality Projects"**.

So the identity is a serif/sans pairing *inside the logo itself*: serif for the
mark and the tagline, sans for the word. That has a direct consequence for the UI,
in §3.

### 1.2 The asset family

Six files, four of them real and two of them duplicates of a legacy path:

| File | What it is | Size | Notes |
| --- | --- | --- | --- |
| `VerichatCRM.Web/wwwroot/images/brand/verichat-mark.png` | the mark, navy on transparent | 220 × 248 | canonical dark mark |
| `.../brand/verichat-mark-light.png` | the mark, white | 220 × 248 | for navy surfaces |
| `.../brand/verichat-logo.png` | full lock-up, navy | 662 × 404 | canonical dark wordmark |
| `.../brand/verichat-logo-light.png` | full lock-up, white | 662 × 404 | for navy surfaces |
| `.../brand/` (same four) in `VerichatMobile/assets/images/` | byte-identical copies | — | md5-matched to the web copies |
| `VerichatCRM.Web/wwwroot/images/logo.png` | **alias** | 220 × 248 | byte-identical to `brand/verichat-mark.png`; predates the rebrand and is what web chrome still loads |

`images/logo.png` is kept because four Razor files and the marketing header point
at it, and because deleting it would break the browser cache of anyone with a tab
open. It is the same bytes as the canonical mark, so nothing renders differently —
but it is the reason a reader can grep for "logo.png" and conclude the placeholder
is still shipping when it is not.

### 1.3 Which lock-up goes where

The rule, applied consistently, is **mark on chrome, wordmark on statements**:

- **Mark** (square, ≤ 40 px, on the warm plate) where the logo is *wayfinding* —
  it identifies the product in a corner and must not compete with the page:
  the app sidebar (`_Sidebar.cshtml:3`, 32 px), the sign-in and sign-up brand
  panel (`Login.cshtml:10`, `Register.cshtml:10`, 40 px), the marketing header
  (`_LandingLayout.cshtml:31`, 34 px).
- **Wordmark** (wide, ≥ 176 px) where the logo *is the message* — the marketing
  footer (`_LandingLayout.cshtml:69`, light version on navy), the mobile sign-in
  and home headers (`clientLogin.tsx:52`, `adminLogin.tsx:58`, `index.tsx:111`,
  `newUserAccount.tsx:74`), because a phone screen has no sidebar to hide it in.
- **Mark on a navy plate** for anything that is a *surface* rather than a page:
  the app icon (`assets/images/icon.png`), the in-app loader
  (`verichat-loader-mark.png` via `BrandLoader.tsx:17`).

The plate behind the mark is `#F4F3EF`, a warm off-white, because the mark's
hairline serifs vanish against pure `#FFFFFF` at 32 px. It is declared three times
today (`verichat-theme.css:78`, `pages/auth.css:33`, `pages/landing.css:111`) and
should be a token — see §6.

### 1.4 The tagline problem

Three different taglines are currently in circulation for one brand:

| Where | Text | Source |
| --- | --- | --- |
| The logo artwork itself | "Quality Projects" | `brand/verichat-logo*.png` |
| App sidebar and auth panels | "MEDICAL PROCUREMENT" | `_Sidebar.cshtml:4`, `Login.cshtml:13` |
| Marketing header | "Supply · Service · CRM" | `_LandingLayout.cshtml:33` |

Only the first is part of the artwork. The other two are descriptions of what the
system does, set in the logo's slot, and they disagree with each other. They are
defensible as *product* descriptors — the sidebar line tells a signed-in user what
this application is — but they are not brand taglines and should not be placed
where one would be read.

---

## 2. Colour

### 2.1 The core palette, and the lockstep rule

The web app's `:root` (`wwwroot/css/verichat-theme.css:2-36`) and the mobile
palette (`VerichatMobile/src/constants/crmTheme.ts:15-39`) are **the same palette
by intent, expressed in two syntaxes**. They match value for value:

| Role | Web token | Mobile token | Hex |
| --- | --- | --- | --- |
| Navy (chrome, sidebar) | `--vc-navy` | `primaryDark` | `#101B33` |
| Navy, hover | `--vc-navy-light` | — | `#1B2A4A` |
| Navy border on dark | `--vc-navy-border` | — | `#26365C` |
| Accent / primary action | `--vc-accent` | `primary` | `#2F6FED` |
| Accent wash | `--vc-accent-soft` | `surfaceAlt` | `#E8EFFE` |
| App background | `--vc-bg` | `background` | `#F4F6FA` |
| Panel surface | `--vc-surface` | `surface` | `#FFFFFF` |
| Hairline | `--vc-border` | `border` | `#E3E7EF` |
| Body text | `--vc-text` | `text` | `#182233` |
| Secondary text | `--vc-text-muted` | `muted` | `#6B7385` |
| Text on navy | `--vc-text-inverse` | — | `#F5F7FB` |
| Secondary on navy | `--vc-text-inverse-muted` | — | `#9AA6C3` |
| Ink (icon background) | — | `ink` | `#0D141D` |
| Success / wash | `--vc-success(-soft)` | `success(-Soft)` | `#1FA971` / `#E4F7EE` |
| Warning / wash | `--vc-warning(-soft)` | `warning(-Soft)` | `#E8940C` / `#FDF1DC` |
| Danger / wash | `--vc-danger(-soft)` | `danger(-Soft)` | `#E24C4C` / `#FBE6E6` |
| Cyan (gradient partner) | *untokenised* `#00B4D8` | *untokenised* | `#00B4D8` |

**Rule:** a colour change is one commit that touches both files. A colour that
exists in only one of them is a defect, not a platform difference.

### 2.2 The four navies

`#101B33` is the brand navy. Four other dark blues are in use, each for a reason
that is not written down anywhere, which is what makes them drift:

| Hex | Where | Legitimate? |
| --- | --- | --- |
| `#101B33` | `--vc-navy`, `primaryDark` | yes — the brand navy |
| `#1B2A4A` | `--vc-navy-light`, hover states | yes — a derived step, tokenised |
| `#0D141D` | `CRMTheme.ink`, `app.json` Android adaptive background, `android-icon-background.png` | arguable — a deeper navy for icon tiles, where the mark needs maximum contrast. Should be a named token, not an accident |
| `#070D1C` | `--lnd-ink`, `<meta name="theme-color">` on the landing page | marketing-only, deliberately darker for the hero. Fine while it stays behind the `lnd-` namespace |
| `#0A1F44` | the PDF quotation generator | **no** — a fifth navy that no stylesheet uses; `documentation.md` §10 records it as a known divergence |

### 2.3 What is not a token yet

`--vc-info` and `--vc-info-soft` are *referenced* by `.badge-info`
(`verichat-theme.css:172`) and never declared, so every "Medium priority" badge
renders with no background. `--space-xl` is referenced by
`Procurement/WarehouseAlerts.cshtml:72` and exists nowhere. `#00B4D8` — the cyan
half of every primary button gradient — appears as a literal in six theme rules,
two chart scripts and the landing sheet.

---

## 3. Typography

- **In the logo:** high-contrast serif (the mark, and "Quality Projects") plus a
  wide-tracked sans for `VERICHAT`.
- **In the product:** `--vc-font-heading` = **Poppins**, `--vc-font-body` =
  **Inter** (`verichat-theme.css:34-35`), loaded from Google Fonts in
  `_Layout.cshtml:15`, `_LandingLayout.cshtml:17` and `_AuthLayout.cshtml`.
  Mobile uses the system stack and expresses weight/size as literals.

**The logo's serif is display-only and never a UI font.** It is a Didone: at 12 px
its hairlines disappear and its thick/thin rhythm turns to grey. The UI pairing was
chosen to sit *next to* it — a geometric sans for headings that echoes the tracked
`VERICHAT` caps, a neutral sans for body copy — not to imitate it.

---

## 4. Dashboard layout: the contract

Every dashboard in this system is built from the same six parts, in this order.
A screen may omit any of them; it may not invent a seventh.

| Part | Class | One definition lives at |
| --- | --- | --- |
| KPI row | `.stat-grid` + `.stat-card` + `.stat-label`/`.stat-value`/`.stat-meta` + `.stat-accent` | `verichat-theme.css` (compat aliases `.vc-kpi-card*`, `.vc-portal-kpi*` point at the same rules) |
| Panel | `.vc-card` (aliases `.card`, `.vc-table-wrap`, `.stat-card`) | `verichat-theme.css` |
| Badge | `.badge-*` (aliases `.vc-badge--*`) | `verichat-theme.css` |
| Table | `.data-table` inside `.vc-table-wrap` | `verichat-theme.css` |
| Timeline / activity feed | `.activity-list` + `.activity-item` + `.activity-dot` (aliases `.timeline*`, `.vc-timeline`) | `verichat-theme.css` |
| Chart frame | `.chart-grid` + `.chart-card-canvas` | `verichat-theme.css` |

**Uniqueness rule.** Two role dashboards must not be the same screen with different
nouns. What makes them different is *which parts they lead with and which they omit
entirely*, not a different card radius:

| Screen | Audience | Leads with | Deliberately has no |
| --- | --- | --- | --- |
| `Dashboard/Index.cshtml` | System Administrator | KPI row → two-up charts → activity timeline | per-role actions; it is the whole-business view |
| `Pipeline/Index.cshtml` | Sales Representative | goal progress → kanban board | charts; a pipeline is read as columns, not as a curve |
| `Procurement/Dashboard.cshtml` | Procurement Manager | low-stock KPIs → spend chart → activity list | ticket or shipment rows; those are not this role's queue |
| `Support/Dashboard.cshtml` | Field Support Engineer | open/escalated/SLA KPIs → **ticket table** | charts; support is a queue, and a queue is a table |
| `Logistics/Tracker.cshtml` + `Logistics/Assets.cshtml` | Logistics | shipment cards, then asset KPIs + table | charts |
| `Portal/Index.cshtml` | Client User | hero + own four KPIs → quotations/tickets split → deliveries | tables, charts and any other client's data |
| `Portal/Index.cshtml` (student variant) | Medical Student | same shell, training-context panel and the student's own requests | supplier pricing and Accept/Decline on quotes |

Where two roles share a permission — Field Support Engineer and Logistics both hold
`Perm.SupportDashboard` (`Authorization/Perm.cs:99,107`) — they must not be handed a
byte-identical screen; the sections are branched on the role claim.

**Consistency rule.** A screen gets its own layout, never its own palette, radius,
shadow or type scale. The mechanism is the token set in §2.1: if a value is not a
token, it is not a design decision, it is a typo that survived review.

---

## 5. The landing page is a deliberate second system

`pages/landing.css` keeps its own `--lnd-*` namespace, scoped to `.lnd` and
therefore invisible to the app, and `_LandingLayout.cshtml` loads no sidebar, no
topbar, no jQuery, no Bootstrap bundle and no SignalR — see its header comment.
It shares exactly two tokens with the product: the heading and body font stacks.

That separation is intentional and should be preserved: a marketing page optimises
for a visitor who has never authenticated, and the app optimises for someone who
has. What must not be duplicated is the brand — `--lnd-navy` is `--vc-navy`, and
`--lnd-blue` is `--vc-accent`, both restated as literals
(`landing.css:10,16`).

`pages/portal.css` and `pages/auth.css` are **not** second systems: 47 and 16
`var(--vc-*)` references respectively, two and three literal hexes. The client
portal is the same product, so it uses the same tokens.

---

## 6. Defect register

Found by tracing every asset reference and every style rule, 1 Nov 2026. Status is
as of the commit that added this file unless noted.

| # | Defect | Evidence | Status |
| --- | --- | --- | --- |
| 1 | Nine classes are used by views and defined nowhere, so those screens render with browser defaults | `.data-table` (7 views), `.empty-state` (2), `.kpi-grid`/`.kpi-card`/`.kpi-label`/`.kpi-value` (2), `.timeline*` and `.chart-grid`/`.chart-card-canvas` (`Dashboard/Index.cshtml:9-31`), `.status-text` (2), `.alert-icon` (2), `.route-label` | **[Fixed]** — defined once in `verichat-theme.css` |
| 2 | `.vc-accent` sets `color:#5B5BD6`, a purple that is not in the palette, while `--vc-accent` is `#2F6FED` | `verichat-theme.css:136` vs `:7`; applied to the KPI value on five dashboards | **[Fixed]** |
| 3 | `.badge-info` references two undeclared tokens, so Medium-priority badges have no background | `verichat-theme.css:172`; users `Support/Dashboard.cshtml:66`, `TicketHub.cshtml:47`, `Logistics/Tracker.cshtml:40` | **[Fixed]** |
| 4 | `.vc-icon-btn` is defined twice and the second block silently restyles the first | `verichat-theme.css:97` vs `:251` | **[Fixed]** |
| 5 | The card recipe is declared eleven times and the KPI recipe four times | `verichat-theme.css:101,130,137,149,165,168`; `pages/portal.css:45,82,167,189,237,276`; `_StatusCard.cshtml:12` | **[Partly fixed]** — one canonical rule per component, aliases grouped onto it |
| 6 | ~31 per-cell `style="padding:10px"` attributes exist because `.data-table` was undefined | `Inquiry/Index.cshtml:72-109`, `Sales/Leads.cshtml:48-85` | **[Fixed]** |
| 7 | The same inline toast style string is duplicated in two views instead of using `.vc-notification-toast` | `Pipeline/Index.cshtml:10`, `Sales/Appointments.cshtml:8` | **[Fixed]** |
| 8 | Eight separate badge-class switch statements | `Support/Dashboard.cshtml:62`, `TicketHub.cshtml:43`, `Portal/Index.cshtml:5`, `Quotations.cshtml:6`, `Tickets.cshtml:6`, `Tracker.cshtml:36`, `WarehouseAlerts.cshtml:78,84`, `Assets.cshtml:30` | **[Fixed]** — one `BadgeClass` helper |
| 9 | The mobile cold-start splash shows **the Expo logo** on web and a cream Canva re-export on native | `animated-icon.web.tsx:69` → `expo-logo.png`; `animated-icon.tsx:36,107` → `canva/design_pt.png` | **[Fixed]** |
| 10 | The iOS app icon is still the Expo default blue "V" chevron | `app.json` `ios.icon` → `./assets/expo.icon` | **[Open]** — needs a generated icon set, see §6.1 |
| 11 | The Android adaptive monochrome layer is a byte-identical copy of the faint hairline foreground; a monochrome layer must be a solid silhouette | `android-icon-monochrome.png` md5 == `android-icon-foreground.png` | **[Open]** |
| 12 | The favicon predates the rebrand and no layout declares `<link rel="icon">` | `wwwroot/favicon.ico` from `bfc1663` | **[Partly fixed]** — PNG icon declared in all three layouts; the `.ico` itself is not regenerated |
| 13 | The app icon's mark fills ~30 % of a 1024 px canvas, so it reads as a dot at launcher size | `assets/images/icon.png` | **[Open]** |
| 14 | `images/logo.png` duplicates `brand/verichat-mark.png` byte for byte | md5 match | **[Works]** — kept deliberately, §1.2 |
| 15 | Three taglines for one brand | §1.4 | **[Open]** — a copy decision, not a code one |
| 16 | `#F4F3EF` (logo plate) is declared three times and is not a token | `verichat-theme.css:78`, `auth.css:33`, `landing.css:111` | **[Fixed]** |
| 17 | The sidebar is hidden at ≤ 1024 px with nothing to bring it back | `verichat-theme.css:140` | **[Fixed]** — topbar toggle added |
| 18 | The theme's `[aria-current="page"]` rule is dead because `_Sidebar.cshtml` never sets it | `verichat-theme.css:84` | **[Fixed]** |

### 6.1 Why 10–13 are open

They need *artwork*, not a code change: a 1024 px iOS icon with the mark at
launcher scale, a solid single-colour silhouette for the Android monochrome layer,
and a re-cut `.ico`. Regenerating them means producing new bitmaps and rebuilding
the native app, which is an EAS build rather than a commit, and the mark's exact
serif geometry should not be re-drawn by approximation. The Expo defaults are
removed from every surface that a rebuild does not have to author; these three are
where the defaults remain until someone with the original vector supplies it.

---

## 7. Rules for new screens

1. Lead with §4's six parts. If a screen needs a seventh, the pattern belongs in
   `verichat-theme.css` first, with one definition.
2. Colour, radius, shadow and type come from tokens. A literal hex in a view is a
   bug unless it is in this file.
3. Web and mobile change palette together, in one commit.
4. The mark goes in chrome; the wordmark goes in statements. Never both on one
   screen, and never a Canva export.
5. A role's dashboard earns its own screen by what it shows first and what it
   refuses to show — not by a different card.
