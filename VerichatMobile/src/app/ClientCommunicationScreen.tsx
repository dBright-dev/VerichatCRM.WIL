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