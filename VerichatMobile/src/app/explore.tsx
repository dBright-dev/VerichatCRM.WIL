import { useState } from 'react'
import { Alert, Image, KeyboardAvoidingView, Platform, ScrollView, StyleSheet, Text, TextInput, View } from 'react-native'

const ClientLoginPage = () => {
  const[email, setEmail] = useState('') 
  const[password, setPassword] = useState('') 

  const handleLogin = () => {
    // Handle login logic here
    if (!email.trim() || !password) {
    Alert.alert('Missing details', 'Enter your email address and password to log in.')
    return
    }

    //TODO: call your authentication endpoint here, then navigate to client dashboard successfully.
    console.log('Client login:', email.trim())
  }
  return (
    <KeyboardAvoidingView
      style={{flex: 1}}
      behavior={Platform.OS === 'ios' ? 'padding' : 'height'} //change to undefined
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
        <Text style={styles.title}>Client Log In</Text>
        <View style={styles.card}>
        
        <Text style={styles.label}>Email address</Text>
        <TextInput
        style={styles.input}
        value={email}
        onChangeText={setEmail}
        keyboardType="email.address"
        autoCapitalize="none"
        autoCorrect={false}
        autoComplete="email"
        placeholder="Enter your email address"
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
        placeholder="Enter your password"
        textContentType="password"
        />
        </View>
      </ScrollView>
      {/* Your login form components go here */}
    </KeyboardAvoidingView>
  )
  
  
  
  
  
}



const styles = StyleSheet.create({
  container: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: '#004e7a',
  },
  text: {
    fontSize: 18,
    color: '#ffffff',
    fontWeight: '700',
  },
  Logo: {
    marginBottom: 50,
  },
  logo_image: {
    width: 200,
    height: 200,
    resizeMode: 'contain',
  },
})