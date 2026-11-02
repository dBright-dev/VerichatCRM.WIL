import AsyncStorage from '@react-native-async-storage/async-storage'
import { Appearance } from 'react-native'

const DARK_MODE_KEY = 'verichat.preferences.dark-mode'

export async function applyDarkMode(enabled: boolean) {
  Appearance.setColorScheme(enabled ? 'dark' : 'light')
  try {
    await AsyncStorage.setItem(DARK_MODE_KEY, String(enabled))
  } catch {
    // The API remains the persisted source of truth if local storage is unavailable.
  }
}

export async function restoreDarkMode() {
  try {
    const stored = await AsyncStorage.getItem(DARK_MODE_KEY)
    if (stored === 'true' || stored === 'false') Appearance.setColorScheme(stored === 'true' ? 'dark' : 'light')
  } catch {
    // Follow the device theme when no local preference is available.
  }
}

export function followSystemTheme() {
  Appearance.setColorScheme('unspecified')
}
