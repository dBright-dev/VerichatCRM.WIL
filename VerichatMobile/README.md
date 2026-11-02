# Verichat Mobile

Expo Router client for the Verichat CRM API. The app targets iOS, Android, and web, and uses the same API-backed catalogue, client account, order, and quotation flows across supported platforms.

For the complete system architecture, database configuration, security notes, API/web setup, and test commands, see the [repository README](../README.md).

## Run locally

```bash
npm ci
```

Set `EXPO_PUBLIC_API_URL` to an API URL reachable from the selected device, then start Expo:

```bash
npx expo start
```

The default URL is the configured hosted Verichat API. For a physical phone, use the development computer's LAN address and a trusted HTTPS certificate; `localhost` on the phone points to the phone itself.

## Checks

```bash
npx expo lint
npx tsc --noEmit
```

Routes live under `src/app/`; API and session helpers live under `src/constants/`. Route files are screens and should not be used for general-purpose shared components.


## Publish a release

Everything below is done with EAS in the cloud — no local Xcode or Android Studio. Run the CLI as `npx eas-cli <command>`.

### Android works over cleartext; iOS does not

**The API host still has no TLS certificate**, and the two platforms do not forgive that the same way.

- **Android** release builds now reach `http://api-site.runasp.net`. `app.json` passes `android.usesCleartextTraffic: true` to `expo-build-properties`, which writes `android:usesCleartextTraffic` into the generated manifest — Android blocks cleartext from API 28 onward unless the app asks not to. The cost is stated plainly: the sign-in password and the JWT cross the network in the clear, so anyone positioned on the same network can read them. This is a concession to a missing certificate, not a design, and it should come out of `app.json` on the same commit that flips the origin to `https://`. Google's Play Console review may ask why cleartext is enabled; the answer is that the API has no certificate yet, which is also why it should not stay that way.
- **iOS** has no equivalent switch. A release build that resolves to an `http://` origin therefore opens on a full-screen notice naming the setting that is wrong, instead of loading empty screens. `RELEASE_ORIGIN_PROBLEM` comes from `describeReleaseOriginProblem()` in `src/constants/releaseGuard.ts` — a module with no React Native imports, so the rule can be run and checked outside a bundle — and `src/app/_layout.tsx` renders it. **This used to be a `throw` at module scope, and that was worse than the bug it was guarding:** a throw during module evaluation happens before any component or error boundary exists, so a misconfigured build closed the instant it was opened, and the person holding the phone could not reach even the privacy notice.

Development bundles and the static web export are exempt from the guard, because both can reach an `http` host.

Measured on 2 Nov: `https://api-site.runasp.net/api/health` fails at the TLS handshake (port 443 accepts the TCP connection, then `schannel: failed to receive handshake`), while the same path over `http://` answers. Binding a certificate there is what retires both the manifest flag and the iOS notice — `DEPLOYMENT.md` §5 is that runbook.

Two related prerequisites, both outside this repository: the deployed web site has to be rebuilt and published, because `/privacy` answers 404 there today (the policy exists in the code and returns 200 locally, but the deployed build predates it) and both stores require a reachable privacy-policy URL; and the deployed API is 6 migrations behind, so it starts and reports `Degraded` until `Database/apply-pending-migrations.sql` is run against the hosted database — `DEPLOYMENT.md` §7 and task "Migrate the hosted database" cover that.

### The backup origin, and why it is unset

`EXPO_PUBLIC_API_BACKUP_URL` names a second origin a request is retried against when the first cannot serve it, mirroring `BackupApiBaseUrl` on the web tier. It is deliberately **not set** in any profile today, because the Azure copy of the API has no database it can reach — it answers `503` with `CanConnectAsync returned false`. Failing over to it would trade an honest error for a screen full of nothing, which is a worse outcome than not failing over at all.

Set it once the Azure App Service has a working database shared with the primary (so both hosts read the same rows and validate the same `Jwt:Key` — see `DEPLOYMENT.md` §7.2). The retry rules live in `fetchWithFailover()` in `src/constants/api.ts`: a read is replayed on any of `500/502/503/504` or a socket failure, a write only when the primary proved it never ran the handler, because replaying a `500` would apply the same change twice.


### Build

```bash
npx expo-doctor          # dependency and configuration diagnosis
npx expo lint
npx tsc --noEmit
npx eas-cli login
npx eas-cli build --platform ios --profile production
npx eas-cli build --platform android --profile production
```

`eas.json` sets `appVersionSource: "remote"`, so EAS owns the version numbers: `expo.version` in `app.json` is the marketing version and `autoIncrement` on the production profile bumps the iOS build number and the Android `versionCode` for you. Do not hand-edit `ios.buildNumber` or add an Android `versionCode` to a build you intend to submit twice — the store rejects a binary whose version is not higher than the one already on it.

Two things about the checks above, both found by running them:

- `npx tsc --noEmit` reads `.expo/types/router.d.ts`, which Expo *generates* — it is not in the repository. On a fresh clone that file either does not exist or predates the newest screens, and the typecheck fails with about ten `TS2345` errors on routes such as `/privacy`, `/forgotPassword`, `/resetPassword` and `/loginHelp` because they are missing from the typed-route union. Run `npx expo start` once (or `npx expo export`) to regenerate it before trusting a typecheck failure. With the file regenerated, this project typechecks clean and `npx expo export --platform web` produces a release bundle.
- `npx expo-doctor` reports five packages a few patch versions behind inside SDK 57 (`expo`, `expo-router`, `expo-linking`, `expo-splash-screen`, `@expo/ui`). Left alone deliberately: `npx expo install --fix` is the right command, but it is not the right thing to run against a dependency tree on the eve of a store submission. Take the upgrades on their own commit, then rebuild and re-test.

`app.json` carries the identity the stores key on: `expo.ios.bundleIdentifier` is `com.dbright.Verichat`, matching `expo.android.package` and the `namespace`/`applicationId` in the checked-in Android project. Both must match the App Store Connect record and the Play package exactly and cannot be changed after the first upload. `expo.owner` is `dbright`, the EAS account that has to run the build.

### Submit

```bash
npx eas-cli submit --platform ios --profile production
npx eas-cli submit --platform android --profile production
```

Credentials go in the EAS secret store or in the `submit.production` block of `eas.json` as *references* — never as committed files. For iOS the values are `ascAppId`, `ascApiKeyId`, `ascApiKeyIssuerId` and `ascApiKeyPath` (an App Store Connect API key, which avoids having to store an Apple ID password); for Android they are `serviceAccountKeyPath`, `track` and `releaseStatus`, from a Google Play service account with permission to publish. `eas-cli submit --help` prints the current spelling of each; do not trust a blog post's version of these key names.

### What the stores will ask that is not in this repository

- **Privacy policy URL.** The published page is `/privacy` on the web origin. It must be reachable from the internet before either listing is reviewed.
- **Data-safety / privacy declarations.** The app stores the session token in the device keychain (`expo-secure-store`) and caches orders and enquiries locally, and it sends push tokens when a device registers. Both stores ask you to declare that yourself; the code will not fill the form in.
- **A login for the reviewer.** This is the part that bites. The web tier's per-role sign-in accounts are created only on a Development host with `Bootstrap:DemoUsers=true`, so on the deployed system they do not exist — and a self-registered account cannot be used either, because it sits in the approval queue until a staff member releases it. Create one real account in the deployed database, approve it, and give its address to App Store Connect's review notes and to Play's tester field. Do not reuse a customer's credentials.
- **Encryption export compliance.** `app.json` sets `ios.infoPlist.ITSAppUsesNonExemptEncryption` to `false`, which is the answer for an app whose only protection is standard HTTPS. That is a declaration about Verichat's actual crypto, so it belongs to whoever owns the deployment — if the app ever ships its own encryption, change it to `true` and prepare the paperwork.
- **Icon.** iOS now builds its icon set from `assets/images/icon.png` (1024×1024, the Verichat mark on dark navy). It previously pointed at `assets/expo.icon`, which is the unmodified Expo template — an iPhone would have installed the app under the Expo logo. Android's adaptive icon was already correct: a white mark on `#0D141D`, supplied as three 1082×1082 layers.
