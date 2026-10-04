import { MaterialCommunityIcons } from '@expo/vector-icons'
import { ComponentProps } from 'react'
import { ActivityIndicator, FlatList, StyleSheet, Text, TouchableOpacity, View } from 'react-native'
import { HealthMetric, MetricType } from '../constants/wellnessService'

/* ------------------------------ Constants ------------------------------- */

type IconName = ComponentProps<typeof MaterialCommunityIcons>['name']

const METRIC_ICONS: Record<MetricType, { name: IconName; color: string }> = {
  bloodPressure: { name: 'gauge', color: '#546e7a' },
  heartRate: { name: 'heart-pulse', color: '#d32f2f' },
  bloodGlucose: { name: 'diabetes', color: '#546e7a' },
  temperature: { name: 'thermometer', color: '#d32f2f' },
  bodyWeight: { name: 'scale-bathroom', color: '#64b5f6' },
}

const GOOD_COLOR = '#2e7d32'
const WARNING_COLOR = '#c62828'

/* -------------------------------- Component ----------------------------- */

type Props = {
  metrics: HealthMetric[]
  loading: boolean
  error: string | null
  refreshing: boolean
  onRefresh: () => void
}

const VitalsTab = ({ metrics, loading, error, refreshing, onRefresh }: Props) => {
  const renderMetric = ({ item }: { item: HealthMetric }) => {
    const icon = METRIC_ICONS[item.type]
    const statusColor = item.inRange ? GOOD_COLOR : WARNING_COLOR

    return (
      <View style={styles.card}>
        <View style={styles.icon_box}>
          <MaterialCommunityIcons name={icon.name} size={44} color={icon.color} />
        </View>

        <View style={styles.card_info}>
          <Text style={styles.metric_label}>{item.label}</Text>
          <Text style={styles.metric_value}>
            {item.value ?? '--'} <Text style={styles.metric_unit}>{item.unit}</Text>
          </Text>
        </View>

        <View style={[styles.status_pill, { borderColor: statusColor }]}>
          <Text style={[styles.status_text, { color: statusColor }]}>{item.statusLabel}</Text>
        </View>
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
        <MaterialCommunityIcons name="clipboard-pulse-outline" size={48} color="#9e9e9e" />
        <Text style={styles.empty_text}>No readings logged yet</Text>
      </View>
    )
  }

  return (
    <FlatList
      style={styles.flex}
      data={metrics}
      keyExtractor={(item) => item.id}
      renderItem={renderMetric}
      ListEmptyComponent={renderEmpty}
      contentContainerStyle={styles.list}
      showsVerticalScrollIndicator={false}
      refreshing={refreshing}
      onRefresh={onRefresh}
    />
  )
}

export default VitalsTab

/* -------------------------------- Styles -------------------------------- */

const styles = StyleSheet.create({
  flex: {
    flex: 1,
  },
  list: {
    paddingHorizontal: 20,
    paddingTop: 20,
    paddingBottom: 24,
    gap: 16,
    flexGrow: 1,
  },
  card: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: '#efefef',
    borderRadius: 14,
    paddingVertical: 14,
    paddingHorizontal: 12,
    gap: 12,
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
  metric_label: {
    fontSize: 12,
    fontWeight: 'bold',
    color: '#000000',
  },
  metric_value: {
    fontSize: 16,
    fontWeight: '900',
    color: '#000000',
  },
  metric_unit: {
    fontSize: 14,
    fontWeight: '600',
    fontStyle: 'italic',
    color: '#757575',
  },
  status_pill: {
    alignSelf: 'flex-start',
    backgroundColor: '#dcdcdc',
    borderWidth: 1,
    borderRadius: 10,
    paddingVertical: 2,
    paddingHorizontal: 16,
  },
  status_text: {
    fontSize: 12,
    fontWeight: 'bold',
  },
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