import { DarkTheme, DefaultTheme, Stack, ThemeProvider, useRouter, useSegments } from 'expo-router';
import * as SplashScreen from 'expo-splash-screen';
import { AppState, useColorScheme } from 'react-native';
import { GestureHandlerRootView } from 'react-native-gesture-handler';
import { useEffect, useState } from 'react';

import { AnimatedSplashOverlay } from '@/components/animated-icon';
import AppTabs from '@/components/app-tabs';
import OfflineStatusBanner from '@/components/offline-status-banner';
import { getAuthToken, hydrateAuthToken, startOfflineSync, subscribeAuth } from '@/constants/api';
import { emitGlobalRefresh } from '@/utils/refreshEvents';
import { registerBackgroundAssignmentSync } from '@/utils/backgroundFetch';
import { applyDarkMode, followSystemTheme, restoreDarkMode } from '@/constants/appTheme';
import { fetchSettings } from '@/constants/settingsService';

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

  useEffect(() => startOfflineSync(), []);

  useEffect(() => {
    const subscription = AppState.addEventListener('change', (state) => {
      if (state === 'active') emitGlobalRefresh();
    });
    void registerBackgroundAssignmentSync();
    return () => subscription.remove();
  }, []);

  useEffect(() => {
    void restoreDarkMode();
  }, []);

  useEffect(() => {
    if (!hydrated) return;
    if (!signedIn) {
      followSystemTheme();
      return;
    }
    let cancelled = false;
    void fetchSettings()
      .then((settings) => { if (!cancelled) void applyDarkMode(settings.preferences.darkMode); })
      .catch(() => { /* use the locally restored preference when offline */ });
    return () => { cancelled = true; };
  }, [hydrated, signedIn]);

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
    <GestureHandlerRootView style={{ flex: 1 }}>
      <ThemeProvider value={colorScheme === 'dark' ? DarkTheme : DefaultTheme}>
        <AnimatedSplashOverlay />
        {hydrated && <OfflineStatusBanner />}
        {!hydrated ? null : signedIn ? (
          <AppTabs />
        ) : (
          <Stack screenOptions={{ headerShown: false, animation: 'slide_from_right' }}>
            <Stack.Screen name="index" />
            <Stack.Screen name="clientLogin" />
            <Stack.Screen name="adminLogin" />
            <Stack.Screen name="newUserAccount" />
          </Stack>
        )}
      </ThemeProvider>
    </GestureHandlerRootView>
  );
}
