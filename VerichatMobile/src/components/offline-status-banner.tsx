import NetInfo, { NetInfoState } from '@react-native-community/netinfo'
import { useEffect, useState } from 'react'
import { StyleSheet, Text, View } from 'react-native'
import { getPendingActionCount, subscribePendingActionCount } from '../constants/api'

function isOffline(state: NetInfoState): boolean {
  return state.isConnected === false || state.isInternetReachable === false
}

export default function OfflineStatusBanner() {
  const [offline, setOffline] = useState(false)
  const [pendingCount, setPendingCount] = useState(0)

  useEffect(() => {
    const refresh = (state: NetInfoState) => {
      setOffline(isOffline(state))
      void getPendingActionCount().then(setPendingCount)
      if (!isOffline(state)) {
        setTimeout(() => void getPendingActionCount().then(setPendingCount), 1500)
      }
    }
    void NetInfo.fetch().then(refresh)
    const unsubscribeNetwork = NetInfo.addEventListener(refresh)
    const unsubscribeQueue = subscribePendingActionCount(setPendingCount)
    return () => {
      unsubscribeNetwork()
      unsubscribeQueue()
    }
  }, [])

  if (!offline && pendingCount === 0) return null

  return (
    <View style={[styles.banner, offline ? styles.offline : styles.syncing]} accessibilityLiveRegion="polite">
      <Text style={styles.text}>
        {offline
          ? pendingCount > 0
            ? `Offline · ${pendingCount} change${pendingCount === 1 ? '' : 's'} saved to sync later`
            : 'Offline · showing saved information'
          : `Syncing ${pendingCount} saved change${pendingCount === 1 ? '' : 's'}…`}
      </Text>
    </View>
  )
}

const styles = StyleSheet.create({
  banner: { paddingHorizontal: 14, paddingVertical: 8 },
  offline: { backgroundColor: '#8a4b08' },
  syncing: { backgroundColor: '#0057a8' },
  text: { color: '#fff', textAlign: 'center', fontSize: 12, fontWeight: '700' },
})
