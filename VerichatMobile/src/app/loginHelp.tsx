import { CRMTheme } from '../constants/crmTheme'
import { useRouter } from 'expo-router'
import { useRef, useState } from 'react'
import {
    Alert, Image, KeyboardAvoidingView, Platform, ScrollView, StyleSheet, Text, TextInput, TouchableOpacity, View,
} from 'react-native'
import { askForHelp } from '../constants/authService'
import FloatingInput from '../components/ui/FloatingInput'
import AnimatedButton from '../components/ui/AnimatedButton'
import { useKeyboardAutoScroll } from '../hooks/useKeyboardAutoScroll'

/**
 * The way out of a sign-in screen that will not let somebody through, for the cases a
 * reset link cannot reach: an account that was switched off, an account that exists and
 * does not behave, an address typed one letter wrong at sign-up.
 *
 * It is deliberately not a support ticket. There is no thread and no status to follow -
 * the note goes to every System Administrator by email and lands on their Sign-in Help
 * queue, and the reply comes back to the address typed into this form, which is the only
 * channel the sender has proved they can read.
 */
const LoginHelpScreen = () => {
  const router = useRouter()
  const [name, setName] = useState('')
  const [email, setEmail] = useState('')
  const [expectedRole, setExpectedRole] = useState('')
  const [message, setMessage] = useState('')
  const [busy, setBusy] = useState(false)
  const [sentMessage, setSentMessage] = useState<string | null>(null)
  const emailInput = useRef<TextInput>(null)
  const messageInput = useRef<TextInput>(null)
  const { scrollRef, contentRef, onFieldFocus } = useKeyboardAutoScroll()

  const handleSubmit = async () => {
    if (!name.trim() || !email.trim()) {
      Alert.alert('Missing details', 'An administrator needs a name and an address to answer.')
      return
    }
    if (message.trim().length < 10) {
      Alert.alert('Say a little more', 'What you tried, and what the screen said when you tried it.')
      return
    }

    setBusy(true)
    try {
      // The server's own sentence, so this screen and the website's cannot drift apart.
      setSentMessage(await askForHelp({
        name: name.trim(),
        email: email.trim(),
        expectedRole: expectedRole.trim() || undefined,
        message: message.trim(),
      }))
      setMessage('')
    } catch (e) {
      Alert.alert('Could not send the note', e instanceof Error ? e.message : 'Something went wrong.')
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
          {sentMessage ? 'Note sent' : 'Cannot sign in?'}
        </Text>

        <View style={styles.card}>
          {sentMessage === null ? (
            <>
              <Text style={styles.note}>
                A reset link is no help if the account itself is the problem. This goes
                to a Verichat administrator, who can see the user list and the roles on
                it - the place a locked-out account is actually fixed.
              </Text>

              <FloatingInput
                label="Your name"
                value={name}
                onChangeText={setName}
                autoCapitalize="words"
                autoComplete="name"
                textContentType="name"
              />

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
                label="Role you signed up as (optional)"
                containerStyle={{ marginTop: 14, marginBottom: 0 }}
                value={expectedRole}
                onChangeText={setExpectedRole}
                autoCapitalize="words"
              />

              <FloatingInput
                label="What is going wrong"
                containerStyle={{ marginTop: 14, marginBottom: 0, minHeight: 96 }}
                style={{ minHeight: 78, paddingTop: 22, textAlignVertical: 'top' }}
                multiline
                maxLength={2000}
                value={message}
                onChangeText={setMessage}
                inputRef={messageInput}
                onFocus={() => onFieldFocus(messageInput.current)}
              />

              <AnimatedButton
                style={[styles.button, busy && styles.button_disabled]}
                onPress={handleSubmit}
                disabled={busy}
                loading={busy}
              >
                <Text style={styles.button_text}>Send to an administrator</Text>
              </AnimatedButton>

              <TouchableOpacity
                style={styles.link}
                onPress={() => router.push('/forgotPassword')}
              >
                <Text style={styles.link_text}>I only need a new password</Text>
              </TouchableOpacity>
            </>
          ) : (
            <>
              <Text style={styles.note}>{sentMessage}</Text>
              <Text style={styles.note}>
                Keep the app up to date and try again in a little while. Nothing here
                changes your account on its own - an administrator reads it first.
              </Text>

              <AnimatedButton style={styles.button} onPress={() => setSentMessage(null)}>
                <Text style={styles.button_text}>Send another note</Text>
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

export default LoginHelpScreen

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
    width: 240,
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
