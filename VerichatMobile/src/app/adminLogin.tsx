import { CRMTheme } from '../constants/crmTheme'
import { useRouter } from 'expo-router'
import { useRef, useState } from 'react'
import {
    Alert, Image, KeyboardAvoidingView, Platform, ScrollView, StyleSheet, Text, TextInput, TouchableOpacity, View,
} from 'react-native'
import { login } from '../constants/authService'
import FloatingInput from '../components/ui/FloatingInput'
import AnimatedButton from '../components/ui/AnimatedButton'
import { useKeyboardAutoScroll } from '../hooks/useKeyboardAutoScroll'

const AdminLoginScreen = () => {
  const router = useRouter()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [busy, setBusy] = useState(false)
  const emailInput = useRef<TextInput>(null)
  const passwordInput = useRef<TextInput>(null)
  const { scrollRef, contentRef, onFieldFocus } = useKeyboardAutoScroll()

  const handleLogin = async () => {
    if (!email.trim() || !password) {
      Alert.alert('Missing details', 'Enter your email address and password to log in.')
      return
    }

    setBusy(true)
    try {
      await login(email.trim(), password, 'staff')
      router.replace('/')
    } catch (e) {
      Alert.alert('Sign in failed', e instanceof Error ? e.message : 'Something went wrong.')
    } finally {
      setBusy(false)
    }
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
            source={require('../../assets/images/canva/design_pt.png')}
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
    width: 150,
    height: 150,
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
})
