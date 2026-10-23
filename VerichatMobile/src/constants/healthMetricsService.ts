import { api } from './api'
import type { MetricType } from './devicesServices'

/** Payload sent to POST /api/healthmetrics when a device reading is synced. */
export type LogHealthMetricInput = {
  clientId: string
  deviceId: string
  metricType: MetricType
  value: string
  unit?: string
  timestamp?: string
}

/** Response from POST /api/healthmetrics. */
export type LogHealthMetricResult = {
  metricId: string
  clientId: string
  deviceId: string
  metricType: string
  value: string
  unit: string
  timestamp: string
}

/**
 * Posts a single health metric reading (captured from a paired IoT device) to
 * the C# API: POST /api/healthmetrics. Requires a valid JWT.
 */
export async function logHealthMetric(input: LogHealthMetricInput): Promise<LogHealthMetricResult> {
  return api<LogHealthMetricResult>('/api/healthmetrics', {
    method: 'POST',
    body: JSON.stringify({
      clientId: input.clientId,
      deviceId: input.deviceId,
      metricType: input.metricType,
      value: input.value,
      unit: input.unit ?? '',
      timestamp: input.timestamp ?? new Date().toISOString(),
    }),
  })
}
