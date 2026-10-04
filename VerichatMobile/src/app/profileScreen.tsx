import { MaterialCommunityIcons } from '@expo/vector-icons'
import { LinearGradient } from 'expo-linear-gradient'
import { useRouter } from 'expo-router'
import { useCallback, useEffect, useState } from 'react'
import {
    ActivityIndicator,
    Platform,
    RefreshControl,
    ScrollView,
    StyleSheet,
    Text,
    TouchableOpacity,
    View,
} from 'react-native'
import { useSafeAreaInsets } from 'react-native-safe-area-context'
import { fetchProfile, Profile } from '../constants/profileService'

const SERIF = Platform.select({ ios: 'Georgia', android: 'serif', default: 'serif' })

/* ------------------------------- Helpers -------------------------------- */

const getInitials = (name: string) => {
  const parts = name.trim().split(/\s+/).filter(Boolean)
  if (parts.length === 0) return '?'
  const first = parts[0][0]
  const last = parts.length > 1 ? parts[parts.length - 1][0] : ''
  return (first + last).toUpperCase()
}

const getYear = (iso: string) => {
  const date = new Date(iso)
  return isNaN(date.getTime()) ? '--' : String(date.getFullYear())
}

/* -------------------------------- Screen -------------------------------- */

const ProfileScreen = () => {
  const insets = useSafeAreaInsets()
  const router = useRouter()
  const [profile, setProfile] = useState<Profile | null>(null)
  const [loading, setLoading] = useState(true)
  const [refreshing, setRefreshing] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    try {
      const profile = await fetchProfile()
      setError(null)
      setProfile(profile)
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

  const goBack = () => {
    if (router.canGoBack()) {
      router.back()
    } else {
      router.replace('/')
    }
  }

  if (loading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator size="large" color="#004e7a" />
      </View>
    )
  }

  if (!profile) {
    return (
      <View style={styles.center}>
        <Text style={styles.error_text}>{error ?? 'No profile available.'}</Text>
        <TouchableOpacity style={styles.retry_button} onPress={onRefresh}>
          <Text style={styles.retry_text}>Try again</Text>
        </TouchableOpacity>
      </View>
    )
  }

  const address = profile.defaultAddress

  const details: { label: string; value: string | null }[] = [
    { label: 'Email:', value: profile.email },
    { label: 'Phone', value: profile.phone },
    { label: 'Company', value: profile.company },
    { label: 'VAT No.', value: profile.vatNumber },
  ]

  return (
    <View style={styles.screen}>
      {/* Header */}
      <LinearGradient
        colors={['#1f5f8b', '#3d7bd9']}
        start={{ x: 0, y: 0 }}
        end={{ x: 1, y: 1 }}
        style={[styles.header, { paddingTop: insets.top + 16 }]}
      >
        <View style={styles.header_top}>
          <TouchableOpacity onPress={goBack} hitSlop={10} style={styles.back_button}>
            <MaterialCommunityIcons name="chevron-left" size={32} color="#ffffff" />
          </TouchableOpacity>

          <Text style={styles.name} numberOfLines={2}>
            {profile.fullName}
          </Text>

          <View style={styles.avatar}>
            <Text style={styles.avatar_text}>{getInitials(profile.fullName)}</Text>
          </View>
        </View>

        <View style={styles.stats_panel}>
          <View style={styles.stat_pill}>
            <Text style={styles.stat_text}>{profile.orderCount}</Text>
            <Text style={styles.stat_text}>{profile.orderCount === 1 ? 'order' : 'orders'}</Text>
          </View>
          <View style={styles.stat_pill}>
            <Text style={styles.stat_text}>Since</Text>
            <Text style={styles.stat_text}>{getYear(profile.memberSince)}</Text>
          </View>
        </View>
      </LinearGradient>

      <ScrollView
        contentContainerStyle={styles.content}
        showsVerticalScrollIndicator={false}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} />}
      >
        {/* Account details */}
        <Text style={styles.section_label}>ACCOUNT DETAILS</Text>
        <View style={styles.details_card}>
          {details.map((item) => (
            <View key={item.label} style={styles.detail_row}>
              <Text style={styles.detail_label}>{item.label}</Text>
              <Text style={styles.detail_value}>{item.value || '--'}</Text>
            </View>
          ))}
        </View>

        {/* Default address */}
        <Text style={[styles.section_label, styles.address_label]}>DEFAULT ADDRESS</Text>
        <View style={styles.address_card}>
          {address ? (
            <>
              <Text style={styles.address_name}>{address.name}</Text>
              <Text style={styles.address_text}>{address.street},</Text>
              <Text style={styles.address_text}>
                {address.city}, {address.country}
              </Text>
            </>
          ) : (
            <Text style={styles.address_text}>No default address set</Text>
          )}

          <TouchableOpacity
            style={styles.change_link}
            onPress={() => {
              // TODO: open the change/add address flow
            }}
          >
            <Text style={styles.change_text}>{address ? 'Change Address' : 'Add Address'}</Text>
            <MaterialCommunityIcons name="arrow-right" size={26} color="#1b5e20" />
          </TouchableOpacity>
        </View>
      </ScrollView>
    </View>
  )
}

export default ProfileScreen

/* -------------------------------- Styles -------------------------------- */

const styles = StyleSheet.create({
  screen: {
    flex: 1,
    backgroundColor: '#ffffff',
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
    paddingHorizontal: 16,
    paddingBottom: 22,
    borderBottomLeftRadius: 40,
    borderBottomRightRadius: 40,
  },
  header_top: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: 18,
  },
  back_button: {
    marginRight: 4,
  },
  name: {
    flex: 1,
    paddingRight: 12,
    fontSize: 30,
    color: '#ffffff',
    fontStyle: 'italic',
    fontWeight: '700',
    fontFamily: SERIF,
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
  stats_panel: {
    flexDirection: 'row',
    justifyContent: 'space-around',
    backgroundColor: '#0b4a8f',
    borderWidth: 1,
    borderColor: 'rgba(255,255,255,0.45)',
    borderRadius: 16,
    paddingVertical: 12,
    paddingHorizontal: 10,
  },
  stat_pill: {
    backgroundColor: '#f7f7f7',
    borderRadius: 12,
    alignItems: 'center',
    justifyContent: 'center',
    paddingVertical: 8,
    paddingHorizontal: 28,
  },
  stat_text: {
    fontSize: 14,
    fontWeight: '900',
    color: '#000000',
  },

  // Content
  content: {
    paddingHorizontal: 20,
    paddingTop: 22,
    paddingBottom: 30,
  },
  section_label: {
    fontSize: 12,
    fontWeight: '900',
    color: '#424242',
    marginLeft: 4,
    marginBottom: 8,
  },
  address_label: {
    marginTop: 26,
  },

  // Account details
  details_card: {
    backgroundColor: '#efefef',
    borderRadius: 12,
    paddingVertical: 10,
    paddingHorizontal: 12,
  },
  detail_row: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingVertical: 16,
    gap: 8,
  },
  detail_label: {
    width: 90,
    textAlign: 'center',
    fontSize: 14,
    color: '#424242',
  },
  detail_value: {
    flex: 1,
    textAlign: 'center',
    fontSize: 14,
    color: '#333333',
  },

  // Address
  address_card: {
    backgroundColor: '#efefef',
    borderRadius: 14,
    padding: 16,
    gap: 4,
  },
  address_name: {
    fontSize: 15,
    fontWeight: '900',
    color: '#000000',
    marginBottom: 2,
  },
  address_text: {
    fontSize: 15,
    color: '#333333',
  },
  change_link: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
    alignSelf: 'flex-start',
    marginTop: 10,
  },
  change_text: {
    fontSize: 14,
    fontWeight: '900',
    color: '#1b5e20',
  },
})
