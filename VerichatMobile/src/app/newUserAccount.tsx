import { useState } from 'react'
import {
    Alert, Image, KeyboardAvoidingView, Platform, ScrollView, StyleSheet, Text, TextInput, TouchableOpacity, View,
} from 'react-native'

const CreateAccountScreen = () => {
  const [fullName, setFullName] = useState('')
  const [companyName, setCompanyName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')

  const handleSignUp = () => {
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

    // TODO: call your sign up endpoint here with
    // { fullName, companyName, email, password },
    // then navigate to the next screen on success.
    console.log('Sign up:', fullName.trim(), companyName.trim(), email.trim())
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

        <Text style={styles.title}>Create Account</Text>

        <View style={styles.form}>
          <Text style={styles.label}>Full Name</Text>
          <TextInput
            style={styles.input}
            value={fullName}
            onChangeText={setFullName}
            autoCapitalize="words"
            autoComplete="name"
            textContentType="name"
          />

          <Text style={styles.label}>Company Name</Text>
          <TextInput
            style={styles.input}
            value={companyName}
            onChangeText={setCompanyName}
            autoCapitalize="words"
            textContentType="organizationName"
          />

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
            autoComplete="new-password"
            textContentType="newPassword"
          />

          <Text style={styles.label}>Confirm Password</Text>
          <TextInput
            style={styles.input}
            value={confirmPassword}
            onChangeText={setConfirmPassword}
            secureTextEntry
            autoCapitalize="none"
            autoCorrect={false}
            autoComplete="new-password"
            textContentType="newPassword"
          />

          <TouchableOpacity style={styles.button} onPress={handleSignUp}>
            <Text style={styles.button_text}>Sign up</Text>
          </TouchableOpacity>
        </View>
      </ScrollView>
    </KeyboardAvoidingView>
  )
}

export default CreateAccountScreen

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
    backgroundColor: '#0057a8',
    paddingVertical: 14,
    width: 170,
    alignSelf: 'center',
    alignItems: 'center',
    borderRadius: 20,
    borderWidth: 1,
    borderColor: '#9e9e9e',
    marginTop: 24,
  },
  button_text: {
    fontSize: 16,
    fontWeight: 'bold',
    color: '#ffffff',
  },
})