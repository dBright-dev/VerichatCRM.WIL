import { MaterialCommunityIcons } from '@expo/vector-icons'
import { LinearGradient } from 'expo-linear-gradient'
import { useCallback, useEffect, useState } from 'react'
import { ActivityIndicator, FlatList, StyleSheet, Text, TouchableOpacity, View } from 'react-native'
import { Appointment, AppointmentStatus, fetchAppointments } from '../constants/telehealthService'

/* ------------------------------ Constants ------------------------------- */

const STATUS_STYLES: Record<AppointmentStatus, { label: string; color: string }> = {
  confirmed: { label: 'Confirmed', color: '#2e7d32' },
  cancelled: { label: 'Cancelled', color: '#c62828' },
  pending: { label: 'Pending', color: '#ef6c00' },
  completed: { label: 'Completed', color: '#757575' },
}

const DAYS = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat']
const MONTHS = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec']

/* ------------------------------- Helpers -------------------------------- */

const getInitials = (name: string) => {
  const parts = name
    .replace(/^dr\.?\s+/i, '')
    .trim()
    .split(/\s+/)
    .filter(Boolean)
  if (parts.length === 0) return '?'
  const first = parts[0][0]
  const last = parts.length > 1 ? parts[parts.length - 1][0] : ''
  return (first + last).toUpperCase()
}

const formatDate = (iso: string) => {
  const date = new Date(iso)
  if (isNaN(date.getTime())) return '--'
  return `${DAYS[date.getDay()]} ${date.getDate()} ${MONTHS[date.getMonth()]} ${date.getFullYear()}`
}

const formatTime = (iso: string) => {
  const date = new Date(iso)
  if (isNaN(date.getTime())) return '--:--'
  const hours = String(date.getHours()).padStart(2, '0')
  const minutes = String(date.getMinutes()).padStart(2, '0')
  return `${hours}:${minutes}`
}

/* -------------------------------- Component ----------------------------- */

const AppointmentsTab = () => {
  const [appointments, setAppointments] = useState<Appointment[]>([])
  const [loading, setLoading] = useState(true)
  const [refreshing, setRefreshing] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    try {
      const appointments = await fetchAppointments()
      setError(null)
      setAppointments(appointments)
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Something went wrong.')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    // This effect starts an async fetch; its state updates run after the request settles.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    void load()
  }, [load])

  const onRefresh = async () => {
    setRefreshing(true)
    await load()
    setRefreshing(false)
  }

  const renderAppointment = ({ item }: { item: Appointment }) => {
    const status = STATUS_STYLES[item.status]

    return (
      <View style={styles.card}>
        <View style={styles.card_top}>
          <View style={styles.avatar}>
            <Text style={styles.avatar_text}>{getInitials(item.doctorName)}</Text>
          </View>

          <View style={styles.card_info}>
            <View style={styles.name_row}>
              <Text style={styles.doctor_name} numberOfLines={1}>
                {item.doctorName}
              </Text>
              <View style={[styles.status_pill, { borderColor: status.color }]}>
                <Text style={[styles.status_text, { color: status.color }]}>{status.label}</Text>
              </View>
            </View>

            <Text style={styles.specialty} numberOfLines={2}>
              {item.specialty}, {item.facility}
            </Text>

            <View style={styles.chips_row}>
              <View style={styles.chip}>
                <MaterialCommunityIcons name="calendar-month" size={14} color="#d32f2f" />
                <Text style={styles.chip_text}>{formatDate(item.startsAt)}</Text>
              </View>
              <View style={styles.chip}>
                <MaterialCommunityIcons name="alarm" size={14} color="#d32f2f" />
                <Text style={styles.chip_text}>{formatTime(item.startsAt)}</Text>
              </View>
            </View>
          </View>
        </View>

        {item.status === 'confirmed' && (
          <>
            <View style={styles.divider} />
            <View style={styles.actions_row}>
              <TouchableOpacity
                style={styles.reschedule_button}
                onPress={() => {
                  // TODO: open the reschedule flow for item.id
                }}
              >
                <Text style={styles.reschedule_text}>Reschedule</Text>
              </TouchableOpacity>

              <TouchableOpacity
                activeOpacity={0.85}
                onPress={() => {
                  // TODO: start the video call for item.id
                }}
              >
                <LinearGradient
                  colors={['#004e7a', '#2f7be0']}
                  start={{ x: 0, y: 0 }}
                  end={{ x: 1, y: 0 }}
                  style={styles.join_button}
                >
                  <MaterialCommunityIcons name="video-outline" size={18} color="#ffffff" />
                  <Text style={styles.join_text}>Join call</Text>
                </LinearGradient>
              </TouchableOpacity>
            </View>
          </>
        )}
      </View>
    )
  }

  const renderEmpty = () => {
    if (loading) {
      return (
        <View style={styles.empty}>
          <ActivityIndicator size="large" color="#004e7a" />
        </View>
      )
    }

    if (error) {
      return (
        <View style={styles.empty}>
          <Text style={styles.error_text}>{error}</Text>
          <TouchableOpacity style={styles.retry_button} onPress={onRefresh}>
            <Text style={styles.retry_text}>Try again</Text>
          </TouchableOpacity>
        </View>
      )
    }

    return (
      <View style={styles.empty}>
        <MaterialCommunityIcons name="calendar-blank-outline" size={48} color="#9e9e9e" />
        <Text style={styles.empty_text}>No appointments yet</Text>
      </View>
    )
  }

  return (
    <FlatList
      style={styles.flex}
      data={appointments}
      keyExtractor={(item) => item.id}
      renderItem={renderAppointment}
      ListHeaderComponent={<Text style={styles.section_title}>Upcoming and Recent</Text>}
      ListEmptyComponent={renderEmpty}
      contentContainerStyle={styles.list}
      showsVerticalScrollIndicator={false}
      refreshing={refreshing}
      onRefresh={onRefresh}
    />
  )
}

export default AppointmentsTab

/* -------------------------------- Styles -------------------------------- */

const styles = StyleSheet.create({
  flex: {
    flex: 1,
  },
  list: {
    paddingHorizontal: 16,
    paddingTop: 20,
    paddingBottom: 24,
    gap: 16,
    flexGrow: 1,
  },
  section_title: {
    fontSize: 20,
    fontWeight: '900',
    color: '#000000',
    marginLeft: 8,
    marginBottom: 4,
  },

  // Appointment card
  card: {
    backgroundColor: '#efefef',
    borderRadius: 14,
    borderWidth: 1,
    borderColor: '#dcdcdc',
    overflow: 'hidden',
  },
  card_top: {
    flexDirection: 'row',
    padding: 12,
    gap: 12,
  },
  avatar: {
    width: 52,
    height: 52,
    borderRadius: 8,
    backgroundColor: '#0097b2',
    justifyContent: 'center',
    alignItems: 'center',
  },
  avatar_text: {
    color: '#ffffff',
    fontSize: 18,
    fontWeight: 'bold',
  },
  card_info: {
    flex: 1,
    gap: 6,
  },
  name_row: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    gap: 8,
  },
  doctor_name: {
    flex: 1,
    fontSize: 14,
    fontWeight: '900',
    color: '#000000',
  },
  status_pill: {
    backgroundColor: '#dcdcdc',
    borderWidth: 1,
    borderRadius: 12,
    paddingVertical: 2,
    paddingHorizontal: 12,
  },
  status_text: {
    fontSize: 12,
    fontWeight: 'bold',
  },
  specialty: {
    fontSize: 11,
    fontStyle: 'italic',
    fontWeight: '600',
    color: '#757575',
  },
  chips_row: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    marginTop: 2,
  },
  chip: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
    backgroundColor: '#d6d6d6',
    borderRadius: 10,
    paddingVertical: 4,
    paddingHorizontal: 10,
  },
  chip_text: {
    fontSize: 11,
    fontWeight: 'bold',
    color: '#000000',
  },
  divider: {
    height: 1,
    backgroundColor: '#d0d0d0',
  },
  actions_row: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingHorizontal: 16,
    paddingVertical: 10,
  },
  reschedule_button: {
    backgroundColor: '#d0d0d0',
    borderRadius: 14,
    paddingVertical: 6,
    paddingHorizontal: 26,
  },
  reschedule_text: {
    fontSize: 12,
    fontWeight: 'bold',
    color: '#000000',
  },
  join_button: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
    borderRadius: 14,
    paddingVertical: 6,
    paddingHorizontal: 28,
  },
  join_text: {
    fontSize: 12,
    fontWeight: 'bold',
    color: '#ffffff',
  },

  // Empty / error states
  empty: {
    alignItems: 'center',
    justifyContent: 'center',
    paddingTop: 60,
    gap: 12,
  },
  empty_text: {
    fontSize: 16,
    fontWeight: '600',
    color: '#757575',
  },
  error_text: {
    fontSize: 16,
    color: '#333333',
    textAlign: 'center',
    paddingHorizontal: 24,
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
})
