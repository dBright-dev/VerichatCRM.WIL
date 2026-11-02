import { Platform } from 'react-native'
import { api } from '../constants/api'

type NotificationsModule = typeof import('expo-notifications')

let notificationsModule: NotificationsModule | null | undefined

/** Load notifications on demand so Expo Go can run when its native module is unavailable. */
async function getNotifications(): Promise<NotificationsModule | null> {
  if (notificationsModule !== undefined) return notificationsModule
  try {
    notificationsModule = await import('expo-notifications')
    notificationsModule.setNotificationHandler({
      handleNotification: async () => ({
        shouldPlaySound: true,
        shouldSetBadge: true,
        shouldShowBanner: true,
        shouldShowList: true,
      }),
    })
  } catch (error) {
    notificationsModule = null
    console.warn('[Notifications] Native module is unavailable in this app build:', error)
  }
  return notificationsModule
}

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
  const Notifications = await getNotifications()
  if (!Notifications) return null
  // Ask for notification permissions on supported mobile platforms.
  if (Platform.OS === 'ios' || Platform.OS === 'android') {
    const { status: permStatus } = await Notifications.requestPermissionsAsync()
    if (permStatus !== 'granted') {
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
    await api(`/api/notifications/send`, {
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

/** Subscribe to notifications received while the app is open. */
export function setupForegroundNotificationHandler() {
  return getNotifications().then((Notifications) => Notifications?.addNotificationReceivedListener((notification) => {
    console.log('[Notifications] Received foreground notification:', notification)
  }) ?? null)
}

/** Listen for delivered notifications. */
export function setupDeliveredNotificationHandler() {
  return getNotifications().then((Notifications) => Notifications?.addNotificationResponseReceivedListener((response) => {
    console.log('[Notifications] Notification response:', response)
    // Handle notification tap (e.g., navigate to specific screen)
  }) ?? null)
}

/** Request permissions and return the token. */
export async function initPushNotifications(): Promise<string | null> {
  try {
    const Notifications = await getNotifications()
    if (!Notifications) return null
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
  const Notifications = await getNotifications()
  if (!Notifications) return
  await Notifications.scheduleNotificationAsync({
    content: { title, body, data },
    trigger: null,
  })
}
