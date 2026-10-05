import { MaterialCommunityIcons } from '@expo/vector-icons'
import { useCallback, useEffect, useState } from 'react'
import { ActivityIndicator, Alert, FlatList, RefreshControl, StyleSheet, Text, TouchableOpacity, View } from 'react-native'
import { CartLine, changeCartQuantity, getCartLines, getPlacedCartOrders, PlacedCartOrder, recordPlacedOrder, subscribeCart } from '../constants/cartStore'
import { fetchMyOrders, MobileOrderTracking, submitCartOrder } from '../constants/catalogueService'

const PROGRESS = ['pending', 'processing', 'confirmed', 'shipped', 'delivered']

function getProgress(status: string) {
  const value = status.toLowerCase()
  if (value.includes('cancel')) return -1
  if (value.includes('deliver')) return 4
  if (value.includes('ship') || value.includes('transit')) return 3
  if (value.includes('confirm')) return 2
  if (value.includes('process')) return 1
  return 0
}

export default function CartScreen() {
  const [lines, setLines] = useState<CartLine[]>(() => getCartLines())
  const [placed, setPlaced] = useState<PlacedCartOrder[]>(() => getPlacedCartOrders())
  const [orders, setOrders] = useState<MobileOrderTracking[]>([])
  const [loading, setLoading] = useState(true)
  const [refreshing, setRefreshing] = useState(false)
  const [busy, setBusy] = useState(false)

  const loadOrders = useCallback(async () => {
    try {
      setOrders(await fetchMyOrders())
    } catch (error) {
      Alert.alert('Could not load order status', error instanceof Error ? error.message : 'Please try again.')
    } finally {
      setLoading(false)
      setRefreshing(false)
    }
  }, [])

  useEffect(() => {
    const unsubscribe = subscribeCart(() => {
      setLines([...getCartLines()])
      setPlaced([...getPlacedCartOrders()])
    })
    // Screen data comes from the authenticated API once navigation mounts this route.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    void loadOrders()
    const timer = setInterval(() => {
      void fetchMyOrders().then(setOrders).catch(() => {})
    }, 30_000)
    return () => {
      unsubscribe()
      clearInterval(timer)
    }
  }, [loadOrders])

  const refresh = () => {
    setRefreshing(true)
    void loadOrders()
  }

  const checkout = async () => {
    if (busy || lines.length === 0) return
    setBusy(true)
    try {
      const result = await submitCartOrder(lines.map(({ sku, quantity }) => ({ sku, quantity })))
      recordPlacedOrder({ orderId: result.orderId, sku: lines[0].sku, productName: lines.length === 1 ? lines[0].productName : `${lines.length} products`, status: result.status, total: result.totalAmount })
      for (const line of [...lines]) changeCartQuantity(line.sku, 0)
      Alert.alert('Order submitted', 'Your items were sent to Verichat. Order status will update here from the API.')
      await loadOrders()
    } catch (error) {
      Alert.alert('Order not fully submitted', error instanceof Error ? error.message : 'Items already submitted remain saved. Unsubmitted items are still in your cart.')
      await loadOrders()
    } finally {
      setBusy(false)
    }
  }

  const subtotal = lines.reduce((sum, line) => sum + line.price * line.quantity, 0)

  const renderCartLine = ({ item }: { item: CartLine }) => (
    <View style={styles.item_card}>
      <View style={styles.item_info}>
        <Text style={styles.item_name}>{item.productName}</Text>
        <Text style={styles.item_sku}>{item.sku}</Text>
        <Text style={styles.item_price}>${item.price.toFixed(2)} each</Text>
      </View>
      <View style={styles.quantity}>
        <TouchableOpacity accessibilityLabel="Remove one" onPress={() => changeCartQuantity(item.sku, item.quantity - 1)}><MaterialCommunityIcons name="minus-circle-outline" size={27} color="#0057a8" /></TouchableOpacity>
        <Text style={styles.quantity_value}>{item.quantity}</Text>
        <TouchableOpacity accessibilityLabel="Add one" onPress={() => changeCartQuantity(item.sku, item.quantity + 1)}><MaterialCommunityIcons name="plus-circle-outline" size={27} color="#0057a8" /></TouchableOpacity>
      </View>
    </View>
  )

  const renderTrackedOrder = (order: MobileOrderTracking) => {
    const progress = getProgress(order.status)
    return (
      <View key={order.orderId} style={styles.order_card}>
        <View style={styles.order_heading}>
          <View><Text style={styles.order_reference}>Order {order.trackingNumber}</Text><Text style={styles.order_id}>ID: {order.orderId}</Text></View>
          <Text style={[styles.order_status, progress === -1 && styles.cancelled]}>{order.status}</Text>
        </View>
        {progress >= 0 && <View style={styles.progress_row}>{PROGRESS.map((step, index) => <View key={step} style={styles.progress_step}><View style={[styles.progress_dot, index <= progress && styles.progress_done]} /><Text style={styles.progress_label}>{step}</Text></View>)}</View>}
      </View>
    )
  }

  return (
    <View style={styles.screen}>
      <View style={styles.header}><Text style={styles.title}>Cart & order tracking</Text><Text style={styles.subtitle}>Orders and delivery status from Verichat</Text></View>
      <FlatList
        data={lines}
        keyExtractor={(item) => item.sku}
        renderItem={renderCartLine}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={refresh} />}
        ListHeaderComponent={<View>
          <Text style={styles.section_title}>Your cart</Text>
          {lines.length === 0 && <Text style={styles.empty}>Your cart is empty. Add products from Shop.</Text>}
        </View>}
        ListFooterComponent={<View>
          {lines.length > 0 && <View style={styles.checkout_card}><Text style={styles.subtotal}>Estimated subtotal: ${subtotal.toFixed(2)}</Text><Text style={styles.disclaimer}>Final prices and order confirmation come from the Verichat API.</Text><TouchableOpacity style={[styles.checkout_button, busy && styles.disabled]} disabled={busy} onPress={() => void checkout()}><Text style={styles.checkout_text}>{busy ? 'Submitting…' : 'Submit orders'}</Text></TouchableOpacity></View>}
          <View style={styles.orders_header}><Text style={styles.section_title}>Placed and tracked orders</Text><TouchableOpacity onPress={refresh} accessibilityLabel="Refresh order status"><MaterialCommunityIcons name="refresh" size={23} color="#0057a8" /></TouchableOpacity></View>
          {placed.map((order) => <View key={order.orderId} style={styles.order_card}><View style={styles.order_heading}><View><Text style={styles.order_reference}>{order.productName}</Text><Text style={styles.order_id}>Order ID: {order.orderId}</Text></View><Text style={styles.order_status}>{order.status}</Text></View><Text style={styles.order_total}>API total: ${order.total.toFixed(2)}</Text></View>)}
          {loading ? <ActivityIndicator style={{ marginVertical: 12 }} color="#0057a8" /> : orders.length === 0 && placed.length === 0 ? <Text style={styles.empty}>No tracked orders yet.</Text> : null}
          {orders.map(renderTrackedOrder)}
        </View>}
        contentContainerStyle={styles.list}
      />
    </View>
  )
}

const styles = StyleSheet.create({
  screen: { flex: 1, backgroundColor: '#f6f8fa' },
  header: { backgroundColor: '#1f5f8b', paddingHorizontal: 20, paddingTop: 58, paddingBottom: 18 },
  title: { color: '#ffffff', fontWeight: '900', fontSize: 24 },
  subtitle: { color: '#ffffff', marginTop: 4 },
  list: { padding: 16, paddingBottom: 40, gap: 12 },
  section_title: { color: '#0a2f55', fontSize: 18, fontWeight: '900', marginTop: 8, marginBottom: 8 },
  empty: { color: '#59636e', paddingVertical: 12 },
  item_card: { backgroundColor: '#ffffff', borderRadius: 14, padding: 14, flexDirection: 'row', alignItems: 'center', gap: 12 },
  item_info: { flex: 1, gap: 4 },
  item_name: { color: '#111111', fontWeight: '800', fontSize: 15 },
  item_sku: { color: '#68737d', fontSize: 12 },
  item_price: { color: '#0057a8', fontWeight: '700' },
  quantity: { flexDirection: 'row', alignItems: 'center', gap: 8 },
  quantity_value: { minWidth: 20, textAlign: 'center', color: '#111111', fontWeight: '800' },
  checkout_card: { backgroundColor: '#ffffff', borderRadius: 14, padding: 16, gap: 8, marginVertical: 8 },
  subtotal: { color: '#111111', fontWeight: '900', fontSize: 17 },
  disclaimer: { color: '#68737d', fontSize: 12 },
  checkout_button: { backgroundColor: '#0057a8', alignItems: 'center', borderRadius: 12, padding: 13, marginTop: 4 },
  checkout_text: { color: '#ffffff', fontWeight: '900' },
  disabled: { opacity: 0.6 },
  orders_header: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', marginTop: 10 },
  order_card: { backgroundColor: '#ffffff', padding: 14, borderRadius: 14, marginBottom: 10, gap: 12 },
  order_heading: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', gap: 8 },
  order_reference: { color: '#0a2f55', fontWeight: '900' },
  order_id: { color: '#68737d', fontSize: 11, marginTop: 3 },
  order_status: { color: '#0057a8', fontWeight: '800', textTransform: 'capitalize' },
  cancelled: { color: '#c62828' },
  order_total: { color: '#45515d' },
  progress_row: { flexDirection: 'row', justifyContent: 'space-between' },
  progress_step: { alignItems: 'center', flex: 1, gap: 4 },
  progress_dot: { width: 10, height: 10, borderRadius: 5, backgroundColor: '#cbd2d9' },
  progress_done: { backgroundColor: '#219653' },
  progress_label: { fontSize: 9, color: '#59636e', textTransform: 'capitalize' },
})
