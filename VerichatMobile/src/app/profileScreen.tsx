import { MaterialCommunityIcons } from '@expo/vector-icons'
import { LinearGradient } from 'expo-linear-gradient'
import { useRouter } from 'expo-router'
import { useCallback, useEffect, useState } from 'react'
import {
    ActivityIndicator,
    Alert,
    Modal,
    Platform,
    RefreshControl,
    ScrollView,
    StyleSheet,
    Text,
    TextInput,
    TouchableOpacity,
    View,
} from 'react-native'
import { useSafeAreaInsets } from 'react-native-safe-area-context'
import { logout } from '../constants/authService'
import { fetchProfile, Profile, updateProfile } from '../constants/profileService'

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
  const [addressOpen, setAddressOpen] = useState(false)
  const [savingAddress, setSavingAddress] = useState(false)
  const [addressForm, setAddressForm] = useState({ name: '', street: '', city: '', country: '' })

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

  useEffect(() => {
    const timer = setInterval(() => void load(), 60_000)
    return () => clearInterval(timer)
  }, [load])

  const onRefresh = async () => {
    setRefreshing(true)
    await load()
    setRefreshing(false)
  }

  const goSettings = () => {
    router.push('/settingsScreen')
  }

  const editAddress = () => {
    setAddressForm({
      name: profile?.defaultAddress?.name ?? '',
      street: profile?.defaultAddress?.street ?? '',
      city: profile?.defaultAddress?.city ?? '',
      country: profile?.defaultAddress?.country ?? '',
    })
    setAddressOpen(true)
  }

  const saveAddress = async () => {
    if (!addressForm.name.trim() || !addressForm.street.trim() || !addressForm.city.trim() || !addressForm.country.trim()) {
      Alert.alert('Address incomplete', 'Fill in the address name, street, city and country.')
      return
    }
    setSavingAddress(true)
    try {
      const updated = await updateProfile({
        addressName: addressForm.name.trim(),
        street: addressForm.street.trim(),
        city: addressForm.city.trim(),
        country: addressForm.country.trim(),
      })
      setProfile(updated)
      setAddressOpen(false)
    } catch (cause) {
      Alert.alert('Could not update address', cause instanceof Error ? cause.message : 'Please try again.')
    } finally {
      setSavingAddress(false)
    }
  }

  const confirmSignOut = () => {
    Alert.alert('Sign out', 'Are you sure you want to sign out?', [
      { text: 'Cancel', style: 'cancel' },
      {
        text: 'Sign out',
        style: 'destructive',
        onPress: () => {
          // Clears the persisted JWT and returns to the role chooser.
          logout()
          router.replace('/')
        },
      },
    ])
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
          <TouchableOpacity onPress={goSettings} hitSlop={10} style={styles.back_button}>
            <MaterialCommunityIcons name="cog-outline" size={28} color="#ffffff" />
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
            onPress={editAddress}
          >
            <Text style={styles.change_text}>{address ? 'Change Address' : 'Add Address'}</Text>
            <MaterialCommunityIcons name="arrow-right" size={26} color="#1b5e20" />
          </TouchableOpacity>
        </View>

        {/* Account actions */}
        <Text style={[styles.section_label, styles.address_label]}>ACCOUNT</Text>
        <View style={styles.details_card}>
          <TouchableOpacity style={styles.menu_row} onPress={goSettings}>
            <MaterialCommunityIcons name="cog-outline" size={22} color="#1b5e20" />
            <Text style={styles.menu_text}>Settings</Text>
            <MaterialCommunityIcons name="arrow-right" size={24} color="#9e9e9e" />
          </TouchableOpacity>

          <TouchableOpacity style={styles.menu_row} onPress={() => router.push('/cartScreen')}>
            <MaterialCommunityIcons name="package-variant-closed-check" size={22} color="#1b5e20" />
            <Text style={styles.menu_text}>Purchase history & order tracking</Text>
            <MaterialCommunityIcons name="arrow-right" size={24} color="#9e9e9e" />
          </TouchableOpacity>

          <TouchableOpacity style={styles.menu_row} onPress={confirmSignOut}>
            <MaterialCommunityIcons name="logout" size={22} color="#b3261e" />
            <Text style={[styles.menu_text, styles.sign_out_text]}>Sign out</Text>
          </TouchableOpacity>
        </View>
      </ScrollView>

      <Modal visible={addressOpen} transparent animationType="slide" onRequestClose={() => setAddressOpen(false)}>
        <View style={styles.modal_backdrop}>
          <View style={styles.address_modal}>
            <Text style={styles.modal_title}>Update delivery address</Text>
            {([
              ['name', 'Address name'],
              ['street', 'Street address'],
              ['city', 'City or region'],
              ['country', 'Country'],
            ] as const).map(([key, label]) => (
              <TextInput key={key} style={styles.address_input} value={addressForm[key]} onChangeText={(value) => setAddressForm((current) => ({ ...current, [key]: value }))} placeholder={label} autoCapitalize="words" />
            ))}
            <TouchableOpacity style={styles.save_address_button} disabled={savingAddress} onPress={() => void saveAddress()}><Text style={styles.save_address_text}>{savingAddress ? 'Saving…' : 'Save address'}</Text></TouchableOpacity>
            <TouchableOpacity onPress={() => setAddressOpen(false)}><Text style={styles.cancel_address}>Cancel</Text></TouchableOpacity>
          </View>
        </View>
      </Modal>
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
  menu_row: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingVertical: 16,
    gap: 12,
  },
  menu_text: {
    flex: 1,
    fontSize: 16,
    fontWeight: '600',
    color: '#1b1b1b',
  },
  sign_out_text: {
    color: '#b3261e',
  },
  modal_backdrop: { flex: 1, justifyContent: 'center', backgroundColor: 'rgba(0,0,0,0.45)', padding: 20 },
  address_modal: { backgroundColor: '#ffffff', borderRadius: 18, padding: 20, gap: 12 },
  modal_title: { color: '#0a2f55', fontSize: 20, fontWeight: '900', marginBottom: 4 },
  address_input: { height: 46, borderRadius: 10, borderWidth: 1, borderColor: '#aab2b9', paddingHorizontal: 12, color: '#111111' },
  save_address_button: { backgroundColor: '#0057a8', padding: 13, alignItems: 'center', borderRadius: 12, marginTop: 4 },
  save_address_text: { color: '#ffffff', fontWeight: '900' },
  cancel_address: { color: '#59636e', textAlign: 'center', padding: 6 },
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
