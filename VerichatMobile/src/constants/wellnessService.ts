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

// Backend contracts: GET /api/mobile/vitals, POST /api/mobile/vitals (JWT).
export async function fetchWellnessSummary(): Promise<WellnessSummary> {
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
