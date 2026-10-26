import { useCallback, useEffect, useState } from 'react'
import { ActivityIndicator, Alert, FlatList, RefreshControl, StyleSheet, Text, TouchableOpacity, View } from 'react-native'
import { CRMTheme } from '../constants/crmTheme'
import { MaterialCommunityIcons } from '@expo/vector-icons'
import { CartLine, changeCartQuantity, getCartLines, subscribeCart } from '../constants/cartStore'
import { fetchMyOrders, fetchMyQuotations, MobileOrderTracking, MobileQuotation, requestQuotation, respondToQuotation } from '../constants/catalogueService'
import AnimatedButton from '../components/ui/AnimatedButton'
import { subscribeGlobalRefresh } from '../utils/refreshEvents'

export default function CartScreen() {
  const [lines, setLines] = useState<CartLine[]>(() => getCartLines())
  const [orders, setOrders] = useState<MobileOrderTracking[]>([])
  const [quotations, setQuotations] = useState<MobileQuotation[]>([])
  const [loading, setLoading] = useState(true)
  const [refreshing, setRefreshing] = useState(false)
  const [busy, setBusy] = useState(false)
  const [activityError, setActivityError] = useState<string | null>(null)

  const load = useCallback(async () => {
    try {
      const [ordersResult, quotesResult] = await Promise.allSettled([fetchMyOrders(), fetchMyQuotations()])
      if (ordersResult.status === 'fulfilled') setOrders(ordersResult.value)
      if (quotesResult.status === 'fulfilled') setQuotations(quotesResult.value)
      const errors = [
        ordersResult.status === 'rejected' ? `Orders: ${ordersResult.reason instanceof Error ? ordersResult.reason.message : 'request failed.'}` : null,
        quotesResult.status === 'rejected' ? `Quotations: ${quotesResult.reason instanceof Error ? quotesResult.reason.message : 'request failed.'}` : null,
      ].filter((message): message is string => message !== null)
      setActivityError(errors.length ? errors.join(' · ') : null)
    } catch (e) { setActivityError(e instanceof Error ? e.message : 'Please try again.') }
    finally { setLoading(false); setRefreshing(false) }
  }, [])
  useEffect(() => {
    const unsubscribe = subscribeCart(() => setLines([...getCartLines()]))
    // Pull account data on mount, then refresh while this route is active.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    void load()
    const timer = setInterval(() => { void load() }, 60_000)
    const stopGlobalRefresh = subscribeGlobalRefresh(() => { void load() })
    return () => { unsubscribe(); clearInterval(timer); stopGlobalRefresh() }
  }, [load])

  const submit = async () => {
    if (busy || lines.length === 0) return
    setBusy(true)
    try {
      const quote = await requestQuotation(lines.map(({ sku, quantity }) => ({ sku, quantity })))
      for (const line of [...lines]) changeCartQuantity(line.sku, 0)
      Alert.alert('Quote request sent', `${quote.quoteReference} is now in the sales team's draft queue.`)
      await load()
    } catch (e) { Alert.alert('Quote request not sent', e instanceof Error ? e.message : 'Please retry. Your cart remains available.') }
    finally { setBusy(false) }
  }
  const respond = async (quotationId: string, status: 'Accepted' | 'Rejected') => {
    try { await respondToQuotation(quotationId, status); await load() }
    catch (e) { Alert.alert('Could not update quotation', e instanceof Error ? e.message : 'Please try again.') }
  }
  const subtotal = lines.reduce((sum, item) => sum + item.price * item.quantity, 0)
  const renderLine = ({ item }: { item: CartLine }) => <View style={s.item}><View style={s.itemText}><Text style={s.name}>{item.productName}</Text><Text style={s.sku}>{item.sku}</Text><Text style={s.price}>Catalogue estimate: R {item.price.toFixed(2)}</Text></View><View style={s.quantity}><TouchableOpacity onPress={() => changeCartQuantity(item.sku, item.quantity - 1)} accessibilityLabel="Remove one"><MaterialCommunityIcons name="minus-circle-outline" size={26} color={CRMTheme.primary}/></TouchableOpacity><Text style={s.quantityText}>{item.quantity}</Text><TouchableOpacity onPress={() => changeCartQuantity(item.sku, item.quantity + 1)} accessibilityLabel="Add one"><MaterialCommunityIcons name="plus-circle-outline" size={26} color={CRMTheme.primary}/></TouchableOpacity></View></View>
  return <View style={s.screen}>
    <View style={s.header}><Text style={s.title}>Quote Request Cart</Text><Text style={s.subtitle}>Ask your account manager to confirm contract pricing and availability.</Text></View>
    <FlatList data={lines} keyExtractor={x => x.sku} renderItem={renderLine} contentContainerStyle={s.list} refreshControl={<RefreshControl refreshing={refreshing} onRefresh={() => { setRefreshing(true); void load() }}/>} ListHeaderComponent={<Text style={s.section}>Equipment requested</Text>} ListEmptyComponent={<Text style={s.empty}>Your request cart is empty. Add products from the Equipment Shop.</Text>} ListFooterComponent={<View>
      {lines.length > 0 && <View style={s.box}><Text style={s.subtotal}>Indicative total: R {subtotal.toFixed(2)}</Text><Text style={s.caption}>This is a request, not a purchase. The server recalculates from current catalogue prices.</Text><AnimatedButton style={s.button} loading={busy} disabled={busy} shimmer onPress={() => void submit()}><Text style={s.buttonText}>Request a Quote</Text></AnimatedButton></View>}
      {activityError ? <View style={s.errorCard}><Text style={s.errorTitle}>Some activity could not be loaded</Text><Text style={s.caption}>{activityError}</Text><TouchableOpacity onPress={() => { setRefreshing(true); void load() }}><Text style={s.retry}>Try again</Text></TouchableOpacity></View> : null}
      <Text style={s.section}>Quote requests</Text>{quotations.length === 0 ? <Text style={s.empty}>No quote requests available for this account.</Text> : quotations.map(q => <View key={q.quotationId} style={s.record}><View style={s.row}><Text style={s.recordTitle}>{q.quoteReference}</Text><Text style={s.status}>{q.status}</Text></View><Text style={s.caption}>{q.lineItems.map(i => `${i.productName} × ${i.quantity}`).join(', ')}</Text><Text style={s.caption}>{q.status === 'Sent' ? 'Verified quotation total incl. VAT:' : 'Estimated total incl. VAT:'} R {q.grandTotal.toFixed(2)}</Text>{q.status === 'Sent' ? <View style={s.row}><TouchableOpacity style={s.responseButton} onPress={() => void respond(q.quotationId, 'Accepted')}><Text style={s.responseText}>Accept quotation</Text></TouchableOpacity><TouchableOpacity style={s.declineButton} onPress={() => void respond(q.quotationId, 'Rejected')}><Text style={s.responseText}>Decline</Text></TouchableOpacity></View> : null}</View>)}
      <Text style={s.section}>Order tracking</Text>{loading ? <ActivityIndicator color={CRMTheme.primary}/> : orders.length === 0 ? <Text style={s.empty}>No confirmed orders yet.</Text> : orders.map(o => <View key={o.orderId} style={s.record}><Text style={s.recordTitle}>Order {o.trackingNumber}</Text><Text style={s.caption}>{o.status} · R {o.totalAmount.toFixed(2)}</Text><Text style={s.caption}>Expected delivery: {o.expectedDeliveryDate ? new Date(o.expectedDeliveryDate).toLocaleDateString() : 'Not provided'}</Text></View>)}
    </View>}/>
  </View>
}

const s=StyleSheet.create({screen:{flex:1,backgroundColor:CRMTheme.background},header:{backgroundColor:CRMTheme.primaryDark,paddingHorizontal:20,paddingTop:58,paddingBottom:20},title:{fontSize:24,fontWeight:'900',color:'#fff'},subtitle:{color:'#e1edf7',marginTop:5,lineHeight:20},list:{padding:16,gap:11,paddingBottom:36},section:{fontSize:18,fontWeight:'900',color:CRMTheme.text,marginTop:12,marginBottom:8},empty:{color:CRMTheme.muted,paddingVertical:12},item:{backgroundColor:CRMTheme.surface,padding:14,borderRadius:14,flexDirection:'row',alignItems:'center',gap:10},itemText:{flex:1,gap:3},name:{color:CRMTheme.text,fontWeight:'900'},sku:{color:CRMTheme.muted,fontSize:12},price:{color:CRMTheme.primary,fontWeight:'700',fontSize:12},quantity:{flexDirection:'row',alignItems:'center',gap:8},quantityText:{minWidth:20,textAlign:'center',fontWeight:'900',color:CRMTheme.text},box:{backgroundColor:CRMTheme.surface,padding:16,borderRadius:14,gap:8,marginVertical:10},subtotal:{fontSize:17,fontWeight:'900',color:CRMTheme.text},caption:{fontSize:12,color:CRMTheme.muted,lineHeight:18},button:{backgroundColor:CRMTheme.primary,minHeight:46,borderRadius:12,alignItems:'center',justifyContent:'center',padding:12,marginTop:4},buttonText:{color:'#fff',fontWeight:'900'},record:{backgroundColor:CRMTheme.surface,padding:14,borderRadius:13,gap:5,marginBottom:9},row:{flexDirection:'row',justifyContent:'space-between',gap:8},responseButton:{backgroundColor:'#24764a',paddingHorizontal:12,paddingVertical:9,borderRadius:9},declineButton:{backgroundColor:'#9b3e39',paddingHorizontal:12,paddingVertical:9,borderRadius:9},responseText:{color:'#fff',fontWeight:'800',fontSize:12},recordTitle:{fontWeight:'900',color:CRMTheme.text},status:{fontWeight:'800',color:CRMTheme.primary,textTransform:'capitalize'},errorCard:{backgroundColor:'#fff4f2',borderColor:'#f3c3bd',borderWidth:1,borderRadius:13,padding:13,gap:5,marginVertical:6},errorTitle:{fontWeight:'800',color:'#8c3028'},retry:{color:CRMTheme.primary,fontWeight:'800',paddingTop:4}})




