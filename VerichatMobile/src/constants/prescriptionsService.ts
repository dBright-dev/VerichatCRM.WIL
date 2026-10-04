export type PrescriptionStatus = 'active' | 'dispensed' | 'expired'

export type Prescription = {
  id: string
  reference: string // e.g. "DJM-2026-088"
  medication: string // e.g. "Paracetamol (500mg)"
  instructions: string // e.g. "2 capsules 3 times a day after food"
  prescriber: string // e.g. "Dr Joseph Mushawatu"
  issuedAt: string // ISO date-time, e.g. "2026-08-08T09:00:00"
  status: PrescriptionStatus
  qrPayload: string // the exact value the pharmacist's scanner expects (supplied by the backend)
}

// Flip to false once the backend endpoint is ready.
const USE_MOCK_DATA = true

const API_BASE_URL = process.env.EXPO_PUBLIC_API_URL ?? ''

// Static data for the front-end build. Remove when the endpoint is live.
// NOTE: the qrPayload values below are placeholders, not real scannable scripts.
const MOCK_PRESCRIPTIONS: Prescription[] = [
  {
    id: '1',
    reference: 'DJM-2026-088',
    medication: 'Paracetamol (500mg)',
    instructions: '2 capsules 3 times a day after food',
    prescriber: 'Dr Joseph Mushawatu',
    issuedAt: '2026-08-08T09:00:00',
    status: 'active',
    qrPayload: 'MOCK-DJM-2026-088',
  },
  {
    id: '2',
    reference: 'DNM-2026-812',
    medication: 'Amoxicillin (500mg)',
    instructions: '1 capsules daily after food',
    prescriber: 'Dr Nyasha Mukiwa',
    issuedAt: '2026-08-12T11:30:00',
    status: 'active',
    qrPayload: 'MOCK-DNM-2026-812',
  },
  {
    id: '3',
    reference: 'DNM-2026-078',
    medication: 'Vitamin B Supplement',
    instructions: '1 capsules daily after food',
    prescriber: 'Dr Joseph Mushawatu',
    issuedAt: '2026-07-08T15:10:00',
    status: 'active',
    qrPayload: 'MOCK-DNM-2026-078',
  },
]

// TODO: confirm the endpoint path and response shape with your backend,
// and attach the logged-in client's auth token to the request headers.
export async function fetchPrescriptions(): Promise<Prescription[]> {
  if (USE_MOCK_DATA) {
    return MOCK_PRESCRIPTIONS
  }

  const response = await fetch(`${API_BASE_URL}/client/prescriptions`, {
    headers: {
      'Content-Type': 'application/json',
      // Authorization: `Bearer ${token}`,
    },
  })

  if (!response.ok) {
    throw new Error(`Could not load your prescriptions (${response.status}).`)
  }

  return response.json()
}