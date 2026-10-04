// Verichat CRM API — auth endpoints for the mobile app.
// POST /api/auth/login            { email, password }        → { success, token, user }
// POST /api/auth/register         staff/admin registration
// POST /api/mobile/register       client self-registration   → { success, token, user }
import { api, apiUrl, setAuthToken } from './api'

export type AuthUser = {
  userId: string
  displayName: string
  email: string
  role: string
}

export type AuthResponse = {
  success: boolean
  message: string
  token?: string
  user?: AuthUser
}

function fail(response: AuthResponse): never {
  throw new Error(response.message || 'Authentication failed.')
}

/** Staff/admin sign-in (same JWT contract as the Web app). */
export async function login(email: string, password: string): Promise<AuthResponse> {
  const response = await fetch(apiUrl('/api/auth/login'), {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password }),
  })
  const data = (await response.json()) as AuthResponse
  if (!response.ok || !data.success) fail(data)
  if (data.token) setAuthToken(data.token)
  return data
}

/** Client self-registration → "Client User" role. */
export async function registerClient(payload: {
  displayName: string
  email: string
  password: string
  companyName?: string
}): Promise<AuthResponse> {
  const data = await api<AuthResponse>('/api/mobile/register', {
    method: 'POST',
    body: JSON.stringify(payload),
  })
  if (!data.success) fail(data)
  if (data.token) setAuthToken(data.token)
  return data
}

/** Staff/admin registration (mirrors the Web register flow). */
export async function registerStaff(payload: {
  displayName: string
  email: string
  password: string
  role: string
}): Promise<AuthResponse> {
  const data = await api<AuthResponse>('/api/auth/register', {
    method: 'POST',
    body: JSON.stringify(payload),
  })
  if (!data.success) fail(data)
  if (data.token) setAuthToken(data.token)
  return data
}

/** Sign out — clears the stored JWT. */
export function logout() {
  setAuthToken(null)
}
