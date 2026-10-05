import * as SecureStore from 'expo-secure-store';
import { Platform } from 'react-native';

// Verichat CRM API — shared base URL resolution.
// Set EXPO_PUBLIC_API_URL in your .env / EAS config; falls back to the
// deployed production API so the app works out of the box.
export const API_BASE_URL =
  (process.env.EXPO_PUBLIC_API_URL?.trim().replace(/\/+$/, '') ||
    'http://verichat-crm-api.runasp.net')

const TOKEN_KEY = 'verichat.auth.token'

/**
 * JWT persistence. The token lives in the iOS Keychain / Android Keystore via
 * expo-secure-store; on web we fall back to localStorage because
 * expo-secure-store has no web implementation.
 */
async function readStoredToken(): Promise<string | null> {
  try {
    if (Platform.OS === 'web') {
      return globalThis.localStorage?.getItem(TOKEN_KEY) ?? null
    }
    return await SecureStore.getItemAsync(TOKEN_KEY)
  } catch {
    // Corrupt/unavailable keystore — treat as signed out rather than crashing.
    return null
  }
}

async function writeStoredToken(token: string): Promise<void> {
  try {
    if (Platform.OS === 'web') {
      globalThis.localStorage?.setItem(TOKEN_KEY, token)
      return
    }
    await SecureStore.setItemAsync(TOKEN_KEY, token)
  } catch {
    // Persistence is best-effort; the in-memory token still works this session.
  }
}

async function clearStoredToken(): Promise<void> {
  try {
    if (Platform.OS === 'web') {
      globalThis.localStorage?.removeItem(TOKEN_KEY)
      return
    }
    await SecureStore.deleteItemAsync(TOKEN_KEY)
  } catch {
    // Ignore.
  }
}

let authToken: string | null = null
const authListeners = new Set<(token: string | null) => void>()

/** Called by the login/register screens after a successful auth call. */
export function setAuthToken(token: string | null) {
  authToken = token
  authListeners.forEach((listener) => listener(token))
  // Persist in the background so the UI is never blocked on the keystore.
  if (token) void writeStoredToken(token)
  else void clearStoredToken()
}

/**
 * Restores a previously persisted JWT on cold start. Resolves once the token
 * state is settled so the auth gate can render the right navigator.
 */
export async function hydrateAuthToken(): Promise<string | null> {
  if (authToken) return authToken
  const stored = await readStoredToken()
  authToken = stored
  authListeners.forEach((listener) => listener(stored))
  return stored
}

export function subscribeAuth(listener: (token: string | null) => void) {
  authListeners.add(listener)
  return () => { authListeners.delete(listener) }
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
  let response: Response
  try {
    response = await fetch(`${API_BASE_URL}${path}`, {
      ...init,
      headers: authHeaders(init?.headers as Record<string, string> | undefined),
    })
  } catch {
    throw new Error('Could not connect to Verichat. Check your internet connection and try again.')
  }

  if (!response.ok) {
    let detail = ''
    try {
      detail = await response.text()
    } catch {
      /* ignore */
    }
    if (response.status === 401 && authToken) setAuthToken(null)
    let message = detail
    try {
      const parsed = JSON.parse(detail) as { message?: string; title?: string; error?: string }
      message = parsed.message ?? parsed.title ?? parsed.error ?? detail
    } catch { /* response was plain text */ }
    throw new Error(message || `Request failed (${response.status}).`)
  }

  if (response.status === 204) return undefined as T
  const contentType = response.headers.get('content-type') ?? ''
  if (!contentType.includes('json')) return undefined as T
  return response.json()
}
