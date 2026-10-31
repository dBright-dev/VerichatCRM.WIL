import { useCallback, useEffect, useRef } from 'react'
import {
  Keyboard,
  ScrollView,
  TextInput,
  View,
} from 'react-native'

/** Scrolls a focused input above the keyboard on compact screens. */
export function useKeyboardAutoScroll() {
  const scrollRef = useRef<ScrollView>(null)
  const contentRef = useRef<View>(null)
  const keyboardVisible = useRef(false)

  useEffect(() => {
    const show = Keyboard.addListener('keyboardDidShow', () => {
      keyboardVisible.current = true
    })
    const hide = Keyboard.addListener('keyboardDidHide', () => {
      keyboardVisible.current = false
    })
    return () => {
      show.remove()
      hide.remove()
    }
  }, [])

  const onFieldFocus = useCallback((input: TextInput | null) => {
    if (!input) return

    // Wait until the keyboard and KeyboardAvoidingView have finished resizing.
    setTimeout(() => {
      if (!keyboardVisible.current || !contentRef.current) return
      input.measureLayout(
        contentRef.current,
        (_x, y) => {
          // Keep the active field near the top, leaving room to see what is typed.
          scrollRef.current?.scrollTo({ y: Math.max(y - 96, 0), animated: true })
        },
        () => {
          // A failed measurement is harmless; the user can still scroll manually.
        },
      )
    }, 250)
  }, [])

  return {
    scrollRef,
    contentRef,
    onFieldFocus,
  }
}
