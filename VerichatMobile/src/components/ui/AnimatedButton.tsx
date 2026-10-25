import { ReactNode, useEffect } from 'react'
import { ActivityIndicator, Pressable, PressableProps, StyleProp, ViewStyle } from 'react-native'
import { LinearGradient } from 'expo-linear-gradient'
import Animated, { useAnimatedStyle, useSharedValue, withRepeat, withSpring, withTiming } from 'react-native-reanimated'

type Props = Omit<PressableProps, 'style' | 'children'> & {
  style?: StyleProp<ViewStyle>
  loading?: boolean
  shimmer?: boolean
  children: ReactNode
}

export default function AnimatedButton({ style, loading = false, shimmer = false, disabled, onPressIn, onPressOut, children, ...props }: Props) {
  const scale = useSharedValue(1)
  const sweep = useSharedValue(-1)
  useEffect(() => {
    if (shimmer) sweep.value = withRepeat(withTiming(1, { duration: 2200 }), -1, false)
  }, [shimmer, sweep])
  const animatedStyle = useAnimatedStyle(() => ({ transform: [{ scale: scale.value }] }))
  const shimmerStyle = useAnimatedStyle(() => ({ transform: [{ translateX: sweep.value * 280 }] }))
  const handlePressIn: PressableProps['onPressIn'] = (event) => {
    // Reanimated shared values are intended to be updated from event handlers.
    // eslint-disable-next-line react-hooks/immutability
    scale.value = withSpring(0.95, { damping: 16, stiffness: 260 })
    onPressIn?.(event)
  }
  const handlePressOut: PressableProps['onPressOut'] = (event) => {
    // eslint-disable-next-line react-hooks/immutability
    scale.value = withSpring(1, { damping: 12, stiffness: 220 })
    onPressOut?.(event)
  }

  return (
    <Animated.View style={animatedStyle}>
      <Pressable
        {...props}
        disabled={disabled || loading}
        onPressIn={handlePressIn}
        onPressOut={handlePressOut}
        android_ripple={{ color: 'rgba(255,255,255,0.18)' }}
        style={style}
      >
        {loading ? <ActivityIndicator color="#ffffff" size="small" /> : children}
        {shimmer && <Animated.View pointerEvents="none" style={[{ position: 'absolute', top: 0, bottom: 0, left: 0, width: '45%' }, shimmerStyle]}><LinearGradient colors={['transparent', 'rgba(255,255,255,0.22)', 'transparent']} start={{ x: 0, y: 0 }} end={{ x: 1, y: 0 }} style={{ flex: 1 }} /></Animated.View>}
      </Pressable>
    </Animated.View>
  )
}
