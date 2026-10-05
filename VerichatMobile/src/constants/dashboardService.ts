import { api } from './api'

export type Vitals = {
  heartRate: number | null // bpm
  temperature: number | null // °C
  systolic: number | null // mmHg
  diastolic: number | null // mmHg
}

export type RecurringOrder = {
  id: string
  productName: string // e.g. "Surgical Face Masks (Box 100)"
  dueDate: string // ISO date string
}

export type TrackingOrder = {
  id: string
  reference: string // e.g. "VQP-FMN-456"
  status: string // e.g. "out for delivery"
}

export type ClientDashboard = {
  fullName: string
  activeCount: number
  pendingCount: number
  recurringCount: number
  vitals: Vitals | null
  nextRecurringOrder: RecurringOrder | null
  trackingOrders: TrackingOrder[]
}

// Backend contract: GET /api/mobile/dashboard (JWT, token owner = client).
export async function fetchClientDashboard(): Promise<ClientDashboard> {
  return api<ClientDashboard>('/api/mobile/dashboard')
}
