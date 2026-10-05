import { api } from './api'

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

// Backend contract: GET /api/mobile/prescriptions (JWT).
export async function fetchPrescriptions(): Promise<Prescription[]> {
  return api<Prescription[]>('/api/mobile/prescriptions')
}
