import { useEffect } from 'react'
import { StyleSheet, Text, View } from 'react-native'
import { Image } from 'expo-image'
import Animated, { useAnimatedStyle, useSharedValue, withRepeat, withSequence, withTiming } from 'react-native-reanimated'

type Props = { label?: string; compact?: boolean }

export default function BrandLoader({ label = 'Loading…', compact = false }: Props) {
  const pulse = useSharedValue(1)
  useEffect(() => {
    pulse.value = withRepeat(withSequence(withTiming(1.07, { duration: 650 }), withTiming(0.96, { duration: 650 })), -1, true)
  }, [pulse])
  const animatedStyle = useAnimatedStyle(() => ({ transform: [{ scale: pulse.value }] }))
  return (
    <View style={styles.container} accessibilityRole="progressbar" accessibilityLabel={label}>
      <Animated.View style={[styles.logoBack, compact && styles.compactBack, animatedStyle]}>
        <Image source={require('../../../assets/images/verichat-loader-mark.png')} contentFit="contain" style={[styles.logo, compact && styles.compactLogo]} />
      </Animated.View>
      {!!label && <Text style={[styles.label, compact && styles.compactLabel]}>{label}</Text>}
    </View>
  )
}

const styles = StyleSheet.create({
  container: { flex: 1, minHeight: 120, alignItems: 'center', justifyContent: 'center', gap: 12 },
  logoBack: { width: 170, height: 120, alignItems: 'center', justifyContent: 'center' },
  logo: { width: 156, height: 108 },
  label: { color: '#53657b', fontSize: 14, fontWeight: '600' },
  compactBack: { width: 52, height: 38 },
  compactLogo: { width: 48, height: 34 },
  compactLabel: { fontSize: 12 },
})
