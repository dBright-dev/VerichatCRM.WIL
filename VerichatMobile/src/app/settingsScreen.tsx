import { CRMTheme } from '../constants/crmTheme'
import { MaterialCommunityIcons } from '@expo/vector-icons'
import { LinearGradient } from 'expo-linear-gradient'
import { useRouter } from 'expo-router'
import { ComponentProps, useCallback, useEffect, useState } from 'react'
import {
    ActivityIndicator,
    Alert,
    Platform,
    RefreshControl,
    ScrollView,
    StyleSheet,
    Switch,
    Text,
    TouchableOpacity,
    View,
} from 'react-native'
import { useSafeAreaInsets } from 'react-native-safe-area-context'
import { logout } from '../constants/authService'
import { authenticateBiometric, hasUsableBiometrics } from '../constants/biometricAuth'
import { getBiometricLoginEnabled, setBiometricLoginEnabled } from '../constants/api'
import { applyDarkMode } from '../constants/appTheme'
import { fetchSettings, Preferences, updatePreferences, UserSettings } from '../constants/settingsService'

/* ------------------------------ Constants ------------------------------- */

type IconName = ComponentProps<typeof MaterialCommunityIcons>['name']

const SERIF = Platform.select({ ios: 'Georgia', android: 'serif', default: 'serif' })

const PREFERENCE_ROWS: { key: keyof Preferences; title: string; subtitle: string }[] = [
  {
    key: 'pushNotifications',
    title: 'Push Notifications',
    subtitle: 'Orders, SLA alerts & promotions',
  },
  {
    key: 'biometricLogin',
    title: 'Biometric Login',
    subtitle: 'Sign in with fingerprint or Face ID',
  },
  {
    key: 'darkMode',
    title: 'Dark Mode',
    subtitle: 'Reduce eye strain',
  },
]

const MORE_ROWS: { key: string; title: string; icon: IconName; color: string }[] = [
  { key: 'history', title: 'Purchase History & Reports', icon: 'chart-bar', color: '#e65100' },
  { key: 'privacy', title: 'Privacy', icon: 'lock', color: CRMTheme.text },
  { key: 'terms', title: 'Terms and Conditions', icon: 'text-box-check-outline', color: '#8d6e63' },
  { key: 'about', title: 'About Verichat', icon: 'information', color: CRMTheme.primary },
]

/* ------------------------------- Helpers -------------------------------- */

const getInitials = (name: string) => {
  const parts = name.trim().split(/\s+/).filter(Boolean)
  if (parts.length === 0) return '?'
  const first = parts[0][0]
  const last = parts.length > 1 ? parts[parts.length - 1][0] : ''
  return (first + last).toUpperCase()
}

/* -------------------------------- Screen -------------------------------- */

const SettingsScreen = () => {
  const insets = useSafeAreaInsets()
  const router = useRouter()
  const [settings, setSettings] = useState<UserSettings | null>(null)
  const [loading, setLoading] = useState(true)
  const [refreshing, setRefreshing] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    try {
      const settings = await fetchSettings()
      settings.preferences.biometricLogin = await getBiometricLoginEnabled()
      await applyDarkMode(settings.preferences.darkMode)
      setError(null)
      setSettings(settings)
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

  const setPreference = async (key: keyof Preferences, value: boolean) => {
    if (!settings) return

    if (key === 'biometricLogin') {
      if (value) {
        try {
          if (!(await hasUsableBiometrics())) {
            Alert.alert('Biometrics unavailable', 'Set up Face ID or fingerprint authentication on this device first.')
            return
          }
          if (!(await authenticateBiometric('Enable biometric sign-in for Verichat'))) return
          await setBiometricLoginEnabled(true)
        } catch {
          Alert.alert('Could not enable', 'Biometric sign-in could not be enabled on this device.')
          return
        }
      } else {
        try {
          await setBiometricLoginEnabled(false)
        } catch {
          Alert.alert('Could not disable', 'Biometric sign-in could not be changed on this device.')
          return
        }
      }
    }

    const previous = settings.preferences[key]
    // Update the switch straight away, then roll back if saving fails.
    setSettings((current) =>
      current ? { ...current, preferences: { ...current.preferences, [key]: value } } : current,
    )
    if (key === 'darkMode') await applyDarkMode(value)

    try {
      await updatePreferences({ [key]: value })
    } catch {
      if (key === 'biometricLogin') await setBiometricLoginEnabled(previous)
      if (key === 'darkMode') await applyDarkMode(previous)
      setSettings((current) =>
        current ? { ...current, preferences: { ...current.preferences, [key]: previous } } : current,
      )
      Alert.alert('Could not save', 'We could not update that setting. Please try again.')
    }
  }

  const handleMorePress = (key: string) => {
    if (key === 'history') {
      router.push('/cartScreen')
      return
    }
    const title = MORE_ROWS.find((row) => row.key === key)?.title ?? 'Information'
    Alert.alert(title, 'This information is not available in the mobile API yet.')
  }

  const handleSignOut = () => {
    Alert.alert('Sign out', 'Are you sure you want to sign out?', [
      { text: 'Cancel', style: 'cancel' },
      {
        text: 'Sign out',
        style: 'destructive',
        onPress: () => {
          // Clear the stored JWT, then return to the login/chooser screen.
          logout()
          router.replace('/')
        },
      },
    ])
  }

  if (loading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator size="large" color={CRMTheme.primary} />
      </View>
    )
  }

  if (!settings) {
    return (
      <View style={styles.center}>
        <Text style={styles.error_text}>{error ?? 'No settings available.'}</Text>
        <TouchableOpacity style={styles.retry_button} onPress={onRefresh}>
          <Text style={styles.retry_text}>Try again</Text>
        </TouchableOpacity>
      </View>
    )
  }

  return (
    <View style={styles.screen}>
      {/* Header */}
      <LinearGradient
        colors={[CRMTheme.primaryDark, CRMTheme.primary]}
        start={{ x: 0, y: 0 }}
        end={{ x: 1, y: 1 }}
        style={[styles.header, { paddingTop: insets.top + 16 }]}
      >
        <TouchableOpacity
          onPress={() => router.back()}
          hitSlop={10}
          style={styles.back_button}
          accessibilityLabel="Back to profile"
        >
          <MaterialCommunityIcons name="chevron-left" size={32} color="#ffffff" />
        </TouchableOpacity>

        <Text style={styles.name} numberOfLines={2}>
          {settings.fullName}
        </Text>
        <TouchableOpacity
          style={styles.avatar}
          activeOpacity={0.8}
          onPress={() => router.push('/profileScreen')}
        >
          <Text style={styles.avatar_text}>{getInitials(settings.fullName)}</Text>
        </TouchableOpacity>
      </LinearGradient>

      <ScrollView
        contentContainerStyle={styles.content}
        showsVerticalScrollIndicator={false}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} />}
      >
        {/* Preferences */}
        <Text style={styles.section_label}>PREFERENCES</Text>
        <View style={styles.group}>
          {PREFERENCE_ROWS.map((row, index) => (
            <View key={row.key}>
              {index > 0 && <View style={styles.divider} />}
              <View style={styles.pref_row}>
                <View style={styles.pref_text}>
                  <Text style={styles.pref_title}>{row.title}</Text>
                  <Text style={styles.pref_subtitle}>{row.subtitle}</Text>
                </View>
                <Switch
                  value={settings.preferences[row.key]}
                  onValueChange={(value) => setPreference(row.key, value)}
                  trackColor={{ false: '#9e9e9e', true: CRMTheme.primary }}
                  thumbColor="#ffffff"
                  ios_backgroundColor="#9e9e9e"
                />
              </View>
            </View>
          ))}
        </View>

        {/* More */}
        <Text style={[styles.section_label, styles.more_label]}>MORE</Text>
        <View style={styles.group}>
          {MORE_ROWS.map((row, index) => (
            <View key={row.key}>
              {index > 0 && <View style={styles.divider} />}
              <TouchableOpacity style={styles.more_row} onPress={() => handleMorePress(row.key)}>
                <MaterialCommunityIcons name={row.icon} size={30} color={row.color} />
                <Text style={styles.more_title}>{row.title}</Text>
              </TouchableOpacity>
            </View>
          ))}
        </View>

        {/* Sign out */}
        <TouchableOpacity style={styles.sign_out} onPress={handleSignOut}>
          <Text style={styles.sign_out_text}>Sign Out</Text>
        </TouchableOpacity>
      </ScrollView>
    </View>
  )
}

export default SettingsScreen

/* -------------------------------- Styles -------------------------------- */

const styles = StyleSheet.create({
  screen: {
    flex: 1,
    backgroundColor: CRMTheme.background,
  },
  center: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: CRMTheme.background,
    padding: 24,
    gap: 16,
  },
  error_text: {
    fontSize: 16,
    color: '#333333',
    textAlign: 'center',
  },
  retry_button: {
    backgroundColor: CRMTheme.primary,
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
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingHorizontal: 20,
    paddingBottom: 22,
    borderBottomLeftRadius: 30,
    borderBottomRightRadius: 30,
  },
  back_button: {
    width: 32,
    height: 32,
    justifyContent: 'center',
    marginRight: 6,
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
    backgroundColor: CRMTheme.primary,
    borderWidth: 2,
    borderColor: CRMTheme.primaryDark,
    justifyContent: 'center',
    alignItems: 'center',
  },
  avatar_text: {
    color: '#ffffff',
    fontWeight: 'bold',
    fontSize: 18,
  },

  // Content
  content: {
    paddingHorizontal: 16,
    paddingTop: 18,
    paddingBottom: 30,
  },
  section_label: {
    fontSize: 12,
    fontWeight: '900',
    color: CRMTheme.muted,
    marginLeft: 6,
    marginBottom: 6,
  },
  more_label: {
    marginTop: 26,
  },
  group: {
    backgroundColor: CRMTheme.surface,
    borderRadius: 18,
    overflow: 'hidden',
  },
  divider: {
    height: 1,
    backgroundColor: CRMTheme.border,
    marginHorizontal: 8,
  },

  // Preferences
  pref_row: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingVertical: 12,
    paddingHorizontal: 14,
    gap: 12,
  },
  pref_text: {
    flex: 1,
    gap: 2,
  },
  pref_title: {
    fontSize: 16,
    fontWeight: '900',
    color: CRMTheme.text,
  },
  pref_subtitle: {
    fontSize: 12,
    fontWeight: '600',
    color: CRMTheme.muted,
  },

  // More
  more_row: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 12,
    paddingVertical: 12,
    paddingHorizontal: 14,
  },
  more_title: {
    flex: 1,
    fontSize: 15,
    fontWeight: '900',
    color: CRMTheme.text,
  },

  // Sign out
  sign_out: {
    marginTop: 24,
    backgroundColor: CRMTheme.surface,
    borderWidth: 2,
    borderColor: '#b71c3c',
    borderRadius: 14,
    paddingVertical: 18,
    alignItems: 'center',
  },
  sign_out_text: {
    fontSize: 17,
    fontWeight: '900',
    color: '#b71c3c',
  },
})




