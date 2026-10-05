import { DarkTheme, DefaultTheme, Stack, ThemeProvider, useRouter, useSegments } from 'expo-router';
import * as SplashScreen from 'expo-splash-screen';
import { useColorScheme } from 'react-native';
import { useEffect, useState } from 'react';

import { AnimatedSplashOverlay } from '@/components/animated-icon';
import AppTabs from '@/components/app-tabs';
import { getAuthToken, hydrateAuthToken, subscribeAuth } from '@/constants/api';

SplashScreen.preventAutoHideAsync();

/** Routes that are only reachable while signed out. */
const AUTH_ROUTES = ['clientLogin', 'adminLogin', 'newUserAccount'];

export default function TabLayout() {
  const colorScheme = useColorScheme();
  const router = useRouter();
  const segments = useSegments();
  const [signedIn, setSignedIn] = useState(() => getAuthToken() !== null);
  // While false we have not yet read the persisted JWT from secure storage.
  const [hydrated, setHydrated] = useState(false);

  useEffect(() => subscribeAuth((token) => setSignedIn(token !== null)), []);

  useEffect(() => {
    // Restore a persisted session before deciding which navigator to mount.
    void hydrateAuthToken().finally(() => setHydrated(true));
  }, []);

  useEffect(() => {
    if (!hydrated) return;
    const current = segments[0];
    const onAuthRoute = !current || AUTH_ROUTES.includes(current);
    if (!signedIn && !onAuthRoute) router.replace('/');
    if (signedIn && AUTH_ROUTES.includes(current ?? '')) router.replace('/');
  }, [router, segments, signedIn, hydrated]);

  return (
    <ThemeProvider value={colorScheme === 'dark' ? DarkTheme : DefaultTheme}>
      <AnimatedSplashOverlay />
      {!hydrated ? null : signedIn ? (
        <AppTabs />
      ) : (
        <Stack screenOptions={{ headerShown: false }}>
          <Stack.Screen name="index" />
          <Stack.Screen name="clientLogin" />
          <Stack.Screen name="adminLogin" />
          <Stack.Screen name="newUserAccount" />
        </Stack>
      )}
    </ThemeProvider>
  );
}
