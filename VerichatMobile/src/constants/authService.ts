// Verichat CRM API — auth endpoints for the mobile app.
// POST /api/auth/login            { email, password }   → { success, token, user }
// POST /api/mobile/register       client account        → { success, user } (then signed in here)
// POST /api/auth/forgot-password  { email }              → { success, message } (same answer either way)
// POST /api/auth/reset-password   { token, newPassword } → { success, message } | 400 with the reason
// POST /api/auth/help             { name, email, expectedRole, message, channel } → { success, message }
//
// A self-registered account is a client or student account, and it is live the moment
// the API writes it, so registerClient signs the person in with the password they just
// chose. Staff accounts are not created from an app at all - an administrator makes
// them on the web console - so /api/auth/register refuses any staff role.
// A password reset does not change a role; it proves the mailbox.
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

/**
 * Client self-registration → a "Client User" account, signed in on the spot. The API
 * writes the account and its CRM client record together and returns no token; the
 * password the person just chose is theirs, so this exchanges it for a session with
 * the same call sign-in uses, rather than sending them back to the sign-in form.
 */
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
  return login(payload.email, payload.password, 'client')
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

/**
 * Sends an "I cannot get in" note to a Verichat administrator, for the cases a reset
 * link cannot reach: a deactivated account, an address typed one letter wrong, a role
 * that is not the one the person wanted.
 *
 * `channel` says which app it came from, which is the mobile app's own fact to state -
 * it is not a claim about the person, and it is what makes a spike from one build
 * visible on the queue. The answer is the same whatever the address is registered as,
 * so this screen cannot be used to find out who has an account.
 */
export async function askForHelp(payload: {
  name: string
  email: string
  expectedRole?: string
  message: string
}): Promise<string> {
  const data = await api<AuthResponse>('/api/auth/help', {
    method: 'POST',
    body: JSON.stringify({ ...payload, channel: 'Mobile' }),
  })
  return data.message || 'Your note is with a Verichat administrator.'
}
