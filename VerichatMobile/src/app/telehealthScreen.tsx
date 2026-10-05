import { LinearGradient } from 'expo-linear-gradient'
import { useLocalSearchParams } from 'expo-router'
import { useState } from 'react'
import { Platform, StyleSheet, Text, TouchableOpacity, View } from 'react-native'
import { useSafeAreaInsets } from 'react-native-safe-area-context'
import AppointmentsTab from './AppointmentsTab'
import LabResultsTab from './LabResultsTab'
import PrescriptionsTab from './PrescriptionsTab'

type TabKey = 'appointments' | 'labResults' | 'ePrescription'

const TABS: { key: TabKey; label: string }[] = [
  { key: 'appointments', label: 'Appointments' },
  { key: 'labResults', label: 'Lab Results' },
  { key: 'ePrescription', label: 'E-Prescription' },
]

const SERIF = Platform.select({ ios: 'Georgia', android: 'serif', default: 'serif' })

const TelehealthScreen = () => {
  const insets = useSafeAreaInsets()
  const { tab } = useLocalSearchParams<{ tab?: string }>()
  const initialTab: TabKey = tab === 'ePrescription' || tab === 'labResults' ? tab : 'appointments'
  const [activeTab, setActiveTab] = useState<TabKey>(initialTab)

  return (
    <View style={styles.screen}>
      {/* Header */}
      <LinearGradient
        colors={['#1f5f8b', '#3d7bd9']}
        start={{ x: 0, y: 0 }}
        end={{ x: 1, y: 1 }}
        style={[styles.header, { paddingTop: insets.top + 16 }]}
      >
        <Text style={styles.title}>Telehealth</Text>

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
      {activeTab === 'appointments' && <AppointmentsTab />}
      {activeTab === 'labResults' && <LabResultsTab />}
      {activeTab === 'ePrescription' && <PrescriptionsTab />}
    </View>
  )
}

export default TelehealthScreen

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
    fontSize: 32,
    color: '#ffffff',
    fontStyle: 'italic',
    fontWeight: '900',
    fontFamily: SERIF,
    textAlign: 'center',
    marginBottom: 20,
    textShadowColor: 'rgba(255,255,255,0.45)',
    textShadowOffset: { width: 0, height: 0 },
    textShadowRadius: 8,
  },
  segmented: {
    flexDirection: 'row',
    backgroundColor: '#1c4fb8',
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
    fontSize: 12,
    fontWeight: 'bold',
    color: '#ffffff',
  },
  segment_text_selected: {
    color: '#000000',
  },
})
