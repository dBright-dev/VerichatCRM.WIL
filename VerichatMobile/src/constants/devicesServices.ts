import { api } from './api'

export type DeviceType =
  | 'weightScale'
  | 'thermometer'
  | 'bloodPressure'
  | 'glucoseMeter'
  | 'heartRate'
  | 'other'

export type DeviceStatus = 'connected' | 'disconnected'

export type Device = {
  id: string
  name: string // e.g. "Weight Scale"
  type: DeviceType
  status: DeviceStatus
  lastSyncAt: string | null // ISO date-time, null if it has never synced
}

// Live endpoints: GET /api/mobile/devices (+ POST sync/reconnect/DELETE).

/* ------------------------------ Public API ------------------------------ */

export async function fetchDevices(): Promise<Device[]> {
  return api<Device[]>('/api/mobile/devices')
}

// Pulls the latest readings from the device. Returns the updated device.
export async function syncDevice(id: string): Promise<Device> {
  return api<Device>(`/api/mobile/devices/${id}/sync`, { method: 'POST' })
}

// Re-establishes the connection to a disconnected device. Returns the updated device.
export async function reconnectDevice(id: string): Promise<Device> {
  return api<Device>(`/api/mobile/devices/${id}/reconnect`, { method: 'POST' })
}

export async function removeDevice(id: string): Promise<void> {
  await api<void>(`/api/mobile/devices/${id}`, { method: 'DELETE' })
}
