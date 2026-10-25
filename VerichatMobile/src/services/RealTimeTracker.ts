import { useEffect, useState } from 'react'
import { api } from '../constants/api'

export type DeliveryTracking = {
  id: string
  trackingNumber: string
  clientName: string
  clientAddress: string
  status: string
  currentLocation: { lat: number; lng: number } | null
  destination: { lat: number; lng: number } | null
  lastUpdated: string
}

export type SupportTicket = {
  id: string
  subject: string
  description: string
  status: string
  priority: string
  clientId: string
  updatedAt: string
}

/** Fetch all deliveries for real-time tracking. */
export async function fetchDeliveries(): Promise<DeliveryTracking[]> {
  return api<DeliveryTracking[]>('/api/delivery')
}

/** Fetch all support tickets for real-time tracking. */
export async function fetchTickets(): Promise<SupportTicket[]> {
  return api<SupportTicket[]>('/api/supporttickets')
}

/** Update delivery status (call this when real-time update received). */
export async function updateDeliveryStatus(
  deliveryId: string,
  status: string,
  details?: string,
): Promise<DeliveryTracking> {
  return api(`/api/delivery/${deliveryId}/status`, {
    method: 'PATCH',
    body: JSON.stringify({ status, details }),
  })
}

/** Update ticket status (call this when real-time update received). */
export async function updateTicketStatus(
  ticketId: string,
  status: string,
  priority?: string,
): Promise<SupportTicket> {
  return api(`/api/supporttickets/${ticketId}/status`, {
    method: 'PATCH',
    body: JSON.stringify({ status, priority }),
  })
}

/** Simulate real-time push to connected mobile clients (for demo). */
export function broadcastRealTimeEvent(event: string, data: any): void {
  // In a real app, this would be sent via SignalR to connected mobile clients
  console.log(`[RealTimeTracker] Broadcast ${event}:`, data)

  // Also dispatch as custom event so the UI can listen
  window.dispatchEvent(
    new CustomEvent('verichat-realtime-update', { detail: { event, data } }),
  )
}

/** Listen for real-time updates from SignalR (web) or simulated events (mobile). */
export function useRealTimeUpdates() {
  const [listeners, setListeners] = useState<Set<(data: any) => void>>(new Set())

  useEffect(() => {
    const handleRealTime = (e: Event) => {
      const evt = e as CustomEvent<{ event: string; data: any }>
      listeners.forEach((listener) => listener(evt.detail.data))
    }

    window.addEventListener('verichat-realtime-update', handleRealTime)

    // In a real app, connect to SignalR:
    // const connection = new HubConnectionBuilder()
    //   .withUrl('https://api.verichat-crm.com/hub/notificationHub')
    //   .withAutomaticReconnect()
    //   .build()
    // await connection.start()
    // connection.on('DeliveryStatusUpdated', (data) => listeners.forEach(l => l(data)))
    // connection.on('TicketStatusUpdated', (data) => listeners.forEach(l => l(data)))

    return () => {
      window.removeEventListener('verichat-realtime-update', handleRealTime)
    }
  }, [listeners])

  const subscribe = (listener: (data: any) => void) => {
    setListeners(prev => new Set(prev).add(listener))
    return () => setListeners(prev => {
      const next = new Set(prev)
      next.delete(listener)
      return next
    })
  }

  return { subscribe }
}
