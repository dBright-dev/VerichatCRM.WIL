import React, { useEffect, useState } from "react"
import { FlatList, Modal, ScrollView, StyleSheet, Text, TextInput, TouchableOpacity, View, ActivityIndicator, Alert } from "react-native"
import { useSafeAreaInsets } from "react-native-safe-area-context"

type SupportTicket = {
  id: string
  clientId: string
  status: string
  title: string
  description?: string
  createdAt: string
  updatedAt: string
}

type Inquiry = {
  id: string
  clientId: string
  status: string
  subject: string
  message: string
  createdAt: string
  updatedAt: string
}

type Appointment = {
  id: string
  clientId: string
  date: string
  time: string
  status: string
  notes?: string
}

type FollowUp = {
  id: string
  clientId: string
  nextFollowUpDate: string
  notes?: string
}

type TimelineItem = {
  id: string
  type: "ticket" | "inquiry" | "appointment" | "followup"
  title: string
  details: string
  date: string
  timestamp: string
}

type Props = {
  clientId: string
}

const ClientCommunicationScreen = ({ clientId }: Props) => {
  const insets = useSafeAreaInsets()
  const [tickets, setTickets] = useState<SupportTicket[]>([])
  const [inquiries, setInquiries] = useState<Inquiry[]>([])
  const [appointments, setAppointments] = useState<Appointment[]>([])
  const [followUps, setFollowUps] = useState<FollowUp[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    ;(async () => {
      try {
        const ticketsResp = await fetch(`/api/supportticket/client/${clientId}`, {
          method: "GET",
          headers: { "Content-Type": "application/json" },
        })
        const ticketsData = await ticketsResp.json()
        setTickets(ticketsData || [])

        const inquiriesResp = await fetch(`/api/inquiry/client/${clientId}`, {
          method: "GET",
          headers: { "Content-Type": "application/json" },
        })
        const inquiriesData = await inquiriesResp.json()
        setInquiries(inquiriesData || [])
      } catch (e) {
        setError(e instanceof Error ? e.message : "Failed to load data.")
      } finally {
        setLoading(false)
      }
    })()
  }, [clientId])

  const formatDate = (isoDate: string) => {
    const date = new Date(isoDate)
    return `${date.toLocaleDateString()} ${date.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })}`
  }

  const timelineItems = [
    ...tickets.map((t) => ({
      id: t.id,
      type: "ticket" as const,
      title: t.title,
      details: t.description || "",
      date: t.createdAt,
      timestamp: t.updatedAt,
    })),
    ...inquiries.map((i) => ({
      id: i.id,
      type: "inquiry" as const,
      title: i.subject,
      details: i.message,
      date: i.createdAt,
      timestamp: i.updatedAt,
    })),
  ].sort((a, b) => new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime())

  if (loading) {
    return <View><ActivityIndicator size="large" color="#0057a8" /></View>
  }

  if (error) {
    return <View>{error}</View>
  }

  return (
    <ScrollView style={{ paddingTop: insets.top, paddingBottom: insets.bottom + 24 }}>
      <View style={{ marginBottom: 24 }}>
        {timelineItems.length === 0 && <Text>No communication records yet</Text>}
        {timelineItems.map((item) => (
          <View key={item.id} style={{ borderLeftWidth: 4, borderLeftColor: "#0057a8", marginBottom: 12, paddingLeft: 8 }}>
            <Text style={{ fontWeight: "700", fontSize: 12 }}>{item.type}</Text>
            <Text style={{ fontSize: 12, color: "#888" }}>{formatDate(item.date)}</Text>
            <Text style={{ fontSize: 14 }}>{item.details}</Text>
          </View>
        ))}
      </View>
    </ScrollView>
  )
}

export default ClientCommunicationScreen
import React, { useEffect, useState } from "react"
import { Modal, ScrollView, Text, TextInput, TouchableOpacity, View, ActivityIndicator, Alert } from "react-native"
import { useSafeAreaInsets } from "react-native-safe-area-context"
import { styles } from "./ClientCommunicationScreenStyles"

const ClientCommunicationScreen = ({ clientId }: { clientId: string }) => {
  const insets = useSafeAreaInsets()
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [refreshing, setRefreshing] = useState(false)
  const [showFollowUpModal, setShowFollowUpModal] = useState(false)
  const [followUpText, setFollowUpText] = useState("")

  useEffect(() => { loadData() }, [clientId])
  const loadData = async () => {
    try {
      const [ticketsResp, inquiriesResp, appointmentsResp, followUpsResp, complaintsResp] = await Promise.all([
        fetch(`/api/supportticket/client/${clientId}`, { method: "GET", headers: { "Content-Type": "application/json" } }),
        fetch(`/api/inquiry/client/${clientId}`, { method: "GET", headers: { "Content-Type": "application/json" } }),
        fetch(`/api/appointments/client/${clientId}`, { method: "GET", headers: { "Content-Type": "application/json" } }),
        fetch(`/api/followup/client/${clientId}`, { method: "GET", headers: { "Content-Type": "application/json" } }),
        fetch(`/api/complaint/client/${clientId}`, { method: "GET", headers: { "Content-Type": "application/json" } }),
      ])
      const ticketsData = await ticketsResp.json()
      const inquiriesData = await inquiriesResp.json()
      const appointmentsData = await appointmentsResp.json()
      const followUpsData = await followUpsResp.json()
      const complaintsData = await complaintsResp.json()
      setTickets(ticketsData || [])
      setInquiries(inquiriesData || [])
      setAppointments(appointmentsData || [])
      setFollowUps(followUpsData || [])
      setComplaints(complaintsData || [])
      setError(null)
    } catch (e) { setError(e instanceof Error ? e.message : "Failed to load data.") } finally { setLoading(false) }
  }

  const onRefresh = async () => { setRefreshing(true); await loadData(); setRefreshing(false) }

  const handleLogFollowUp = async () => {
    if (!followUpText.trim()) { Alert.alert("Enter follow-up text", "Please type follow-up notes before saving."); return }
    try {
      const resp = await fetch(`/api/inquiry/client/${clientId}`, { method: "PUT", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ nextFollowUpDate: new Date().toISOString(), notes: followUpText }) })
      const result = await resp.json()
      if (resp.ok) { setShowFollowUpModal(false); setFollowUpText(""); await loadData(); Alert.alert("Follow-up logged", "Follow-up logged successfully.") }
      else Alert.alert("Error", result.message || "Failed to log follow-up.")
    } catch (e) { Alert.alert("Error", e instanceof Error ? e.message : "Failed to log follow-up.") }
  }

  if (loading) return <View style={styles.loading}><ActivityIndicator size="large" color="#0057a8" /></View>
  if (error) return <View style={styles.errorContainer}><Text style={styles.errorText}>{error}</Text><TouchableOpacity style={styles.retryButton} onPress={() => window.location.reload()}><Text style={styles.retryText}>Retry</Text></TouchableOpacity></View>

  return (
    <ScrollView style={[styles.container, { paddingTop: insets.top, paddingBottom: insets.bottom + 24 }]} contentContainerStyle={{ gap: 12 }} refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} colors={['#0057a8']} />}>
      {error && <View style={styles.errorBanner}><Text style={styles.errorText}>{error}</Text></View>}
      <View style={styles.header}><Text style={styles.headerTitle}>Client Communication</Text><Text style={styles.headerSubtitle}>Client ID: {clientId}</Text></View>
      {loading && <View style={styles.emptyState}><ActivityIndicator size="large" color="#0057a8" /></View>}
      <TouchableOpacity style={[styles.actionButton, styles.followUpButton]} onPress={() => setShowFollowUpModal(true)}><Text style={styles.actionButtonText}>📝 Log Follow-up</Text></TouchableOpacity>
      <Modal visible={showFollowUpModal} transparent animationType="fade" onRequestClose={() => setShowFollowUpModal(false)}>
        <View style={styles.modalBackground}><View style={styles.modalContent}><Text style={styles.modalTitle}>Log Follow-up</Text><Text style={styles.modalLabel}>Follow-up notes</Text>
          <TextInput style={styles.followUpInput} value={followUpText} onChangeText={setFollowUpText} multiline numberOfLines={3} placeholder="Enter follow-up notes..." />
          <View style={styles.modalActionButtons}><TouchableOpacity style={styles.modalSaveButton} onPress={handleLogFollowUp}><Text style={styles.modalSaveButtonText}>Save</Text></TouchableOpacity><TouchableOpacity style={styles.modalCancelButton2} onPress={() => setShowFollowUpModal(false)}><Text style={styles.modalCancelButtonText2}>Cancel</Text></TouchableOpacity></View>
        </View></View>
      </Modal>
    </ScrollView>
  )
}

export default ClientCommunicationScreen