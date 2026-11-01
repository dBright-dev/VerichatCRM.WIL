import { CRMGradient, CRMTheme } from '../constants/crmTheme'
import { MaterialCommunityIcons } from '@expo/vector-icons'
import { LinearGradient } from 'expo-linear-gradient'
import { useRouter } from 'expo-router'
import { ComponentProps } from 'react'
import { Linking, ScrollView, StyleSheet, Text, TouchableOpacity, View } from 'react-native'
import { useSafeAreaInsets } from 'react-native-safe-area-context'

/**
 * The privacy notice, in the app.
 *
 * Both stores require a policy that is reachable before somebody signs in, and the
 * web notice at /Home/Privacy is the authoritative text. This screen repeats it in
 * the order the website uses, because the two have to say the same thing - the only
 * difference is that a phone cannot show a sticky contents rail.
 */

type IconName = ComponentProps<typeof MaterialCommunityIcons>['name']

const CONTACT = {
  company: 'Verichat Quality Projects',
  address: '11 Lincoln Road, Avondale, Harare, Zimbabwe',
  phones: ['+263 771 180 001', '+263 774 697 662'],
  email: 'sales@verichat.co.zw',
}

const SECTIONS: { title: string; body: string; bullets?: string[] }[] = [
  {
    title: '1. What this covers',
    body:
      'It applies to the Verichat website, the signed-in portal, the API behind it and this app. ' +
      'Verichat Quality Projects decides why the information here is kept. Where a client sits in ' +
      'South Africa we apply the same principles POPIA would require there.',
  },
  {
    title: '2. What we hold about you',
    body: 'Only what the work needs. If a field exists, it exists because a screen reads it.',
    bullets: [
      'Your account: name, email address, the role you applied for, whether an employee has approved it, and a salted hash of your password - never the password itself.',
      'Your organisation: sector, contract status, a named contact with phone and email, a street address, and the latitude and longitude of the site so a delivery can be routed and an engineer can find it.',
      'Business with us: enquiries, quotations, orders, deliveries and their tracking numbers, service tickets with the photos you attach, and the calibration history of equipment we installed.',
      'This app: a push token for the device, the device name and type, and the last time it synchronised.',
      'Teaching and telehealth screens: observations, prescriptions, laboratory results and appointments you or your supervisor enter. That is special personal data, used only for the feature that opened it.',
      'An audit trail of every change made in the portal - who, which record, the value before and after, and the IP address of the connection.',
    ],
  },
  {
    title: '3. Why we hold it',
    body:
      'To supply, deliver and service the goods, which is the contract you have with us. To keep an ' +
      'unapproved account from reading another organisation\'s prices, and to make a disputed delivery ' +
      'note explainable years later, which is a legitimate interest. For calibration records, which is a ' +
      'professional obligation. Push notifications and the health screens run on consent, and you can ' +
      'switch notifications off at any time in Settings.',
  },
  {
    title: '4. Ranking an enquiry',
    body:
      'New enquiries are scored for urgency - sector, contract status, the date you need it and value - ' +
      'so a scarce delivery slot goes to the site that needs it first. That is a queue position, not a ' +
      'judgement about a person, and a sales representative reviews and can override every one of them.',
  },
  {
    title: '5. Who can read it',
    body:
      'Access is per role and checked on the server for every request, not merely hidden from the menu you ' +
      'are shown. A Client User sees only their own organisation. A Medical Student sees the catalogue and ' +
      'their own quote requests, never a client file. Sales sees the accounts assigned to them; Procurement ' +
      'the suppliers and stock; Logistics the delivery addresses; a Field Support Engineer the assets at the ' +
      'sites they attend. A System Administrator sees everything, including the audit trail, and is the role ' +
      'that approves accounts. Nobody at another company gets access, and we do not sell or licence your data.',
  },
  {
    title: '6. Accounts and the links we email',
    body:
      'Signing up is an application, not access: a Verichat employee checks that you really are a client ' +
      'before the system opens. Confirmation and reset links carry a random one-time code. We store only a ' +
      'SHA-256 hash of that code, never the code. A reset link expires in one hour and stops working once ' +
      'used; a confirmation link lasts seven days. Resetting a password proves you hold the mailbox - it does ' +
      'not approve an application or reopen a declined account. The answer to a forgotten password is the same ' +
      'sentence whether or not the address has an account, so the form cannot be used to find out who registered.',
  },
  {
    title: '7. Storage on this device',
    body:
      'Your sign-in token is kept in the phone\'s own secure store - the Keychain on iOS, secure storage on ' +
      'Android. So the app still works where there is no signal, recent responses and any actions you queued ' +
      'are held in the app\'s private storage, protected by the device\'s encryption but not additionally ' +
      'encrypted by us; signing out or clearing the app\'s data removes them. Keep the device locked. The web ' +
      'portal sets one essential cookie that keeps you signed in for 120 minutes of idle time - no advertising, ' +
      'analytics or social cookies anywhere.',
  },
  {
    title: '8. Other services',
    body:
      'Our hosting provider runs the portal, the API and the SQL Server database. An SMTP provider sends ' +
      'confirmation, approval, reset, help and delivery notices on our behalf - they move the message and the ' +
      'body is not copied into the database. The app build and its push channel pass through Expo, and the ' +
      'store you install from records the download. We do not give any of them your data for their own use.',
  },
  {
    title: '9. How long, and how to get out of it',
    body:
      'For as long as the relationship lasts, plus whatever the accounting and equipment-safety rules require - ' +
      'a machine you bought from us needs its calibration history for its life. Recovery codes expire on their ' +
      'own. Ask us to show you what we hold, correct it, or delete it, and we answer within 30 days: we remove ' +
      'what we can and tell you in writing what we must keep. Deactivating an account stops access immediately.',
  },
  {
    title: '10. How we protect it',
    body:
      'HTTPS to the deployed portal and API. PBKDF2 with SHA-256 and a per-account salt for passwords. Server-side ' +
      'authorisation on every read and write, ownership checks on records that belong to a client, anti-forgery ' +
      'tokens on forms, and free text sanitised on the way in. Nothing can promise that nothing ever goes wrong: if ' +
      'a breach puts your information at risk we will tell you and the regulator without unreasonable delay.',
  },
  {
    title: '11. Your rights, and who to complain to',
    body:
      'You can ask to know, correct, delete, or object to a ranking and have a human review it instead, and you can ' +
      'withdraw consent for notifications without losing the account. If you are not satisfied, complain to ' +
      'Zimbabwe\'s Data Protection Authority under the Cyber and Data Protection Act [Chapter 11:34], or - for a ' +
      'client in South Africa - the Information Regulator under POPIA. We do not knowingly collect information from ' +
      'children.',
  },
  {
    title: '12. Status of this notice',
    body:
      'Version 1.0, published 1 November 2026, written against the running system. It has not yet been reviewed by ' +
      'a legal practitioner. If we change what we collect, who can read it or what we do with it, approved accounts ' +
      'are emailed before it takes effect and the new notice is shown at the next sign-in.',
  },
]

const openUrl = (url: string) => {
  void Linking.openURL(url).catch(() => undefined)
}

export default function PrivacyScreen() {
  const insets = useSafeAreaInsets()
  const router = useRouter()

  return (
    <View style={styles.screen}>
      <ScrollView contentContainerStyle={[styles.scroll, { paddingBottom: insets.bottom + 32 }]} showsVerticalScrollIndicator={false}>
        <LinearGradient colors={[...CRMGradient]} start={{ x: 0, y: 0 }} end={{ x: 1, y: 1 }} style={[styles.hero, { paddingTop: insets.top + 14 }]}>
          <View style={styles.heroTop}>
            <TouchableOpacity accessibilityRole="button" accessibilityLabel="Back" onPress={() => (router.canGoBack() ? router.back() : router.replace('/'))} style={styles.backButton}>
              <MaterialCommunityIcons name="arrow-left" size={20} color="#fff" />
            </TouchableOpacity>
            <Text style={styles.eyebrow}>LEGAL · VERSION 1.0</Text>
          </View>
          <Text style={styles.title}>Privacy notice</Text>
          <Text style={styles.standfirst}>
            Verichat runs the supply chain of a clinic, a mine rescue station or a teaching
            laboratory. To do that we hold names, addresses, delivery locations, serial numbers
            and calibration dates. This says what we hold, why, who may read it, and how you get
            it changed or removed.
          </Text>
        </LinearGradient>

        <View style={styles.body}>
          {SECTIONS.map((section) => (
            <View key={section.title} style={styles.section}>
              <Text style={styles.sectionTitle}>{section.title}</Text>
              <Text style={styles.paragraph}>{section.body}</Text>
              {section.bullets ? (
                <View style={styles.bullets}>
                  {section.bullets.map((bullet) => (
                    <View key={bullet} style={styles.bulletRow}>
                      <View style={styles.bulletMark} />
                      <Text style={styles.bulletText}>{bullet}</Text>
                    </View>
                  ))}
                </View>
              ) : null}
            </View>
          ))}

          <View style={styles.contact}>
            <Text style={styles.contactTitle}>Contact</Text>
            <Text style={styles.contactLine}>{CONTACT.company}</Text>
            <Text style={styles.contactLine}>{CONTACT.address}</Text>
            {CONTACT.phones.map((phone) => (
              <ContactRow key={phone} icon="phone-outline" label={phone} onPress={() => openUrl(`tel:${phone.replace(/[^+\d]/g, '')}`)} />
            ))}
            <ContactRow icon="email-outline" label={CONTACT.email} onPress={() => openUrl(`mailto:${CONTACT.email}`)} />
            <Text style={styles.contactNote}>
              Cannot sign in at all? The “Cannot sign in?” link on the sign-in screen sends your
              problem to an administrator who can see the approval queue - the only place a
              locked-out account can actually be fixed.
            </Text>
          </View>
        </View>
      </ScrollView>
    </View>
  )
}

function ContactRow({ icon, label, onPress }: { icon: IconName; label: string; onPress: () => void }) {
  return (
    <TouchableOpacity accessibilityRole="button" onPress={onPress} style={styles.contactRow}>
      <MaterialCommunityIcons name={icon} size={18} color={CRMTheme.primary} />
      <Text style={styles.contactLink}>{label}</Text>
    </TouchableOpacity>
  )
}

const styles = StyleSheet.create({
  screen: { flex: 1, backgroundColor: CRMTheme.background },
  scroll: { flexGrow: 1 },
  hero: { paddingHorizontal: 20, paddingBottom: 24, borderBottomLeftRadius: 26, borderBottomRightRadius: 26 },
  heroTop: { flexDirection: 'row', alignItems: 'center', gap: 12 },
  backButton: { width: 38, height: 38, borderRadius: 13, backgroundColor: '#ffffff22', alignItems: 'center', justifyContent: 'center' },
  eyebrow: { color: '#c8e2f5', fontSize: 10, fontWeight: '900', letterSpacing: 1.6 },
  title: { color: '#fff', fontSize: 30, fontWeight: '900', marginTop: 14 },
  standfirst: { color: '#dcecf8', fontSize: 13.5, lineHeight: 21, marginTop: 12 },
  body: { paddingHorizontal: 18, paddingTop: 22, gap: 18 },
  section: { backgroundColor: '#fff', borderRadius: 17, padding: 16, borderWidth: 1, borderColor: CRMTheme.border, gap: 8 },
  sectionTitle: { color: CRMTheme.text, fontSize: 15, fontWeight: '900' },
  paragraph: { color: '#43536a', fontSize: 13.5, lineHeight: 22 },
  bullets: { gap: 9, marginTop: 2 },
  bulletRow: { flexDirection: 'row', gap: 10 },
  bulletMark: { width: 6, height: 6, borderRadius: 3, backgroundColor: '#C9A96A', marginTop: 8 },
  bulletText: { flex: 1, color: '#43536a', fontSize: 13, lineHeight: 21 },
  contact: { backgroundColor: CRMTheme.primaryDark, borderRadius: 17, padding: 18, gap: 7, marginBottom: 8 },
  contactTitle: { color: '#fff', fontSize: 16, fontWeight: '900', marginBottom: 4 },
  contactLine: { color: '#cfe1f2', fontSize: 13, lineHeight: 20 },
  contactRow: { flexDirection: 'row', alignItems: 'center', gap: 9, paddingVertical: 5 },
  contactLink: { color: '#fff', fontSize: 13.5, fontWeight: '700' },
  contactNote: { color: '#a8c3dc', fontSize: 12.5, lineHeight: 20, marginTop: 8 },
})
