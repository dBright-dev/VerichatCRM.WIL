// Verichat CRM API — auth endpoints for the mobile app.
// POST /api/auth/login            { email, password }   → { success, token, user }
// POST /api/auth/register         staff application     → { success, approvalStatus, user } (no token)
// POST /api/mobile/register       client application    → { success, approvalStatus, user } (no token)
//
// Sign-up on either path is an application, not access: the account is written down
// and waits for a Verichat employee to approve it, so no token comes back and the
// caller has to say so. Signing in is refused with the same wording until then.
import { api, setAuthToken, STAFF_MOBILE_ROLES } from './api'

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
  approvalStatus?: string
}

function fail(response: AuthResponse): never {
  throw new Error(response.message || 'Authentication failed.')
}

/** Staff/admin sign-in (same JWT contract as the Web app). */
export async function login(email: string, password: string, audience?: 'client' | 'staff'): Promise<AuthResponse> {
  const data = await api<AuthResponse>('/api/auth/login', {
    method: 'POST',
    body: JSON.stringify({ email, password }),
  })
  if (!data.success) fail(data)
  if (!data.token) throw new Error('The server did not return a sign-in token.')
  const role = data.user?.role
  // The client audience is the catalogue, the quick cart and a quote request.
  // A Medical Student trains on exactly that flow, so they sign in here too;
  // they are not staff and get no staff screens.
  if (audience === 'client' && role !== 'Client User' && role !== 'Medical Student') {
    throw new Error('This account is for staff. Choose Staff sign-in instead.')
  }
  if (audience === 'staff' && (!role || !(STAFF_MOBILE_ROLES as readonly string[]).includes(role))) {
    throw new Error(`Staff sign-in is limited to ${STAFF_MOBILE_ROLES.join(', ')}. Client accounts must use Client sign-in.`)
  }
  setAuthToken(data.token)
  return data
}

/** Client self-registration → a "Client User" application, not a signed-in session. */
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
  // A token only exists once an employee has approved the account. Nothing is
  // stored here, so the applicant lands on the sign-in screen and the API refuses
  // those credentials until then - the same rule the web app applies.
  if (data.token) setAuthToken(data.token)
  return data
}

/** Staff registration — also an application. See the file header. */
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
