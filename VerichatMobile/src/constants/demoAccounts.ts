import { CRMTheme } from './crmTheme'

/**
 * The seven development sign-in accounts, mirrored from
 * `VerichatCRM.Core/Models/DemoAccounts.cs` — the list the API's `DemoUserSeeder`
 * actually writes. Keep the two in step: a chip here that names an address the
 * seeder did not write is a chip that fails, and the only way to notice is to tap it.
 *
 * These accounts exist only while the API runs in Development with
 * `Bootstrap:DemoUsers=true`, and the chip row that reads this list renders only in a
 * dev build, so a released app shows neither the shortcut nor the credentials.
 *
 * `dot` repeats the seven colours the web sign-in panel uses
 * (`wwwroot/pages/auth.css`, `[data-demo=…]`), which are themselves the theme's
 * status tokens — the same account is the same colour on both clients.
 *
 * `opens` is what this app does with the session, not what the web tier does: the
 * root screen sends a staff role to the shared board and a client role to the client
 * home. The staff board is not yet split per role, so all five staff chips land on
 * the same screen with different data behind it.
 */
export type DemoAccount = {
  code: string
  label: string
  role: string
  email: string
  password: string
  audience: 'client' | 'staff'
  opens: string
  dot: string
}

export const DEMO_ACCOUNTS: DemoAccount[] = [
  { code: 'admin', label: 'Admin', role: 'System Administrator', email: 'admin@verichat.test.local', password: 'Demo!Admin2026', audience: 'staff', opens: 'Staff board', dot: CRMTheme.primary },
  { code: 'sales', label: 'Sales Rep', role: 'Sales Representative', email: 'sales@verichat.test.local', password: 'Demo!Sales2026', audience: 'staff', opens: 'Staff board', dot: CRMTheme.warning },
  { code: 'procurement', label: 'Procurement', role: 'Procurement Manager', email: 'procurement@verichat.test.local', password: 'Demo!Stock2026', audience: 'staff', opens: 'Staff board', dot: CRMTheme.success },
  { code: 'engineer', label: 'Support', role: 'Field Support Engineer', email: 'engineer@verichat.test.local', password: 'Demo!Fix2026', audience: 'staff', opens: 'Staff board', dot: CRMTheme.danger },
  { code: 'logistics', label: 'Logistics', role: 'Logistics', email: 'logistics@verichat.test.local', password: 'Demo!Move2026', audience: 'staff', opens: 'Staff board', dot: '#00B4D8' },
  { code: 'student', label: 'Student', role: 'Medical Student', email: 'student@verichat.test.local', password: 'Demo!Study2026', audience: 'client', opens: 'Client home', dot: '#3182CE' },
  { code: 'client', label: 'Client', role: 'Client User', email: 'client@verichat.test.local', password: 'Demo!Client2026', audience: 'client', opens: 'Client home', dot: '#4A5568' },
]

/**
 * The chips a sign-in screen may offer. Staff and client are separate audiences in
 * `authService.login`, and crossing them is refused by the server, so a staff chip on
 * the client screen would only ever produce an error.
 */
export function demoAccountsFor(audience: 'client' | 'staff'): DemoAccount[] {
  return DEMO_ACCOUNTS.filter((account) => account.audience === audience)
}
