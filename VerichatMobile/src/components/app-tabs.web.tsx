import {
  Tabs,
  TabList,
  TabTrigger,
  TabSlot,
  TabTriggerSlotProps,
  TabListProps,
} from 'expo-router/ui';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { Pressable, View, StyleSheet, Text } from 'react-native';

import { ThemedView } from './themed-view';

import { MaxContentWidth, Spacing } from '@/constants/theme';

/**
 * Web mirror of the native bottom bar: Home, Shop, Telehealth, Vitals, Profile.
 */
export default function AppTabs() {
  return (
    <Tabs>
      <TabSlot style={{ height: '100%' }} />
      <TabList asChild>
        <CustomTabList>
          <TabTrigger name="home" href="/" asChild>
            <TabButton icon="home">Home</TabButton>
          </TabTrigger>
          <TabTrigger name="shop" href="/medicalCatalogueShop" asChild>
            <TabButton icon="shopping">Shop</TabButton>
          </TabTrigger>
          <TabTrigger name="telehealth" href="/telehealthScreen" asChild>
            <TabButton icon="video-outline">Health</TabButton>
          </TabTrigger>
          <TabTrigger name="vitals" href="/wellnessTrackingScreen" asChild>
            <TabButton icon="heart-pulse">Vitals</TabButton>
          </TabTrigger>
          <TabTrigger name="profile" href="/profileScreen" asChild>
            <TabButton icon="account-circle">Profile</TabButton>
          </TabTrigger>
          {/* Web equivalent of the native `hidden` trigger: registers
              /settingsScreen as a navigable route with no tab-bar button. */}
          <TabTrigger name="settings" href="/settingsScreen" asChild>
            <HiddenTabButton />
          </TabTrigger>
          <TabTrigger name="cart" href="/cartScreen" asChild>
            <HiddenTabButton />
          </TabTrigger>
        </CustomTabList>
      </TabList>
    </Tabs>
  );
}

/** Renders nothing so a route is registered without a visible tab. */
function HiddenTabButton() {
  return null;
}

type TabButtonProps = TabTriggerSlotProps & {
  icon: React.ComponentProps<typeof MaterialCommunityIcons>['name']
}

export function TabButton({ children, isFocused, icon, ...props }: TabButtonProps) {
  return (
    <Pressable
      {...props}
      accessibilityRole="tab"
      style={({ pressed }) => [styles.tabButton, pressed && styles.pressed, isFocused && styles.focused]}>
      <MaterialCommunityIcons name={icon} size={22} color={isFocused ? '#0057a8' : '#59636e'} />
      <Text style={[styles.tabLabel, isFocused && styles.focusedLabel]} numberOfLines={1}>
        {children}
      </Text>
    </Pressable>
  );
}

export function CustomTabList(props: TabListProps) {
  return (
    <View {...props} style={styles.tabListContainer}>
      <ThemedView type="backgroundElement" style={styles.innerContainer}>
        {props.children}
      </ThemedView>
    </View>
  );
}

const styles = StyleSheet.create({
  tabListContainer: {
    position: 'absolute',
    width: '100%',
    padding: Spacing.three,
    justifyContent: 'center',
    alignItems: 'center',
    flexDirection: 'row',
  },
  innerContainer: {
    paddingVertical: Spacing.two,
    paddingHorizontal: Spacing.two,
    borderRadius: Spacing.five,
    flexDirection: 'row',
    alignItems: 'center',
    flexGrow: 1,
    gap: Spacing.one,
    maxWidth: MaxContentWidth,
  },
  tabButton: { flex: 1, minWidth: 0, minHeight: 52, alignItems: 'center', justifyContent: 'center', gap: 2, borderRadius: Spacing.three },
  focused: { backgroundColor: '#e4eef8' },
  tabLabel: { fontSize: 11, color: '#59636e', fontWeight: '600' },
  focusedLabel: { color: '#0057a8', fontWeight: '800' },
  pressed: {
    opacity: 0.7,
  },
});
