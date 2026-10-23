import { MaterialCommunityIcons } from '@expo/vector-icons'
import { useCallback, useEffect, useState } from 'react'
import {
  ActivityIndicator,
  Alert,
  ComponentProps,
  FlatList,
  Modal,
  ScrollView,
  StyleSheet,
  Text,
  TextInput,
  TouchableOpacity,
  View,
} from 'react-native'
import { useSafeAreaInsets } from 'react-native-safe-area-context'
import { DiscoveredDevice, isBleSupported, ParsedReading, readMeasurement, scanHealthDevices } from '../constants/bleDevices'
import {
  Device,
  DeviceStatus,
  DeviceType,
  MetricType,
  fetchDevices,
  registerDevice,
  removeDevice,
  syncDevice,
} from '../constants/devicesServices'
import { logHealthMetric, LogHealthMetricResult } from '../constants/healthMetricsService'

type IconName = ComponentProps<typeof MaterialCommunityIcons>['name']
type BusyAction = 'pair' | 'scan' | 'read' | 'sync' | 'remove'

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

const METRIC_LABELS: Record<string, string> = {
  bloodPressure: 'Blood Pressure',
  heartRate: 'Heart Rate',
  bloodGlucose: 'Blood Glucose',
  temperature: 'Temperature',
  bodyWeight: 'Body Weight',
}

const PAIRING_TYPES: { type: DeviceType; label: string }[] = [
  { type: 'weightScale', label: 'Weight Scale' },
  { type: 'heartRate', label: 'Heart Rate' },
  { type: 'bloodPressure', label: 'Blood Pressure' },
  { type: 'thermometer', label: 'Thermometer' },
  { type: 'glucoseMeter', label: 'Glucose Meter' },
  { type: 'other', label: 'Other' },
]

type Props = {
  /** ClientId the readings should be attributed to. */
  clientId?: string
}

const DeviceSyncScreen = ({ clientId: clientIdProp }: Props) => {
  const insets = useSafeAreaInsets()
  const [devices, setDevices] = useState<Device[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [busy, setBusy] = useState<Record<string, BusyAction>>({})

  const [pairingOpen, setPairingOpen] = useState(false)
  const [pairingType, setPairingType] = useState<DeviceType>('weightScale')
  const [scanning, setScanning] = useState(false)
  const [discovered, setDiscovered] = useState<DiscoveredDevice[]>([])
  const [pairingDeviceId, setPairingDeviceId] = useState<string | null>(null)

  const [manualOpen, setManualOpen] = useState(false)
  const [manualDeviceId, setManualDeviceId] = useState<string | null>(null)
  const [manualValue, setManualValue] = useState('')
  const [manualType, setManualType] = useState<MetricType>('bodyWeight')

  const clientId = clientIdProp ?? ''

  const load = useCallback(async () => {
    try {
      const devices = await fetchDevices()
      setError(null)
      setDevices(devices)
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Could not load devices.')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    void load()
  }, [load])

  const onRefresh = async () => {
    await load()
  }

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

  const postHealthMetric = async (input: {
    deviceId: string
    metricType: MetricType
    value: string
    unit: string
    timestamp: string
  }): Promise<LogHealthMetricResult | null> => {
    if (!clientId) {
      throw new Error('No client is signed in, so the reading cannot be attributed to a client.')
    }
    const result = await logHealthMetric({
      clientId,
      deviceId: input.deviceId,
      metricType: input.metricType,
      value: input.value,
      unit: input.unit,
      timestamp: input.timestamp,
    })
    await load()
    return result
  }
const handleReadAndPost = (device: Device) =>
    run(device.id, 'read', async () => {
      if (!device.externalId) {
        Alert.alert(
          'No BLE device',
          'This device was not paired with a BLE peripheral id, so it cannot be read over Bluetooth.',
        )
        return
      }
      if (!isBleSupported()) {
        Alert.alert(
          'Bluetooth unavailable',
          'Reading a device needs the native iOS/Android build of the app.',
const handleManualPost = (device: Device) => {
    if (!manualValue.trim()) {
      Alert.alert('Enter a value', 'Please type the reading value before saving.')
      return
    }
    setBusy((current) => ({ ...current, [device.id]: 'read' }))
    try {
      await postHealthMetric({
        deviceId: device.id,
const handleSync = (device: Device) =>
    run(device.id, 'sync', async () => {
      let readings: ParsedReading[] = []
      if (device.externalId && isBleSupported()) {
        try {
          const reading = await readMeasurement(device.externalId, device.type)
          if (reading) readings = [reading]
        } catch {
          // Still record the sync attempt.
const handleRemove = (device: Device) =>
    Alert.alert('Remove device', `Remove ${device.name}?`, [
      { text: 'Cancel', style: 'cancel' },
      {
        text: 'Remove',
        style: 'destructive',
        onPress: () => {
          run(device.id, 'remove', async () => {
            await removeDevice(device.id)
const openPairing = () => {
    setDiscovered([])
    setPairingDeviceId(null)
    setPairingOpen(true)
  }
            setDevices((current) => current.filter((d) => d.id !== device.id))
          })
        },
      },
const scanForDevices = async () => {
    setScanning(true)
    try {
      setDiscovered(await scanHealthDevices())
    } catch (e) {
      Alert.alert('Cannot scan', e instanceof Error ? e.message : 'Please try again.')
    } finally {
      setScanning(false)
    }
const pairDevice = async (found: DiscoveredDevice) => {
    setPairingDeviceId(found.id)
    try {
      const device = await registerDevice(found.name, pairingType, found.id)
      setDevices((current) => [...current, device].sort((a, b) => a.name.localeCompare(b.name)))
      setPairingOpen(false)
    } catch (e) {
      Alert.alert('Pairing failed', e instanceof Error ? e.message : 'Please try again.')
    } finally {
const formatLastSync = (iso: string | null) => {
    if (!iso) return 'Never synced'
    const date = new Date(iso)
    if (isNaN(date.getTime())) return 'Unknown'
    const time = `${String(date.getHours()).padStart(2, '0')}:${String(date.getMinutes()).padStart(2, '0')}`
    const days = Math.round(
      (new Date().setHours(0, 0, 0, 0) - date.setHours(0, 0, 0, 0)) / 86400000,
    )
const statusFor = (device: Device) => STATUS_STYLES[device.status]
    if (days <= 0) return `Today, ${time}`
    if (days === 1) return `Yesterday, ${time}`
    if (days < 7) return `${days} days ago`
    return `${date.getDate()} ${date.toLocaleString('default', { month: 'short' })} ${date.getFullYear()}`
  }
      setPairingDeviceId(null)
    }
  }
  }
    ])
        }
      }
      replaceDevice(await syncDevice(device.id, readings))
    })
        metricType: manualType,
        value: manualValue.trim(),
        unit: '',
        timestamp: new Date().toISOString(),
      })
      setManualOpen(false)
      setManualValue('')
      await load()
    } finally {
      setBusy((current) => {
        const next = { ...current }
        delete next[device.id]
        return next
      })
    }
  }
        )
        return
      }
      let reading: ParsedReading | null = null
      try {
        reading = await readMeasurement(device.externalId, device.type)
      } catch (e) {
        Alert.alert(
          'Device read failed',
          e instanceof Error ? e.message : 'Could not read the device.',
        )
        return
      }
      if (!reading) {
        Alert.alert('No reading', 'The device did not return a measurement.')
        return
      }
      await postHealthMetric({
        deviceId: device.id,
        metricType: reading.type,
        value: reading.value,
        unit: reading.unit,
        timestamp: reading.recordedAt,
      })
      replaceDevice(await syncDevice(device.id, [reading]))
    })
