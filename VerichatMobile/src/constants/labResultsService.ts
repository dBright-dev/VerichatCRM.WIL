import { api } from './api'

export type LabResultStatus = 'ready' | 'processing'

export type LabResult = {
  id: string
  reference: string // e.g. "LLB-2026-086"
  testName: string // e.g. "Full Blood Count (FBC)"
  laboratory: string // e.g. "Lancet Laboratories"
  reportedAt: string // ISO date-time, e.g. "2026-08-08T10:00:00"
  status: LabResultStatus
  fileSizeKb: number | null // size of the PDF, null if not known yet
  isNew: boolean // not yet viewed by the client
  pdfUrl: string | null // where the PDF can be opened/downloaded
}

export type LabResultsData = {
  limsConnected: boolean // whether the encrypted LIMS connection is active
  connectedLabs: string[] // e.g. ["Lancet", "Interpath"]
  results: LabResult[]
}

// Backend contract: GET /api/mobile/lab-results (JWT).
export async function fetchLabResults(): Promise<LabResultsData> {
  return api<LabResultsData>('/api/mobile/lab-results')
}

// Clears "NEW" on the server when a result is opened.
export async function markLabResultViewed(id: string): Promise<void> {
  await api(`/api/mobile/lab-results/${id}/viewed`, { method: 'POST' })
}
