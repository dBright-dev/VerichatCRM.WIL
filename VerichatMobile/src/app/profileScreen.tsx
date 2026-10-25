import { CRMTheme } from '../constants/crmTheme'
import { MaterialCommunityIcons } from '@expo/vector-icons'
import { LinearGradient } from 'expo-linear-gradient'
import { useRouter } from 'expo-router'
import { useCallback, useEffect, useState } from 'react'
import {
  Alert,
  Modal,
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
import BrandLoader from '../components/ui/BrandLoader'

const initials = (name: string) => name.trim().split(/\s+/).filter(Boolean).slice(0, 2).map(part => part[0]).join('').toUpperCase() || '?'
const memberYear = (date: string) => {
  const parsed = new Date(date)
  return Number.isNaN(parsed.getTime()) ? '—' : String(parsed.getFullYear())
}

export default function ProfileScreen() {
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
      setProfile(await fetchProfile())
      setError(null)
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : 'Something went wrong.')
    } finally {
      setLoading(false)
      setRefreshing(false)
    }
  }, [])

  useEffect(() => {
    // Load the authenticated user's profile after this route mounts.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    void load()
  }, [load])

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
      const changes = {
        addressName: addressForm.name.trim(),
        street: addressForm.street.trim(),
        city: addressForm.city.trim(),
        country: addressForm.country.trim(),
      }
      const updated = await updateProfile(changes)
      setProfile(updated ?? (profile ? {
        ...profile,
        defaultAddress: { name: changes.addressName, street: changes.street, city: changes.city, country: changes.country },
      } : profile))
      setAddressOpen(false)
    } catch (cause) {
      Alert.alert('Could not update address', cause instanceof Error ? cause.message : 'Please try again.')
    } finally {
      setSavingAddress(false)
    }
  }

  const confirmSignOut = () => Alert.alert('Sign out', 'Are you sure you want to sign out?', [
    { text: 'Cancel', style: 'cancel' },
    { text: 'Sign out', style: 'destructive', onPress: () => { logout(); router.replace('/') } },
  ])

  if (loading) return <View style={styles.loading}><BrandLoader label="Loading your profile…" /></View>
  if (!profile) return <View style={styles.loading}><Text style={styles.error}>{error ?? 'No profile available.'}</Text><TouchableOpacity style={styles.retry} onPress={() => { setRefreshing(true); void load() }}><Text style={styles.retryText}>Try again</Text></TouchableOpacity></View>

  const accountRows = [
    { label: 'Email address', value: profile.email, icon: 'email-outline' as const },
    { label: 'Phone number', value: profile.phone || 'Add your phone number', icon: 'phone-outline' as const },
    { label: 'Company', value: profile.company || 'Add your company', icon: 'domain' as const },
    { label: 'VAT number', value: profile.vatNumber || 'Not provided', icon: 'card-account-details-outline' as const },
  ]
  const address = profile.defaultAddress

  return (
    <View style={styles.screen}>
      <ScrollView
        showsVerticalScrollIndicator={false}
        contentContainerStyle={{ paddingBottom: insets.bottom + 28 }}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={() => { setRefreshing(true); void load() }} tintColor="#1167a8" />}
      >
        <LinearGradient colors={['#123c61', '#17689b']} start={{ x: 0, y: 0 }} end={{ x: 1, y: 1 }} style={[styles.hero, { paddingTop: insets.top + 18 }]}>
          <View style={styles.topline}>
            <View><Text style={styles.eyebrow}>ACCOUNT</Text><Text style={styles.pageTitle}>Your profile</Text></View>
            <TouchableOpacity accessibilityRole="button" accessibilityLabel="Open settings" style={styles.settingsButton} onPress={() => router.push('/settingsScreen')}>
              <MaterialCommunityIcons name="cog-outline" size={22} color="#fff" />
            </TouchableOpacity>
          </View>
          <View style={styles.identity}>
            <View style={styles.avatar}><Text style={styles.avatarText}>{initials(profile.fullName)}</Text></View>
            <View style={styles.identityText}><Text style={styles.name} numberOfLines={2}>{profile.fullName}</Text><Text style={styles.company}>{profile.company || 'Verichat client account'}</Text></View>
          </View>
          <View style={styles.summary}>
            <View style={styles.summaryCell}><Text style={styles.summaryValue}>{profile.orderCount}</Text><Text style={styles.summaryLabel}>Orders</Text></View>
            <View style={styles.summaryDivider} />
            <View style={styles.summaryCell}><Text style={styles.summaryValue}>{memberYear(profile.memberSince)}</Text><Text style={styles.summaryLabel}>Member since</Text></View>
          </View>
        </LinearGradient>

        <View style={styles.body}>
          {error ? <Text style={styles.inlineError}>{error} · Pull to refresh to try again.</Text> : null}
          <SectionHeading title="Account details" subtitle="Your contact and business information" />
          <View style={styles.card}>
            {accountRows.map((row, index) => <View key={row.label} style={[styles.detailRow, index > 0 && styles.divided]}>
              <View style={styles.rowIcon}><MaterialCommunityIcons name={row.icon} size={19} color="#17689b" /></View>
              <View style={styles.rowCopy}><Text style={styles.rowLabel}>{row.label}</Text><Text style={styles.rowValue}>{row.value}</Text></View>
            </View>)}
          </View>

          <SectionHeading title="Delivery address" subtitle="Used when preparing orders and quotations" />
          <View style={styles.addressCard}>
            <View style={styles.addressIcon}><MaterialCommunityIcons name="map-marker-outline" size={22} color="#17689b" /></View>
            <View style={styles.addressCopy}>
              <Text style={styles.addressName}>{address?.name || 'No default address'}</Text>
              <Text style={styles.addressText}>{address ? `${address.street}, ${address.city}, ${address.country}` : 'Add your organisation’s delivery location.'}</Text>
            </View>
            <TouchableOpacity accessibilityRole="button" accessibilityLabel={address ? 'Edit delivery address' : 'Add delivery address'} onPress={editAddress} style={styles.editButton}>
              <MaterialCommunityIcons name="pencil-outline" size={18} color="#17689b" />
            </TouchableOpacity>
          </View>

          <SectionHeading title="Preferences and activity" subtitle="Manage your account" />
          <View style={styles.card}>
            <ActionRow icon="cog-outline" title="Settings" subtitle="Theme, notifications and sign-in" onPress={() => router.push('/settingsScreen')} />
            <ActionRow icon="clipboard-list-outline" title="Orders and quote requests" subtitle="Track requests and delivery progress" onPress={() => router.push('/cartScreen')} />
          </View>
          <TouchableOpacity accessibilityRole="button" onPress={confirmSignOut} style={styles.signOut}>
            <MaterialCommunityIcons name="logout" size={19} color="#b42318" /><Text style={styles.signOutText}>Sign out</Text>
          </TouchableOpacity>
          <Text style={styles.version}>VERICHAT CRM · BUSINESS ACCOUNT</Text>
        </View>
      </ScrollView>

      <Modal visible={addressOpen} transparent animationType="slide" onRequestClose={() => setAddressOpen(false)}>
        <View style={styles.modalBackdrop}><View style={[styles.modal, { paddingBottom: insets.bottom + 20 }]}>
          <View style={styles.modalHandle} /><Text style={styles.modalTitle}>Delivery address</Text><Text style={styles.modalSubtitle}>Keep your organisation delivery details up to date.</Text>
          {([['name', 'Address label'], ['street', 'Street address'], ['city', 'City or region'], ['country', 'Country']] as const).map(([key, label]) => <TextInput key={key} style={styles.input} value={addressForm[key]} onChangeText={value => setAddressForm(current => ({ ...current, [key]: value }))} placeholder={label} autoCapitalize="words" />)}
          <TouchableOpacity style={styles.saveButton} disabled={savingAddress} onPress={() => void saveAddress()}><Text style={styles.saveText}>{savingAddress ? 'Saving…' : 'Save address'}</Text></TouchableOpacity>
          <TouchableOpacity onPress={() => setAddressOpen(false)}><Text style={styles.cancel}>Cancel</Text></TouchableOpacity>
        </View></View>
      </Modal>
    </View>
  )
}

function SectionHeading({ title, subtitle }: { title: string; subtitle: string }) {
  return <View style={styles.sectionHeading}><Text style={styles.sectionTitle}>{title}</Text><Text style={styles.sectionSubtitle}>{subtitle}</Text></View>
}

function ActionRow({ icon, title, subtitle, onPress }: { icon: 'cog-outline' | 'clipboard-list-outline'; title: string; subtitle: string; onPress: () => void }) {
  return <TouchableOpacity accessibilityRole="button" onPress={onPress} style={styles.actionRow}>
    <View style={styles.rowIcon}><MaterialCommunityIcons name={icon} size={20} color="#17689b" /></View>
    <View style={styles.rowCopy}><Text style={styles.actionTitle}>{title}</Text><Text style={styles.rowLabel}>{subtitle}</Text></View>
    <MaterialCommunityIcons name="chevron-right" size={23} color="#8b9bad" />
  </TouchableOpacity>
}

const styles = StyleSheet.create({
  screen: { flex: 1, backgroundColor: CRMTheme.background },
  loading: { flex: 1, alignItems: 'center', justifyContent: 'center', padding: 24, backgroundColor: CRMTheme.background },
  error: { textAlign: 'center', color: '#465a6e', marginBottom: 14 },
  retry: { backgroundColor: '#17689b', paddingHorizontal: 22, paddingVertical: 11, borderRadius: 12 },
  retryText: { color: '#fff', fontWeight: '800' },
  hero: { paddingHorizontal: 22, paddingBottom: 22, borderBottomLeftRadius: 26, borderBottomRightRadius: 26 },
  topline: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between' },
  eyebrow: { color: '#b9d9ef', fontSize: 10, fontWeight: '900', letterSpacing: 1.5 },
  pageTitle: { color: '#fff', fontSize: 26, fontWeight: '900', marginTop: 2 },
  settingsButton: { width: 42, height: 42, borderRadius: 14, backgroundColor: '#ffffff20', alignItems: 'center', justifyContent: 'center' },
  identity: { flexDirection: 'row', alignItems: 'center', gap: 14, marginTop: 23, marginBottom: 22 },
  avatar: { width: 62, height: 62, borderRadius: 21, backgroundColor: '#d7edf9', alignItems: 'center', justifyContent: 'center', borderWidth: 2, borderColor: '#ffffff70' },
  avatarText: { color: '#14557f', fontSize: 20, fontWeight: '900' },
  identityText: { flex: 1 },
  name: { color: '#fff', fontSize: 20, fontWeight: '900' },
  company: { color: '#d8eaf6', fontSize: 13, marginTop: 4 },
  summary: { flexDirection: 'row', alignItems: 'center', backgroundColor: '#ffffff18', borderWidth: 1, borderColor: '#ffffff2b', borderRadius: 16, paddingVertical: 13 },
  summaryCell: { flex: 1, alignItems: 'center', gap: 3 },
  summaryValue: { color: '#fff', fontSize: 19, fontWeight: '900' },
  summaryLabel: { color: '#d5e7f3', fontSize: 11, fontWeight: '600' },
  summaryDivider: { width: 1, height: 32, backgroundColor: '#ffffff40' },
  body: { paddingHorizontal: 18, paddingTop: 20 },
  inlineError: { color: '#9f322b', backgroundColor: '#fff0ee', padding: 12, borderRadius: 12, marginBottom: 14 },
  sectionHeading: { marginTop: 5, marginBottom: 10 },
  sectionTitle: { color: CRMTheme.text, fontSize: 16, fontWeight: '900' },
  sectionSubtitle: { color: CRMTheme.muted, fontSize: 12, marginTop: 3 },
  card: { backgroundColor: '#fff', borderRadius: 17, paddingHorizontal: 14, borderWidth: 1, borderColor: CRMTheme.border, marginBottom: 20, shadowColor: '#153c5c', shadowOpacity: 0.04, shadowRadius: 9, shadowOffset: { width: 0, height: 3 }, elevation: 1 },
  detailRow: { flexDirection: 'row', alignItems: 'center', gap: 12, paddingVertical: 13 },
  divided: { borderTopWidth: 1, borderTopColor: '#edf1f5' },
  rowIcon: { width: 38, height: 38, borderRadius: 12, backgroundColor: CRMTheme.surfaceAlt, alignItems: 'center', justifyContent: 'center' },
  rowCopy: { flex: 1 },
  rowLabel: { color: '#7a8999', fontSize: 11, fontWeight: '600' },
  rowValue: { color: '#243c53', fontSize: 14, fontWeight: '700', marginTop: 3 },
  addressCard: { flexDirection: 'row', alignItems: 'center', gap: 12, backgroundColor: '#fff', borderRadius: 17, padding: 14, borderWidth: 1, borderColor: CRMTheme.border, marginBottom: 20 },
  addressIcon: { width: 42, height: 42, borderRadius: 13, backgroundColor: CRMTheme.surfaceAlt, alignItems: 'center', justifyContent: 'center' },
  addressCopy: { flex: 1 },
  addressName: { color: '#243c53', fontSize: 14, fontWeight: '800' },
  addressText: { color: CRMTheme.muted, fontSize: 12, lineHeight: 18, marginTop: 3 },
  editButton: { width: 36, height: 36, alignItems: 'center', justifyContent: 'center', borderRadius: 11, backgroundColor: '#f3f8fb' },
  actionRow: { flexDirection: 'row', alignItems: 'center', gap: 12, minHeight: 68 },
  actionTitle: { color: '#243c53', fontSize: 14, fontWeight: '800', marginBottom: 3 },
  signOut: { minHeight: 50, borderRadius: 14, borderWidth: 1, borderColor: '#f0d6d3', backgroundColor: '#fff', flexDirection: 'row', alignItems: 'center', justifyContent: 'center', gap: 9, marginTop: 1 },
  signOutText: { color: '#b42318', fontWeight: '800', fontSize: 14 },
  version: { textAlign: 'center', color: '#99a7b6', fontSize: 9, fontWeight: '800', letterSpacing: 1, marginTop: 20, marginBottom: 4 },
  modalBackdrop: { flex: 1, justifyContent: 'flex-end', backgroundColor: '#0a203866' },
  modal: { backgroundColor: '#fff', borderTopLeftRadius: 25, borderTopRightRadius: 25, padding: 22, gap: 11 },
  modalHandle: { width: 42, height: 4, borderRadius: 3, backgroundColor: '#d6dfe8', alignSelf: 'center', marginBottom: 5 },
  modalTitle: { color: CRMTheme.text, fontSize: 20, fontWeight: '900' },
  modalSubtitle: { color: '#718295', fontSize: 13, marginBottom: 3 },
  input: { minHeight: 48, borderRadius: 12, borderWidth: 1, borderColor: '#dce4ec', paddingHorizontal: 13, color: CRMTheme.text, backgroundColor: '#fbfcfd' },
  saveButton: { minHeight: 49, borderRadius: 13, alignItems: 'center', justifyContent: 'center', backgroundColor: '#17689b', marginTop: 4 },
  saveText: { color: '#fff', fontWeight: '900' },
  cancel: { color: '#66788b', textAlign: 'center', padding: 8, fontWeight: '700' },
})



