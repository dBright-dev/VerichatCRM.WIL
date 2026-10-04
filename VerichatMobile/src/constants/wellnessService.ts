import { api } from './api'

export type MetricType =
  | 'bloodPressure'
  | 'heartRate'
  | 'bloodGlucose'
  | 'temperature'
  | 'bodyWeight'

export type HealthMetric = {
  id: string
  type: MetricType
  label: string // e.g. "Blood Pressure"
  value: string | null // e.g. "118/76" (null when no reading has been logged)
  unit: string // e.g. "mmHg"
  statusLabel: string // e.g. "Optimal", "Normal", "Healthy", "High"
  inRange: boolean // whether the reading is within the normal range
}

export type WellnessSummary = {
  healthScore: number | null // 0-100, null when there is not enough data
  metrics: HealthMetric[]
}

// Flip to false to keep using the bundled mock data.
const USE_MOCK_DATA = false

const API_BASE_URL = process.env.EXPO_PUBLIC_API_URL ?? ''

// Static data for the front-end build. Remove when the endpoint is live.
const MOCK_SUMMARY: WellnessSummary = {
  healthScore: 85,
  metrics: [
    {
      id: '1',
      type: 'bloodPressure',
      label: 'Blood Pressure',
      value: '118/76',
      unit: 'mmHg',
      statusLabel: 'Optimal',
      inRange: true,
    },
    {
      id: '2',
      type: 'heartRate',
      label: 'Heart Rate',
      value: '72',
      unit: 'bpm',
      statusLabel: 'Normal',
      inRange: true,
    },
    {
      id: '3',
      type: 'bloodGlucose',
      label: 'Blood Glucose',
      value: '5.4',
      unit: 'mmol/L',
      statusLabel: 'Normal',
      inRange: true,
    },
    {
      id: '4',
      type: 'temperature',
      label: 'Temperature',
      value: '34.1',
      unit: '°C',
      statusLabel: 'Normal',
      inRange: true,
    },
    {
      id: '5',
      type: 'bodyWeight',
      label: 'Body Weight',
      value: '74.2',
      unit: 'kg',
      statusLabel: 'Healthy',
      inRange: true,
    },
  ],
}

// Backend contracts: GET /api/mobile/vitals, POST /api/mobile/vitals (JWT).
export async function fetchWellnessSummary(): Promise<WellnessSummary> {
  if (USE_MOCK_DATA) {
    return MOCK_SUMMARY
  }

  return api<WellnessSummary>('/api/mobile/vitals')
}

/** Log a manual reading (or one parsed from a Bluetooth device value). */
export async function logVitalReading(input: {
  type: MetricType
  value: string
  unit?: string
  statusLabel?: string
  inRange?: boolean
  recordedAt?: string
}): Promise<void> {
  await api('/api/mobile/vitals', {
    method: 'POST',
    body: JSON.stringify({
      type: input.type,
      value: input.value,
      unit: input.unit ?? '',
      statusLabel: input.statusLabel ?? 'Normal',
      inRange: input.inRange ?? true,
      recordedAt: input.recordedAt,
    }),
  })
}