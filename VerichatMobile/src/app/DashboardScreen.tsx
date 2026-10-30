import { MaterialCommunityIcons } from '@expo/vector-icons'
import { LinearGradient } from 'expo-linear-gradient'
import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react'
import { Alert, RefreshControl, ScrollView, StyleSheet, Text, TouchableOpacity, View } from 'react-native'
import { useSafeAreaInsets } from 'react-native-safe-area-context'
import { api } from '../constants/api'
import { CRMTheme } from '../constants/crmTheme'
import SalesGoalCard from './GoalsScreen'
import BarcodeScannerModal from '../components/barcode-scanner-modal'
import BrandLoader from '../components/ui/BrandLoader'
import { subscribeGlobalRefresh } from '../utils/refreshEvents'

const C = CRMTheme
type Asset = { assetId: string; model: string; serialNumber: string; manufacturer: string; status: string; clientId: string; nextServiceDue?: string | null }
type Ticket = { ticketId: string; title: string; status: string; priority: string; createdAt: string; trackingNumber: string }
type Delivery = { deliveryId: string; trackingNumber: string; clientId: string; destination: string; status: string; estimatedArrival: string }

export default function DashboardScreen({ clientId }: { clientId?: string }) {
  const insets = useSafeAreaInsets()
  const [assets, setAssets] = useState<Asset[]>([])
  const [tickets, setTickets] = useState<Ticket[]>([])
  const [deliveries, setDeliveries] = useState<Delivery[]>([])
  const [loading, setLoading] = useState(true)
  const [refreshing, setRefreshing] = useState(false)
  const [scanOpen, setScanOpen] = useState(false)
  const [query, setQuery] = useState('')
  const [today] = useState(() => Date.now())

  const load = useCallback(async () => {
    try {
      const assetPath = clientId ? `/api/asset/client/${encodeURIComponent(clientId)}` : '/api/asset'
      const [assetRows, ticketRows, deliveryRows] = await Promise.all([
        api<Asset[]>(assetPath),
        api<Ticket[]>(clientId ? `/api/supportticket/client/${encodeURIComponent(clientId)}` : '/api/supportticket').catch(() => []),
        api<Delivery[]>(clientId ? `/api/delivery/client/${encodeURIComponent(clientId)}` : '/api/delivery').catch(() => []),
      ])
      setAssets(assetRows); setTickets(ticketRows); setDeliveries(deliveryRows)
    } catch (cause) { Alert.alert('Could not refresh operations', cause instanceof Error ? cause.message : 'Please try again.') }
    finally { setLoading(false); setRefreshing(false) }
  }, [clientId])

  useEffect(() => {
    // Load the latest CRM operations records when this dashboard opens.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    void load()
  }, [load])
  useEffect(() => subscribeGlobalRefresh(() => { void load() }), [load])

  const filteredAssets = useMemo(() => assets.filter(asset => !query || [asset.model, asset.serialNumber, asset.assetId].some(value => value.toLowerCase().includes(query.toLowerCase()))), [assets, query])
  const dueSoon = assets.filter(asset => asset.nextServiceDue && new Date(asset.nextServiceDue).getTime() < today + 30 * 86400000).length
  const statusColor = (status: string) => ['active', 'operational', 'in service', 'resolved', 'delivered'].includes(status.toLowerCase()) ? '#24764a' : '#a46100'

  if (loading) return <View style={[s.screen, { paddingTop: insets.top + 12 }]}><Text style={s.loadingTitle}>Operations dashboard</Text><BrandLoader label="Loading operations…" /></View>

  return <View style={s.screen}>
    <ScrollView contentContainerStyle={{ paddingBottom: insets.bottom + 26 }} refreshControl={<RefreshControl refreshing={refreshing} tintColor={C.primary} onRefresh={() => { setRefreshing(true); void load() }} />}>
      <LinearGradient colors={[C.primaryDark, C.primary]} start={{ x: 0, y: 0 }} end={{ x: 1, y: 1 }} style={[s.hero, { paddingTop: insets.top + 18 }]}>
        <Text style={s.eyebrow}>VERICHAT CRM</Text><Text style={s.title}>Operations dashboard</Text><Text style={s.subtitle}>Your equipment, service activity, deliveries, and sales goals.</Text>
        <View style={s.metrics}>
          <Metric icon="medical-cotton-swab" label="Assets" value={assets.length} />
          <View style={s.metricDivider} />
          <Metric icon="wrench-outline" label="Service due" value={dueSoon} />
          <View style={s.metricDivider} />
          <Metric icon="ticket-outline" label="Open tickets" value={tickets.filter(ticket => !['resolved', 'closed'].includes(ticket.status.toLowerCase())).length} />
        </View>
      </LinearGradient>

      <View style={s.content}>
        <SalesGoalCard />
        <View style={s.sectionHeading}><View><Text style={s.sectionTitle}>My assets</Text><Text style={s.sectionSubtitle}>Equipment linked to this account</Text></View><TouchableOpacity style={s.scan} accessibilityRole="button" accessibilityLabel="Scan equipment QR or barcode" onPress={() => setScanOpen(true)}><MaterialCommunityIcons name="qrcode-scan" size={19} color="#fff"/><Text style={s.scanText}>Scan</Text></TouchableOpacity></View>
        {query ? <TouchableOpacity onPress={() => setQuery('')}><Text style={s.clearFilter}>Scan: {query} · Clear</Text></TouchableOpacity> : null}
        <View style={s.card}>
          {filteredAssets.length ? filteredAssets.map((asset, index) => <View key={asset.assetId} style={[s.assetRow, index > 0 && s.divider]}><View style={s.icon}><MaterialCommunityIcons name="medical-cotton-swab" size={21} color={C.primary}/></View><View style={s.grow}><Text style={s.primary}>{asset.model} · {asset.serialNumber}</Text><Text style={s.secondary}>{asset.manufacturer} · Next service {asset.nextServiceDue ? new Date(asset.nextServiceDue).toLocaleDateString() : 'not scheduled'}</Text></View><Text style={[s.badge, { color: statusColor(asset.status) }]}>{asset.status}</Text></View>) : <Text style={s.empty}>{query ? 'No equipment matched this code.' : 'No equipment is registered to this account yet.'}</Text>}
        </View>

        <Section title="Support tickets" subtitle="Recent service and repair requests" icon="ticket-outline">
          {tickets.length ? tickets.slice(0, 4).map(ticket => <View key={ticket.ticketId} style={s.listRow}><View style={s.grow}><Text style={s.primary}>{ticket.trackingNumber} · {ticket.title}</Text><Text style={s.secondary}>{ticket.priority} priority · {new Date(ticket.createdAt).toLocaleDateString()}</Text></View><Text style={[s.badge, { color: statusColor(ticket.status) }]}>{ticket.status}</Text></View>) : <Text style={s.empty}>No support tickets found.</Text>}
        </Section>
        <Section title="Deliveries" subtitle="Order fulfilment progress" icon="truck-delivery-outline">
          {deliveries.length ? deliveries.slice(0, 4).map(delivery => <View key={delivery.deliveryId} style={s.listRow}><View style={s.grow}><Text style={s.primary}>{delivery.trackingNumber}</Text><Text style={s.secondary}>{delivery.destination} · ETA {new Date(delivery.estimatedArrival).toLocaleDateString()}</Text></View><Text style={[s.badge, { color: statusColor(delivery.status) }]}>{delivery.status}</Text></View>) : <Text style={s.empty}>No deliveries found.</Text>}
        </Section>
      </View>
    </ScrollView>
    <BarcodeScannerModal visible={scanOpen} title="Scan asset QR or barcode" onClose={() => setScanOpen(false)} onScanned={code => { setQuery(code); setScanOpen(false) }} />
  </View>
}

function Metric({ icon, label, value }: { icon: 'medical-cotton-swab' | 'wrench-outline' | 'ticket-outline'; label: string; value: number }) {
  return <View style={s.metric}><MaterialCommunityIcons name={icon} size={19} color="#d4eafa"/><Text style={s.metricValue}>{value}</Text><Text style={s.metricLabel}>{label}</Text></View>
}

function Section({ title, subtitle, icon, children }: { title: string; subtitle: string; icon: 'ticket-outline' | 'truck-delivery-outline'; children: ReactNode }) {
  return <View style={s.sectionWrap}><View style={s.sectionHeading}><View><Text style={s.sectionTitle}>{title}</Text><Text style={s.sectionSubtitle}>{subtitle}</Text></View><MaterialCommunityIcons name={icon} size={22} color={C.primary}/></View><View style={s.card}>{children}</View></View>
}

const s = StyleSheet.create({
  screen: { flex: 1, backgroundColor: C.background }, loadingTitle: { color: C.text, fontSize: 22, fontWeight: '900', margin: 20 },
  hero: { paddingHorizontal: 21, paddingBottom: 22, borderBottomLeftRadius: 26, borderBottomRightRadius: 26 }, eyebrow: { color: '#c8dfef', letterSpacing: 1.6, fontSize: 10, fontWeight: '900' },
  title: { color: '#fff', fontSize: 27, fontWeight: '900', marginTop: 5 }, subtitle: { color: '#e0eef7', fontSize: 13, lineHeight: 19, marginTop: 5, marginBottom: 17 },
  metrics: { flexDirection: 'row', alignItems: 'center', backgroundColor: '#ffffff1c', borderRadius: 16, paddingVertical: 12 }, metric: { flex: 1, alignItems: 'center', gap: 3 }, metricValue: { color: '#fff', fontSize: 19, fontWeight: '900' }, metricLabel: { color: '#dfedf6', fontSize: 10, fontWeight: '700' }, metricDivider: { height: 34, width: 1, backgroundColor: '#ffffff40' },
  content: { paddingHorizontal: 17, paddingTop: 18 }, sectionWrap: { marginTop: 4 }, sectionHeading: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginTop: 16, marginBottom: 10 }, sectionTitle: { color: C.text, fontSize: 17, fontWeight: '900' }, sectionSubtitle: { color: C.muted, fontSize: 12, marginTop: 3 },
  card: { backgroundColor: C.surface, borderRadius: 17, borderWidth: 1, borderColor: C.border, paddingHorizontal: 14, paddingVertical: 3, marginBottom: 13 }, assetRow: { flexDirection: 'row', alignItems: 'center', gap: 10, paddingVertical: 12 }, divider: { borderTopWidth: 1, borderTopColor: C.border }, icon: { width: 40, height: 40, borderRadius: 13, backgroundColor: C.surfaceAlt, alignItems: 'center', justifyContent: 'center' }, grow: { flex: 1 }, primary: { color: C.text, fontSize: 13, fontWeight: '800' }, secondary: { color: C.muted, fontSize: 11, marginTop: 3 }, badge: { fontSize: 10, fontWeight: '900', textTransform: 'capitalize' }, empty: { color: C.muted, paddingVertical: 14, fontSize: 13 },
  scan: { flexDirection: 'row', alignItems: 'center', gap: 6, backgroundColor: C.primary, borderRadius: 12, paddingHorizontal: 11, paddingVertical: 8 }, scanText: { color: '#fff', fontWeight: '800', fontSize: 12 }, clearFilter: { color: C.primary, fontWeight: '700', marginBottom: 8 }, listRow: { flexDirection: 'row', alignItems: 'center', gap: 10, paddingVertical: 12, borderBottomWidth: 1, borderBottomColor: C.border },
})

