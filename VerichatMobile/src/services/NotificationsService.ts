import { api } from '../constants/api'

export type Notification = {
  id: string
  type: 'info' | 'warning' | 'error' | 'success'
  title: string
  message: string
  timestamp: string
  read: boolean
  clientId?: string
  ticketId?: string
  deliveryId?: string
}

/** Fetch all notifications for the current user. */
export async function fetchNotifications(): Promise<Notification[]> {
  return api<Notification[]>('/api/notifications')
}

/** Mark a single notification as read. */
export async function markAsRead(notificationId: string): Promise<void> {
  await api(`/api/notifications/${notificationId}/read`, { method: 'POST' })
}

/** Mark all notifications as read. */
export async function markAllAsRead(): Promise<void> {
  await api('/api/notifications/read-all', { method: 'POST' })
}

/** Delete a notification. */
export async function deleteNotification(notificationId: string): Promise<void> {
  await api(`/api/notifications/${notificationId}`, { method: 'DELETE' })
}

/** Simulate receiving a real-time notification (used for demo/offline-first testing). */
export function simulateNotificationReceived(
  type: Notification['type'],
  title: string,
  message: string,
  clientId?: string,
  ticketId?: string,
  deliveryId?: string,
): Notification {
  const now = new Date().toISOString()
  const notification: Notification = {
    id: `sim-${Date.now()}`,
    type,
    title,
    message,
    timestamp: now,
    read: false,
    clientId,
    ticketId,
    deliveryId,
  }

  // In a real app, SignalR would deliver this directly to the hub callback.
  // For demo purposes, we log it and optionally dispatch a custom event.
  console.log('[NotificationService] Received:', notification)

  // Dispatch to any listening components
  window.dispatchEvent(
    new CustomEvent('verichat-notification', { detail: notification }),
  )

  return notification
}

/** Subscribe to real-time notifications (SignalR pattern). */
export function onNotification(callback: (n: Notification) => void) {
  const handler = (e: Event) => {
    const evt = e as CustomEvent<Notification>
    callback(evt.detail)
  }
  window.addEventListener('verichat-notification', handler)
  return () => window.removeEventListener('verichat-notification', handler)
}