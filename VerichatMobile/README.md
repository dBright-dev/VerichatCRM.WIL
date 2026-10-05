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

