import { api } from './api'
import type { ParsedReading } from './bleDevices'

export type DeviceType =
  | 'weightScale'
  | 'thermometer'
  | 'bloodPressure'
  | 'glucoseMeter'
  | 'heartRate'
  | 'other'

export type DeviceStatus = 'connected' | 'disconnected'

export type MetricType =
  | 'bloodPressure'
  | 'heartRate'
  | 'bloodGlucose'
  | 'temperature'
  | 'bodyWeight'

export type Device = {
  id: string
  name: string // e.g. "Weight Scale"
  type: DeviceType
  status: DeviceStatus
  lastSyncAt: string | null // ISO date-time, null if it has never synced
  externalId?: string | null // BLE peripheral id of the physical device
}

// Live endpoints: GET /api/mobile/devices (+ POST sync/reconnect/DELETE).

/* ------------------------------ Public API ------------------------------ */

export async function fetchDevices(): Promise<Device[]> {
  return api<Device[]>('/api/mobile/devices')
}

/** Registers a scanned BLE device so the app can reconnect to it later. */
export async function registerDevice(name: string, type: DeviceType, externalId: string): Promise<Device> {
  return api<Device>('/api/mobile/devices', {
    method: 'POST',
    body: JSON.stringify({ name, type, externalId }),
  })
}

/**
 * Pulls the latest readings from the device (when the caller has them) and
 * saves them to the database. Returns the updated device.
 */
export async function syncDevice(id: string, readings?: ParsedReading[]): Promise<Device> {
  return api<Device>(`/api/mobile/devices/${id}/sync`, {
    method: 'POST',
    body: JSON.stringify({ readings: readings ?? [] }),
  })
}

// Re-establishes the connection to a disconnected device. Returns the updated device.
export async function reconnectDevice(id: string): Promise<Device> {
  return api<Device>(`/api/mobile/devices/${id}/reconnect`, { method: 'POST' })
}

export async function removeDevice(id: string): Promise<void> {
  await api<void>(`/api/mobile/devices/${id}`, { method: 'DELETE' })
}
