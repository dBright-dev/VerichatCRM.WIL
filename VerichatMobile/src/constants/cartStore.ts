export type CartLine = {
  sku: string
  productName: string
  price: number
  quantity: number
}

export type PlacedCartOrder = {
  orderId: string
  sku: string
  productName: string
  status: string
  total: number
}

let lines: CartLine[] = []
let placedOrders: PlacedCartOrder[] = []
const listeners = new Set<() => void>()

function notify() {
  listeners.forEach((listener) => listener())
}

export function getCartLines() {
  return lines
}

export function getPlacedCartOrders() {
  return placedOrders
}

export function subscribeCart(listener: () => void) {
  listeners.add(listener)
  return () => { listeners.delete(listener) }
}

export function addCartItem(item: Omit<CartLine, 'quantity'>) {
  const existing = lines.find((line) => line.sku === item.sku)
  lines = existing
    ? lines.map((line) => line.sku === item.sku ? { ...line, quantity: line.quantity + 1 } : line)
    : [...lines, { ...item, quantity: 1 }]
  notify()
}

export function changeCartQuantity(sku: string, quantity: number) {
  lines = quantity <= 0
    ? lines.filter((line) => line.sku !== sku)
    : lines.map((line) => line.sku === sku ? { ...line, quantity } : line)
  notify()
}

export function recordPlacedOrder(order: PlacedCartOrder) {
  placedOrders = [order, ...placedOrders].slice(0, 20)
  const line = lines.find((item) => item.sku === order.sku)
  if (line) changeCartQuantity(order.sku, line.quantity - 1)
  else notify()
}

export function clearCart() {
  lines = []
  notify()
}
