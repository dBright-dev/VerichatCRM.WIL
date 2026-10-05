import { NativeTabs } from 'expo-router/unstable-native-tabs';
import { useColorScheme } from 'react-native';

import { Colors } from '@/constants/theme';

/**
 * Bottom navigation: Home, Shop, Telehealth, Vitals, Profile.
 * `index` doubles as the auth gate — it shows the "Log in As" chooser when
 * signed out and the client dashboard when signed in.
 */
export default function AppTabs() {
  const scheme = useColorScheme();
  const colors = Colors[scheme === 'unspecified' ? 'light' : scheme];
  const activeColor = scheme === 'dark' ? '#80bfff' : '#0057a8';

  return (
    <NativeTabs
      backgroundColor={colors.background}
      indicatorColor={colors.backgroundSelected}
      labelVisibilityMode="labeled"
      labelStyle={{
        default: { color: colors.textSecondary, fontSize: 11 },
        selected: { color: activeColor, fontSize: 11, fontWeight: '700' },
      }}
      iconColor={{ default: colors.textSecondary, selected: activeColor }}>
      <NativeTabs.Trigger name="index">
        <NativeTabs.Trigger.Label>Home</NativeTabs.Trigger.Label>
        <NativeTabs.Trigger.Icon sf="house.fill" md="home" />
      </NativeTabs.Trigger>

      <NativeTabs.Trigger name="medicalCatalogueShop">
        <NativeTabs.Trigger.Label>Shop</NativeTabs.Trigger.Label>
        <NativeTabs.Trigger.Icon sf="bag.fill" md="shopping_cart" />
      </NativeTabs.Trigger>

      <NativeTabs.Trigger name="telehealthScreen">
        <NativeTabs.Trigger.Label>Health</NativeTabs.Trigger.Label>
        <NativeTabs.Trigger.Icon sf="video" md="videocam" />
      </NativeTabs.Trigger>

      <NativeTabs.Trigger name="wellnessTrackingScreen">
        <NativeTabs.Trigger.Label>Vitals</NativeTabs.Trigger.Label>
        <NativeTabs.Trigger.Icon sf="heart.fill" md="favorite" />
      </NativeTabs.Trigger>

      <NativeTabs.Trigger name="profileScreen">
        <NativeTabs.Trigger.Label>Profile</NativeTabs.Trigger.Label>
        <NativeTabs.Trigger.Icon sf="person.crop.circle.fill" md="account_circle" />
      </NativeTabs.Trigger>

      {/* Settings is pushed on top of the Profile tab rather than owning a
          tab-bar slot, so it stays hidden until navigated to. */}
      <NativeTabs.Trigger name="settingsScreen" hidden />
      <NativeTabs.Trigger name="cartScreen" hidden />
    </NativeTabs>
  );
}
