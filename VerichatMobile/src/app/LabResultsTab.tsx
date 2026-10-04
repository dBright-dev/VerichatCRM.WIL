import { MaterialCommunityIcons } from '@expo/vector-icons'
import { useCallback, useEffect, useState } from 'react'
import {
    ActivityIndicator,
    Alert,
    FlatList,
    Linking,
    StyleSheet,
    Text,
    TouchableOpacity,
    View,
} from 'react-native'
import {
    fetchLabResults,
    LabResult,
    LabResultsData,
    LabResultStatus,
} from '../constants/labResultsService'

/* ------------------------------ Constants ------------------------------- */

const STATUS_STYLES: Record<LabResultStatus, { label: string; color: string }> = {
  ready: { label: 'Ready', color: '#2e7d32' },
  processing: { label: 'Processing', color: '#ef6c00' },
}

const MONTHS = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec']

/* ------------------------------- Helpers -------------------------------- */

const formatDate = (iso: string) => {
  const date = new Date(iso)
  if (isNaN(date.getTime())) return '--'
  const day = String(date.getDate()).padStart(2, '0')
  return `${day} ${MONTHS[date.getMonth()]} ${date.getFullYear()}`
}

const formatSize = (kb: number | null) => {
  if (kb === null) return ''
  return kb >= 1024 ? `${(kb / 1024).toFixed(1)}MB` : `${kb}KB`
}

const formatList = (items: string[]) => {
  if (items.length <= 1) return items.join('')
  return `${items.slice(0, -1).join(', ')} and ${items[items.length - 1]}`
}

/* -------------------------------- Component ----------------------------- */

const LabResultsTab = () => {
  const [data, setData] = useState<LabResultsData | null>(null)
  const [loading, setLoading] = useState(true)
  const [refreshing, setRefreshing] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    try {
      setError(null)
      setData(await fetchLabResults())
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

  const openResult = async (item: LabResult) => {
    if (item.status !== 'ready') {
      Alert.alert('Not ready yet', 'This result is still being processed.')
      return
    }

    if (!item.pdfUrl) {
      Alert.alert('PDF unavailable', 'The PDF for this result is not available yet.')
      return
    }

    try {
      // TODO: if the PDF needs the client's auth token, download it first
      // (e.g. expo-file-system) and share/open the local file instead.
      await Linking.openURL(item.pdfUrl)

      // Clear the "NEW" badge locally.
      // TODO: also tell the backend this result has been viewed.
      setData((current) =>
        current
          ? {
              ...current,
              results: current.results.map((r) => (r.id === item.id ? { ...r, isNew: false } : r)),
            }
          : current,
      )
    } catch {
      Alert.alert('Could not open', 'We could not open this PDF. Please try again.')
    }
  }

  const renderResult = ({ item }: { item: LabResult }) => {
    const status = STATUS_STYLES[item.status]

    return (
      <TouchableOpacity style={styles.card} activeOpacity={0.8} onPress={() => openResult(item)}>
        <MaterialCommunityIcons name="test-tube" size={46} color="#d32f2f" />

        <View style={styles.card_body}>
          <View style={styles.top_row}>
            <View style={styles.ref_group}>
              {item.isNew && (
                <View style={styles.new_badge}>
                  <Text style={styles.new_text}>NEW</Text>
                </View>
              )}
              <Text style={styles.reference} numberOfLines={1}>
                {item.reference}
              </Text>
            </View>

            <View style={[styles.status_pill, { borderColor: status.color }]}>
              <Text style={[styles.status_text, { color: status.color }]}>{status.label}</Text>
            </View>
          </View>

          <Text style={styles.test_name} numberOfLines={2}>
            {item.testName}
          </Text>

          <View style={styles.bottom_row}>
            <Text style={styles.lab_text} numberOfLines={1}>
              {item.laboratory} | {formatDate(item.reportedAt)}
            </Text>
            <Text style={styles.size_text}>{formatSize(item.fileSizeKb)}</Text>
          </View>
        </View>
      </TouchableOpacity>
    )
  }

  const renderHeader = () => (
    <View style={styles.header_block}>
      {data && (
        <View style={[styles.banner, !data.limsConnected && styles.banner_offline]}>
          <MaterialCommunityIcons
            name={data.limsConnected ? 'microscope' : 'cloud-off-outline'}
            size={40}
            color="#37474f"
          />
          <View style={styles.banner_text_block}>
            <Text style={styles.banner_title}>
              {data.limsConnected
                ? 'Encrypted LIMS Connection Active'
                : 'LIMS connection unavailable'}
            </Text>
            <Text style={styles.banner_text}>
              {data.limsConnected
                ? data.connectedLabs.length > 0
                  ? `Results pushed securely from ${formatList(data.connectedLabs)}.`
                  : 'Results are pushed securely from your laboratory partners.'
                : 'New results may be delayed. Pull down to refresh.'}
              {data.results.length > 0 ? ' Tap any result to view or download PDF.' : ''}
            </Text>
          </View>
        </View>
      )}

      <Text style={styles.section_title}>Your Results</Text>
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

    if (error && !data) {
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
        <MaterialCommunityIcons name="test-tube-empty" size={48} color="#9e9e9e" />
        <Text style={styles.empty_text}>No lab results yet</Text>
      </View>
    )
  }

  return (
    <FlatList
      style={styles.flex}
      data={data?.results ?? []}
      keyExtractor={(item) => item.id}
      renderItem={renderResult}
      ListHeaderComponent={renderHeader}
      ListEmptyComponent={renderEmpty}
      contentContainerStyle={styles.list}
      showsVerticalScrollIndicator={false}
      refreshing={refreshing}
      onRefresh={onRefresh}
    />
  )
}

export default LabResultsTab

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
  banner_offline: {
    backgroundColor: '#ffe0b2',
    borderColor: '#ffb74d',
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

  // Result card
  card: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 10,
    backgroundColor: '#efefef',
    borderRadius: 14,
    borderWidth: 1,
    borderColor: '#dcdcdc',
    paddingVertical: 16,
    paddingHorizontal: 14,
  },
  card_body: {
    flex: 1,
    gap: 6,
  },
  top_row: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    gap: 8,
  },
  ref_group: {
    flex: 1,
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
  },
  new_badge: {
    backgroundColor: '#0057a8',
    borderRadius: 10,
    paddingVertical: 2,
    paddingHorizontal: 12,
  },
  new_text: {
    color: '#ffffff',
    fontSize: 10,
    fontWeight: '900',
  },
  reference: {
    flexShrink: 1,
    fontSize: 12,
    fontStyle: 'italic',
    color: '#616161',
  },
  status_pill: {
    backgroundColor: '#dcdcdc',
    borderWidth: 1,
    borderRadius: 10,
    paddingVertical: 2,
    paddingHorizontal: 14,
  },
  status_text: {
    fontSize: 12,
    fontWeight: 'bold',
  },
  test_name: {
    fontSize: 14,
    fontWeight: '900',
    color: '#000000',
  },
  bottom_row: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    gap: 8,
  },
  lab_text: {
    flex: 1,
    fontSize: 12,
    fontStyle: 'italic',
    color: '#616161',
  },
  size_text: {
    fontSize: 12,
    fontWeight: '600',
    fontStyle: 'italic',
    color: '#424242',
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