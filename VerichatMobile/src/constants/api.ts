import * as SecureStore from 'expo-secure-store';
import { Platform } from 'react-native';
import AsyncStorage from '@react-native-async-storage/async-storage';
import NetInfo from '@react-native-community/netinfo';
import { authenticateBiometric, hasUsableBiometrics } from './biometricAuth';

// Verichat CRM API — shared base URL resolution.
// Set EXPO_PUBLIC_API_URL in your .env / EAS config; falls back to the
// deployed production API so the app works out of the box.
export const API_BASE_URL =
  (process.env.EXPO_PUBLIC_API_URL?.trim().replace(/\/+$/, '') ||
    'https://verichat-crm-api.runasp.net')

const TOKEN_KEY = 'verichat.auth.token'
const BIOMETRIC_LOGIN_KEY = 'verichat.auth.biometric-login'
const CACHE_PREFIX = 'verichat.api.cache.'
const QUEUE_PREFIX = 'verichat.api.queue.'

type QueuedAction = {
  id: string
  path: string
  method: string
  body?: string
  createdAt: string
}

function tokenScope(): string {
  const token = authToken ?? 'signed-out'
  let hash = 0
  for (let i = 0; i < token.length; i += 1) hash = (Math.imul(hash, 31) + token.charCodeAt(i)) | 0
  return `${hash >>> 0}`
}

function cacheKey(path: string): string {
  return `${CACHE_PREFIX}${tokenScope()}.${encodeURIComponent(path)}`
}

function queueKey(scope = tokenScope()): string {
  return `${QUEUE_PREFIX}${scope}`
}

async function networkAvailable(): Promise<boolean> {
  try {
    const state = await NetInfo.fetch()
    return state.isConnected !== false && state.isInternetReachable !== false
  } catch {
    // If the platform cannot report connectivity, try the request itself.
    return true
  }
}

async function readCached<T>(path: string): Promise<T | null> {
  try {
    const value = await AsyncStorage.getItem(cacheKey(path))
    return value === null ? null : JSON.parse(value) as T
  } catch {
    return null
  }
}

async function cacheResponse(path: string, value: unknown): Promise<void> {
  try {
    await AsyncStorage.setItem(cacheKey(path), JSON.stringify(value))
  } catch (error) {
    console.warn('[Offline] Could not cache API response:', error)
  }
}

async function readQueue(storageKey = queueKey()): Promise<QueuedAction[]> {
  try {
    const value = await AsyncStorage.getItem(storageKey)
    return value ? JSON.parse(value) as QueuedAction[] : []
  } catch {
    return []
  }
}

async function writeQueue(queue: QueuedAction[], storageKey = queueKey()): Promise<void> {
  if (queue.length === 0) await AsyncStorage.removeItem(storageKey)
  else await AsyncStorage.setItem(storageKey, JSON.stringify(queue))
}

let queueOperation: Promise<void> = Promise.resolve()

function serializeQueueWrite(operation: () => Promise<void>): Promise<void> {
  const nextOperation = queueOperation.then(operation, operation)
  queueOperation = nextOperation.catch(() => {})
  return nextOperation
}

async function enqueueAction(path: string, method: string, body?: string): Promise<void> {
  if (!authToken) throw new Error('Sign in before saving changes offline.')
  const storageKey = queueKey()
  await serializeQueueWrite(async () => {
    const queue = await readQueue(storageKey)
    queue.push({
      id: `${Date.now()}-${Math.random().toString(36).slice(2)}`,
      path,
      method,
      body,
      createdAt: new Date().toISOString(),
    })
    await writeQueue(queue, storageKey)
  })
}

let flushingQueue = false
const pendingCountListeners = new Set<(count: number) => void>()

function notifyPendingCountListeners(): void {
  void getPendingActionCount().then((count) => pendingCountListeners.forEach((listener) => listener(count)))
}

export function subscribePendingActionCount(listener: (count: number) => void): () => void {
  pendingCountListeners.add(listener)
  void getPendingActionCount().then(listener)
  return () => { pendingCountListeners.delete(listener) }
}

/** Replay queued authenticated writes in order when connectivity returns. */
export async function flushQueuedActions(): Promise<void> {
  if (flushingQueue || !authToken || !(await networkAvailable())) return
  const ownerToken = authToken
  const storageKey = queueKey()
  flushingQueue = true
  try {
    let queue = await readQueue(storageKey)
    const hadQueuedActions = queue.length > 0
    while (queue.length > 0 && authToken === ownerToken && await networkAvailable()) {
      const action = queue[0]
      let response: Response
      try {
        response = await fetch(`${API_BASE_URL}${action.path}`, {
          method: action.method,
          headers: authHeaders(action.body ? { 'Idempotency-Key': action.id } : undefined, ownerToken),
          body: action.body,
        })
      } catch {
        break
      }
      if (response.status === 401) {
        setAuthToken(null)
        break
      }
      if (!response.ok && response.status >= 500) break
      if (!response.ok) {
        console.warn(`[Offline] Dropping queued ${action.method} ${action.path}: server returned ${response.status}.`)
      }
      await serializeQueueWrite(async () => {
        const currentQueue = await readQueue(storageKey)
        await writeQueue(currentQueue.filter((item) => item.id !== action.id), storageKey)
      })
      queue = await readQueue(storageKey)
      notifyPendingCountListeners()
    }
    if (hadQueuedActions && queue.length === 0 && authToken === ownerToken) await invalidateCachedResponses()
  } finally {
    flushingQueue = false
  }
}

/** Start connectivity monitoring and replay pending changes on launch/reconnect. */
export function startOfflineSync(): () => void {
  const unsubscribe = NetInfo.addEventListener((state) => {
    if (state.isConnected && state.isInternetReachable !== false) void flushQueuedActions()
  })
  void NetInfo.fetch().then((state) => {
    if (state.isConnected && state.isInternetReachable !== false) void flushQueuedActions()
  })
  return unsubscribe
}

/** Pending change count for the active account. */
export async function getPendingActionCount(): Promise<number> {
  return (await readQueue()).length
}

function canQueueOffline(path: string, method: string): boolean {
  if (method === 'GET' || path.startsWith('/api/auth/') || path === '/api/mobile/register') return false
  // These creates need a server-generated identifier/confirmation for safe UI
  // behavior and should be retried by the user when connectivity returns.
  if (method === 'POST' && (
    path === '/api/mobile/appointments' ||
    path === '/api/mobile/devices' ||
    path === '/api/mobile/orders' ||
    path.startsWith('/api/mobile/reorder/')
  )) return false
  return path.startsWith('/api/')
}

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
  notifyPendingCountListeners()
  // Persist in the background so the UI is never blocked on the keystore.
  if (token) void writeStoredToken(token)
  else void clearStoredToken()
}

/** Biometric login is an opt-in local to this device. */
export async function getBiometricLoginEnabled(): Promise<boolean> {
  if (Platform.OS === 'web') return false
  try {
    return (await SecureStore.getItemAsync(BIOMETRIC_LOGIN_KEY)) === 'true'
  } catch {
    return false
  }
}

export async function setBiometricLoginEnabled(enabled: boolean): Promise<void> {
  if (Platform.OS === 'web') throw new Error('Biometric login is only available in the mobile app.')
  if (enabled) await SecureStore.setItemAsync(BIOMETRIC_LOGIN_KEY, 'true')
  else await SecureStore.deleteItemAsync(BIOMETRIC_LOGIN_KEY)
}

/**
 * Restores a previously persisted JWT on cold start. Resolves once the token
 * state is settled so the auth gate can render the right navigator.
 */
export async function hydrateAuthToken(): Promise<string | null> {
  if (authToken) return authToken
  if (await getBiometricLoginEnabled()) {
    try {
      if (!(await hasUsableBiometrics())) return null
      if (!(await authenticateBiometric('Unlock Verichat'))) return null
    } catch {
      return null
    }
  }
  const stored = await readStoredToken()
  authToken = stored
  authListeners.forEach((listener) => listener(stored))
  notifyPendingCountListeners()
  if (stored) void flushQueuedActions()
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

/** Reads the signed role claim so the app can keep client and staff workflows separate. */
export function getAuthRole(): string | null {
  if (!authToken) return null
  try {
    const payload = authToken.split('.')[1]
    if (!payload) return null
    const normalized = payload.replace(/-/g, '+').replace(/_/g, '/')
    const binary = globalThis.atob(normalized.padEnd(Math.ceil(normalized.length / 4) * 4, '='))
    const json = decodeURIComponent(Array.from(binary, character => `%${character.charCodeAt(0).toString(16).padStart(2, '0')}`).join(''))
    const claims = JSON.parse(json) as Record<string, unknown>
    const role = claims.role ?? claims['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
    return typeof role === 'string' ? role : null
  } catch {
    return null
  }
}

export const STAFF_MOBILE_ROLES = [
  'System Administrator',
  'Sales Representative',
  'Procurement Manager',
  'Field Support Engineer',
] as const

export function isStaffMobileRole(role: string | null): boolean {
  return role !== null && (STAFF_MOBILE_ROLES as readonly string[]).includes(role)
}

function authHeaders(extra?: Record<string, string>, token = authToken): Record<string, string> {
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    ...(extra ?? {}),
  }
  if (token) headers.Authorization = `Bearer ${token}`
  return headers
}

/** Typed fetch wrapper: attaches JWT, throws a readable Error on non-2xx. */
export async function api<T>(path: string, init?: RequestInit, options: { allowCachedFallback?: boolean } = {}): Promise<T> {
  const allowCachedFallback = options.allowCachedFallback !== false
  const method = (init?.method ?? 'GET').toUpperCase()
  const isRead = method === 'GET'
  const isAuthRequest = path.startsWith('/api/auth/') || path === '/api/mobile/register'
  const isCacheable = isRead && path.startsWith('/api/') && !isAuthRequest
  const isQueueable = canQueueOffline(path, method) && !isAuthRequest
  const body = typeof init?.body === 'string' ? init.body : undefined
  const canQueueBody = init?.body == null || typeof init.body === 'string'

  if (!(await networkAvailable())) {
    if (isCacheable && allowCachedFallback) {
      const cached = await readCached<T>(path)
      if (cached !== null) return cached
      throw new Error('This information is not available offline yet. Connect once to download it.')
    }
    if (isQueueable) {
      if (!canQueueBody) throw new Error('This change cannot be saved offline because its request body is unsupported.')
      await enqueueAction(path, method, body)
      notifyPendingCountListeners()
      return undefined as T
    }
    throw new Error('Could not connect to Verichat. Check your internet connection and try again.')
  }

  let response: Response
  try {
    response = await fetch(`${API_BASE_URL}${path}`, {
      ...init,
      headers: authHeaders(init?.headers as Record<string, string> | undefined),
    })
  } catch {
    if (isCacheable && allowCachedFallback) {
      const cached = await readCached<T>(path)
      if (cached !== null) return cached
    }
    if (isQueueable) {
      if (!canQueueBody) throw new Error('This change cannot be saved offline because its request body is unsupported.')
      await enqueueAction(path, method, body)
      notifyPendingCountListeners()
      return undefined as T
    }
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
    if (response.status === 403 && path === '/api/quotation/request') {
      throw new Error('Quote requests are for client accounts. Sign out and use Client sign-in; staff should review incoming requests in the website Pipeline.')
    }
    throw new Error(message || `Request failed (${response.status}).`)
  }

  if (response.status === 204) {
    if (!isRead) await invalidateCachedResponses()
    return undefined as T
  }
  const contentType = response.headers.get('content-type') ?? ''
  if (!contentType.includes('json')) {
    if (!isRead) await invalidateCachedResponses()
    return undefined as T
  }
  const data = await response.json() as T
  if (isCacheable) await cacheResponse(path, data)
  else if (!isRead) await invalidateCachedResponses()
  return data
}

async function invalidateCachedResponses(): Promise<void> {
  try {
    const prefix = `${CACHE_PREFIX}${tokenScope()}.`
    const keys = (await AsyncStorage.getAllKeys()).filter((key) => key.startsWith(prefix))
    if (keys.length) await AsyncStorage.multiRemove(keys)
  } catch {
    // Cache invalidation is best-effort; a later online read refreshes it.
  }
}
