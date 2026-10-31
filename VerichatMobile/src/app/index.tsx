import { CRMTheme } from '../constants/crmTheme'
import { useRouter } from 'expo-router'
import { useEffect, useState } from 'react'
import { ActivityIndicator, Image, StyleSheet, Text, TouchableOpacity, View } from 'react-native'
import { getAuthRole, getAuthToken, getBiometricLoginEnabled, isStaffMobileRole, setAuthToken, subscribeAuth } from '../constants/api'
import { authenticateBiometric, hasUsableBiometrics } from '../constants/biometricAuth'
import ClientHomeScreen from './clientHomescreen'
import DashboardScreen from './DashboardScreen'

/**
 * Auth gate: when a JWT is present the Home tab renders the client dashboard;
 * otherwise it shows the role chooser for signing in / registering.
 */
const HomeScreen = () => {
  const router = useRouter()
  const [signedIn, setSignedIn] = useState(() => getAuthToken() !== null)
  const [checkingBiometrics, setCheckingBiometrics] = useState(false)
  const [biometricFailed, setBiometricFailed] = useState(false)

  useEffect(() => {
    return subscribeAuth((token) => setSignedIn(token !== null))
  }, [])

  useEffect(() => {
    let mounted = true
    const tryBiometricLogin = async () => {
      if (!mounted || signedIn) return

      try {
        const biometricEnabled = await getBiometricLoginEnabled()
        if (!biometricEnabled) return

        const canUse = await hasUsableBiometrics()
        if (!canUse) return

        setCheckingBiometrics(true)
        const success = await authenticateBiometric('Sign in with biometrics')
        if (!mounted) return

        if (success) {
          const token = getAuthToken()
          if (token) {
            setSignedIn(true)
          } else {
            setBiometricFailed(true)
          }
        } else {
          setBiometricFailed(true)
        }
      } catch {
        // Biometric not available or cancelled - fall back to normal login
      } finally {
        if (mounted) setCheckingBiometrics(false)
      }
    }

    void tryBiometricLogin()
    return () => { mounted = false }
  }, [])

  const handleBiometricRetry = async () => {
    setBiometricFailed(false)
    setCheckingBiometrics(true)
    try {
      const success = await authenticateBiometric('Sign in with biometrics')
      if (success) {
        const token = getAuthToken()
        if (token) setSignedIn(true)
        else setBiometricFailed(true)
      } else {
        setBiometricFailed(true)
      }
    } catch {
      setBiometricFailed(true)
    } finally {
      setCheckingBiometrics(false)
    }
  }

  if (checkingBiometrics) {
    return (
      <View style={styles.biometricContainer}>
        <ActivityIndicator size="large" color="#ffffff" />
        <Text style={styles.biometricText}>Signing in with biometrics...</Text>
      </View>
    )
  }

  if (biometricFailed) {
    return (
      <View style={styles.biometricFailedContainer}>
        <Text style={styles.biometricFailedText}>Biometric sign-in failed</Text>
        <TouchableOpacity style={styles.retryButton} onPress={handleBiometricRetry}>
          <Text style={styles.retryButtonText}>Try Again</Text>
        </TouchableOpacity>
        <TouchableOpacity style={styles.backToLoginButton} onPress={() => setBiometricFailed(false)}>
          <Text style={styles.backToLoginButtonText}>Use Password Instead</Text>
        </TouchableOpacity>
      </View>
    )
  }

  if (signedIn) {
    return isStaffMobileRole(getAuthRole()) ? <DashboardScreen /> : <ClientHomeScreen />
  }

  return (
    <View style={styles.container}>

      <View style={styles.Logo}>
          <Image source={require('../../assets/images/canva/design_pt.png')} style={styles.logo_image} />
      </View>

      <View style={styles.login_section}>

        <Text style={styles.user_login}>Choose your account</Text>
        <TouchableOpacity style={styles.button} onPress={() => router.push('/clientLogin')}>
          <Text style={styles.button_text}>Client</Text>
        </TouchableOpacity>
        <TouchableOpacity style={styles.button} onPress={() => router.push('/adminLogin')}>
          <Text style={styles.button_text}>Staff / Admin</Text>
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
  biometricContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: CRMTheme.primaryDark,
    gap: 16,
  },
  biometricText: {
    color: '#ffffff',
    fontSize: 16,
    fontWeight: '600',
  },
  biometricFailedContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: CRMTheme.primaryDark,
    gap: 12,
    paddingHorizontal: 32,
  },
  biometricFailedText: {
    color: '#ffffff',
    fontSize: 16,
    fontWeight: '600',
    textAlign: 'center',
  },
  retryButton: {
    backgroundColor: '#ffffff',
    paddingVertical: 12,
    paddingHorizontal: 24,
    borderRadius: 8,
  },
  retryButtonText: {
    color: CRMTheme.primaryDark,
    fontSize: 16,
    fontWeight: 'bold',
  },
  backToLoginButton: {
    marginTop: 12,
  },
  backToLoginButtonText: {
    color: 'rgba(255,255,255,0.8)',
    fontSize: 14,
  },
  container: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: CRMTheme.primaryDark,
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

