import { CRMTheme } from '../constants/crmTheme'
import { MaterialCommunityIcons } from '@expo/vector-icons'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { Alert, FlatList, Modal, StyleSheet, Text, TextInput, TouchableOpacity, View } from 'react-native'
import { useSafeAreaInsets } from 'react-native-safe-area-context'
import { api } from '../constants/api'
import BarcodeScannerModal from '../components/barcode-scanner-modal'
import BrandLoader from '../components/ui/BrandLoader'

type Asset = { assetId: string; serialNumber: string; model: string; manufacturer: string; clientId: string; location?: string | null; nextServiceDue?: string | null; warrantyExpiry?: string | null; status: string }
export default function EquipmentStatusScreen() {
  const insets = useSafeAreaInsets()
  const [assets, setAssets] = useState<Asset[]>([])
  const [loading, setLoading] = useState(true)
  const [scanner, setScanner] = useState(false)
  const [ticketAsset, setTicketAsset] = useState<Asset | null>(null)
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [busy, setBusy] = useState(false)
  const [filterId, setFilterId] = useState<string | null>(null)
  const [today] = useState(() => Date.now())

  const load = useCallback(async () => {
    try { setAssets(await api<Asset[]>('/api/asset/mine')) }
    catch (e) { Alert.alert('Could not load equipment', e instanceof Error ? e.message : 'Please try again.') }
    finally { setLoading(false) }
  }, [])
  useEffect(() => {
    // Load this account's CRM assets when the route mounts.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    void load()
  }, [load])
  const visibleAssets = useMemo(() => filterId ? assets.filter(a => a.assetId === filterId || a.serialNumber.toLowerCase() === filterId.toLowerCase()) : assets, [assets, filterId])

  const submitTicket = async () => {
    if (!ticketAsset || !title.trim() || !description.trim() || busy) { Alert.alert('Add ticket details', 'Enter a short title and describe what is happening.'); return }
    setBusy(true)
    try {
      await api('/api/supportticket', { method: 'POST', body: JSON.stringify({ clientId: ticketAsset.clientId, equipmentId: ticketAsset.assetId, title: title.trim(), description: description.trim(), priority: 'Medium', category: 'Technical' }) })
      setTicketAsset(null); setTitle(''); setDescription(''); Alert.alert('Service ticket logged', 'The support team can now see this equipment issue.')
    } catch (e) { Alert.alert('Could not log service ticket', e instanceof Error ? e.message : 'Please try again.') }
    finally { setBusy(false) }
  }

  return <View style={[s.screen, { paddingTop: insets.top + 12 }]}>
    <View style={s.heading}><View><Text style={s.title}>My Assets</Text><Text style={s.subtitle}>Equipment registered to your organisation</Text></View><TouchableOpacity onPress={() => setScanner(true)} style={s.scan}><MaterialCommunityIcons name="qrcode-scan" size={21} color="#fff"/><Text style={s.scanText}>Scan</Text></TouchableOpacity></View>
    {filterId && <TouchableOpacity onPress={() => setFilterId(null)} style={s.filter}><Text style={s.filterText}>Showing scan result · Clear</Text></TouchableOpacity>}
    {loading ? <BrandLoader label="Loading your equipment…"/> : <FlatList data={visibleAssets} keyExtractor={x => x.assetId} contentContainerStyle={s.list} refreshing={loading} onRefresh={() => { setLoading(true); void load() }} ListEmptyComponent={<Text style={s.empty}>{assets.length ? 'No equipment matches that QR code.' : 'No equipment is currently linked to your client account.'}</Text>} renderItem={({item}) => {
      const needsService = item.nextServiceDue && new Date(item.nextServiceDue) < new Date(today + 30 * 86400000)
      const ok = ['active','operational','in service'].includes(item.status.toLowerCase())
      return <View style={s.card}><View style={s.cardTop}><View style={s.machine}><MaterialCommunityIcons name="medical-cotton-swab" size={28} color={CRMTheme.primary}/></View><View style={s.machineInfo}><Text style={s.model}>{item.model}</Text><Text style={s.manufacturer}>{item.manufacturer} · S/N {item.serialNumber}</Text></View><View style={[s.badge, ok ? s.good : s.warn]}><Text style={[s.badgeText, ok ? s.goodText : s.warnText]}>{item.status}</Text></View></View><View style={s.meta}><Text style={s.metaText}>⌖ {item.location || 'Location not recorded'}</Text><Text style={s.metaText}>Next calibration/service: {item.nextServiceDue ? new Date(item.nextServiceDue).toLocaleDateString() : 'Not scheduled'}</Text><Text style={s.metaText}>Warranty expiry: {item.warrantyExpiry ? new Date(item.warrantyExpiry).toLocaleDateString() : 'Not recorded'}</Text></View>{needsService && <Text style={s.due}>Service due soon</Text>}<TouchableOpacity style={s.ticket} onPress={() => setTicketAsset(item)}><MaterialCommunityIcons name="wrench-outline" size={18} color="#fff"/><Text style={s.ticketText}>Log Service Ticket</Text></TouchableOpacity></View>
    }}/>} 
    <BarcodeScannerModal visible={scanner} title="Scan equipment QR code" onClose={() => setScanner(false)} onScanned={code => { const found = assets.find(a => a.assetId.toLowerCase() === code.toLowerCase() || a.serialNumber.toLowerCase() === code.toLowerCase()); setFilterId(found ? code : code); setScanner(false); if (!found) Alert.alert('Equipment not found', 'This QR code does not match equipment registered to your account.') }}/>
    <Modal visible={!!ticketAsset} transparent animationType="slide" onRequestClose={() => setTicketAsset(null)}><View style={s.backdrop}><View style={s.sheet}><Text style={s.modalTitle}>Log service ticket</Text><Text style={s.modalHint}>{ticketAsset?.model} · {ticketAsset?.serialNumber}</Text><TextInput style={s.input} value={title} onChangeText={setTitle} placeholder="Issue summary" maxLength={200}/><TextInput style={[s.input,s.multiline]} value={description} onChangeText={setDescription} placeholder="Describe the fault or service needed" multiline maxLength={4000}/><TouchableOpacity style={s.ticket} disabled={busy} onPress={() => void submitTicket()}><Text style={s.ticketText}>{busy ? 'Sending…' : 'Submit support ticket'}</Text></TouchableOpacity><TouchableOpacity onPress={() => setTicketAsset(null)}><Text style={s.cancel}>Cancel</Text></TouchableOpacity></View></View></Modal>
  </View>
}

const s=StyleSheet.create({screen:{flex:1,backgroundColor:CRMTheme.background},heading:{paddingHorizontal:18,paddingBottom:14,flexDirection:'row',justifyContent:'space-between',alignItems:'center'},title:{color:CRMTheme.text,fontSize:25,fontWeight:'900'},subtitle:{color:'#63758a',fontSize:12,marginTop:4},scan:{backgroundColor:CRMTheme.primary,borderRadius:13,paddingHorizontal:12,paddingVertical:9,flexDirection:'row',alignItems:'center',gap:6},scanText:{color:'#fff',fontWeight:'800'},filter:{paddingHorizontal:18,paddingBottom:8},filterText:{color:CRMTheme.primary,fontWeight:'700'},list:{padding:16,gap:12},card:{padding:15,backgroundColor:'#fff',borderRadius:17,borderWidth:1,borderColor:'#e4eaf1',gap:12},cardTop:{flexDirection:'row',alignItems:'center',gap:10},machine:{width:48,height:48,borderRadius:14,backgroundColor:'#e9f3fb',alignItems:'center',justifyContent:'center'},machineInfo:{flex:1},model:{color:'#1c344b',fontWeight:'900',fontSize:16},manufacturer:{color:'#66788c',fontSize:11,marginTop:3},badge:{paddingHorizontal:9,paddingVertical:5,borderRadius:12},good:{backgroundColor:'#e2f5ea'},warn:{backgroundColor:'#fff0dd'},badgeText:{fontSize:10,fontWeight:'900',textTransform:'capitalize'},goodText:{color:'#167346'},warnText:{color:'#a65d00'},meta:{gap:6,borderTopWidth:1,borderTopColor:'#eff2f5',paddingTop:10},metaText:{fontSize:12,color:'#52677d'},due:{color:'#a65d00',fontWeight:'800'},ticket:{minHeight:42,backgroundColor:CRMTheme.primary,borderRadius:11,alignItems:'center',justifyContent:'center',flexDirection:'row',gap:8},ticketText:{color:'#fff',fontWeight:'900'},empty:{padding:24,textAlign:'center',color:'#65778b'},backdrop:{flex:1,backgroundColor:'#0007',justifyContent:'flex-end'},sheet:{backgroundColor:'#fff',borderTopLeftRadius:24,borderTopRightRadius:24,padding:22,paddingBottom:36,gap:12},modalTitle:{color:CRMTheme.text,fontSize:20,fontWeight:'900'},modalHint:{color:'#60758a'},input:{borderWidth:1,borderColor:'#d6dee8',borderRadius:11,padding:12,color:'#18334d'},multiline:{minHeight:100,textAlignVertical:'top'},cancel:{textAlign:'center',padding:10,color:'#64748b'}})


