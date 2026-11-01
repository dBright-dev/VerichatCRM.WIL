// Verichat CRM API — auth endpoints for the mobile app.
// POST /api/auth/login            { email, password }   → { success, token, user }
// POST /api/auth/register         staff application     → { success, approvalStatus, user } (no token)
// POST /api/mobile/register       client application    → { success, approvalStatus, user } (no token)
// POST /api/auth/forgot-password  { email }              → { success, message } (same answer either way)
// POST /api/auth/reset-password   { token, newPassword } → { success, message } | 400 with the reason
//
// Sign-up on either path is an application, not access: the account is written down
// and waits for a Verichat employee to approve it, so no token comes back and the
// caller has to say so. Signing in is refused with the same wording until then.
// A password reset does not change that - it proves the mailbox, not the applicant.
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

/**
 * Asks the API for a reset link. Returns the server's sentence, which is the same
 * whether or not the address has an account - the endpoint is built that way so the
 * screen cannot be used to find out who registered.
 */
export async function forgotPassword(email: string): Promise<string> {
  const data = await api<AuthResponse>('/api/auth/forgot-password', {
    method: 'POST',
    body: JSON.stringify({ email }),
  })
  return data.message || 'If that address has a Verichat account, a reset link is on its way.'
}

/**
 * Trades a reset link for a new password.
 *
 * The link itself opens the website, because a mail client can only hand a token to
 * a browser; this screen is the backup for somebody who has the code and no browser
 * to hand, which is why the API takes the token in a body. A used or expired code
 * comes back as a 400, and `api()` throws with the server's reason.
 */
export async function resetPassword(token: string, newPassword: string): Promise<string> {
  const data = await api<AuthResponse>('/api/auth/reset-password', {
    method: 'POST',
    body: JSON.stringify({ token, newPassword }),
  })
  return data.message || 'Your password is set.'
}
