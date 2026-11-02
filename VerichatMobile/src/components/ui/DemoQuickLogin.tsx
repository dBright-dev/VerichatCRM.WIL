import { CRMTheme } from '../../constants/crmTheme'
import { demoAccountsFor } from '../../constants/demoAccounts'
import { StyleSheet, Text, TouchableOpacity, View } from 'react-native'

type Props = {
  /** Which sign-in screen this row sits on; staff chips are refused by the client audience. */
  audience: 'client' | 'staff'
  onPick: (email: string, password: string) => void
  disabled?: boolean
}

/**
 * One tap per role, for the people who demo this app rather than use it.
 *
 * Seven roles, seven boards, and the slow way to show any of them is typing an
 * address and a password out of the README table. This row trades that for a chip per
 * role the screen's own audience can sign in as; the screen keeps the sign-in path, so
 * the chip only supplies the credential and the server still decides where the session
 * goes.
 *
 * Dev builds only. `__DEV__` is false in a release bundle, and the accounts themselves
 * exist only while the API runs in Development with `Bootstrap:DemoUsers=true`, so a
 * published app has neither the shortcut nor anything for it to sign in to.
 */
const DemoQuickLogin = ({ audience, onPick, disabled }: Props) => {
  if (!__DEV__) return null

  const accounts = demoAccountsFor(audience)

  return (
    <View style={styles.wrap}>
      <Text style={styles.label}>Demo: quick login</Text>

      <View style={styles.grid}>
        {accounts.map((account) => (
          <TouchableOpacity
            key={account.code}
            style={styles.chip}
            disabled={disabled}
            onPress={() => onPick(account.email, account.password)}
            accessibilityRole="button"
            accessibilityLabel={`${account.role}, ${account.email}, opens ${account.opens}`}
          >
            <View style={[styles.dot, { backgroundColor: account.dot }]} />
            <View style={styles.text}>
              <Text style={styles.role}>{account.label}</Text>
              <Text style={styles.opens}>{account.opens}</Text>
            </View>
          </TouchableOpacity>
        ))}
      </View>

      <Text style={styles.note}>
        Seeded development accounts, one per role. They exist only while the API runs in
        Development with Bootstrap:DemoUsers set.
      </Text>
    </View>
  )
}

const styles = StyleSheet.create({
  wrap: {
    marginTop: 22,
    paddingTop: 16,
    borderTopWidth: 1,
    borderTopColor: CRMTheme.border,
    borderStyle: 'dashed',
    gap: 10,
  },
  label: {
    fontSize: 11,
    fontWeight: '700',
    letterSpacing: 1.2,
    textTransform: 'uppercase',
    color: CRMTheme.muted,
  },
  grid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 8,
  },
  chip: {
    flexGrow: 1,
    flexBasis: '47%',
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
    paddingVertical: 9,
    paddingHorizontal: 10,
    backgroundColor: CRMTheme.surface,
    borderWidth: 1,
    borderColor: CRMTheme.border,
    borderRadius: 12,
  },
  dot: {
    width: 8,
    height: 8,
    borderRadius: 4,
  },
  text: {
    flexShrink: 1,
  },
  role: {
    fontSize: 13,
    fontWeight: '600',
    color: CRMTheme.text,
  },
  opens: {
    fontSize: 11,
    color: CRMTheme.muted,
    marginTop: 1,
  },
  note: {
    fontSize: 11,
    lineHeight: 15,
    color: CRMTheme.muted,
  },
})

export default DemoQuickLogin
