import { useEffect } from 'react'
import { StyleSheet, View } from 'react-native'
import { LinearGradient } from 'expo-linear-gradient'
import Animated, { useAnimatedStyle, useSharedValue, withRepeat, withTiming } from 'react-native-reanimated'

export default function SkeletonCard() {
  const sweep = useSharedValue(-1)
  useEffect(() => { sweep.value = withRepeat(withTiming(1, { duration: 1400 }), -1, false) }, [sweep])
  const shimmerStyle = useAnimatedStyle(() => ({ transform: [{ translateX: sweep.value * 320 }] }))
  return (
    <View style={styles.card} accessibilityLabel="Loading content">
      <View style={styles.title} />
      <View style={styles.line} />
      <View style={styles.shortLine} />
      <Animated.View pointerEvents="none" style={[styles.shimmer, shimmerStyle]}>
        <LinearGradient colors={['transparent', 'rgba(255,255,255,0.7)', 'transparent']} start={{ x: 0, y: 0 }} end={{ x: 1, y: 0 }} style={StyleSheet.absoluteFill} />
      </Animated.View>
    </View>
  )
}

const styles = StyleSheet.create({
  card: { overflow: 'hidden', position: 'relative', minHeight: 112, marginHorizontal: 16, marginVertical: 7, padding: 16, borderRadius: 14, backgroundColor: '#e8edf2', gap: 12 },
  title: { height: 15, width: '55%', borderRadius: 8, backgroundColor: '#d3dbe4' },
  line: { height: 11, width: '92%', borderRadius: 6, backgroundColor: '#d3dbe4' },
  shortLine: { height: 11, width: '68%', borderRadius: 6, backgroundColor: '#d3dbe4' },
  shimmer: { position: 'absolute', top: 0, bottom: 0, left: 0, width: '65%' },
})
