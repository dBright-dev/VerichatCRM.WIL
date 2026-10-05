import { api } from './api'

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
  const orders = await api<{ orderId: string; trackingNumber?: string | null; status: string; totalAmount: number; expectedDeliveryDate?: string | null }[]>('/api/mobile/orders')
  return orders.map((o) => ({ orderId: o.orderId, trackingNumber: o.trackingNumber || o.orderId, status: o.status, totalAmount: o.totalAmount, expectedDeliveryDate: o.expectedDeliveryDate || undefined }))
}

export async function submitCartOrder(lines: { sku: string; quantity: number }[]) {
  return api<{ orderId: string; totalAmount: number; status: string }>('/api/mobile/orders', {
    method: 'POST', body: JSON.stringify({ lineItems: lines }),
  })
}

export type MobileQuotation = { quotationId: string; quoteReference: string; status: string; grandTotal: number; validUntil: string; lineItems: { productName: string; quantity: number; lineTotal: number }[] }

export async function fetchMyQuotations(): Promise<MobileQuotation[]> {
  return api<MobileQuotation[]>('/api/mobile/quotations')
}
