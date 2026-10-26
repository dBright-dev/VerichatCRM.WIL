# VeriChat Mobile feature and data-flow guide

This guide documents the current Expo Router app under `VerichatMobile`. It describes implemented behavior and source-code contracts; it does not claim that a deployed API or database is reachable, that a migration has run in production, or that server-pushed mobile notifications are configured.

## Product scope

VeriChat Mobile is a B2B equipment supply and field operations companion. The client-facing workflows are equipment catalogue browsing, quote requests, consultation booking, registered equipment and service-ticket creation, client history, account settings, and CRM account access. Sales quota goals are separate from wellness goals. The health/vitals, exercise, prescription/lab-result, device-sync, and Bluetooth user flows have been removed from mobile navigation and app modules. Existing legacy API/database wellness code and historical migrations may remain in the server repository; their presence does not make them part of the current app workflow.

## Systems and persistence

- **Mobile client:** Expo / React Native in this folder. Screens are Expo Router routes in `src/app`; shared UI and services are outside `src/app`.
- **API:** `VerichatCRM.API`. The app's API helper reads `EXPO_PUBLIC_API_URL` (or its configured fallback) and sends the saved bearer token. See `src/constants/api.ts`.
- **Database:** API configuration uses EF Core `AppDbContext` and SQL Server (`ConnectionStrings:DefaultConnection` in API configuration). Route data is persisted only where the corresponding controller/service saves through that context. The database server and database name are deployment configuration, not mobile constants.
- **Web:** `VerichatCRM.Web` calls the same API with its authenticated session. The appointment list and sales pipeline display appointment and quotation records created by the API.
- **Local state:** Auth tokens, UI preferences, and unsent cart state are held on device/browser storage. Those values are not CRM records until the app submits them to an API.

## Visible navigation

`src/components/app-tabs.tsx` defines five native tabs: **Home**, **Catalogue**, **Consult**, **My Assets**, and **Profile**. The Catalogue reads live products available to request; My Assets lists equipment already assigned to the client. `src/components/app-tabs.web.tsx` provides the web-specific tab UI. Other routes are hidden from the tab bar but can be opened via Expo Router. Active tab icons use a short spring bounce.

These two inventory screens have different jobs: **Catalogue** is the API-backed set of products the client can ask the sales team to quote; **My Assets** is the client's installed/purchased equipment with serial number, status, warranty/service dates, QR lookup, and ticket action. The Home screen now shows only a compact asset count and service-due count, linking to My Assets for the full list, so it no longer repeats the asset cards.

## Route and component inventory

### `src/app`

| File | Responsibility and data flow |
| --- | --- |
| `_layout.tsx` | Root Expo Router/auth layout. Hydrates the saved session, chooses routes, applies the app theme, and starts foreground refresh/background sync helpers where configured. |
| `index.tsx` | Auth gate and Home route. Shows the role/login entry when signed out and client home when signed in. |
| `clientLogin.tsx`, `adminLogin.tsx` | Client and staff login forms; both use `src/constants/authService.ts` → `POST /api/auth/login`. Client sign-in is for the `Client User` role. Staff/Admin sign-in accepts only System Administrator, Sales Representative, Procurement Manager, or Field Support Engineer. The API validates credentials and issues a JWT; the mobile app validates the role before saving the session. |
| `newUserAccount.tsx` | Client registration; uses auth service → `POST /api/mobile/register`. API creates identity/client records. |
| `clientHomescreen.tsx` | Client home summary, equipment/quote shortcuts, and consultation status. Reads dashboard, assets, quotations, and the caller's appointments from the API. Consultation cards update on foreground refresh; the profile palette is shared with the main app tabs. |
| `Dashboard.tsx`, `DashboardScreen.tsx` | `Dashboard.tsx` routes to the operations overview. `DashboardScreen` loads equipment, tickets, and delivery data from API routes, refreshes on foreground, and shows sales goals from `GoalsScreen.tsx` → `/api/salesgoals/my-goals`. |
| `medicalCatalogueShop.tsx` | Equipment catalogue UI (legacy filename). Uses `catalogueService.ts` → `GET /api/product`; product rows come from the CRM Products table through the API. Catalog refreshes on focus, pull-to-refresh, and app foreground; failed network requests do not silently replace current catalogue data with old cached rows. Adds selected rows to the local quote cart. |
| `cartScreen.tsx` | Quote request cart and order/quote history. Cart contents are local until submitted. `POST /api/quotation/request` creates a Draft quotation from current server-side product/SKU data; only Client User accounts may submit. `GET /api/mobile/quotations` and `GET /api/mobile/orders` load records scoped to the signed-in user's ID. Order and quote reads are independent and display which endpoint failed. After staff marks a quote Sent, the client can Accept or Decline it through `PATCH /api/mobile/quotations/{id}/response`. |
| `BookAppointmentScreen.tsx` | Client consultation topic/date/notes form and tracker. `POST /api/appointments` creates a Pending CRM Appointment; `GET /api/appointments` returns requests according to the caller's role. The client sees Pending → Approved/Declined → Completed and can open a representative-supplied Teams/Zoom/other HTTPS link. The API does not provision a meeting provider or create a call automatically. |
| `EquipmentStatusScreen.tsx` | Lists the signed-in client's registered equipment via `GET /api/asset/mine`, supports QR/barcode lookup against those records, and creates a support ticket via `POST /api/supportticket`. API verifies the client owns a submitted equipment ID. Equipment and tickets are CRM database records. |
| `GoalsScreen.tsx` | Sales Goals card, fetch and progress update. `GET /api/salesgoals/my-goals` and `PATCH /api/salesgoals/{id}/progress`; values are stored in the API's SalesGoals table. |
| `ClientTimelineScreen.tsx` | Chat-style client history and note composer. Reads `GET /api/client/{clientId}/timeline`; posts `POST /api/client/{clientId}/notes`. API aggregates tickets, ticket activities, inquiries, appointments, quotations, and timeline notes from CRM storage. Staff access is role-gated by API; client access is restricted to their own client ID. Refreshes on the app-wide foreground event. |
| `ClientCommunicationScreen.tsx` | Older communication presentation retained as a hidden route; it is not the canonical data-backed timeline. Use `ClientTimelineScreen.tsx` for the new unified CRM timeline. |
| `profileScreen.tsx` | Account profile and navigation links, including cart/settings. The user can edit account contact name, phone, company, VAT number, and delivery address; changes are saved through `profileService.ts` → `PATCH /api/mobile/profile`. The sign-in email is displayed read-only because it is the login identity, not an editable profile field in this API contract. |
| `settingsScreen.tsx` | Notification preference, biometric opt-in, and theme controls. Server settings use `/api/mobile/settings`; theme/biometric preferences also have device-local behavior. |
| `explore.tsx` | Expo starter/example route retained in the source; not a CRM data feature. |

### `src/components`

| File/folder | Responsibility |
| --- | --- |
| `app-tabs.tsx`, `app-tabs.web.tsx` | Native and web tab navigation; lists visible CRM tabs and hides secondary routes. |
| `ui/AnimatedButton.tsx` | Reusable press-scale/animated button, loading state, and optional sheen; individual screens may still use React Native `TouchableOpacity` directly. |
| `ui/BrandLoader.tsx` | Verichat-branded loading treatment used by data screens. |
| `ui/SkeletonCard.tsx` | Equipment/list placeholder used while dashboard content loads. |
| `ui/FloatingInput.tsx` | Reusable animated floating-label input. It is available to screens that adopt it; it is not automatically applied to every input. |
| `barcode-scanner-modal.tsx` | Camera QR/barcode scanner UI; decoded values are matched against app/API equipment records. It does not establish a Bluetooth connection. |
| `offline-status-banner.tsx` | Connectivity status presentation. |
| `animated-icon.tsx`, `animated-icon.web.tsx`, `animated-icon.module.css` | Platform-specific animated icon presentation. |
| `external-link.tsx`, `hint-row.tsx`, `themed-text.tsx`, `themed-view.tsx`, `web-badge.tsx`, `ui/collapsible.tsx` | Shared starter/design utility components; they do not create CRM records or call business endpoints themselves. |

### `src/constants`

| File | Responsibility and API/database relationship |
| --- | --- |
| `api.ts` | Shared API transport, API base URL, bearer auth, local caching/offline queue support, and token storage. It does not itself define a database. |
| `authService.ts` | Login/register/logout wrappers; `/api/auth/login`, `/api/auth/register`, `/api/mobile/register`. Identity records are API-side. |
| `catalogueService.ts` | Product (`GET /api/product`), reorder (`POST /api/mobile/reorder/{sku}`), orders (`GET/POST /api/mobile/orders`), quote create (`POST /api/quotation/request`) and quote list (`GET /api/mobile/quotations`) wrappers. Products/quotes/orders are CRM-side records. |
| `cartStore.ts` | Local persisted quote-cart state; rows are not a database quotation until request submission succeeds. |
| `dashboardService.ts` | Dashboard API wrapper for the mobile dashboard data contract. |
| `profileService.ts` | Profile GET/PATCH wrappers for `/api/mobile/profile`. |
| `settingsService.ts` | Settings GET/PATCH wrappers for `/api/mobile/settings`. |
| `appTheme.ts`, `theme.ts`, `crmTheme.ts` | Theme preference and shared palette. `crmTheme.ts` centralizes the profile's navy/blue/slate brand colors for the main mobile flows and the bottom tab bar. Theme preference is also stored locally. |
| `biometricAuth.ts` | Device biometric capability and prompt helper. Biometric templates are managed by OS; fingerprint/face data is not sent to CRM. |

### `src/utils`, `src/hooks`, `src/types`, and styles

- `utils/refreshEvents.ts` provides the in-process global refresh event used when the app becomes active. Screens that subscribe (including the timeline) can silently reload while mounted.
- `utils/backgroundFetch.ts` contains the background assignment refresh registration. Mobile OS background execution is best-effort and is not guaranteed to run at a particular interval; it is not a replacement for push delivery.
- `hooks/use-color-scheme.ts`, `.web.ts`, and `use-theme.ts` resolve platform theme state.
- `types/styles.d.ts` types style/module imports; `global.css` holds app-wide web styles.
- `services/NotificationsService.ts`, `PushNotifications.ts`, and `RealTimeTracker.ts` are service modules, not screens. Their presence/configuration alone does not prove remote push registration, a connected mobile SignalR client, or delivery on a closed app.
- `services/ClientCommunicationScreenStyles.ts` supplies styles to the retained older communication screen.

## Server and web contracts for the principal flows

| Flow | Mobile API route(s) | API implementation and persistence | Web result |
| --- | --- | --- | --- |
| Equipment catalogue | `GET /api/product` | Product API/service reads CRM `Products`. | Same catalogue is used in CRM management. |
| Quote request | `POST /api/quotation/request`; `GET /api/mobile/quotations` | `QuotationController`; authenticated client request recalculates product prices from current `Products`, creates `Quotation` and line items, and links/creates a `Deal`. The list endpoint returns quotes for the caller's client ID. | `PipelineController` and `Views/Pipeline/Index.cshtml` show draft mobile requests; API emits a SignalR staff event. |
| Appointment | `POST/GET /api/appointments`; staff `PATCH /api/appointments/{id}`; authorized staff `GET /api/appointments/assignees` | `AppointmentController` persists `Appointment`; client GET is self-scoped, staff GET lists according to role/assignment. State transitions are Pending → Approved/Declined → Completed. The nullable `MeetingUrl` is stored in SQL Server after migration `20261025174339_AddAppointmentMeetingLinks`. | Admin, sales, and procurement roles open **Sales → Appointments** (`TelehealthController` → `Views/Sales/Appointments.cshtml`) to assign a sales rep/field engineer, approve or decline, save a meeting link, and mark an approved appointment Completed after the call. Assigned field support users can view their assigned requests. The API sends SignalR notices; no video-call provider is integrated, so reps create the call in their chosen provider and paste its join URL. |
| Mobile quote review | Client `POST /api/quotation/request`; staff `PATCH /api/quotation/{id}/status`; client `PATCH /api/mobile/quotations/{id}/response` | `QuotationController` persists Draft → Reviewed → Sent → Accepted/Rejected in the existing Quotations table. Product lines/prices are verified from the current Products rows at submission. | Sales Representatives and System Administrators use **Pipeline → Mobile quote requests** to review, then send the verified quote. Staff notifications are sent to System Administrator, Sales Representative, and Procurement Manager groups. The client sees Sent status and can accept/decline in the app. A SignalR outage is logged but does not make a saved quote appear to have failed. |
| Equipment | `GET /api/asset/mine`, `GET /api/asset/{assetId}` | `AssetController` uses asset service/CRM `Assets`; client access is constrained to their own records. | Existing web asset views/services share the API. |
| Service ticket | `POST /api/supportticket`, `GET /api/supportticket/client/{clientId}` | `SupportTicketController` sets the client ID from the JWT, checks equipment ownership, and creates ticket through ticket service. Client ticket-list access is self-scoped. | Existing ticket workflow surfaces the created CRM ticket. |
| Client timeline | `GET /api/client/{clientId}/timeline`, `POST /api/client/{clientId}/notes` | `ClientController`; combines CRM ticket/activity/inquiry/appointment/quotation rows and persists notes in `ClientTimelineNotes`. | The same timeline API can be read by web clients that call it; SignalR note update is emitted to the client's user group. |
| Sales goals | `GET /api/salesgoals/my-goals`, `PATCH /api/salesgoals/{id}/progress`; admin GET/POST | `SalesGoalsController` persists target, period, and achieved amount in `SalesGoals`. | API is available for web integration; this change does not add a dedicated goals page. |
| Orders | `GET/POST /api/mobile/orders`; `POST /api/mobile/reorder/{sku}` | Mobile account/order APIs and CRM order tables. This remains available separately from quote request cart. | Existing order management workflow. |
| App refresh | no new data route of its own; reloads active screen API calls | Foreground event is local. Background task checks dashboard/assignment data when OS permits. | N/A |

## Important implementation boundaries

- The quote request currently uses the standard catalogue `Product.Price`; the schema inspected here does not provide client-specific contract pricing. The app explicitly labels the displayed amount as an estimate and asks the account manager to confirm pricing/availability.
- Appointment booking creates a CRM request and the web page can manage its status. It does not itself create a video meeting room.
- Asset QR lookup filters the client's equipment list. It does not read machine telemetry or connect to BLE devices.
- The API emits SignalR notifications for staff appointment/quote updates, and the web view connects to the notifications hub. Mobile appointment status is fetched again when the route regains focus or the app returns to foreground; native mobile SignalR/push delivery is not established by these screens.
- Database migrations in `VerichatCRM.Infrastructure/Migrations` must be applied by the API/deployment before the new Appointment and SalesGoal tables can be used.
- Animation dependencies that were unavailable to install in this environment are not claimed as implemented. Existing buttons/loaders/skeletons and calendar are built from currently installed React Native/Expo components.

## Removed mobile health modules

Deleted mobile screens/services include wellness tracking, vitals, health goals/exercise, BLE device sync, lab results, prescriptions, and telehealth-only appointment forms. `react-native-ble-plx` was removed from mobile dependencies/config. The CRM API still contains some historical wellness/device models and routes; those are legacy server code and are not referenced by the current mobile app.
