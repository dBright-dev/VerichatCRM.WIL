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

const API_BASE_URL = process.env.EXPO_PUBLIC_API_URL ?? ''

// TODO: confirm the endpoint path and response shape with your backend,
// and attach the logged-in client's auth token to the request headers.
export async function fetchClientDashboard(): Promise<ClientDashboard> {
  const response = await fetch(`${API_BASE_URL}/client/dashboard`, {
    headers: {
      'Content-Type': 'application/json',
      // Authorization: `Bearer ${token}`,
    },
  })

  if (!response.ok) {
    throw new Error(`Could not load your dashboard (${response.status}).`)
  }

  return response.json()
}