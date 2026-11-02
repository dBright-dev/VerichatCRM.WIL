/**
 * The single rule that decides whether a release build can reach the API at all.
 *
 * This lives on its own, with no React, React Native or Expo imports, because the
 * check it replaced used to `throw` at module scope inside api.ts. A throw during
 * module evaluation happens before any component exists, so no error boundary and
 * no route can catch it: a misconfigured signed build closed the moment it was
 * opened, which is a worse outcome for the person holding the phone than the
 * silent spinner the throw was written to avoid. Same verdict, returned as text
 * so a screen can render it.
 */

export type ReleaseOriginInput = {
  /** The bundle's `__DEV__` flag. */
  isDev: boolean
  /** The platform string, e.g. `ios`, `android`, `web`. */
  platform: string
  /** The resolved API base URL. */
  baseUrl: string
}

/**
 * A sentence explaining why this build cannot connect, or `null` when it can.
 *
 * Development bundles and the static web export are exempt on purpose: both can
 * reach an HTTP host, and the web bundle is how the app gets reviewed without a
 * device. Android release builds are exempt too, because `app.json` hands
 * `usesCleartextTraffic: true` to `expo-build-properties`, so the generated
 * manifest asks Android to permit cleartext. That is a deliberate, temporary
 * concession to the API host having no certificate — it means the sign-in
 * password and the JWT cross the network unreadable by nobody but whoever is
 * listening to it, so it should go the moment HTTPS is bound.
 *
 * iOS has no equivalent switch, so a cleartext origin there still fails every
 * request and this returns the sentence explaining it.
 */
export function describeReleaseOriginProblem({ isDev, platform, baseUrl }: ReleaseOriginInput): string | null {
  if (isDev || platform === 'web' || platform === 'android') return null
  if (!baseUrl.startsWith('http://')) return null

  return (
    `This build resolves the API to ${baseUrl}, served over plain HTTP, and iOS refuses a ` +
    'cleartext request outright, so every screen would load empty. ' +
    'Set EXPO_PUBLIC_API_URL to an HTTPS API origin before building, or bind a certificate to the host.'
  )
}
