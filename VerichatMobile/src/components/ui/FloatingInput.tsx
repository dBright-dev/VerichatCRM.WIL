import { useEffect } from 'react'
import { TextInput, TextInputProps, StyleProp, ViewStyle } from 'react-native'
import type { RefObject } from 'react'
import Animated, { interpolateColor, useAnimatedStyle, useSharedValue, withSequence, withTiming } from 'react-native-reanimated'

type Props = TextInputProps & {
  label: string
  error?: string
  containerStyle?: StyleProp<ViewStyle>
  inputRef?: RefObject<TextInput | null>
}

export default function FloatingInput({ label, error, value, defaultValue, onFocus, onBlur, containerStyle, style, inputRef, ...props }: Props) {
  const focus = useSharedValue(0)
  const shake = useSharedValue(0)
  useEffect(() => {
    if (error) shake.value = withSequence(withTiming(-7, { duration: 45 }), withTiming(7, { duration: 45 }), withTiming(-5, { duration: 45 }), withTiming(5, { duration: 45 }), withTiming(0, { duration: 45 }))
  }, [error, shake])

  const borderStyle = useAnimatedStyle(() => ({
    borderColor: interpolateColor(focus.value, [0, 1], [error ? '#d32f2f' : '#aab2b9', error ? '#d32f2f' : '#00a9c7']),
    transform: [{ translateX: shake.value }],
  }))
  const labelStyle = useAnimatedStyle(() => {
    const isRaised = focus.value > 0 || Boolean(value || defaultValue)
    return { transform: [{ translateY: withTiming(isRaised ? -15 : 0, { duration: 160 }) }, { scale: withTiming(isRaised ? 0.78 : 1, { duration: 160 }) }] }
  }, [value, defaultValue])
  const handleFocus: TextInputProps['onFocus'] = (event) => {
    // eslint-disable-next-line react-hooks/immutability
    focus.value = withTiming(1)
    onFocus?.(event)
  }
  const handleBlur: TextInputProps['onBlur'] = (event) => {
    // eslint-disable-next-line react-hooks/immutability
    focus.value = withTiming(0)
    onBlur?.(event)
  }

  return (
    <Animated.View style={[{ minHeight: 58, justifyContent: 'center', borderWidth: 1.5, borderRadius: 10, paddingHorizontal: 12, marginBottom: 14, backgroundColor: '#fff' }, borderStyle, containerStyle]}>
      <Animated.Text pointerEvents="none" style={[{ position: 'absolute', left: 13, top: 17, color: '#657285', zIndex: 1, transformOrigin: 'left center' }, labelStyle]}>{label}</Animated.Text>
      <TextInput
        ref={inputRef}
        {...props}
        value={value}
        defaultValue={defaultValue}
        onFocus={handleFocus}
        onBlur={handleBlur}
        placeholder=""
        style={[{ minHeight: 54, color: '#172b4d', paddingTop: 19, paddingHorizontal: 0 }, style]}
      />
    </Animated.View>
  )
}
