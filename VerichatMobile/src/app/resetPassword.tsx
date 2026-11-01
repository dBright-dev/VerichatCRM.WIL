import { CRMTheme } from '../constants/crmTheme'
import { useRouter } from 'expo-router'
import { useRef, useState } from 'react'
import {
    Alert, Image, KeyboardAvoidingView, Platform, ScrollView, StyleSheet, Text, TextInput, TouchableOpacity, View,
} from 'react-native'
import { resetPassword } from '../constants/authService'
import AnimatedButton from '../components/ui/AnimatedButton'
import { useKeyboardAutoScroll } from '../hooks/useKeyboardAutoScroll'

/**
 * The second half of a forgotten password, for whoever has the code but no browser
 * to open the emailed link in. The code is not something a person reads - it is
 * pasted from the email - so the field is plain and the copy says exactly that.
 */
const ResetPasswordScreen = () => {
  const router = useRouter()
  const [code, setCode] = useState('')
  const [password, setPassword] = useState('')
  const [confirm, setConfirm] = useState('')
  const [busy, setBusy] = useState(false)
  const [done, setDone] = useState<string | null>(null)
  const codeInput = useRef<TextInput>(null)
  const passwordInput = useRef<TextInput>(null)
  const confirmInput = useRef<TextInput>(null)
  const { scrollRef, contentRef, onFieldFocus } = useKeyboardAutoScroll()

  const handleSubmit = async () => {
    const token = code.trim().replace(/\s+/g, '')
    if (!token) {
      Alert.alert('Missing code', 'Paste the code from the reset email.')
      return
    }
    if (password.length < 8) {
      Alert.alert('Password too short', 'Use at least 8 characters.')
      return
    }
    if (password !== confirm) {
      Alert.alert('Passwords do not match', 'Type the new password once more, the same way.')
      return
    }

    setBusy(true)
    try {
      // A spent or expired code comes back as the server's own sentence, which is the
      // one thing worth showing: it tells the person to ask again rather than to try
      // the same link a second time.
      setDone(await resetPassword(token, password))
      setCode('')
      setPassword('')
      setConfirm('')
    } catch (e) {
      Alert.alert('That code did not work', e instanceof Error ? e.message : 'Something went wrong.')
    } finally {
      setBusy(false)
    }
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

        <Text style={styles.title}>{done ? 'Password changed' : 'Choose a new password'}</Text>

        <View style={styles.card}>
          {done === null ? (
            <>
              <Text style={styles.label}>Code from the reset email</Text>
              <TextInput
                ref={codeInput}
                onFocus={() => onFieldFocus(codeInput.current)}
                style={[styles.input, styles.code]}
                value={code}
                onChangeText={setCode}
                multiline
                autoCapitalize="none"
                autoCorrect={false}
              />

              <Text style={styles.label}>New password</Text>
              <TextInput
                ref={passwordInput}
                onFocus={() => onFieldFocus(passwordInput.current)}
                style={styles.input}
                value={password}
                onChangeText={setPassword}
                secureTextEntry
                autoCapitalize="none"
                autoCorrect={false}
                autoComplete="new-password"
                textContentType="newPassword"
              />

              <Text style={styles.label}>Confirm new password</Text>
              <TextInput
                ref={confirmInput}
                onFocus={() => onFieldFocus(confirmInput.current)}
                style={styles.input}
                value={confirm}
                onChangeText={setConfirm}
                secureTextEntry
                autoCapitalize="none"
                autoCorrect={false}
                autoComplete="new-password"
                textContentType="newPassword"
              />

              <AnimatedButton
                style={[styles.button, busy && styles.button_disabled]}
                onPress={handleSubmit}
                disabled={busy}
                loading={busy}
              >
                <Text style={styles.button_text}>Change my password</Text>
              </AnimatedButton>
            </>
          ) : (
            <>
              <Text style={styles.note}>{done}</Text>
              <Text style={styles.note}>
                The link that got you here is spent, so a second copy of that email buys
                nothing. Sign in with the password you just set.
              </Text>
              <TouchableOpacity style={styles.button} onPress={() => router.replace('/clientLogin')}>
                <Text style={styles.button_text}>Sign in</Text>
              </TouchableOpacity>
            </>
          )}

          <TouchableOpacity style={styles.back} onPress={() => router.replace('/clientLogin')}>
            <Text style={styles.back_text}>← Back to sign in</Text>
          </TouchableOpacity>
        </View>
        </View>
      </ScrollView>
    </KeyboardAvoidingView>
  )
}

export default ResetPasswordScreen

const styles = StyleSheet.create({
  flex: { flex: 1, backgroundColor: CRMTheme.primaryDark },
  container: {
    flexGrow: 1,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: CRMTheme.primaryDark,
    paddingVertical: 40,
  },
  content: { width: '100%', alignItems: 'center' },
  Logo: { marginBottom: 20 },
  logo_image: { width: 220, height: 134, resizeMode: 'contain' },
  title: { fontSize: 22, color: '#ffffff', fontWeight: '700', marginBottom: 24 },
  card: {
    backgroundColor: '#f9f9f9',
    width: '88%',
    maxWidth: 380,
    borderRadius: 25,
    paddingVertical: 28,
    paddingHorizontal: 20,
    gap: 8,
  },
  label: { fontSize: 14, fontWeight: 'bold', color: '#000000', marginLeft: 6 },
  input: {
    backgroundColor: '#ffffff',
    borderWidth: 1,
    borderColor: '#9e9e9e',
    borderRadius: 20,
    minHeight: 46,
    paddingHorizontal: 16,
    paddingVertical: 12,
    fontSize: 16,
    color: '#000000',
    marginBottom: 12,
  },
  code: { height: 84, textAlignVertical: 'top', fontFamily: Platform.OS === 'ios' ? 'Menlo' : 'monospace', fontSize: 13 },
  note: { fontSize: 14, color: '#424242', lineHeight: 20 },
  button: {
    backgroundColor: CRMTheme.primary,
    paddingVertical: 14,
    width: 220,
    alignSelf: 'center',
    alignItems: 'center',
    borderRadius: 20,
    marginTop: 8,
  },
  button_disabled: { opacity: 0.6 },
  button_text: { fontSize: 16, fontWeight: 'bold', color: '#ffffff' },
  back: { alignItems: 'center', marginTop: 14 },
  back_text: { fontSize: 14, color: '#424242', fontWeight: '600' },
})
