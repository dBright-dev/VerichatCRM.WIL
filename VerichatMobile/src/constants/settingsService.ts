import { api } from './api'

export type Preferences = {
  pushNotifications: boolean
  biometricLogin: boolean
  darkMode: boolean
}

export type UserSettings = {
  fullName: string
  preferences: Preferences
}

// Flip to false to keep using the bundled mock data.
const USE_MOCK_DATA = false

const API_BASE_URL = process.env.EXPO_PUBLIC_API_URL ?? ''

// Static data for the front-end build. Remove when the endpoints are live.
const MOCK_SETTINGS: UserSettings = {
  fullName: 'Julia Caeser',
  preferences: {
    pushNotifications: true,
    biometricLogin: false,
    darkMode: false,
  },
}

// Backend contracts: GET /api/mobile/settings, PATCH /api/mobile/settings (JWT).
export async function fetchSettings(): Promise<UserSettings> {
  if (USE_MOCK_DATA) {
    return MOCK_SETTINGS
  }

  return api<UserSettings>('/api/mobile/settings')
}

export async function updatePreferences(changes: Partial<Preferences>): Promise<void> {
  if (USE_MOCK_DATA) {
    return
  }

  await api('/api/mobile/settings', {
    method: 'PATCH',
    body: JSON.stringify(changes),
  })
}