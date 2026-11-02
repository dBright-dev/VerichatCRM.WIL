import { CRMTheme } from '../constants/crmTheme'
import { useRouter } from 'expo-router'
import { useRef, useState } from 'react'
import {
    Alert, Image, KeyboardAvoidingView, Platform, ScrollView, StyleSheet, Text, TextInput, TouchableOpacity, View,
} from 'react-native'
import { forgotPassword } from '../constants/authService'
import FloatingInput from '../components/ui/FloatingInput'
import AnimatedButton from '../components/ui/AnimatedButton'
import { useKeyboardAutoScroll } from '../hooks/useKeyboardAutoScroll'

/**
 * "I cannot get in." The screen asks for one thing - the address the account was
 * created with - and answers the same way whether or not it is registered, because
 * the API does too. A forgotten password is not a reason to hand out a directory of
 * who has accounts.
 */
const ForgotPasswordScreen = () => {
  const router = useRouter()
  const [email, setEmail] = useState('')
  const [busy, setBusy] = useState(false)
  const [sentMessage, setSentMessage] = useState<string | null>(null)
  const emailInput = useRef<TextInput>(null)
  const { scrollRef, contentRef, onFieldFocus } = useKeyboardAutoScroll()

  const handleSubmit = async () => {
    if (!email.trim()) {
      Alert.alert('Missing details', 'Enter the email address your account was created with.')
      return
    }

    setBusy(true)
    try {
      // The server's own sentence is what gets shown, so the wording on this screen
      // cannot drift from the wording on the website's.
      setSentMessage(await forgotPassword(email.trim()))
      setEmail('')
    } catch (e) {
      Alert.alert('Could not send the link', e instanceof Error ? e.message : 'Something went wrong.')
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

        <Text style={styles.title}>
          {sentMessage ? 'Check your email' : 'Reset your password'}
        </Text>

        <View style={styles.card}>
          {sentMessage === null ? (
            <>
              <Text style={styles.note}>
                We send a link that works once and expires in an hour. It never shows
                your old password - it only lets you choose a new one.
              </Text>

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

              <AnimatedButton
                style={[styles.button, busy && styles.button_disabled]}
                onPress={handleSubmit}
                disabled={busy}
                loading={busy}
              >
                <Text style={styles.button_text}>Send me a link</Text>
              </AnimatedButton>

              <TouchableOpacity
                style={styles.link}
                onPress={() => router.push('/resetPassword')}
              >
                <Text style={styles.link_text}>I already have a code from an email</Text>
              </TouchableOpacity>
            </>
          ) : (
            <>
              <Text style={styles.note}>{sentMessage}</Text>
              <Text style={styles.note}>
                Open the link in your email to set a new password. If nothing arrives,
                the address has no Verichat account - create one and start ordering.
              </Text>

              <AnimatedButton style={styles.button} onPress={() => setSentMessage(null)}>
                <Text style={styles.button_text}>Send to a different address</Text>
              </AnimatedButton>

              <TouchableOpacity style={styles.link} onPress={() => router.replace('/clientLogin')}>
                <Text style={styles.link_text}>Back to sign in</Text>
              </TouchableOpacity>
            </>
          )}

          <TouchableOpacity style={styles.back} onPress={() => router.back()}>
            <Text style={styles.back_text}>← Back</Text>
          </TouchableOpacity>
        </View>
        </View>
      </ScrollView>
    </KeyboardAvoidingView>
  )
}

export default ForgotPasswordScreen

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
    gap: 12,
  },
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
  link: { alignItems: 'center', marginTop: 4 },
  link_text: { fontSize: 13, color: CRMTheme.primary, fontWeight: '600' },
  back: { alignItems: 'center', marginTop: 14 },
  back_text: { fontSize: 14, color: '#424242', fontWeight: '600' },
})
