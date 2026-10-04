import { MaterialCommunityIcons } from '@expo/vector-icons'
import { useCallback, useEffect, useState } from 'react'
import {
  ActivityIndicator,
  Alert,
  FlatList,
  Modal,
  Pressable,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from 'react-native'
import QRCode from 'react-native-qrcode-svg'
import { fetchPrescriptions, Prescription, PrescriptionStatus } from '../constants/prescriptionsService'

/* ------------------------------ Constants ------------------------------- */

const MONTHS = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec']

const INACTIVE_MESSAGES: Record<Exclude<PrescriptionStatus, 'active'>, string> = {
  dispensed: 'This prescription has already been dispensed.',
  expired: 'This prescription has expired. Please contact your doctor.',
}

const QR_COLOR = '#0a2f55'

/* ------------------------------- Helpers -------------------------------- */

const formatDate = (iso: string) => {
  const date = new Date(iso)
  if (isNaN(date.getTime())) return '--'
  return `${date.getDate()} ${MONTHS[date.getMonth()]} ${date.getFullYear()}`
}

/* ------------------------------ QR wrapper ------------------------------ */

// react-native-qrcode-svg throws on an empty value, so guard it.
const Qr = ({
  value,
  size,
  color = QR_COLOR,
  backgroundColor = 'transparent',
}: {
  value: string
  size: number
  color?: string
  backgroundColor?: string
}) =>
  value ? (
    <QRCode value={value} size={size} color={color} backgroundColor={backgroundColor} />
  ) : (
    <MaterialCommunityIcons name="qrcode-remove" size={size} color="#9e9e9e" />
  )

/* -------------------------------- Component ----------------------------- */

const PrescriptionsTab = () => {
  const [prescriptions, setPrescriptions] = useState<Prescription[]>([])
  const [loading, setLoading] = useState(true)
  const [refreshing, setRefreshing] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [selected, setSelected] = useState<Prescription | null>(null)

  const load = useCallback(async () => {
    try {
      const prescriptions = await fetchPrescriptions()
      setError(null)
      setPrescriptions(prescriptions)
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

  const openPrescription = (item: Prescription) => {
    if (item.status !== 'active') {
      Alert.alert('Not available', INACTIVE_MESSAGES[item.status])
      return
    }
    setSelected(item)
  }

  const renderPrescription = ({ item }: { item: Prescription }) => (
    <TouchableOpacity
      style={[styles.card, item.status !== 'active' && styles.card_inactive]}
      activeOpacity={0.8}
      onPress={() => openPrescription(item)}
    >
      <View style={styles.card_text}>
        <Text style={styles.reference} numberOfLines={1}>
          {item.reference}
        </Text>
        <Text style={styles.medication} numberOfLines={2}>
          {item.medication}
        </Text>
        <Text style={styles.instructions}>{item.instructions}</Text>
        <Text style={styles.prescriber} numberOfLines={1}>
          {item.prescriber} | {formatDate(item.issuedAt)}
        </Text>
      </View>

      <Qr value={item.qrPayload} size={58} />
    </TouchableOpacity>
  )

  const renderHeader = () => (
    <View style={styles.header_block}>
      <View style={styles.banner}>
        <MaterialCommunityIcons name="pill" size={42} color="#e53935" />
        <View style={styles.banner_text_block}>
          <Text style={styles.banner_title}>Digital Prescriptions</Text>
          <Text style={styles.banner_text}>
            QR-coded scripts sent from your doctor. Show this to your pharmacist
          </Text>
        </View>
      </View>

      <Text style={styles.section_title}>Your Prescriptions</Text>
    </View>
  )

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
        <MaterialCommunityIcons name="file-document-outline" size={48} color="#9e9e9e" />
        <Text style={styles.empty_text}>No e-prescriptions yet</Text>
      </View>
    )
  }

  return (
    <>
      <FlatList
        style={styles.flex}
        data={prescriptions}
        keyExtractor={(item) => item.id}
        renderItem={renderPrescription}
        ListHeaderComponent={renderHeader}
        ListEmptyComponent={renderEmpty}
        contentContainerStyle={styles.list}
        showsVerticalScrollIndicator={false}
        refreshing={refreshing}
        onRefresh={onRefresh}
      />

      {/* Full-size QR for the pharmacist to scan */}
      <Modal
        visible={selected !== null}
        transparent
        animationType="fade"
        onRequestClose={() => setSelected(null)}
      >
        <Pressable style={styles.overlay} onPress={() => setSelected(null)}>
          {/* Inner Pressable stops taps on the sheet from closing the modal */}
          <Pressable style={styles.sheet} onPress={() => {}}>
            {selected && (
              <>
                <Text style={styles.sheet_reference}>{selected.reference}</Text>
                <Text style={styles.sheet_medication}>{selected.medication}</Text>

                <View style={styles.sheet_qr}>
                  <Qr value={selected.qrPayload} size={230} color="#000000" backgroundColor="#ffffff" />
                </View>

                <Text style={styles.sheet_instructions}>{selected.instructions}</Text>
                <Text style={styles.sheet_prescriber}>
                  {selected.prescriber} | {formatDate(selected.issuedAt)}
                </Text>
                <Text style={styles.sheet_hint}>Show this code to your pharmacist</Text>

                <TouchableOpacity style={styles.close_button} onPress={() => setSelected(null)}>
                  <Text style={styles.close_text}>Close</Text>
                </TouchableOpacity>
              </>
            )}
          </Pressable>
        </Pressable>
      </Modal>
    </>
  )
}

export default PrescriptionsTab

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
  header_block: {
    gap: 16,
  },

  // Banner
  banner: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 12,
    backgroundColor: '#9dbbe6',
    borderWidth: 1,
    borderColor: '#7fa3d6',
    borderRadius: 12,
    paddingVertical: 12,
    paddingHorizontal: 14,
  },
  banner_text_block: {
    flex: 1,
    gap: 2,
  },
  banner_title: {
    fontSize: 11,
    fontWeight: '900',
    color: '#0b3d6b',
    textAlign: 'center',
  },
  banner_text: {
    fontSize: 11,
    fontWeight: 'bold',
    color: '#0b3d6b',
    textAlign: 'center',
  },
  section_title: {
    fontSize: 14,
    fontWeight: '900',
    color: '#000000',
    marginLeft: 8,
  },

  // Prescription card
  card: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    gap: 12,
    backgroundColor: '#efefef',
    borderRadius: 14,
    borderWidth: 1,
    borderColor: '#dcdcdc',
    paddingVertical: 16,
    paddingHorizontal: 14,
  },
  card_inactive: {
    opacity: 0.55,
  },
  card_text: {
    flex: 1,
    gap: 4,
  },
  reference: {
    fontSize: 12,
    fontStyle: 'italic',
    color: '#616161',
  },
  medication: {
    fontSize: 14,
    fontWeight: '900',
    color: '#000000',
  },
  instructions: {
    fontSize: 12,
    fontStyle: 'italic',
    color: '#000000',
  },
  prescriber: {
    fontSize: 12,
    color: '#616161',
    marginTop: 4,
  },

  // QR modal
  overlay: {
    flex: 1,
    backgroundColor: 'rgba(0,0,0,0.6)',
    justifyContent: 'center',
    alignItems: 'center',
    padding: 24,
  },
  sheet: {
    width: '100%',
    maxWidth: 360,
    backgroundColor: '#ffffff',
    borderRadius: 24,
    padding: 22,
    alignItems: 'center',
    gap: 8,
  },
  sheet_reference: {
    fontSize: 12,
    fontStyle: 'italic',
    color: '#616161',
  },
  sheet_medication: {
    fontSize: 18,
    fontWeight: '900',
    color: '#000000',
    textAlign: 'center',
  },
  sheet_qr: {
    marginVertical: 12,
    padding: 12,
    backgroundColor: '#ffffff',
    borderWidth: 1,
    borderColor: '#e0e0e0',
    borderRadius: 12,
  },
  sheet_instructions: {
    fontSize: 14,
    fontStyle: 'italic',
    color: '#000000',
    textAlign: 'center',
  },
  sheet_prescriber: {
    fontSize: 12,
    color: '#616161',
  },
  sheet_hint: {
    fontSize: 12,
    fontWeight: 'bold',
    color: '#0b3d6b',
    marginTop: 6,
  },
  close_button: {
    marginTop: 12,
    backgroundColor: '#0057a8',
    borderRadius: 18,
    paddingVertical: 12,
    paddingHorizontal: 40,
  },
  close_text: {
    color: '#ffffff',
    fontWeight: 'bold',
    fontSize: 15,
  },

  // Empty / error states
  empty: {
    alignItems: 'center',
    justifyContent: 'center',
    paddingTop: 40,
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
