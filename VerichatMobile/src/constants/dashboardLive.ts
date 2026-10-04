import { api } from './api'
import type {
  ClientDashboard,
  RecurringOrder,
  TrackingOrder,
  Vitals,
} from './dashboardService'

/** The backend already returns the exact ClientDashboard shape —
 *  thin live wrapper kept so screens can pick either import. */
export async function fetchClientDashboardLive(): Promise<ClientDashboard> {
  return api<ClientDashboard>('/api/mobile/dashboard')
}

export type { ClientDashboard, RecurringOrder, TrackingOrder, Vitals }
