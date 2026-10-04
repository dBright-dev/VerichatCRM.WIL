export type Preferences = {
  pushNotifications: boolean
  biometricLogin: boolean
  darkMode: boolean
}

export type UserSettings = {
  fullName: string
  preferences: Preferences
}

// Flip to false once the backend endpoints are ready.
const USE_MOCK_DATA = true

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

const authHeaders = () => ({
  'Content-Type': 'application/json',
  // TODO: attach the logged-in user's token
  // Authorization: `Bearer ${token}`,
})

// TODO: confirm the endpoint paths and response shapes with your backend.
export async function fetchSettings(): Promise<UserSettings> {
  if (USE_MOCK_DATA) {
    return MOCK_SETTINGS
  }

  const response = await fetch(`${API_BASE_URL}/user/settings`, { headers: authHeaders() })

  if (!response.ok) {
    throw new Error(`Could not load your settings (${response.status}).`)
  }

  return response.json()
}

export async function updatePreferences(changes: Partial<Preferences>): Promise<void> {
  if (USE_MOCK_DATA) {
    return
  }

  const response = await fetch(`${API_BASE_URL}/user/preferences`, {
    method: 'PATCH',
    headers: authHeaders(),
    body: JSON.stringify(changes),
  })

  if (!response.ok) {
    throw new Error(`Could not save your preference (${response.status}).`)
  }
}