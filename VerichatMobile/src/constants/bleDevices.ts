import { BleManager, Characteristic, Device as BlePlxDevice } from 'react-native-ble-plx'
import { PermissionsAndroid, Platform } from 'react-native'
import type { DeviceType, MetricType } from './devicesServices'

/* ------------------------------ Constants ------------------------------- */

// Standard Bluetooth SIG GATT services + measurement characteristics.
const GATT: Record<Exclude<DeviceType, 'other'>, { service: string; measurement: string }> = {
  heartRate: { service: '0000180d-0000-1000-8000-00805f9b34fb', measurement: '00002a37-0000-1000-8000-00805f9b34fb' },
  bloodPressure: { service: '00001810-0000-1000-8000-00805f9b34fb', measurement: '00002a35-0000-1000-8000-00805f9b34fb' },
  thermometer: { service: '00001809-0000-1000-8000-00805f9b34fb', measurement: '00002a1c-0000-1000-8000-00805f9b34fb' },
  glucoseMeter: { service: '00001808-0000-1000-8000-00805f9b34fb', measurement: '00002a18-0000-1000-8000-00805f9b34fb' },
  weightScale: { service: '0000181d-0000-1000-8000-00805f9b34fb', measurement: '00002a9d-0000-1000-8000-00805f9b34fb' },
}

const DEVICE_TYPE_TO_METRIC: Record<Exclude<DeviceType, 'other'>, MetricType> = {
  heartRate: 'heartRate',
  bloodPressure: 'bloodPressure',
  thermometer: 'temperature',
  glucoseMeter: 'bloodGlucose',
  weightScale: 'bodyWeight',
}

const METRIC_UNITS: Record<MetricType, string> = {
  heartRate: 'bpm',
  bloodPressure: 'mmHg',
  bloodGlucose: 'mmol/L',
  temperature: '°C',
  bodyWeight: 'kg',
}

const MONITOR_TIMEOUT_MS = 10_000

/* -------------------------------- Types --------------------------------- */

export type DiscoveredDevice = {
  id: string
  name: string
  rssi: number | null
}

export type ParsedReading = {
  type: MetricType
  value: string
  unit: string
  statusLabel: string
  inRange: boolean
  recordedAt: string
}

/* ------------------------------- Helpers -------------------------------- */

let manager: BleManager | null = null

function getManager(): BleManager {
  if (!manager) manager = new BleManager()
  return manager
}

/** BLE needs native modules — not available on web / Expo Go stubs. */
export function isBleSupported(): boolean {
  return Platform.OS === 'ios' || Platform.OS === 'android'
}

/** Android 12+ needs runtime BLUETOOTH_SCAN / CONNECT grants. */
export async function ensureBlePermissions(): Promise<boolean> {
  if (Platform.OS !== 'android') return true
  const sdk = Platform.Version as number
  if (sdk < 31) {
    const granted = await PermissionsAndroid.request(PermissionsAndroid.PERMISSIONS.ACCESS_FINE_LOCATION)
    return granted === PermissionsAndroid.RESULTS.GRANTED
  }
  const result = await PermissionsAndroid.requestMultiple([
    PermissionsAndroid.PERMISSIONS.BLUETOOTH_SCAN,
    PermissionsAndroid.PERMISSIONS.BLUETOOTH_CONNECT,
  ])
  return (
    result[PermissionsAndroid.PERMISSIONS.BLUETOOTH_SCAN] === PermissionsAndroid.RESULTS.GRANTED &&
    result[PermissionsAndroid.PERMISSIONS.BLUETOOTH_CONNECT] === PermissionsAndroid.RESULTS.GRANTED
  )
}

const B64_CHARS = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/'

/** Minimal base64 → bytes decoder (Hermes atob output can vary by platform). */
function base64ToBytes(b64: string): Uint8Array {
  const clean = b64.replace(/[^A-Za-z0-9+/]/g, '')
  const bytes: number[] = []
  let buffer = 0
  let bits = 0
  for (const ch of clean) {
    buffer = (buffer << 6) | B64_CHARS.indexOf(ch)
    bits += 6
    if (bits >= 8) {
      bits -= 8
      bytes.push((buffer >> bits) & 0xff)
    }
  }
  return new Uint8Array(bytes)
}

/** Bluetooth SIG 11073 FLOAT: 24-bit signed mantissa + 8-bit signed exponent. */
function readFloat11073(bytes: Uint8Array, offset: number): number {
  let mantissa = bytes[offset] | (bytes[offset + 1] << 8) | (bytes[offset + 2] << 16)
  if (mantissa & 0x800000) mantissa -= 0x1000000
  let exponent = bytes[offset + 3]
  if (exponent & 0x80) exponent -= 0x100
  return mantissa * 10 ** exponent
}

/** Bluetooth SIG 11073 SFLOAT: 12-bit signed mantissa + 4-bit signed exponent. */
function readSfloat(bytes: Uint8Array, offset: number): number {
  const raw = bytes[offset] | (bytes[offset + 1] << 8)
  let mantissa = raw & 0x0fff
  if (mantissa & 0x0800) mantissa -= 0x1000
  let exponent = (raw >> 12) & 0x0f
  if (exponent & 0x08) exponent -= 0x10
  return mantissa * 10 ** exponent
}

const round = (value: number, places: number) => Math.round(value * 10 ** places) / 10 ** places

/** Applies the same range labels the wellness screen shows for manual logs. */
function classify(type: MetricType, value: string): { statusLabel: string; inRange: boolean } {
  const [first, second] = value.split('/').map(Number)
  switch (type) {
    case 'heartRate':
      if (first < 60) return { statusLabel: 'Low', inRange: false }
      if (first > 200) return { statusLabel: 'Critical', inRange: false }
      if (first > 100) return { statusLabel: 'High', inRange: false }
      return { statusLabel: 'Normal', inRange: true }
    case 'bloodPressure':
      if (first < 120 && second < 80) return { statusLabel: 'Optimal', inRange: true }
      if (first < 130 && second < 85) return { statusLabel: 'Normal', inRange: true }
      if (first < 140 && second < 90) return { statusLabel: 'Elevated', inRange: true }
      return { statusLabel: 'High', inRange: false }
    case 'temperature':
      if (first < 36.1) return { statusLabel: 'Low', inRange: false }
      if (first > 37.2) return { statusLabel: 'High', inRange: false }
      return { statusLabel: 'Normal', inRange: true }
    case 'bloodGlucose':
      if (first < 3.9) return { statusLabel: 'Low', inRange: false }
      if (first > 7.8) return { statusLabel: 'High', inRange: false }
      return { statusLabel: 'Normal', inRange: true }
    case 'bodyWeight':
    default:
      return { statusLabel: 'Normal', inRange: true }
  }
}

/* ------------------------------ GATT parsers ----------------------------- */

function parseMeasurement(type: DeviceType, bytes: Uint8Array): ParsedReading | null {
  if (type === 'other' || bytes.length === 0) return null
  const metric = DEVICE_TYPE_TO_METRIC[type]
  let value: string

  switch (type) {
    case 'heartRate': {
      // flags bit0: 0 = UINT8 bpm, 1 = UINT16 bpm
      const hr = bytes[0] & 0x01 ? bytes[1] | (bytes[2] << 8) : bytes[1]
      value = String(hr)
      break
    }
    case 'bloodPressure': {
      // flags bit0: unit (0 = mmHg, 1 = kPa); then systolic, diastolic, MAP (UINT16 LE each)
      const toMmHg = (raw: number) => (bytes[0] & 0x01 ? round(raw * 7.50062, 0) : raw)
      const systolic = toMmHg(bytes[1] | (bytes[2] << 8))
      const diastolic = toMmHg(bytes[3] | (bytes[4] << 8))
      value = `${systolic}/${diastolic}`
      break
    }
    case 'thermometer': {
      // flags bit0: 0 = °C, 1 = °F; then IEEE-11073 FLOAT
      const celsius = readFloat11073(bytes, 1)
      const temp = bytes[0] & 0x01 ? (celsius - 32) * (5 / 9) : celsius
      value = String(round(temp, 1))
      break
    }
    case 'glucoseMeter': {
      if (!(bytes[0] & 0x01)) return null // no concentration field present
      // flags bit3: 0 = kg/L, 1 = mol/L; concentration SFLOAT starts after seq(2) + base time(6)
      const sfloat = readSfloat(bytes, 9)
      const mmol = bytes[0] & 0x08 ? sfloat * 1000 : (sfloat * 100000) / 18.0182
      value = String(round(mmol, 1))
      break
    }
    case 'weightScale': {
      // flags bit0: 0 = kg, 1 = lb; weight UINT16 LE in 0.005 steps
      const raw = (bytes[1] | (bytes[2] << 8)) * 0.005
      const kg = bytes[0] & 0x01 ? raw * 0.45359237 : raw
      value = String(round(kg, 1))
      break
    }
    default:
      return null
  }

  const { statusLabel, inRange } = classify(metric, value)
  return {
    type: metric,
    value,
    unit: METRIC_UNITS[metric],
    statusLabel,
    inRange,
    recordedAt: new Date().toISOString(),
  }
}

/* -------------------------------- Public -------------------------------- */

/** Scans for nearby BLE devices advertising a standard health service. */
export async function scanHealthDevices(timeoutMs = 8_000): Promise<DiscoveredDevice[]> {
  if (!isBleSupported()) throw new Error('Bluetooth device pairing needs the native mobile app.')
  const granted = await ensureBlePermissions()
  if (!granted) throw new Error('Bluetooth permission was denied. Enable it in system settings.')

  const ble = getManager()
  const state = await ble.state()
  if (state !== 'PoweredOn') throw new Error('Bluetooth is switched off. Turn it on and try again.')

  const found = new Map<string, DiscoveredDevice>()
  await ble.startDeviceScan(Object.values(GATT).map(g => g.service), null, (error, device) => {
    if (error || !device) return
    found.set(device.id, {
      id: device.id,
      name: device.name ?? device.localName ?? 'Health device',
      rssi: device.rssi ?? null,
    })
  })
  await new Promise(resolve => setTimeout(resolve, timeoutMs))
  await ble.stopDeviceScan()
  return [...found.values()]
}

/**
 * Connects to a registered physical device, reads its latest measurement
 * (notification with read fallback) and disconnects again.
 */
export async function readMeasurement(externalId: string, type: DeviceType): Promise<ParsedReading | null> {
  const gatt = GATT[type as Exclude<DeviceType, 'other'>]
  if (!gatt) return null

  const ble = getManager()
  let device: BlePlxDevice | null = null
  try {
    device = await ble.connectToDevice(externalId)
    device = await device.discoverAllServicesAndCharacteristics()

    let bytes: Uint8Array | null = null

    // Most health devices push via notify; read is the fallback.
    try {
      const characteristic: Characteristic = await new Promise((resolve, reject) => {
        const timer = setTimeout(() => {
          subscription.remove()
          reject(new Error('Timed out waiting for a device reading.'))
        }, MONITOR_TIMEOUT_MS)
        const subscription = device!.monitorCharacteristicForService(
          gatt.service,
          gatt.measurement,
          (error, characteristic) => {
            if (error) {
              clearTimeout(timer)
              reject(error)
              return
            }
            if (characteristic?.value) {
              clearTimeout(timer)
              subscription.remove()
              resolve(characteristic)
            }
          },
        )
      })
      if (characteristic.value) bytes = base64ToBytes(characteristic.value)
    } catch {
      const characteristic = await device.readCharacteristicForService(gatt.service, gatt.measurement)
      if (characteristic.value) bytes = base64ToBytes(characteristic.value)
    }

    return bytes ? parseMeasurement(type, bytes) : null
  } finally {
    if (device) await ble.cancelDeviceConnection(externalId).catch(() => {})
  }
}

