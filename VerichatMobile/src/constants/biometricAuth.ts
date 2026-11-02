import * as LocalAuthentication from 'expo-local-authentication'
import { Platform } from 'react-native'

export async function hasUsableBiometrics(): Promise<boolean> {
  if (Platform.OS === 'web') return false
  const [hasHardware, isEnrolled] = await Promise.all([
    LocalAuthentication.hasHardwareAsync(),
    LocalAuthentication.isEnrolledAsync(),
  ])
  return hasHardware && isEnrolled
}

export async function authenticateBiometric(promptMessage: string): Promise<boolean> {
  const result = await LocalAuthentication.authenticateAsync({
    promptMessage,
    cancelLabel: 'Cancel',
    disableDeviceFallback: true,
  })
  return result.success
}
