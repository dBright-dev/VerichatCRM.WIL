import { Tabs } from 'expo-router';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { ColorValue } from 'react-native';
import { useEffect } from 'react';
import Animated, { useAnimatedStyle, useSharedValue, withSequence, withSpring } from 'react-native-reanimated';

import { CRMTheme } from '@/constants/crmTheme';
import { getAuthRole, isStaffMobileRole } from '@/constants/api';

/**
 * Bottom navigation: Home, Product Catalogue, Consultations, My Assets, Profile.
 * `index` doubles as the auth gate — signed-in clients get the client dashboard,
 * while staff accounts get the operations dashboard and staff-only tabs.
 */
export default function AppTabs() {
  const activeColor = CRMTheme.primary;
  const staffAccount = isStaffMobileRole(getAuthRole());

  return (
    <Tabs
      screenOptions={{
        headerShown: false,
        tabBarActiveTintColor: activeColor,
        tabBarInactiveTintColor: CRMTheme.muted,
        tabBarLabelStyle: { fontSize: 11, fontWeight: '600' },
        tabBarStyle: { backgroundColor: CRMTheme.surface, borderTopColor: CRMTheme.border },
      }}>
      <Tabs.Screen name="index" options={{ title: 'Home', tabBarIcon: ({ color, size, focused }) => <BouncyTabIcon name="home" size={size} color={color} focused={focused} /> }} />
      <Tabs.Screen name="medicalCatalogueShop" options={{ href: staffAccount ? null : undefined, title: 'Catalogue', tabBarIcon: ({ color, size, focused }) => <BouncyTabIcon name="shopping" size={size} color={color} focused={focused} /> }} />
      <Tabs.Screen name="BookAppointmentScreen" options={{ href: staffAccount ? null : undefined, title: 'Consult', tabBarIcon: ({ color, size, focused }) => <BouncyTabIcon name="calendar-clock" size={size} color={color} focused={focused} /> }} />
      <Tabs.Screen name="EquipmentStatusScreen" options={{ href: staffAccount ? null : undefined, title: 'My Assets', tabBarIcon: ({ color, size, focused }) => <BouncyTabIcon name="medical-cotton-swab" size={size} color={color} focused={focused} /> }} />
      <Tabs.Screen name="profileScreen" options={{ title: 'Profile', tabBarIcon: ({ color, size, focused }) => <BouncyTabIcon name="account-circle" size={size} color={color} focused={focused} /> }} />
      {/* Keep non-tab routes out of the bar while allowing in-app navigation. */}
      <Tabs.Screen name="adminLogin" options={{ href: null }} />
      <Tabs.Screen name="ClientCommunicationScreen" options={{ href: null }} />
      <Tabs.Screen name="clientHomescreen" options={{ href: null }} />
      <Tabs.Screen name="clientLogin" options={{ href: null }} />
      <Tabs.Screen name="ClientTimelineScreen" options={{ href: null }} />
      <Tabs.Screen name="Dashboard" options={{ href: null }} />
      <Tabs.Screen name="DashboardScreen" options={{ href: null }} />
      <Tabs.Screen name="explore" options={{ href: null }} />
      <Tabs.Screen name="GoalsScreen" options={{ href: null }} />
      <Tabs.Screen name="newUserAccount" options={{ href: null }} />
      <Tabs.Screen name="settingsScreen" options={{ href: null }} />
      <Tabs.Screen name="cartScreen" options={{ href: null }} />
    </Tabs>
  );
}

function BouncyTabIcon({ name, size, color, focused }: { name: React.ComponentProps<typeof MaterialCommunityIcons>['name']; size: number; color: ColorValue; focused: boolean }) {
  const scale = useSharedValue(1)
  useEffect(() => {
    if (focused) scale.value = withSequence(withSpring(1.22, { damping: 8, stiffness: 240 }), withSpring(1, { damping: 10, stiffness: 180 }))
  }, [focused, scale])
  const style = useAnimatedStyle(() => ({ transform: [{ scale: scale.value }, { translateY: focused ? -2 : 0 }] }))
  return <Animated.View style={style}><MaterialCommunityIcons name={name} size={size} color={color} /></Animated.View>
}
