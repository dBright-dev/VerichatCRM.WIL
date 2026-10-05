import { useRouter } from 'expo-router'
import { useEffect, useState } from 'react'
import { Image, StyleSheet, Text, TouchableOpacity, View } from 'react-native'
import { getAuthToken, subscribeAuth } from '../constants/api'
import ClientHomeScreen from './clientHomescreen'

/**
 * Auth gate: when a JWT is present the Home tab renders the client dashboard;
 * otherwise it shows the role chooser for signing in / registering.
 */
const HomeScreen = () => {
  const router = useRouter()
  const [signedIn, setSignedIn] = useState(() => getAuthToken() !== null)

  useEffect(() => {
    return subscribeAuth((token) => setSignedIn(token !== null))
  }, [])

  if (signedIn) {
    return <ClientHomeScreen />
  }

  return (
    <View style={styles.container}>

      <View style={styles.Logo}>
          <Image source={require('../../assets/images/canva/design_pt.png')} style={styles.logo_image} />
      </View>

      <View style={styles.login_section}>

        <Text style={styles.user_login}>Log in As:</Text>
        <TouchableOpacity style={styles.button} onPress={() => router.push('/clientLogin')}>
          <Text style={styles.button_text}>Client</Text>
        </TouchableOpacity>
        <TouchableOpacity style={styles.button} onPress={() => router.push('/adminLogin')}>
          <Text style={styles.button_text}>Admin</Text>
        </TouchableOpacity>
        <TouchableOpacity style={styles.button} onPress={() => router.push('/newUserAccount')}>
          <Text style={styles.button_text}>New User</Text>
        </TouchableOpacity>

      </View>
    </View>
  )
}

export default HomeScreen

const styles = StyleSheet.create({
  container: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: '#004e7a',
  },
  Logo: {
    marginBottom: 50,
  },
  user_login: {
    fontSize: 18,
    color: '#ffffff',
    fontWeight: 700,
  },

  button: {
    backgroundColor: '#ffffff',

    padding: 20,
    width: 160,
    alignItems: 'center',
    borderRadius: 15,
  },
  login_section: {
    alignItems: 'center',
    gap: 20,
  },
  button_text: {
    fontSize: 16,
    fontWeight: 'bold',
  },
  logo_image: {
    width: 200,
    height: 200,
    resizeMode: 'contain',
  },
})
