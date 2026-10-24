import React, { useEffect, useState } from "react"
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  ActivityIndicator,
  TouchableOpacity,
  RefreshControl,
} from "react-native"
import { useSafeAreaInsets } from "react-native-safe-area-context"
import { fetchGoals, fetchNotifications } from "./NotificationsService"
import SalesGoalCard from "./GoalsScreen"

type Delivery = {
  id: string
  trackingNumber: string
  clientName: string
  clientAddress: string
  status: string
  currentLocation: { lat: number; lng: number } | null
  destination: { lat: number; lng: number } | null
  lastUpdated: string
}

type Ticket = {
  id: string
  subject: string
  status: string
  priority: string
  updatedAt: string
}

const DashboardScreen = ({ clientId }: { clientId?: string }) => {
  const insets = useSafeAreaInsets()
  const [goals, setGoals] = useState<any[]>([])
  const [deliveries, setDeliveries] = useState<Delivery[]>([])
  const [tickets, setTickets] = useState<Ticket[]>([])
  const [loading, setLoading] = useState(true)
  const [refreshing, setRefreshing] = useState(false)

  useEffect(() => {
    loadData()
  }, [clientId])

  useEffect(() => {
    const handleNotification = (notification: any) => {
      if (notification.deliveryId && deliveries.some(d => d.id === notification.deliveryId)) {
        refreshData()
      }
      if (notification.ticketId && tickets.some(t => t.id === notification.ticketId)) {
        refreshData()
      }
    }

    window.addEventListener('verichat-notification', handleNotification)

    return () => {
      window.removeEventListener('verichat-notification', handleNotification)
    }
  }, [deliveries, tickets])

  const loadData = async () => {
    try {
      const [goalsData, notifications] = await Promise.all([
        fetchGoals().catch(() => ({ goals: [] })),
        fetchNotifications().catch(() => []),
      ])
      setGoals(goalsData.goals || [])
      setDeliveries(notifications.filter((n: any) => n.deliveryId).map((n: any) => ({
        id: n.deliveryId,
        trackingNumber: n.deliveryId,
        clientName: 'Client Name',
        clientAddress: 'Client Address',
        status: 'Scheduled',
        currentLocation: null,
        destination: null,
        lastUpdated: new Date().toISOString(),
      })))
      setTickets(notifications.filter((n: any) => n.ticketId).map((n: any) => ({
        id: n.ticketId,
        subject: n.title,
        status: n.type === 'warning' ? 'In Progress' : 'New',
        priority: 'Normal',
        updatedAt: n.timestamp,
      })))
      setLoading(false)
    } catch (e) {
      setLoading(false)
    }
  }

  const onRefresh = async () => {
    setRefreshing(true)
    await loadData()
    setRefreshing(false)
  }

  if (loading) {
    return (
      <View style={[styles.loading, { paddingTop: insets.top }]}>
        <ActivityIndicator size="large" color="#0057a8" />
      </View>
    )
  }

  return (
    <ScrollView
      style={[styles.container, { paddingTop: insets.top }]}
      contentContainerStyle={{ paddingBottom: insets.bottom + 20 }}
      refreshControl={
        <RefreshControl
          refreshing={refreshing}
          onRefresh={onRefresh}
          colors={['#0057a8']}
        />
      }
    >
      <Text style={styles.title}>Dashboard</Text>
      <Text style={styles.subtitle}>Real-time goals and tracking</Text>

      <SalesGoalCard />

      <View style={styles.card}>
        <Text style={styles.cardTitle}>📦 Delivery Status</Text>
        {deliveries.map(delivery => (
          <View key={delivery.id} style={styles.deliveryItem}>
            <Text style={styles.deliveryStatus}>
              {delivery.status} • {delivery.trackingNumber}
            </Text>
            <Text style={styles.deliveryClient}>{delivery.clientName}</Text>
            <Text style={styles.deliveryTime}>Updated: {new Date(delivery.lastUpdated).toLocaleTimeString()}</Text>
          </View>
        ))}
      </View>

      <View style={styles.card}>
        <Text style={styles.cardTitle}>🎫 Support Tickets</Text>
        {tickets.map(ticket => (
          <View key={ticket.id} style={styles.ticketItem}>
            <Text style={styles.ticketStatus}>{ticket.status}</Text>
            <Text style={styles.ticketSubject}>{ticket.subject}</Text>
            <Text style={styles.ticketTime}>Updated: {new Date(ticket.updatedAt).toLocaleTimeString()}</Text>
          </View>
        ))}
      </View>
    </ScrollView>
  )
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: "#f5f7fa",
  },
  loading: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
  },
  title: {
    fontSize: 24,
    fontWeight: "900",
    color: "#0a2f55",
    marginBottom: 8,
  },
  subtitle: {
    fontSize: 14,
    color: "#666",
    marginBottom: 20,
  },
  card: {
    backgroundColor: "#ffffff",
    borderRadius: 12,
    padding: 16,
    marginBottom: 16,
  },
  cardTitle: {
    fontSize: 18,
    fontWeight: "700",
    color: "#0a2f55",
    marginBottom: 12,
  },
  deliveryItem: {
    padding: 12,
    backgroundColor: "#f8f9fa",
    borderRadius: 8,
    marginBottom: 8,
  },
  deliveryStatus: {
    fontSize: 16,
    fontWeight: "700",
    color: "#0057a8",
  },
  deliveryClient: {
    fontSize: 14,
    color: "#333",
    marginTop: 4,
  },
  deliveryTime: {
    fontSize: 12,
    color: "#888",
  },
  ticketItem: {
    padding: 12,
    backgroundColor: "#e8f4fd",
    borderRadius: 8,
    marginBottom: 8,
  },
  ticketStatus: {
    fontSize: 12,
    color: "#ffffff",
    backgroundColor: "#0057a8",
    paddingHorizontal: 8,
    paddingVertical: 4,
    borderRadius: 12,
    marginBottom: 4,
  },
  ticketSubject: {
    fontSize: 14,
    fontWeight: "600",
    color: "#333",
  },
  ticketTime: {
    fontSize: 12,
    color: "#888",
  },
})

export default DashboardScreen