import { PermissionsAndroid, Platform } from 'react-native'
import * as Notifications from 'expo-notifications'
import * as Device from 'expo-device'
import { api } from '../constants/api'

// Configure desired notification content handlers
Notifications.setNotificationHandler({
  handleNotification: async () => ({
    shouldShowAlert: true,
    shouldPlaySound: true,
    shouldSetBadge: true,
  }),
})

export type NotificationType = 'info' | 'warning' | 'error' | 'success'
export type NotificationPriority = 'low' | 'normal' | 'high'

export type PushNotification = {
  id: string
  type: NotificationType
  title: string
  body: string
  data?: Record<string, string>
  priority?: NotificationPriority
}

/** Request notification permissions and return the Expo push token. */
export async function requestNotificationsPermission(): Promise<string | null> {
  // Ask for permissions on iOS
  if (Platform.OS === 'ios' || Platform.OS === 'android') {
    const { status: permStatus } = await Notifications.requestPermissionsAsync()
    if (permStatus !== 'granted') {
      // On Android, also ask for notification permission in manifest
      if (Platform.OS === 'android' && permStatus !== 'granted') {
        const androidPerms = await PermissionsAndroid.requestMultiple([
          PermissionsAndroid.PERMISSIONS.POST_NOTIFICATIONS,
        ])
        if (androidPerms['android.permission.POST_NOTIFICATIONS'] !== 'granted') {
          console.warn('[Notifications] POST_NOTIFICATIONS permission denied')
        }
      }
      return null
    }
  }

  // Get the Expo push token
  const token = await Notifications.getExpoPushTokenAsync({
    projectId: '12345678-1234-1234-1234-123456789012',
  })

  return token.data
}

/** Save the device token to the backend. */
export async function saveDeviceToken(token: string, userId: string): Promise<void> {
  try {
    await api(`/api/notifications/device-token`, {
      method: 'POST',
      body: JSON.stringify({ userId, token }),
    })
  } catch (e) {
    console.warn('[Notifications] Could not save device token:', e)
  }
}

/** Update the device token and send a push notification. */
export async function updateDeviceToken(token: string, userId: string): Promise<void> {
  try {
    await api(`/api/notifications/device-token`, {
      method: 'PUT',
      body: JSON.stringify({ userId, token }),
    })
  } catch (e) {
    console.warn('[Notifications] Could not update device token:', e)
  }
}

/** Remove the device token when user logs out. */
export async function removeDeviceToken(userId: string): Promise<void> {
  try {
    await api(`/api/notifications/device-token`, {
      method: 'DELETE',
      body: JSON.stringify({ userId }),
    })
  } catch (e) {
    console.warn('[Notifications] Could not remove device token:', e)
  }
}

/** Send a push notification to a user's device. */
export async function sendPushNotification(
  userId: string,
  title: string,
  body: string,
  data?: Record<string, string>,
): Promise<boolean> {
  try {
    const response = await api(`/api/notifications/send`, {
      method: 'POST',
      body: JSON.stringify({
        userId,
        title,
        body,
        data,
      }),
    })
    return true
  } catch (e) {
    console.warn('[Notifications] Push notification failed:', e)
    return false
  }
}

/** Poll for foreground notifications (trap them so they appear on the lock screen). */
export function setupForegroundNotificationHandler() {
  return Notifications.addNotificationReceivedHandler((notification) => {
    console.log('[Notifications] Received foreground notification:', notification)
    return {
      shouldShowAlert: true,
      shouldPlaySound: true,
      shouldSetBadge: true,
    }
  })
}

/** Listen for delivered notifications. */
export function setupDeliveredNotificationHandler() {
  return Notifications.addNotificationResponseReceivedListener((response) => {
    console.log('[Notifications] Notification response:', response)
    // Handle notification tap (e.g., navigate to specific screen)
  })
}

/** Request permissions and return the token. */
export async function initPushNotifications(): Promise<string | null> {
  try {
    const { status: permStatus } = await Notifications.requestPermissionsAsync()
    if (permStatus !== 'granted') {
      return null
    }

    const token = await Notifications.getExpoPushTokenAsync({
      projectId: '12345678-1234-1234-1234-123456789012',
    })

    console.log('[Notifications] Expo push token:', token.data)
    return token.data
  } catch (error) {
    console.error('[Notifications] Failed to get push token:', error)
    return null
  }
}

/** Show a local notification (appears even when app is foregrounded). */
export async function showLocalNotification(
  title: string,
  body: string,
  data?: Record<string, string>,
) {
  const notification = new Notifications.NotificationContent()
  notification.setTitle(title)
  notification.setBody(body)
  if (data) {
    Object.entries(data).forEach(([key, value]) => {
      notification.setValue(key, value)
    })
  }

  await Notifications.displayNotificationAsync(notification)
}
