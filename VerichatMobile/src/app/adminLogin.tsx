import { useRouter } from 'expo-router'
import { useState } from 'react'
import {
    Alert, Image, KeyboardAvoidingView, Platform, ScrollView, StyleSheet, Text, TextInput, TouchableOpacity, View,
} from 'react-native'
import { login } from '../constants/authService'

const AdminLoginScreen = () => {
  const router = useRouter()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [role, setRole] = useState('')
  const [busy, setBusy] = useState(false)

  const handleLogin = async () => {
    if (!email.trim() || !password || !role.trim()) {
      Alert.alert('Missing details', 'Enter your email address, password and role to log in.')
      return
    }

    // The API authenticates on email + password only; the role field is
    // display-only (the JWT returned carries the real role).
    setBusy(true)
    try {
      await login(email.trim(), password)
      router.replace('/clientHomescreen')
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
      behavior={Platform.OS === 'ios' ? 'padding' : undefined}
    >
      <ScrollView
        contentContainerStyle={styles.container}
        keyboardShouldPersistTaps="handled"
        showsVerticalScrollIndicator={false}
      >
        <View style={styles.Logo}>
          <Image
            source={require('../../assets/images/canva/design_pt.png')}
            style={styles.logo_image}
          />
        </View>

        <Text style={styles.welcome_text}>Welcome Back!</Text>
        <Text style={styles.title}>Admin Log in</Text>

        <View style={styles.card}>
          <Text style={styles.label}>Email address</Text>
          <TextInput
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
            style={styles.input}
            value={password}
            onChangeText={setPassword}
            secureTextEntry
            autoCapitalize="none"
            autoCorrect={false}
            autoComplete="password"
            textContentType="password"
          />

          <Text style={styles.label}>Role (eg. Sales Rep, Procurement Rep)</Text>
          <TextInput
            style={styles.input}
            value={role}
            onChangeText={setRole}
            autoCapitalize="words"
            autoCorrect={false}
          />

          <TouchableOpacity style={[styles.button, styles.first_button]} onPress={handleLogin}>
            <Text style={styles.button_text}>Log in</Text>
          </TouchableOpacity>

          <TouchableOpacity style={styles.button} onPress={handleSSOLogin}>
            <Text style={styles.button_text}>Log in with SSO</Text>
          </TouchableOpacity>
        </View>
      </ScrollView>
    </KeyboardAvoidingView>
  )
}

export default AdminLoginScreen

const styles = StyleSheet.create({
  flex: {
    flex: 1,
    backgroundColor: '#004e7a',
  },
  container: {
    flexGrow: 1,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: '#004e7a',
    paddingVertical: 40,
  },
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
    backgroundColor: '#0057a8',
    paddingVertical: 14,
    width: 190,
    alignSelf: 'center',
    alignItems: 'center',
    borderRadius: 20,
  },
  first_button: {
    marginTop: 12,
  },
  button_text: {
    fontSize: 16,
    fontWeight: 'bold',
    color: '#ffffff',
  },
})