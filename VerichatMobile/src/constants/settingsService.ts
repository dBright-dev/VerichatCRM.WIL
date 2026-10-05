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

// Backend contracts: GET /api/mobile/settings, PATCH /api/mobile/settings (JWT).
export async function fetchSettings(): Promise<UserSettings> {
  return api<UserSettings>('/api/mobile/settings')
}

export async function updatePreferences(changes: Partial<Preferences>): Promise<void> {
  await api('/api/mobile/settings', {
    method: 'PATCH',
    body: JSON.stringify(changes),
  })
}
