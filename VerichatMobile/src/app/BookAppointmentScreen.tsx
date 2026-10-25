import { useCallback, useEffect, useState } from 'react'
import { Alert, Modal, ScrollView, StyleSheet, Text, TextInput, TouchableOpacity, View } from 'react-native'
import { useSafeAreaInsets } from 'react-native-safe-area-context'
import { CRMTheme } from '../constants/crmTheme'
import { api } from '../constants/api'
import AnimatedButton from '../components/ui/AnimatedButton'
import BrandLoader from '../components/ui/BrandLoader'

type Appointment = { id: string; topic: string; scheduledDate: string; status: string; notes?: string | null }
const TOPICS = ['New Quote', 'Equipment Calibration', 'Equipment Service', 'Invoicing Issue', 'Product Consultation', 'Other']

export default function BookAppointmentScreen() {
  const insets = useSafeAreaInsets()
  const [appointments, setAppointments] = useState<Appointment[]>([])
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [topic, setTopic] = useState(TOPICS[0])
  const [topicOpen, setTopicOpen] = useState(false)
  const [notes, setNotes] = useState('')
  const [selectedDate, setSelectedDate] = useState(() => {
    const next = new Date(); next.setDate(next.getDate() + 1); next.setHours(10, 0, 0, 0); return next
  })
  const [calendarOpen, setCalendarOpen] = useState(false)
  const [month, setMonth] = useState(() => new Date(selectedDate.getFullYear(), selectedDate.getMonth(), 1))

  const load = useCallback(async () => {
    try { setAppointments(await api<Appointment[]>('/api/appointments')) }
    catch (e) { Alert.alert('Could not load appointments', e instanceof Error ? e.message : 'Please try again.') }
    finally { setLoading(false) }
  }, [])
  useEffect(() => {
    // Initial API load is an external synchronization task.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    void load()
  }, [load])

  const days = new Date(month.getFullYear(), month.getMonth() + 1, 0).getDate()
  const offset = (new Date(month.getFullYear(), month.getMonth(), 1).getDay() + 6) % 7
  const book = async () => {
    if (saving || selectedDate <= new Date()) { Alert.alert('Choose a future date', 'Appointment requests must be scheduled in the future.'); return }
    setSaving(true)
    try {
      await api('/api/appointments', { method: 'POST', body: JSON.stringify({ topic, scheduledDate: selectedDate.toISOString(), notes }) })
      setNotes(''); Alert.alert('Request sent', 'Your consultation request is now in the sales team appointment queue.'); await load()
    } catch (e) { Alert.alert('Could not book appointment', e instanceof Error ? e.message : 'Please try again.') }
    finally { setSaving(false) }
  }

  return <ScrollView style={s.screen} contentContainerStyle={[s.content, { paddingTop: insets.top + 18, paddingBottom: insets.bottom + 30 }]}>
    <Text style={s.title}>Book a consultation</Text><Text style={s.subtitle}>Choose a topic and preferred date. A Verichat representative will confirm your request.</Text>
    <Text style={s.label}>What can we help with?</Text>
    <TouchableOpacity style={s.select} onPress={() => setTopicOpen(true)}><Text style={s.selectText}>{topic}</Text><Text style={s.selectArrow}>⌄</Text></TouchableOpacity>
    <Text style={s.label}>Preferred date and time</Text>
    <TouchableOpacity style={s.select} onPress={() => { setMonth(new Date(selectedDate.getFullYear(), selectedDate.getMonth(), 1)); setCalendarOpen(true) }}><Text style={s.selectText}>{selectedDate.toLocaleString([], { weekday: 'short', day: 'numeric', month: 'long', year: 'numeric', hour: '2-digit', minute: '2-digit' })}</Text><Text>▦</Text></TouchableOpacity>
    <Text style={s.label}>Request details</Text><TextInput value={notes} onChangeText={setNotes} placeholder="Products, equipment model, or other details" multiline maxLength={4000} style={s.notes} />
    <AnimatedButton style={s.button} disabled={saving} loading={saving} shimmer onPress={() => void book()}><Text style={s.buttonText}>Send appointment request</Text></AnimatedButton>
    <Text style={s.section}>Your appointment requests</Text>
    {loading ? <BrandLoader label="Loading appointment requests…" /> : appointments.length === 0 ? <Text style={s.empty}>No appointments requested yet.</Text> : appointments.map(a => <View key={a.id} style={s.appointment}><View style={s.row}><Text style={s.appointmentTopic}>{a.topic}</Text><Text style={[s.status, a.status.toLowerCase() === 'approved' && s.approved]}>{a.status}</Text></View><Text style={s.date}>{new Date(a.scheduledDate).toLocaleString()}</Text>{a.notes ? <Text style={s.detail}>{a.notes}</Text> : null}</View>)}
    <Modal visible={topicOpen} transparent animationType="slide" onRequestClose={() => setTopicOpen(false)}><View style={s.backdrop}><View style={s.sheet}><Text style={s.sheetTitle}>Select a topic</Text>{TOPICS.map(x => <TouchableOpacity key={x} style={s.option} onPress={() => { setTopic(x); setTopicOpen(false) }}><Text style={s.optionText}>{x}</Text></TouchableOpacity>)}</View></View></Modal>
    <Modal visible={calendarOpen} transparent animationType="slide" onRequestClose={() => setCalendarOpen(false)}><View style={s.backdrop}><View style={s.sheet}><View style={s.calendarHead}><TouchableOpacity onPress={() => setMonth(new Date(month.getFullYear(), month.getMonth() - 1, 1))}><Text style={s.monthArrow}>‹</Text></TouchableOpacity><Text style={s.sheetTitle}>{month.toLocaleString([], { month: 'long', year: 'numeric' })}</Text><TouchableOpacity onPress={() => setMonth(new Date(month.getFullYear(), month.getMonth() + 1, 1))}><Text style={s.monthArrow}>›</Text></TouchableOpacity></View><View style={s.calendarGrid}>{['M','T','W','T','F','S','S'].map((d,i)=><Text key={`${d}${i}`} style={s.weekday}>{d}</Text>)}{Array.from({ length: offset }, (_, i) => <View key={`blank${i}`} />)}{Array.from({ length: days }, (_, i) => { const date = new Date(month.getFullYear(), month.getMonth(), i + 1); const disabled = new Date(date.getFullYear(), date.getMonth(), date.getDate()) <= new Date(new Date().getFullYear(), new Date().getMonth(), new Date().getDate()); const active = date.toDateString() === selectedDate.toDateString(); return <TouchableOpacity key={i} disabled={disabled} style={[s.day, active && s.dayActive, disabled && s.dayDisabled]} onPress={() => { const next = new Date(date); next.setHours(selectedDate.getHours(), selectedDate.getMinutes(), 0, 0); setSelectedDate(next) }}><Text style={[s.dayText, active && s.dayTextActive]}>{i + 1}</Text></TouchableOpacity> })}</View><Text style={s.label}>Time</Text><View style={s.timeRow}><TextInput style={s.timeInput} value={String(selectedDate.getHours()).padStart(2,'0')} keyboardType="number-pad" maxLength={2} onChangeText={v => { const h = Math.min(23, Math.max(0, Number(v)||0)); setSelectedDate(d => { const n=new Date(d); n.setHours(h); return n }) }} /><Text style={s.colon}>:</Text><TextInput style={s.timeInput} value={String(selectedDate.getMinutes()).padStart(2,'0')} keyboardType="number-pad" maxLength={2} onChangeText={v => { const m = Math.min(59, Math.max(0, Number(v)||0)); setSelectedDate(d => { const n=new Date(d); n.setMinutes(m); return n }) }} /></View><TouchableOpacity style={s.button} onPress={() => setCalendarOpen(false)}><Text style={s.buttonText}>Use this date</Text></TouchableOpacity></View></View></Modal>
  </ScrollView>
}

const s = StyleSheet.create({ screen:{flex:1,backgroundColor:CRMTheme.background},content:{padding:20,gap:10},title:{fontSize:28,fontWeight:'900',color:CRMTheme.text},subtitle:{fontSize:14,lineHeight:21,color:'#52657a',marginBottom:12},label:{fontSize:13,fontWeight:'800',color:'#33485e',marginTop:8},select:{minHeight:50,borderWidth:1,borderColor:'#d3dce7',backgroundColor:'#fff',borderRadius:12,paddingHorizontal:14,flexDirection:'row',alignItems:'center',justifyContent:'space-between'},selectText:{color:'#1b344e',fontWeight:'600'},selectArrow:{fontSize:20,color:CRMTheme.primary},notes:{minHeight:100,textAlignVertical:'top',borderWidth:1,borderColor:'#d3dce7',borderRadius:12,backgroundColor:'#fff',padding:13,color:'#1b344e'},button:{minHeight:50,borderRadius:13,backgroundColor:CRMTheme.primary,alignItems:'center',justifyContent:'center',padding:12,marginTop:6},buttonText:{color:'#fff',fontWeight:'900'},section:{fontSize:20,fontWeight:'900',color:CRMTheme.text,marginTop:24},empty:{color:'#738194',padding:16},appointment:{backgroundColor:'#fff',borderRadius:14,padding:15,gap:6},row:{flexDirection:'row',justifyContent:'space-between',gap:10},appointmentTopic:{color:CRMTheme.text,fontWeight:'800',flex:1},status:{color:'#a46100',fontSize:12,fontWeight:'900',textTransform:'capitalize'},approved:{color:'#197044'},date:{color:'#53677b'},detail:{color:'#53677b'},backdrop:{flex:1,justifyContent:'flex-end',backgroundColor:'#0007'},sheet:{backgroundColor:'#fff',borderTopLeftRadius:24,borderTopRightRadius:24,padding:20,paddingBottom:36,gap:8},sheetTitle:{color:CRMTheme.text,fontSize:19,fontWeight:'900',textAlign:'center'},option:{padding:14,borderBottomWidth:1,borderBottomColor:'#edf1f5'},optionText:{color:'#1b344e',fontSize:16},calendarHead:{flexDirection:'row',justifyContent:'space-between',alignItems:'center',marginBottom:8},monthArrow:{fontSize:32,color:CRMTheme.primary,paddingHorizontal:12},calendarGrid:{flexDirection:'row',flexWrap:'wrap',justifyContent:'space-between'},weekday:{width:'14%',textAlign:'center',padding:7,color:'#66788c',fontWeight:'800'},day:{width:'14%',aspectRatio:1,alignItems:'center',justifyContent:'center',borderRadius:99},dayActive:{backgroundColor:CRMTheme.primary},dayDisabled:{opacity:.3},dayText:{color:'#203952'},dayTextActive:{color:'#fff',fontWeight:'900'},timeRow:{flexDirection:'row',alignItems:'center',justifyContent:'center',gap:8},timeInput:{width:70,height:48,borderWidth:1,borderColor:'#d3dce7',borderRadius:10,textAlign:'center',fontSize:20,color:CRMTheme.text},colon:{fontSize:22,color:CRMTheme.text}})


