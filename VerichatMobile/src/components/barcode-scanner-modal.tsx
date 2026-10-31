import { CameraView, useCameraPermissions } from 'expo-camera'
import { useEffect, useRef } from 'react'
import { ActivityIndicator, Modal, StyleSheet, Text, TouchableOpacity, View } from 'react-native'
import { useSafeAreaInsets } from 'react-native-safe-area-context'

type Props = {
  visible: boolean
  title: string
  onClose: () => void
  onScanned: (value: string) => void
}

const BARCODE_TYPES = [
  'aztec', 'ean13', 'ean8', 'qr', 'pdf417', 'upc_e', 'datamatrix',
  'code39', 'code93', 'itf14', 'codabar', 'code128', 'upc_a',
] as const

export default function BarcodeScannerModal({ visible, title, onClose, onScanned }: Props) {
  const insets = useSafeAreaInsets()
  const [permission, requestPermission] = useCameraPermissions()
  const hasScanned = useRef(false)

  useEffect(() => {
    if (visible) hasScanned.current = false
  }, [visible])

  const handleScan = ({ data }: { data: string }) => {
    if (hasScanned.current || !data.trim()) return
    hasScanned.current = true
    onScanned(data.trim())
    onClose()
  }

  return (
    <Modal visible={visible} animationType="slide" onRequestClose={onClose}>
      <View style={styles.container}>
        {permission?.granted ? (
          <CameraView
            style={StyleSheet.absoluteFill}
            facing="back"
            barcodeScannerSettings={{ barcodeTypes: [...BARCODE_TYPES] }}
            onBarcodeScanned={handleScan}
          />
        ) : (
          <View style={styles.permissionPanel}>
            {!permission ? <ActivityIndicator color="#fff" /> : <>
              <Text style={styles.permissionTitle}>Camera access is needed to scan codes</Text>
              <TouchableOpacity style={styles.permissionButton} onPress={() => void requestPermission()}>
                <Text style={styles.permissionButtonText}>Allow camera access</Text>
              </TouchableOpacity>
            </>}
          </View>
        )}
        <View style={[styles.topBar, { paddingTop: insets.top + 10 }]}>
          <Text style={styles.title}>{title}</Text>
          <TouchableOpacity style={styles.closeButton} onPress={onClose} accessibilityRole="button" accessibilityLabel="Close scanner">
            <Text style={styles.closeText}>Close</Text>
          </TouchableOpacity>
        </View>
        {permission?.granted && <View pointerEvents="none" style={styles.scanFrame}><View style={styles.frame} /><Text style={styles.hint}>Line up a barcode or QR code</Text></View>}
      </View>
    </Modal>
  )
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#101820' },
  permissionPanel: { flex: 1, alignItems: 'center', justifyContent: 'center', padding: 28, gap: 18 },
  permissionTitle: { color: '#fff', fontSize: 18, fontWeight: '700', textAlign: 'center' },
  permissionButton: { backgroundColor: '#fff', paddingVertical: 12, paddingHorizontal: 18, borderRadius: 10 },
  permissionButtonText: { color: '#123', fontWeight: '700' },
  topBar: { position: 'absolute', left: 0, right: 0, paddingHorizontal: 18, paddingBottom: 12, flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', backgroundColor: '#0008' },
  title: { color: '#fff', fontWeight: '800', fontSize: 17 },
  closeButton: { backgroundColor: '#0009', borderRadius: 9, paddingHorizontal: 14, paddingVertical: 9 },
  closeText: { color: '#fff', fontWeight: '700' },
  scanFrame: { position: 'absolute', top: '38%', alignSelf: 'center', alignItems: 'center', gap: 16 },
  frame: { width: 260, height: 180, borderWidth: 3, borderColor: '#fff', borderRadius: 18, backgroundColor: 'transparent' },
  hint: { color: '#fff', backgroundColor: '#0009', overflow: 'hidden', borderRadius: 8, paddingHorizontal: 12, paddingVertical: 8 },
})
