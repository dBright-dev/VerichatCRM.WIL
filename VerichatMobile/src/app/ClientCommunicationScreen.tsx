import { CRMTheme } from '../constants/crmTheme'
import React, { useCallback, useEffect, useState } from "react"
import { Alert, ScrollView, Text, View, ActivityIndicator, TouchableOpacity } from "react-native"
import { useSafeAreaInsets } from "react-native-safe-area-context"
import * as Linking from "expo-linking"
import { api } from "../constants/api"

type ClientContact = {
  clientName: string
  contactPerson?: string | null
  contactEmail?: string | null
  contactPhone?: string | null
}

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

type Props = {
  clientId: string
}

const ClientCommunicationScreen = ({ clientId }: Props) => {
  const insets = useSafeAreaInsets()
  const [tickets, setTickets] = useState<SupportTicket[]>([])
  const [inquiries, setInquiries] = useState<Inquiry[]>([])
  const [client, setClient] = useState<ClientContact | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [timelineError, setTimelineError] = useState(false)

  const loadData = useCallback(async () => {
    setLoading(true)
    try {
      const [clientResult, ticketsResult, inquiriesResult] = await Promise.allSettled([
        api<ClientContact>(`/api/client/${encodeURIComponent(clientId)}`),
        api<SupportTicket[]>(`/api/supportticket/client/${encodeURIComponent(clientId)}`),
        api<Inquiry[]>(`/api/inquiry/client/${encodeURIComponent(clientId)}`),
      ])
      if (clientResult.status === "rejected") throw clientResult.reason
      setClient(clientResult.value)
      setTickets(ticketsResult.status === "fulfilled" ? ticketsResult.value || [] : [])
      setInquiries(inquiriesResult.status === "fulfilled" ? inquiriesResult.value || [] : [])
      setTimelineError(ticketsResult.status === "rejected" && inquiriesResult.status === "rejected")
      setError(null)
    } catch (e) {
      setError(e instanceof Error ? e.message : "Failed to load data.")
    } finally {
      setLoading(false)
    }
  }, [clientId])

  useEffect(() => {
    // This effect starts an async load; state updates happen after the request settles.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    void loadData()
  }, [loadData])

  const formatDate = (isoDate: string) => {
    const date = new Date(isoDate)
    return `${date.toLocaleDateString()} ${date.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })}`
  }

  const openContactLink = async (url: string, service: string) => {
    try {
      await Linking.openURL(url)
    } catch {
      Alert.alert(`${service} unavailable`, `No app could open this ${service.toLowerCase()} link.`)
    }
  }

  const phone = client?.contactPhone?.trim() || ""
  const phoneForDial = phone.replace(/[^\d+]/g, "")
  const phoneForWhatsApp = phone.replace(/\D/g, "")
  const email = client?.contactEmail?.trim() || ""

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

  if (loading && !client) {
    return <View><ActivityIndicator size="large" color={CRMTheme.primary} /></View>
  }

  if (error && !client) {
    return <View style={{ padding: 24, paddingTop: insets.top + 24 }}><Text>{error}</Text><TouchableOpacity onPress={() => void loadData()}><Text style={{ color: CRMTheme.primary, marginTop: 12 }}>Try again</Text></TouchableOpacity></View>
  }

  return (
    <ScrollView contentContainerStyle={{ paddingTop: insets.top + 16, paddingBottom: insets.bottom + 24, paddingHorizontal: 16 }}>
      <View style={{ backgroundColor: "#fff", borderRadius: 12, padding: 16, marginBottom: 24, borderWidth: 1, borderColor: CRMTheme.border }}>
        <Text style={{ fontSize: 18, fontWeight: "700", color: CRMTheme.text }}>{client?.clientName || "Client contact"}</Text>
        {!!client?.contactPerson && <Text style={{ color: CRMTheme.muted, marginTop: 4 }}>{client.contactPerson}</Text>}

        <View style={{ marginTop: 16, gap: 12 }}>
          <View>
            <Text style={{ color: CRMTheme.muted, marginBottom: 8 }}>Phone</Text>
            {phone ? <Text style={{ color: CRMTheme.text, marginBottom: 8 }}>{phone}</Text> : <Text style={{ color: "#8993a4", marginBottom: 8 }}>No phone number on file</Text>}
            <View style={{ flexDirection: "row", gap: 8 }}>
              {!!phoneForDial && <TouchableOpacity accessibilityRole="button" accessibilityLabel={`Call ${phone}`} onPress={() => void openContactLink(`tel:${phoneForDial}`, "Phone")} style={actionStyle}><Text style={actionTextStyle}>Call</Text></TouchableOpacity>}
              {!!phoneForWhatsApp && <TouchableOpacity accessibilityRole="button" accessibilityLabel={`Message ${phone} on WhatsApp`} onPress={() => void openContactLink(`whatsapp://send?phone=${phoneForWhatsApp}`, "WhatsApp")} style={actionStyle}><Text style={actionTextStyle}>WhatsApp</Text></TouchableOpacity>}
            </View>
          </View>
          <View>
            <Text style={{ color: CRMTheme.muted, marginBottom: 8 }}>Email</Text>
            {email ? <Text style={{ color: CRMTheme.text, marginBottom: 8 }}>{email}</Text> : <Text style={{ color: "#8993a4", marginBottom: 8 }}>No email address on file</Text>}
            {!!email && <TouchableOpacity accessibilityRole="button" accessibilityLabel={`Email ${email}`} onPress={() => void openContactLink(`mailto:${encodeURIComponent(email)}`, "Email")} style={actionStyle}><Text style={actionTextStyle}>Email</Text></TouchableOpacity>}
          </View>
        </View>
      </View>
      <View style={{ marginBottom: 24 }}>
        {timelineError && <Text style={{ color: "#8a5a00", marginBottom: 12 }}>Communication history is temporarily unavailable.</Text>}
        {loading && <ActivityIndicator size="small" color={CRMTheme.primary} />}
        {timelineItems.length === 0 && <Text>No communication records yet</Text>}
        {timelineItems.map((item) => (
          <View key={item.id} style={{ borderLeftWidth: 4, borderLeftColor: CRMTheme.primary, marginBottom: 12, paddingLeft: 8 }}>
            <Text style={{ fontWeight: "700", fontSize: 12 }}>{item.type}</Text>
            <Text style={{ fontSize: 12, color: "#888" }}>{formatDate(item.date)}</Text>
            <Text style={{ fontSize: 14 }}>{item.details}</Text>
          </View>
        ))}
      </View>
    </ScrollView>
  )
}

const actionStyle = { alignSelf: "flex-start" as const, backgroundColor: CRMTheme.primary, borderRadius: 8, paddingVertical: 9, paddingHorizontal: 14 }
const actionTextStyle = { color: "#fff", fontSize: 14, fontWeight: "600" as const }

export default ClientCommunicationScreen


