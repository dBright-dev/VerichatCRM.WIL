// Verichat CRM API — shared base URL resolution.
// Set EXPO_PUBLIC_API_URL in your .env / EAS config; falls back to the
// deployed production API so the app works out of the box.
export const API_BASE_URL =
  process.env.EXPO_PUBLIC_API_URL?.replace(/\/$/, '') ??
  'http://verichat-crm-api.runasp.net'

let authToken: string | null = null

/** Called by the login/register screens after a successful auth call. */
export function setAuthToken(token: string | null) {
  authToken = token
}

/** Current JWT (if a user is signed in). */
export function getAuthToken(): string | null {
  return authToken
}

function authHeaders(extra?: Record<string, string>): Record<string, string> {
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    ...(extra ?? {}),
  }
  if (authToken) headers.Authorization = `Bearer ${authToken}`
  return headers
}

/** Typed fetch wrapper: attaches JWT, throws a readable Error on non-2xx. */
export async function api<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...init,
    headers: authHeaders(init?.headers as Record<string, string> | undefined),
  })

  if (!response.ok) {
    let detail = ''
    try {
      detail = await response.text()
    } catch {
      /* ignore */
    }
    throw new Error(
      detail?.length > 0 ? detail : `Request failed (${response.status}).`,
    )
  }

  return response.status === 204 ? (undefined as T) : response.json()
}

/** Direct base-URL access for one-off login/register calls. */
export function apiUrl(path: string): string {
  return `${API_BASE_URL}${path}`
}
