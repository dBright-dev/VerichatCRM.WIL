import { StyleSheet, Text, View } from 'react-native'
import { CRMTheme } from '../constants/crmTheme'

/**
 * Shown instead of the app when a release build resolves the API to a cleartext
 * origin, which it is not permitted to use. Replaces a module-scope throw that
 * closed the app before any screen existed.
 */
export default function ReleaseOriginScreen({ detail }: { detail: string }) {
  return (
    <View style={styles.wrap} accessibilityRole="alert">
      <Text style={styles.title}>Verichat cannot reach its server</Text>
      <Text style={styles.body}>{detail}</Text>
      <Text style={styles.fix}>
        This is a build setting, not a problem with your account or your device. Whoever
        published this build needs to set EXPO_PUBLIC_API_URL to an HTTPS API origin and
        build again — see ‘Publish a release’ in VerichatMobile/README.md.
      </Text>
    </View>
  )
}

const styles = StyleSheet.create({
  wrap: {
    flex: 1,
    backgroundColor: CRMTheme.ink,
    justifyContent: 'center',
    padding: 28,
    gap: 14,
  },
  title: { color: '#EDF1F7', fontSize: 20, fontWeight: '700' },
  body: { color: '#9AA6B8', fontSize: 15, lineHeight: 22 },
  fix: { color: CRMTheme.warning, fontSize: 13, lineHeight: 20 },
})
