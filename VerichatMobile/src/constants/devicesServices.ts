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

// Flip to false to keep using the bundled mock data.
const USE_MOCK_DATA = false

const API_BASE_URL = process.env.EXPO_PUBLIC_API_URL ?? ''

/* ------------------------------ Mock data ------------------------------- */

const wait = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms))

// Builds a timestamp relative to now so the "Today / Yesterday / 3 days ago" labels stay correct.
const daysAgoAt = (days: number, hour: number, minute: number) => {
  const date = new Date()
  date.setDate(date.getDate() - days)
  date.setHours(hour, minute, 0, 0)
  return date.toISOString()
}

// Static data for the front-end build. Remove when the endpoints are live.
let mockDevices: Device[] = [
  {
    id: '1',
    name: 'Weight Scale',
    type: 'weightScale',
    status: 'connected',
    lastSyncAt: daysAgoAt(0, 7, 30),
  },
  {
    id: '2',
    name: 'Temperature',
    type: 'thermometer',
    status: 'connected',
    lastSyncAt: daysAgoAt(1, 23, 15),
  },
  {
    id: '3',
    name: 'Sphygmomanometer',
    type: 'bloodPressure',
    status: 'disconnected',
    lastSyncAt: daysAgoAt(3, 9, 0),
  },
]

/* ------------------------------- Real API ------------------------------- */

// Live endpoints: GET /api/mobile/devices (+ POST sync/reconnect/DELETE).

/* ------------------------------ Public API ------------------------------ */

export async function fetchDevices(): Promise<Device[]> {
  if (USE_MOCK_DATA) {
    return [...mockDevices]
  }
  return api<Device[]>('/api/mobile/devices')
}

// Pulls the latest readings from the device. Returns the updated device.
export async function syncDevice(id: string): Promise<Device> {
  if (USE_MOCK_DATA) {
    await wait(800)
    const updated = mockDevices.find((d) => d.id === id)
    if (!updated) throw new Error('Device not found.')
    updated.lastSyncAt = new Date().toISOString()
    return { ...updated }
  }
  return api<Device>(`/api/mobile/devices/${id}/sync`, { method: 'POST' })
}

// Re-establishes the connection to a disconnected device. Returns the updated device.
export async function reconnectDevice(id: string): Promise<Device> {
  if (USE_MOCK_DATA) {
    await wait(1000)
    const updated = mockDevices.find((d) => d.id === id)
    if (!updated) throw new Error('Device not found.')
    updated.status = 'connected'
    updated.lastSyncAt = new Date().toISOString()
    return { ...updated }
  }
  return api<Device>(`/api/mobile/devices/${id}/reconnect`, { method: 'POST' })
}

export async function removeDevice(id: string): Promise<void> {
  if (USE_MOCK_DATA) {
    await wait(400)
    mockDevices = mockDevices.filter((d) => d.id !== id)
    return
  }
  await api<void>(`/api/mobile/devices/${id}`, { method: 'DELETE' })
}