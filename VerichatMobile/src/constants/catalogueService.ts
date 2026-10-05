import { api } from './api'
import type { ClientDashboard } from './dashboardService'

/** Quick catalogue read against the real product API (auth: any signed-in role). */
export type MobileProduct = {
  sku: string
  productName: string
  category: string
  price: number
  stockQty: number
  isLowStock: boolean
  /** Product photo set by an administrator; absent when none was uploaded. */
  imageUrl?: string | null
}

export async function fetchMobileCatalogue(category?: string): Promise<MobileProduct[]> {
  const query = category && category !== 'All' ? `?category=${encodeURIComponent(category)}` : ''
  return api<MobileProduct[]>(`/api/product${query}`)
}

/** 1-tap reorder of a recurring consumable → creates a pre-filled order. */
export async function quickReorder(sku: string): Promise<{
  orderId: string
  total: number
  status: string
}> {
  return api(`/api/mobile/reorder/${encodeURIComponent(sku)}`, { method: 'POST' })
}

/** Live order tracking for the dashboard's "track my order" cards. */
export type MobileOrderTracking = {
  orderId: string
  trackingNumber: string
  status: string
  expectedDeliveryDate?: string
  totalAmount: number
}

export async function fetchMyOrders(): Promise<MobileOrderTracking[]> {
  const dashboard = await api<ClientDashboard>('/api/mobile/dashboard')
  return dashboard.trackingOrders.map((t) => ({
    orderId: t.id,
    trackingNumber: t.reference,
    status: t.status,
    totalAmount: 0,
  }))
}
