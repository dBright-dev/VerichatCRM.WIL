import { MaterialCommunityIcons } from '@expo/vector-icons'
import { LinearGradient } from 'expo-linear-gradient'
import { ComponentProps, useCallback, useEffect, useState } from 'react'
import {
    ActivityIndicator,
    Alert,
    FlatList,
    Modal,
    StyleSheet,
    Text,
    TouchableOpacity,
    View,
} from 'react-native'
import { DiscoveredDevice, isBleSupported, ParsedReading, readMeasurement, scanHealthDevices } from '../constants/bleDevices'
import {
    Device,
    DeviceStatus,
    DeviceType,
    fetchDevices,
    reconnectDevice,
    registerDevice,
    removeDevice,
    syncDevice,
} from '../constants/devicesServices'

/* ------------------------------ Constants ------------------------------- */

type IconName = ComponentProps<typeof MaterialCommunityIcons>['name']
type BusyAction = 'sync' | 'reconnect' | 'remove'

const DEVICE_ICONS: Record<DeviceType, { name: IconName; color: string }> = {
  weightScale: { name: 'scale-bathroom', color: '#64b5f6' },
  thermometer: { name: 'thermometer', color: '#d32f2f' },
  bloodPressure: { name: 'gauge', color: '#1565c0' },
  glucoseMeter: { name: 'diabetes', color: '#546e7a' },
  heartRate: { name: 'heart-pulse', color: '#d32f2f' },
  other: { name: 'devices', color: '#546e7a' },
}

const STATUS_STYLES: Record<DeviceStatus, { label: string; color: string }> = {
  connected: { label: 'Connected', color: '#2e7d32' },
  disconnected: { label: 'Disconnected', color: '#757575' },
}

const MONTHS = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec']

/* ------------------------------- Helpers -------------------------------- */

const startOfDay = (date: Date) =>
  new Date(date.getFullYear(), date.getMonth(), date.getDate()).getTime()

const formatLastSync = (iso: string | null) => {
  if (!iso) return 'Never synced'

  const date = new Date(iso)
  if (isNaN(date.getTime())) return 'Unknown'

  const time = `${String(date.getHours()).padStart(2, '0')}:${String(date.getMinutes()).padStart(2, '0')}`
  const days = Math.round((startOfDay(new Date()) - startOfDay(date)) / 86400000)

  if (days <= 0) return `Today, ${time}`
  if (days === 1) return `Yesterday, ${time}`
  if (days < 7) return `${days} days ago`
  return `${date.getDate()} ${MONTHS[date.getMonth()]} ${date.getFullYear()}`
}

/* -------------------------------- Component ----------------------------- */

type Props = {
  // Called after a successful sync/reconnect so the parent can refresh the health score.
  onSynced?: () => void
}

const DevicesTab = ({ onSynced }: Props) => {
  const [devices, setDevices] = useState<Device[]>([])
  const [loading, setLoading] = useState(true)
  const [refreshing, setRefreshing] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [busy, setBusy] = useState<Record<string, BusyAction>>({})

  // Pairing flow ("+ Add New Device")
  const [pairingOpen, setPairingOpen] = useState(false)
  const [pairingType, setPairingType] = useState<DeviceType>('heartRate')
  const [scanning, setScanning] = useState(false)
  const [discovered, setDiscovered] = useState<DiscoveredDevice[]>([])
  const [pairingDeviceId, setPairingDeviceId] = useState<string | null>(null)

  const load = useCallback(async () => {
    try {
      const devices = await fetchDevices()
      setError(null)
      setDevices(devices)
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

  useEffect(() => {
    const timer = setInterval(() => void load(), 30_000)
    return () => clearInterval(timer)
  }, [load])

  const onRefresh = async () => {
    setRefreshing(true)
    await load()
    setRefreshing(false)
  }

  // Runs a device action with a per-device busy state and a shared error alert.
  const run = async (id: string, action: BusyAction, task: () => Promise<void>) => {
    setBusy((current) => ({ ...current, [id]: action }))
    try {
      await task()
    } catch (e) {
      Alert.alert('Something went wrong', e instanceof Error ? e.message : 'Please try again.')
    } finally {
      setBusy((current) => {
        const next = { ...current }
        delete next[id]
        return next
      })
    }
  }

  const replaceDevice = (updated: Device) =>
    setDevices((current) => current.map((d) => (d.id === updated.id ? updated : d)))

  const handleSync = (device: Device) =>
    run(device.id, 'sync', async () => {
      let readings: ParsedReading[] = []
      if (device.externalId && isBleSupported()) {
        try {
          const reading = await readMeasurement(device.externalId, device.type)
          if (reading) readings = [reading]
        } catch (e) {
          // Still record the sync attempt; tell the user the read failed.
          Alert.alert('Device read failed', e instanceof Error ? e.message : 'Could not read the device.')
        }
      }
      replaceDevice(await syncDevice(device.id, readings))
      onSynced?.()
    })

  const handleReconnect = (device: Device) =>
    run(device.id, 'reconnect', async () => {
      replaceDevice(await reconnectDevice(device.id))
      onSynced?.()
    })

  const handleRemove = (device: Device) =>
    Alert.alert('Remove device', `Remove ${device.name}? You can add it again later.`, [
      { text: 'Cancel', style: 'cancel' },
      {
        text: 'Remove',
        style: 'destructive',
        onPress: () =>
          run(device.id, 'remove', async () => {
            await removeDevice(device.id)
            setDevices((current) => current.filter((d) => d.id !== device.id))
          }),
      },
    ])

  const PAIRING_TYPES: { type: DeviceType; label: string }[] = [
    { type: 'heartRate', label: 'Heart Rate' },
    { type: 'bloodPressure', label: 'Blood Pressure' },
    { type: 'thermometer', label: 'Thermometer' },
    { type: 'glucoseMeter', label: 'Glucose Meter' },
    { type: 'weightScale', label: 'Weight Scale' },
  ]

  const openPairing = () => {
    setDiscovered([])
    setPairingDeviceId(null)
    setPairingOpen(true)
  }

  const scanForDevices = async () => {
    setScanning(true)
    try {
      setDiscovered(await scanHealthDevices())
    } catch (e) {
      Alert.alert('Cannot scan', e instanceof Error ? e.message : 'Please try again.')
    } finally {
      setScanning(false)
    }
  }

  const pairDevice = async (found: DiscoveredDevice) => {
    setPairingDeviceId(found.id)
    try {
      const device = await registerDevice(found.name, pairingType, found.id)
      setDevices(current => [...current, device].sort((a, b) => a.name.localeCompare(b.name)))
      setPairingOpen(false)
      onSynced?.()
    } catch (e) {
      Alert.alert('Pairing failed', e instanceof Error ? e.message : 'Please try again.')
    } finally {
      setPairingDeviceId(null)
    }
  }

  const handleAddDevice = () => {
    if (!isBleSupported()) {
      Alert.alert(
        'Bluetooth unavailable',
        'Device pairing needs the native iOS/Android build of the app.',
      )
      return
    }
    openPairing()
  }

  const renderDevice = ({ item }: { item: Device }) => {
    const icon = DEVICE_ICONS[item.type] ?? DEVICE_ICONS.other
    const status = STATUS_STYLES[item.status]
    const action = busy[item.id]
    const isBusy = action !== undefined

    return (
      <View style={styles.card}>
        <View style={styles.card_top}>
          <View style={styles.icon_box}>
            <MaterialCommunityIcons name={icon.name} size={44} color={icon.color} />
          </View>

          <View style={styles.card_info}>
            <Text style={styles.device_name}>{item.name}</Text>
            <Text style={styles.last_sync}>Last Sync: {formatLastSync(item.lastSyncAt)}</Text>
          </View>

          <Text style={[styles.status_text, { color: status.color }]}>{status.label}</Text>
        </View>

        <View style={styles.divider} />

        {item.status === 'connected' ? (
          <View style={styles.actions_row}>
            <TouchableOpacity
              style={[styles.outline_button, isBusy && styles.disabled]}
              disabled={isBusy}
              onPress={() => handleSync(item)}
            >
              {action === 'sync' ? (
                <ActivityIndicator size="small" color="#000000" />
              ) : (
                <Text style={styles.outline_text}>Sync Now</Text>
              )}
            </TouchableOpacity>

            <TouchableOpacity
              style={[styles.outline_button, isBusy && styles.disabled]}
              disabled={isBusy}
              onPress={() => handleRemove(item)}
            >
              {action === 'remove' ? (
                <ActivityIndicator size="small" color="#c62828" />
              ) : (
                <Text style={[styles.outline_text, styles.remove_text]}>Remove</Text>
              )}
            </TouchableOpacity>
          </View>
        ) : (
          <View style={styles.actions_row_single}>
            <TouchableOpacity
              style={[styles.reconnect_button, isBusy && styles.disabled]}
              disabled={isBusy}
              onPress={() => handleReconnect(item)}
            >
              {action === 'reconnect' ? (
                <ActivityIndicator size="small" color="#ffffff" />
              ) : (
                <Text style={styles.reconnect_text}>Reconnect Device</Text>
              )}
            </TouchableOpacity>
          </View>
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
        <MaterialCommunityIcons name="devices" size={48} color="#9e9e9e" />
        <Text style={styles.empty_text}>No devices connected yet</Text>
      </View>
    )
  }

  const renderFooter = () =>
    loading || error ? null : (
      <TouchableOpacity activeOpacity={0.85} onPress={handleAddDevice} style={styles.add_wrapper}>
        <LinearGradient
          colors={['#004e7a', '#2f7be0']}
          start={{ x: 0, y: 0 }}
          end={{ x: 1, y: 0 }}
          style={styles.add_button}
        >
          <Text style={styles.add_text}>+ Add New Device</Text>
          <MaterialCommunityIcons name="arrow-right" size={26} color="#ffffff" />
        </LinearGradient>
      </TouchableOpacity>
    )

  return (
    <View style={styles.flex}>
      <Modal visible={pairingOpen} transparent animationType="fade" onRequestClose={() => setPairingOpen(false)}>
        <View style={styles.modal_backdrop}>
          <View style={styles.modal_card}>
            <Text style={styles.modal_title}>Pair a device</Text>

            <Text style={styles.modal_label}>Device type</Text>
            <View style={styles.type_row}>
              {PAIRING_TYPES.map(({ type, label }) => (
                <TouchableOpacity
                  key={type}
                  style={[styles.type_chip, pairingType === type && styles.type_chip_selected]}
                  onPress={() => setPairingType(type)}
                >
                  <Text style={[styles.type_chip_text, pairingType === type && styles.type_chip_text_selected]}>
                    {label}
                  </Text>
                </TouchableOpacity>
              ))}
            </View>

            <TouchableOpacity style={styles.scan_button} disabled={scanning} onPress={scanForDevices}>
              {scanning ? (
                <ActivityIndicator size="small" color="#ffffff" />
              ) : (
                <Text style={styles.scan_button_text}>Scan for devices</Text>
              )}
            </TouchableOpacity>

            {discovered.length > 0 && (
              <FlatList
                style={styles.discovered_list}
                data={discovered}
                keyExtractor={item => item.id}
                renderItem={({ item }) => (
                  <TouchableOpacity
                    style={styles.discovered_row}
                    disabled={pairingDeviceId !== null}
                    onPress={() => pairDevice(item)}
                  >
                    <MaterialCommunityIcons name="bluetooth" size={20} color="#004e7a" />
                    <View style={styles.discovered_info}>
                      <Text style={styles.discovered_name}>{item.name}</Text>
                      <Text style={styles.discovered_id}>{item.id}</Text>
                    </View>
                    {pairingDeviceId === item.id ? (
                      <ActivityIndicator size="small" color="#004e7a" />
                    ) : (
                      <Text style={styles.discovered_pair}>Pair</Text>
                    )}
                  </TouchableOpacity>
                )}
              />
            )}

            {scanning && <Text style={styles.scan_hint}>Looking for nearby health devices…</Text>}
            {!scanning && discovered.length === 0 && (
              <Text style={styles.scan_hint}>Tap scan, then select your device from the list.</Text>
            )}

            <TouchableOpacity style={styles.modal_cancel} onPress={() => setPairingOpen(false)}>
              <Text style={styles.modal_cancel_text}>Cancel</Text>
            </TouchableOpacity>
          </View>
        </View>
      </Modal>

      <FlatList
        style={styles.flex}
        data={devices}
        keyExtractor={(item) => item.id}
        renderItem={renderDevice}
        ListEmptyComponent={renderEmpty}
        ListFooterComponent={renderFooter}
        contentContainerStyle={styles.list}
        showsVerticalScrollIndicator={false}
        refreshing={refreshing}
        onRefresh={onRefresh}
      />
    </View>
  )
}

export default DevicesTab

/* -------------------------------- Styles -------------------------------- */

const styles = StyleSheet.create({
  flex: {
    flex: 1,
  },
  list: {
    paddingHorizontal: 16,
    paddingTop: 20,
    paddingBottom: 24,
    gap: 18,
    flexGrow: 1,
  },

  // Device card
  card: {
    backgroundColor: '#efefef',
    borderRadius: 14,
    borderWidth: 1,
    borderColor: '#dcdcdc',
    overflow: 'hidden',
  },
  card_top: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 12,
    paddingVertical: 14,
    paddingHorizontal: 14,
  },
  icon_box: {
    width: 56,
    height: 56,
    justifyContent: 'center',
    alignItems: 'center',
  },
  card_info: {
    flex: 1,
    gap: 6,
  },
  device_name: {
    fontSize: 12,
    fontWeight: '900',
    color: '#000000',
  },
  last_sync: {
    fontSize: 12,
    fontStyle: 'italic',
    color: '#424242',
  },
  status_text: {
    alignSelf: 'flex-start',
    fontSize: 12,
    fontWeight: 'bold',
  },
  divider: {
    height: 1,
    backgroundColor: '#d0d0d0',
  },
  actions_row: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    paddingVertical: 10,
    paddingHorizontal: 12,
    gap: 16,
  },
  actions_row_single: {
    alignItems: 'center',
    paddingVertical: 10,
    paddingHorizontal: 12,
  },
  outline_button: {
    flex: 1,
    minHeight: 30,
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: '#f5f5f5',
    borderWidth: 1,
    borderColor: '#757575',
    borderRadius: 14,
    paddingVertical: 4,
  },
  outline_text: {
    fontSize: 12,
    fontWeight: '900',
    color: '#000000',
  },
  remove_text: {
    color: '#c62828',
  },
  reconnect_button: {
    minWidth: 190,
    minHeight: 30,
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: '#0097b2',
    borderRadius: 14,
    paddingVertical: 6,
    paddingHorizontal: 28,
  },
  reconnect_text: {
    fontSize: 12,
    fontWeight: '900',
    color: '#ffffff',
  },
  disabled: {
    opacity: 0.5,
  },

  // Add device
  add_wrapper: {
    alignSelf: 'center',
    marginTop: 8,
  },
  add_button: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 12,
    borderRadius: 18,
    paddingVertical: 12,
    paddingHorizontal: 28,
  },
  add_text: {
    color: '#ffffff',
    fontWeight: '900',
    fontSize: 13,
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

  // Pairing modal
  modal_backdrop: {
    flex: 1,
    justifyContent: 'center',
    backgroundColor: 'rgba(0,0,0,0.45)',
    padding: 20,
  },
  modal_card: {
    backgroundColor: '#ffffff',
    borderRadius: 18,
    padding: 20,
    gap: 12,
    maxHeight: '80%',
  },
  modal_title: {
    fontSize: 20,
    fontWeight: '900',
    color: '#0a2f55',
  },
  modal_label: {
    fontSize: 12,
    fontWeight: '900',
    color: '#424242',
  },
  type_row: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 8,
  },
  type_chip: {
    borderRadius: 18,
    borderWidth: 1,
    borderColor: '#9e9e9e',
    paddingHorizontal: 12,
    paddingVertical: 8,
  },
  type_chip_selected: {
    backgroundColor: '#0057a8',
    borderColor: '#0057a8',
  },
  type_chip_text: {
    fontSize: 12,
    color: '#333333',
  },
  type_chip_text_selected: {
    color: '#ffffff',
    fontWeight: '700',
  },
  scan_button: {
    backgroundColor: '#0097b2',
    borderRadius: 14,
    minHeight: 42,
    alignItems: 'center',
    justifyContent: 'center',
  },
  scan_button_text: {
    color: '#ffffff',
    fontWeight: '900',
    fontSize: 13,
  },
  scan_hint: {
    fontSize: 12,
    fontStyle: 'italic',
    color: '#757575',
    textAlign: 'center',
  },
  discovered_list: {
    flexGrow: 0,
    maxHeight: 220,
  },
  discovered_row: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 12,
    paddingVertical: 10,
    paddingHorizontal: 8,
    borderRadius: 12,
  },
  discovered_info: {
    flex: 1,
    gap: 2,
  },
  discovered_name: {
    fontSize: 14,
    fontWeight: '700',
    color: '#000000',
  },
  discovered_id: {
    fontSize: 11,
    color: '#757575',
  },
  discovered_pair: {
    fontSize: 13,
    fontWeight: '900',
    color: '#0057a8',
  },
  modal_cancel: {
    alignItems: 'center',
    paddingVertical: 10,
  },
  modal_cancel_text: {
    color: '#555555',
    fontWeight: '700',
  },
})
