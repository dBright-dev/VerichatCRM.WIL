import { MaterialCommunityIcons } from '@expo/vector-icons'; //insert animated icons in expo.icon
import { LinearGradient } from 'expo-linear-gradient';
import { useCallback, useEffect, useState } from 'react';
import {
  ActivityIndicator,
  Platform,
  RefreshControl,
  ScrollView,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { ClientDashboard, fetchClientDashboard } from '../constants/dashboardService';

const SERIF = Platform.select({ ios: 'Georgia', android: 'serif', default: 'serif' })

const getGreeting = () => {
  const hour = new Date().getHours()
  if (hour < 12) return 'Good morning!'
  if (hour < 18) return 'Good afternoon!'
  return 'Good evening!'
}

const getInitials = (name: string) =>
  name
    .trim()
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0].toUpperCase())
    .join('')

const formatDate = (iso: string) => {
  const date = new Date(iso)
  return isNaN(date.getTime()) ? iso : date.toLocaleDateString('en-GB')
}

const ClientHomeScreen = () => {
  const insets = useSafeAreaInsets()
  const [data, setData] = useState<ClientDashboard | null>(null)
  const [loading, setLoading] = useState(true)
  const [refreshing, setRefreshing] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    try {
      setError(null)
      setData(await fetchClientDashboard())
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Something went wrong.')
    }
  }, [])

  useEffect(() => {
    load().finally(() => setLoading(false))
  }, [load])

  const onRefresh = async () => {
    setRefreshing(true)
    await load()
    setRefreshing(false)
  }

  if (loading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator size="large" color="#004e7a" />
      </View>
    )
  }

  if (!data) {
    return (
      <View style={styles.center}>
        <Text style={styles.error_text}>{error ?? 'No data available.'}</Text>
        <TouchableOpacity style={styles.retry_button} onPress={onRefresh}>
          <Text style={styles.retry_text}>Try again</Text>
        </TouchableOpacity>
      </View>
    )
  }

  const { vitals, nextRecurringOrder } = data
  const trackingOrders = data.trackingOrders ?? []

  const heartRate = vitals?.heartRate != null ? `${vitals.heartRate} bpm` : '--'
  const temperature = vitals?.temperature != null ? `${vitals.temperature}°C` : '--'
  const bloodPressure =
    vitals?.systolic != null && vitals?.diastolic != null
      ? `${vitals.systolic}/${vitals.diastolic} mmHg`
      : '--'

  return (
    <ScrollView
      style={styles.screen}
      contentContainerStyle={styles.content}
      showsVerticalScrollIndicator={false}
      refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} />}
    >
      {/* Header */}
      <LinearGradient
        colors={['#1f5f8b', '#3d7bd9']}
        start={{ x: 0, y: 0 }}
        end={{ x: 1, y: 1 }}
        style={[styles.header, { paddingTop: insets.top + 20 }]}
      >
        <View style={styles.header_top}>
          <View style={styles.header_text}>
            <Text style={styles.greeting}>{getGreeting()}</Text>
            <Text style={styles.name} numberOfLines={2}>
              {data.fullName}
            </Text>
          </View>
          <View style={styles.avatar}>
            <Text style={styles.avatar_text}>{getInitials(data.fullName)}</Text>
          </View>
        </View>

        <View style={styles.orders_panel}>
          <Text style={styles.orders_title}>Your Orders</Text>
          <View style={styles.orders_row}>
            <View style={styles.order_stat}>
              <Text style={styles.order_number}>{data.activeCount ?? 0}</Text>
              <Text style={styles.order_label}>Active</Text>
            </View>
            <View style={styles.order_stat}>
              <Text style={styles.order_number}>{data.pendingCount ?? 0}</Text>
              <Text style={styles.order_label}>Pending</Text>
            </View>
            <View style={styles.order_stat}>
              <Text style={styles.order_number}>{data.recurringCount ?? 0}</Text>
              <Text style={styles.order_label}>Recurring</Text>
            </View>
          </View>
        </View>
      </LinearGradient>

      {/* Quick Vitals */}
      <View style={styles.vitals_card}>
        <Text style={styles.vitals_title}>Quick Vitals Profile</Text>

        <View style={styles.vitals_row}>
          <View style={styles.vital}>
            <MaterialCommunityIcons name="heart-pulse" size={40} color="#d32f2f" />
            <View>
              <Text style={styles.vital_label}>Heart Rate</Text>
              <Text style={styles.vital_value}>{heartRate}</Text>
            </View>
          </View>

          <View style={styles.vital}>
            <MaterialCommunityIcons name="gauge" size={40} color="#546e7a" />
            <View>
              <Text style={styles.vital_label}>Blood Pressure</Text>
              <Text style={styles.vital_value}>{bloodPressure}</Text>
            </View>
          </View>
        </View>

        <View style={styles.vitals_row}>
          <View style={styles.vital}>
            <MaterialCommunityIcons name="thermometer" size={40} color="#d32f2f" />
            <View>
              <Text style={styles.vital_label}>Temperature</Text>
              <Text style={styles.vital_value}>{temperature}</Text>
            </View>
          </View>
        </View>

        <TouchableOpacity
          activeOpacity={0.85}
          onPress={() => {
            // TODO: navigate to the vitals report screen
          }}
        >
          <LinearGradient
            colors={['#004e7a', '#2f7be0']}
            start={{ x: 0, y: 0 }}
            end={{ x: 1, y: 0 }}
            style={styles.report_button}
          >
            <Text style={styles.report_button_text}>View Vitals Report</Text>
            <MaterialCommunityIcons name="arrow-right" size={26} color="#ffffff" />
          </LinearGradient>
        </TouchableOpacity>
      </View>

      {/* Bottom boxes */}
      <View style={styles.boxes_row}>
        <View style={[styles.box, styles.recurring_box]}>
          <MaterialCommunityIcons name="autorenew" size={44} color="#2196f3" />
          <Text style={styles.box_title}>Recurring order</Text>
          {nextRecurringOrder ? (
            <Text style={styles.box_text}>
              {nextRecurringOrder.productName} due {formatDate(nextRecurringOrder.dueDate)}
            </Text>
          ) : (
            <Text style={styles.box_empty}>No recurring orders</Text>
          )}
        </View>

        <View style={[styles.box, styles.tracking_box]}>
          <MaterialCommunityIcons name="truck-delivery" size={44} color="#d32f2f" />
          <Text style={styles.box_title}>Tracking order/s</Text>
          {trackingOrders.length > 0 ? (
            <>
              <Text style={styles.box_text}>
                Order {trackingOrders[0].reference} is {trackingOrders[0].status.toLowerCase()}
              </Text>
              {trackingOrders.length > 1 && (
                <Text style={styles.box_more}>+{trackingOrders.length - 1} more</Text>
              )}
            </>
          ) : (
            <Text style={styles.box_empty}>No orders</Text>
          )}
        </View>
      </View>
    </ScrollView>
  )
}

export default ClientHomeScreen

const styles = StyleSheet.create({
  screen: {
    flex: 1,
    backgroundColor: '#ffffff',
  },
  content: {
    paddingBottom: 30,
  },
  center: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: '#ffffff',
    padding: 24,
    gap: 16,
  },
  error_text: {
    fontSize: 16,
    color: '#333333',
    textAlign: 'center',
  },
  retry_button: {
    backgroundColor: '#0057a8',
    paddingVertical: 12,
    paddingHorizontal: 30,
    borderRadius: 20,
  },
  retry_text: {
    color: '#ffffff',
    fontWeight: 'bold',
    fontSize: 16,
  },

  // Header
  header: {
    paddingHorizontal: 20,
    paddingBottom: 24,
    borderBottomLeftRadius: 30,
    borderBottomRightRadius: 30,
  },
  header_top: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'flex-start',
    marginBottom: 24,
  },
  header_text: {
    flex: 1,
    paddingRight: 12,
  },
  greeting: {
    fontSize: 18,
    color: '#ffffff',
    fontWeight: '700',
  },
  name: {
    fontSize: 32,
    color: '#ffffff',
    fontStyle: 'italic',
    fontWeight: '700',
    fontFamily: SERIF,
    marginTop: 4,
    textShadowColor: 'rgba(255,255,255,0.45)',
    textShadowOffset: { width: 0, height: 0 },
    textShadowRadius: 8,
  },
  avatar: {
    width: 52,
    height: 52,
    borderRadius: 26,
    backgroundColor: '#004e9a',
    borderWidth: 2,
    borderColor: '#0a2f55',
    justifyContent: 'center',
    alignItems: 'center',
  },
  avatar_text: {
    color: '#ffffff',
    fontWeight: 'bold',
    fontSize: 18,
  },
  orders_panel: {
    backgroundColor: '#0057a8',
    borderWidth: 2,
    borderColor: '#7db3ff',
    borderRadius: 18,
    paddingVertical: 12,
    paddingHorizontal: 10,
  },
  orders_title: {
    color: '#ffffff',
    fontSize: 18,
    fontStyle: 'italic',
    fontWeight: '900',
    fontFamily: SERIF,
    textAlign: 'center',
    marginBottom: 8,
  },
  orders_row: {
    flexDirection: 'row',
    justifyContent: 'space-around',
  },
  order_stat: {
    alignItems: 'center',
    flex: 1,
  },
  order_number: {
    color: '#ffffff',
    fontSize: 26,
    fontWeight: 'bold',
  },
  order_label: {
    color: '#ffffff',
    fontSize: 15,
    fontWeight: 'bold',
  },

  // Vitals
  vitals_card: {
    backgroundColor: '#efefef',
    borderRadius: 25,
    marginHorizontal: 16,
    marginTop: 18,
    padding: 18,
    gap: 16,
  },
  vitals_title: {
    fontSize: 18,
    fontStyle: 'italic',
    fontWeight: '900',
    fontFamily: SERIF,
    textAlign: 'center',
    color: '#000000',
  },
  vitals_row: {
    flexDirection: 'row',
    justifyContent: 'space-between',
  },
  vital: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
    flex: 1,
  },
  vital_label: {
    fontSize: 12,
    fontWeight: 'bold',
    color: '#000000',
  },
  vital_value: {
    fontSize: 14,
    fontWeight: '700',
    fontStyle: 'italic',
    color: '#000000',
  },
  report_button: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 12,
    alignSelf: 'flex-start',
    paddingVertical: 14,
    paddingHorizontal: 22,
    borderRadius: 16,
  },
  report_button_text: {
    color: '#ffffff',
    fontWeight: 'bold',
    fontSize: 14,
  },

  // Bottom boxes
  boxes_row: {
    flexDirection: 'row',
    gap: 14,
    marginHorizontal: 16,
    marginTop: 18,
  },
  box: {
    flex: 1,
    minHeight: 170,
    borderRadius: 20,
    padding: 14,
    alignItems: 'center',
    justifyContent: 'center',
    gap: 8,
  },
  recurring_box: {
    backgroundColor: '#cfe2ff',
  },
  tracking_box: {
    backgroundColor: '#ffe0e0',
  },
  box_title: {
    fontSize: 13,
    fontWeight: '900',
    color: '#000000',
    textAlign: 'center',
  },
  box_text: {
    fontSize: 13,
    fontWeight: 'bold',
    color: '#000000',
    textAlign: 'center',
  },
  box_empty: {
    fontSize: 13,
    fontWeight: '600',
    color: '#555555',
    textAlign: 'center',
  },
  box_more: {
    fontSize: 12,
    fontWeight: '600',
    color: '#555555',
  },
})