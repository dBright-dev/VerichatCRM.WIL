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

// Flip to false once the backend endpoint is ready.
const USE_MOCK_DATA = true

const API_BASE_URL = process.env.EXPO_PUBLIC_API_URL ?? ''

// Static data for the front-end build. Remove when the endpoint is live.
const MOCK_LAB_RESULTS: LabResultsData = {
  limsConnected: true,
  connectedLabs: ['Lancet', 'Interpath'],
  results: [
    {
      id: '1',
      reference: 'LLB-2026-086',
      testName: 'Full Blood Count (FBC)',
      laboratory: 'Lancet Laboratories',
      reportedAt: '2026-08-08T10:00:00',
      status: 'ready',
      fileSizeKb: 240,
      isNew: true,
      pdfUrl: null,
    },
    {
      id: '2',
      reference: 'IPL-2026-812',
      testName: 'Lipid Profile',
      laboratory: 'Interpath Laboratories',
      reportedAt: '2026-08-12T09:15:00',
      status: 'ready',
      fileSizeKb: 220,
      isNew: false,
      pdfUrl: null,
    },
    {
      id: '3',
      reference: 'LLB-2026-085',
      testName: 'Typhoid',
      laboratory: 'Lancet Laboratories',
      reportedAt: '2026-08-05T14:40:00',
      status: 'ready',
      fileSizeKb: 180,
      isNew: false,
      pdfUrl: null,
    },
  ],
}

// TODO: confirm the endpoint path and response shape with your backend,
// and attach the logged-in client's auth token to the request headers.
export async function fetchLabResults(): Promise<LabResultsData> {
  if (USE_MOCK_DATA) {
    return MOCK_LAB_RESULTS
  }

  const response = await fetch(`${API_BASE_URL}/client/lab-results`, {
    headers: {
      'Content-Type': 'application/json',
      // Authorization: `Bearer ${token}`,
    },
  })

  if (!response.ok) {
    throw new Error(`Could not load your lab results (${response.status}).`)
  }

  return response.json()
}

// TODO: call this when a result is opened so "NEW" is cleared on the server too.
// export async function markLabResultViewed(id: string): Promise<void> { ... }