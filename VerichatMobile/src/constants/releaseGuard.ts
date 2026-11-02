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
 * device. Everything else that resolves to a cleartext origin is a build that
 * will fail every request — iOS refuses cleartext outright and Android does too
 * unless the manifest opts back in.
 */
export function describeReleaseOriginProblem({ isDev, platform, baseUrl }: ReleaseOriginInput): string | null {
  if (isDev || platform === 'web') return null
  if (!baseUrl.startsWith('http://')) return null

  return (
    `This build resolves the API to ${baseUrl}, served over plain HTTP. ` +
    'A release build is not allowed to send a cleartext request, so every screen would load empty. ' +
    'Set EXPO_PUBLIC_API_URL to an HTTPS API origin before building, or bind a certificate to the host.'
  )
}
