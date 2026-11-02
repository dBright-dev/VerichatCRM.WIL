import { CRMTheme } from '../constants/crmTheme'
import { useRouter } from 'expo-router'
import { useRef, useState } from 'react'
import {
    Alert, Image, KeyboardAvoidingView, Platform, ScrollView, StyleSheet, Text, TextInput, TouchableOpacity, View,
} from 'react-native'
import { login } from '../constants/authService'
import FloatingInput from '../components/ui/FloatingInput'
import AnimatedButton from '../components/ui/AnimatedButton'
import DemoQuickLogin from '../components/ui/DemoQuickLogin'
import { useKeyboardAutoScroll } from '../hooks/useKeyboardAutoScroll'

const AdminLoginScreen = () => {
  const router = useRouter()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [busy, setBusy] = useState(false)
  const emailInput = useRef<TextInput>(null)
  const passwordInput = useRef<TextInput>(null)
  const { scrollRef, contentRef, onFieldFocus } = useKeyboardAutoScroll()

  const handleLogin = () => {
    if (!email.trim() || !password) {
      Alert.alert('Missing details', 'Enter your email address and password to log in.')
      return
    }

    void submit(email.trim(), password)
  }

  /**
   * The sign-in path both entries share, taking the credential as arguments rather
   * than reading the fields: a demo chip supplies its own account and still goes
   * through the same call, so there is one place where a session is issued.
   */
  const submit = async (mail: string, pass: string) => {
    setBusy(true)
    try {
      await login(mail, pass, 'staff')
      router.replace('/')
    } catch (e) {
      Alert.alert('Sign in failed', e instanceof Error ? e.message : 'Something went wrong.')
    } finally {
      setBusy(false)
    }
  }

  // The chip signs in and leaves the address in the field, so the person can see
  // which of the seven accounts the board they landed on belongs to.
  const handleDemoPick = (mail: string, pass: string) => {
    setEmail(mail)
    setPassword(pass)
    void submit(mail, pass)
  }

  const handleSSOLogin = () => {
    // TODO: start your SSO flow here (e.g. expo-auth-session),
    // then navigate to the admin dashboard on success.
    Alert.alert('SSO', 'Single sign-on is not configured yet.')
  }

  return (
    <KeyboardAvoidingView
      style={styles.flex}
      behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
    >
      <ScrollView
        ref={scrollRef}
        contentContainerStyle={styles.container}
        keyboardShouldPersistTaps="handled"
        showsVerticalScrollIndicator={false}
      >
        <View ref={contentRef} collapsable={false} style={styles.content}>
        <View style={styles.Logo}>
          <Image
            source={require('../../assets/images/brand/verichat-logo-light.png')}
            style={styles.logo_image}
          />
        </View>

        <Text style={styles.welcome_text}>Welcome Back!</Text>
        <Text style={styles.title}>Staff / Admin Log in</Text>
        <Text style={styles.accessNote}>For System Administrators, Sales Representatives, Procurement Managers, and Field Support Engineers.</Text>

        <View style={styles.card}>
          <FloatingInput
            label="Email address"
            containerStyle={{ marginBottom: 0 }}
            style={{ fontSize: 16 }}
            value={email}
            onChangeText={setEmail}
            keyboardType="email-address"
            autoCapitalize="none"
            autoCorrect={false}
            autoComplete="email"
            textContentType="emailAddress"
            inputRef={emailInput}
            onFocus={() => onFieldFocus(emailInput.current)}
          />

          <FloatingInput
            label="Password"
            containerStyle={{ marginBottom: 0 }}
            style={{ fontSize: 16 }}
            value={password}
            onChangeText={setPassword}
            secureTextEntry
            autoCapitalize="none"
            autoCorrect={false}
            autoComplete="password"
            textContentType="password"
            inputRef={passwordInput}
            onFocus={() => onFieldFocus(passwordInput.current)}
          />

          <AnimatedButton
            style={[styles.button, styles.first_button, busy && styles.button_disabled]}
            onPress={handleLogin}
            disabled={busy}
            loading={busy}
          >
            <Text style={styles.button_text}>Log in</Text>
          </AnimatedButton>

          <TouchableOpacity style={styles.button} onPress={handleSSOLogin}>
            <Text style={styles.button_text}>Log in with SSO</Text>
          </TouchableOpacity>

          <TouchableOpacity style={styles.link} onPress={() => router.push('/forgotPassword')}>
            <Text style={styles.link_text}>Forgotten the password?</Text>
          </TouchableOpacity>

          <TouchableOpacity style={styles.link} onPress={() => router.push('/loginHelp')}>
            <Text style={styles.link_text}>Cannot sign in at all? Tell an administrator</Text>
          </TouchableOpacity>

          <TouchableOpacity style={styles.link} onPress={() => router.push('/privacy')}>
            <Text style={styles.link_text}>Privacy notice — what we hold about you</Text>
          </TouchableOpacity>

          <DemoQuickLogin audience="staff" onPick={handleDemoPick} disabled={busy} />
        </View>
        </View>
      </ScrollView>
    </KeyboardAvoidingView>
  )
}

export default AdminLoginScreen

const styles = StyleSheet.create({
  flex: {
    flex: 1,
    backgroundColor: CRMTheme.primaryDark,
  },
  container: {
    flexGrow: 1,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: CRMTheme.primaryDark,
    paddingVertical: 40,
  },
  content: { width: '100%', alignItems: 'center' },
  Logo: {
    marginBottom: 20,
  },
  logo_image: {
    width: 220,
    height: 134,
    resizeMode: 'contain',
  },
  welcome_text: {
    fontSize: 26,
    color: '#ffffff',
    fontStyle: 'italic',
    fontWeight: '900',
    fontFamily: Platform.select({ ios: 'Georgia', android: 'serif', default: 'serif' }),
    marginBottom: 20,
  },
  title: {
    fontSize: 22,
    color: '#ffffff',
    fontWeight: '700',
    marginBottom: 24,
  },
  accessNote: { color: '#dceaf5', textAlign: 'center', fontSize: 13, lineHeight: 19, maxWidth: 330, marginTop: -14, marginBottom: 18 },
  card: {
    backgroundColor: '#f9f9f9',
    width: '88%',
    maxWidth: 380,
    borderRadius: 25,
    paddingVertical: 28,
    paddingHorizontal: 20,
    gap: 8,
  },
  label: {
    fontSize: 14,
    fontWeight: 'bold',
    color: '#000000',
    marginLeft: 6,
  },
  input: {
    backgroundColor: '#ffffff',
    borderWidth: 1,
    borderColor: '#9e9e9e',
    borderRadius: 20,
    height: 46,
    paddingHorizontal: 16,
    fontSize: 16,
    color: '#000000',
    marginBottom: 12,
  },
  button: {
    backgroundColor: CRMTheme.primary,
    paddingVertical: 14,
    width: 190,
    alignSelf: 'center',
    alignItems: 'center',
    borderRadius: 20,
  },
  first_button: {
    marginTop: 12,
  },
  button_disabled: {
    opacity: 0.6,
  },
  button_text: {
    fontSize: 16,
    fontWeight: 'bold',
    color: '#ffffff',
  },
  // A quieter text action than the two buttons above it: staff who forgot a
  // password need the same recovery flow as clients, but asking for it is not the
  // primary job of this screen.
  link: {
    alignItems: 'center',
    marginTop: 14,
  },
  // These three links sit inside the pale card, so the near-white this used to ask
  // for rendered them invisible - the client screen's #424242 is the same weight on
  // the same background.
  link_text: {
    fontSize: 13,
    color: '#424242',
    fontWeight: '600',
  },
})
