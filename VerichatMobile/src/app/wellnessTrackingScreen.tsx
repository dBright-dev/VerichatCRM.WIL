import { LinearGradient } from 'expo-linear-gradient'
import { useCallback, useEffect, useState } from 'react'
import { Alert, Modal, Platform, StyleSheet, Text, TextInput, TouchableOpacity, View } from 'react-native'
import { useSafeAreaInsets } from 'react-native-safe-area-context'
import Svg, { Circle } from 'react-native-svg'
import DevicesTab from '../app/DevicesTab'
import GoalsTab from '../app/GoalsTab'
import VitalsTab from '../app/VitalsTab'
import { fetchWellnessSummary, logVitalReading, MetricType, WellnessSummary } from '../constants/wellnessService'

/* ------------------------------ Constants ------------------------------- */

type TabKey = 'vitals' | 'devices' | 'goals'

const TABS: { key: TabKey; label: string }[] = [
  { key: 'vitals', label: 'Vitals' },
  { key: 'devices', label: 'Devices' },
  { key: 'goals', label: 'Goals' },
]
const METRIC_TYPES: { key: MetricType; label: string; unit: string }[] = [
  { key: 'bloodPressure', label: 'Blood pressure', unit: 'mmHg' },
  { key: 'heartRate', label: 'Heart rate', unit: 'bpm' },
  { key: 'bloodGlucose', label: 'Blood glucose', unit: 'mmol/L' },
  { key: 'temperature', label: 'Temperature', unit: '°C' },
  { key: 'bodyWeight', label: 'Body weight', unit: 'kg' },
]

const GOOD_COLOR = '#2e7d32'
const WARNING_COLOR = '#c62828'

const SERIF = Platform.select({ ios: 'Georgia', android: 'serif', default: 'serif' })

const RING_SIZE = 64
const RING_STROKE = 7
const RING_RADIUS = (RING_SIZE - RING_STROKE) / 2
const RING_CIRCUMFERENCE = 2 * Math.PI * RING_RADIUS

/* ------------------------------- Helpers -------------------------------- */

const getScoreLabel = (score: number | null) => {
  if (score === null) return 'No score'
  if (score >= 80) return 'Excellent'
  if (score >= 60) return 'Good'
  if (score >= 40) return 'Fair'
  return 'Needs attention'
}

const getScoreColor = (score: number | null) => {
  if (score === null) return '#9e9e9e'
  if (score >= 80) return GOOD_COLOR
  if (score >= 60) return '#7cb342'
  if (score >= 40) return '#ef6c00'
  return WARNING_COLOR
}

/* ----------------------------- Score ring ------------------------------- */

const ScoreRing = ({ score }: { score: number | null }) => {
  const progress = score === null ? 0 : Math.min(Math.max(score, 0), 100) / 100

  return (
    <View style={styles.ring}>
      <Svg width={RING_SIZE} height={RING_SIZE}>
        <Circle
          cx={RING_SIZE / 2}
          cy={RING_SIZE / 2}
          r={RING_RADIUS}
          stroke="#0a2f55"
          strokeWidth={RING_STROKE}
          fill="none"
        />
        <Circle
          cx={RING_SIZE / 2}
          cy={RING_SIZE / 2}
          r={RING_RADIUS}
          stroke="#7cc443"
          strokeWidth={RING_STROKE}
          strokeLinecap="round"
          strokeDasharray={RING_CIRCUMFERENCE}
          strokeDashoffset={RING_CIRCUMFERENCE * (1 - progress)}
          fill="none"
          rotation="-90"
          origin={`${RING_SIZE / 2}, ${RING_SIZE / 2}`}
        />
      </Svg>
      <View style={styles.ring_center}>
        <Text style={styles.ring_text}>{score === null ? '--' : `${Math.round(score)}%`}</Text>
      </View>
    </View>
  )
}

/* -------------------------------- Screen -------------------------------- */

const WellnessTrackingScreen = () => {
  const insets = useSafeAreaInsets()
  const [activeTab, setActiveTab] = useState<TabKey>('vitals')
  const [summary, setSummary] = useState<WellnessSummary | null>(null)
  const [loading, setLoading] = useState(true)
  const [refreshing, setRefreshing] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [logOpen, setLogOpen] = useState(false)
  const [metricType, setMetricType] = useState<MetricType>('bloodPressure')
  const [reading, setReading] = useState('')
  const [saving, setSaving] = useState(false)

  const load = useCallback(async () => {
    try {
      const summary = await fetchWellnessSummary()
      setError(null)
      setSummary(summary)
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

  const saveReading = async () => {
    if (!reading.trim()) {
      Alert.alert('Reading required', 'Enter a value before saving.')
      return
    }
    setSaving(true)
    try {
      const metric = METRIC_TYPES.find((item) => item.key === metricType)!
      await logVitalReading({ type: metricType, value: reading.trim(), unit: metric.unit })
      setReading('')
      setLogOpen(false)
      await load()
    } catch (cause) {
      Alert.alert('Could not save reading', cause instanceof Error ? cause.message : 'Please try again.')
    } finally {
      setSaving(false)
    }
  }

  const metrics = summary?.metrics ?? []
  const healthScore = summary?.healthScore ?? null
  const inRangeCount = metrics.filter((metric) => metric.inRange).length
  const scoreLabel = getScoreLabel(healthScore)
  const scoreColor = getScoreColor(healthScore)

  const scoreSubtitle = () => {
    if (loading) return 'Loading your score...'
    if (error && !summary) return 'Could not load your score'
    if (metrics.length === 0) return 'Log a reading to see your score'
    return `${inRangeCount} out of ${metrics.length} metrics within normal range`
  }

  return (
    <View style={styles.screen}>
      {/* Header */}
      <LinearGradient
        colors={['#1f5f8b', '#3d7bd9']}
        start={{ x: 0, y: 0 }}
        end={{ x: 1, y: 1 }}
        style={[styles.header, { paddingTop: insets.top + 16 }]}
      >
        <Text style={styles.title}>Wellness Tracking</Text>

        <TouchableOpacity
          style={styles.log_button}
          onPress={() => setLogOpen(true)}
        >
          <Text style={styles.log_button_text}>+ Log New Reading</Text>
        </TouchableOpacity>

        {/* Health score */}
        <View style={styles.score_card}>
          <ScoreRing score={healthScore} />

          <View style={styles.score_info}>
            <View style={styles.score_row}>
              <Text style={styles.score_title}>Health Score</Text>
              <View style={[styles.score_pill, { borderColor: scoreColor }]}>
                <Text style={[styles.score_pill_text, { color: scoreColor }]}>{scoreLabel}</Text>
              </View>
            </View>
            <Text style={styles.score_subtitle}>{scoreSubtitle()}</Text>
          </View>
        </View>

        {/* Segmented tabs */}
        <View style={styles.segmented}>
          {TABS.map((tab) => {
            const selected = tab.key === activeTab
            return (
              <TouchableOpacity
                key={tab.key}
                style={[styles.segment, selected && styles.segment_selected]}
                onPress={() => setActiveTab(tab.key)}
              >
                <Text style={[styles.segment_text, selected && styles.segment_text_selected]}>
                  {tab.label}
                </Text>
              </TouchableOpacity>
            )
          })}
        </View>
      </LinearGradient>

      {/* Active tab */}
      {activeTab === 'vitals' && (
        <VitalsTab
          metrics={metrics}
          loading={loading}
          error={error}
          refreshing={refreshing}
          onRefresh={onRefresh}
        />
      )}
      {activeTab === 'devices' && <DevicesTab onSynced={load} />}
      {activeTab === 'goals' && <GoalsTab />}
      <Modal visible={logOpen} transparent animationType="slide" onRequestClose={() => setLogOpen(false)}>
        <View style={styles.modal_backdrop}>
          <View style={styles.log_modal}>
            <Text style={styles.log_modal_title}>Log a health reading</Text>
            <View style={styles.metric_choices}>
              {METRIC_TYPES.map((metric) => (
                <TouchableOpacity key={metric.key} onPress={() => setMetricType(metric.key)} style={[styles.metric_choice, metricType === metric.key && styles.metric_choice_selected]}>
                  <Text style={[styles.metric_choice_text, metricType === metric.key && styles.metric_choice_text_selected]}>{metric.label}</Text>
                </TouchableOpacity>
              ))}
            </View>
            <TextInput style={styles.reading_input} value={reading} onChangeText={setReading} placeholder={`Value (${METRIC_TYPES.find((item) => item.key === metricType)?.unit})`} keyboardType="decimal-pad" />
            <TouchableOpacity style={styles.save_reading} disabled={saving} onPress={() => void saveReading()}><Text style={styles.save_reading_text}>{saving ? 'Saving…' : 'Save to health record'}</Text></TouchableOpacity>
            <TouchableOpacity onPress={() => setLogOpen(false)}><Text style={styles.cancel_reading}>Cancel</Text></TouchableOpacity>
          </View>
        </View>
      </Modal>
    </View>
  )
}

export default WellnessTrackingScreen

/* -------------------------------- Styles -------------------------------- */

const styles = StyleSheet.create({
  screen: {
    flex: 1,
    backgroundColor: '#ffffff',
  },

  // Header
  header: {
    paddingHorizontal: 16,
    paddingBottom: 18,
    borderBottomLeftRadius: 40,
    borderBottomRightRadius: 40,
  },
  title: {
    fontSize: 30,
    color: '#ffffff',
    fontStyle: 'italic',
    fontWeight: '900',
    fontFamily: SERIF,
    textShadowColor: 'rgba(255,255,255,0.45)',
    textShadowOffset: { width: 0, height: 0 },
    textShadowRadius: 8,
  },
  log_button: {
    alignSelf: 'flex-end',
    backgroundColor: '#0b3d91',
    borderWidth: 1,
    borderColor: 'rgba(255,255,255,0.6)',
    borderRadius: 14,
    paddingVertical: 6,
    paddingHorizontal: 16,
    marginTop: 4,
    marginBottom: 10,
  },
  log_button_text: {
    color: '#ffffff',
    fontSize: 12,
    fontWeight: 'bold',
  },
  modal_backdrop: { flex: 1, justifyContent: 'center', backgroundColor: 'rgba(0,0,0,0.45)', padding: 20 },
  log_modal: { backgroundColor: '#ffffff', borderRadius: 18, padding: 20, gap: 14 },
  log_modal_title: { fontSize: 20, color: '#0a2f55', fontWeight: '900' },
  metric_choices: { flexDirection: 'row', flexWrap: 'wrap', gap: 8 },
  metric_choice: { borderRadius: 18, borderWidth: 1, borderColor: '#9e9e9e', paddingHorizontal: 10, paddingVertical: 8 },
  metric_choice_selected: { backgroundColor: '#0057a8', borderColor: '#0057a8' },
  metric_choice_text: { color: '#333333', fontSize: 12 },
  metric_choice_text_selected: { color: '#ffffff', fontWeight: '700' },
  reading_input: { borderWidth: 1, borderColor: '#9e9e9e', borderRadius: 10, paddingHorizontal: 12, height: 46, color: '#111111' },
  save_reading: { backgroundColor: '#0057a8', borderRadius: 12, alignItems: 'center', padding: 13 },
  save_reading_text: { color: '#ffffff', fontWeight: '800' },
  cancel_reading: { color: '#555555', textAlign: 'center', padding: 6 },

  // Health score
  score_card: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 14,
    backgroundColor: '#1c4fb8',
    borderWidth: 1,
    borderColor: 'rgba(255,255,255,0.45)',
    borderRadius: 16,
    padding: 12,
    marginBottom: 14,
  },
  ring: {
    width: RING_SIZE,
    height: RING_SIZE,
  },
  ring_center: {
    ...StyleSheet.absoluteFill,
    justifyContent: 'center',
    alignItems: 'center',
  },
  ring_text: {
    color: '#ffffff',
    fontSize: 15,
    fontWeight: 'bold',
  },
  score_info: {
    flex: 1,
    gap: 6,
  },
  score_row: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    gap: 8,
  },
  score_title: {
    color: '#ffffff',
    fontSize: 12,
    fontWeight: 'bold',
  },
  score_pill: {
    backgroundColor: '#dcdcdc',
    borderWidth: 1,
    borderRadius: 10,
    paddingVertical: 2,
    paddingHorizontal: 14,
  },
  score_pill_text: {
    fontSize: 12,
    fontWeight: 'bold',
  },
  score_subtitle: {
    color: '#ffffff',
    fontSize: 12,
    fontWeight: 'bold',
  },

  // Segmented tabs
  segmented: {
    flexDirection: 'row',
    backgroundColor: '#0b4a8f',
    borderWidth: 1,
    borderColor: 'rgba(255,255,255,0.45)',
    borderRadius: 16,
    padding: 6,
    gap: 6,
  },
  segment: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    paddingVertical: 12,
    borderRadius: 12,
  },
  segment_selected: {
    backgroundColor: '#f3ede6',
    borderWidth: 1,
    borderColor: '#cfcfcf',
  },
  segment_text: {
    fontSize: 14,
    fontWeight: 'bold',
    color: '#ffffff',
  },
  segment_text_selected: {
    color: '#000000',
  },
})
