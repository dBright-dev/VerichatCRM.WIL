import { CRMTheme } from '../constants/crmTheme'
import { useRouter } from 'expo-router'
import { useRef, useState } from 'react'
import {
    Alert, Image, KeyboardAvoidingView, Platform, ScrollView, StyleSheet, Text, TextInput, TouchableOpacity, View,
} from 'react-native'
import { registerClient } from '../constants/authService'
import { useKeyboardAutoScroll } from '../hooks/useKeyboardAutoScroll'

const CreateAccountScreen = () => {
  const router = useRouter()
  const [fullName, setFullName] = useState('')
  const [companyName, setCompanyName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [busy, setBusy] = useState(false)
  const fullNameInput = useRef<TextInput>(null)
  const companyInput = useRef<TextInput>(null)
  const emailInput = useRef<TextInput>(null)
  const passwordInput = useRef<TextInput>(null)
  const confirmPasswordInput = useRef<TextInput>(null)
  const { scrollRef, contentRef, onFieldFocus } = useKeyboardAutoScroll()

  const handleSignUp = async () => {
    if (
      !fullName.trim() ||
      !companyName.trim() ||
      !email.trim() ||
      !password ||
      !confirmPassword
    ) {
      Alert.alert('Missing details', 'Please fill in all the fields to create your account.')
      return
    }

    if (password !== confirmPassword) {
      Alert.alert('Passwords do not match', 'Make sure both password fields are the same.')
      return
    }

    // Registers a "Client User" account against the CRM API and stores the
    // returned JWT so the rest of the app is authenticated.
    setBusy(true)
    try {
      await registerClient({
        displayName: fullName.trim(),
        email: email.trim(),
        password,
        companyName: companyName.trim(),
      })
      router.replace('/')
    } catch (e) {
      Alert.alert('Sign up failed', e instanceof Error ? e.message : 'Something went wrong.')
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
            source={require('../../assets/images/canva/design_pt.png')}
            style={styles.logo_image}
          />
        </View>

        <Text style={styles.title}>Create Account</Text>

        <View style={styles.form}>
          <Text style={styles.label}>Full Name</Text>
          <TextInput
            ref={fullNameInput}
            onFocus={() => onFieldFocus(fullNameInput.current)}
            style={styles.input}
            value={fullName}
            onChangeText={setFullName}
            autoCapitalize="words"
            autoComplete="name"
            textContentType="name"
          />

          <Text style={styles.label}>Company Name</Text>
          <TextInput
            ref={companyInput}
            onFocus={() => onFieldFocus(companyInput.current)}
            style={styles.input}
            value={companyName}
            onChangeText={setCompanyName}
            autoCapitalize="words"
            textContentType="organizationName"
          />

          <Text style={styles.label}>Email address</Text>
          <TextInput
            ref={emailInput}
            onFocus={() => onFieldFocus(emailInput.current)}
            style={styles.input}
            value={email}
            onChangeText={setEmail}
            keyboardType="email-address"
            autoCapitalize="none"
            autoCorrect={false}
            autoComplete="email"
            textContentType="emailAddress"
          />

          <Text style={styles.label}>Password</Text>
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

          <Text style={styles.label}>Confirm Password</Text>
          <TextInput
            ref={confirmPasswordInput}
            onFocus={() => onFieldFocus(confirmPasswordInput.current)}
            style={styles.input}
            value={confirmPassword}
            onChangeText={setConfirmPassword}
            secureTextEntry
            autoCapitalize="none"
            autoCorrect={false}
            autoComplete="new-password"
            textContentType="newPassword"
          />

          <TouchableOpacity
            style={[styles.button, busy && styles.button_disabled]}
            onPress={handleSignUp}
            disabled={busy}
          >
            <Text style={styles.button_text}>{busy ? 'Creating…' : 'Sign up'}</Text>
          </TouchableOpacity>
        </View>
        </View>
      </ScrollView>
    </KeyboardAvoidingView>
  )
}

export default CreateAccountScreen

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
    marginBottom: 10,
  },
  logo_image: {
    width: 150,
    height: 150,
    resizeMode: 'contain',
  },
  title: {
    fontSize: 30,
    color: '#ffffff',
    fontWeight: '900',
    marginBottom: 24,
  },
  form: {
    width: '88%',
    maxWidth: 380,
    gap: 6,
  },
  label: {
    fontSize: 13,
    fontWeight: 'bold',
    color: '#ffffff',
    marginLeft: 8,
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
    marginBottom: 10,
  },
  button: {
    backgroundColor: CRMTheme.primary,
    paddingVertical: 14,
    width: 170,
    alignSelf: 'center',
    alignItems: 'center',
    borderRadius: 20,
    borderWidth: 1,
    borderColor: '#9e9e9e',
    marginTop: 24,
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
