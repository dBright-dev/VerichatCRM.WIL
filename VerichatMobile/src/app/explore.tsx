import { useState } from 'react'
import { Alert, Image, KeyboardAvoidingView, Platform, ScrollView, StyleSheet, Text, TextInput, TouchableOpacity, View } from 'react-native'

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
        keyboardType="email-address"
        autoCapitalize="none"
        autoCorrect={false}
        autoComplete="email"
        placeholder="Enter your email address" //display faint text to guide the user
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

        <TouchableOpacity style={styles.button} onPress={handleLogin}>
          <Text style={styles.button_text}>Log In</Text>
        </TouchableOpacity>
        </View>
      </ScrollView>
      {/* Your login form components go here */}
    </KeyboardAvoidingView>
  )
  
}

export default ClientLoginPage

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
    fontStyle:'italic',
    fontWeight: '700',
    fontFamily: Platform.select({ ios: 'Georgia', android: 'serif', default: 'serif'}),
    marginBottom: 20,
  },
  title: {
    fontSize: 22,
    color: '#ffffff',
    fontWeight: '700',
    marginBottom: 30,
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
    width: 170,
    alignSelf: 'center',
    alignItems: 'center',
    borderRadius: 20,
    marginTop: 16,
  },
  button_text: {
    fontSize: 16,
    fontWeight: 'bold',
    color: '#ffffff',
    textAlign: 'center', //align text to the center of the button
  }
})