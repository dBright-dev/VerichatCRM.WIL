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

// Flip to false once the backend endpoints are ready.
const USE_MOCK_DATA = true

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

// TODO: confirm the endpoint paths and response shapes with your backend,
// and attach the logged-in client's auth token to the request headers.
async function api<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...init,
    headers: {
      'Content-Type': 'application/json',
      // Authorization: `Bearer ${token}`,
    },
  })

  if (!response.ok) {
    throw new Error(`Request failed (${response.status}).`)
  }

  return response.status === 204 ? (undefined as T) : response.json()
}

/* ------------------------------ Public API ------------------------------ */

export async function fetchDevices(): Promise<Device[]> {
  if (USE_MOCK_DATA) {
    return [...mockDevices]
  }
  return api<Device[]>('/client/devices')
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
  return api<Device>(`/client/devices/${id}/sync`, { method: 'POST' })
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
  return api<Device>(`/client/devices/${id}/reconnect`, { method: 'POST' })
}

export async function removeDevice(id: string): Promise<void> {
  if (USE_MOCK_DATA) {
    await wait(400)
    mockDevices = mockDevices.filter((d) => d.id !== id)
    return
  }
  await api<void>(`/client/devices/${id}`, { method: 'DELETE' })
}