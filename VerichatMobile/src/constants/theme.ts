/**
 * Template plumbing for the Expo components in `src/components` (ThemedText,
 * ThemedView, the tab bar) and the spacing scale the screens use.
 *
 * The colours are not a second palette: they are read from CRMTheme, the single
 * source of brand colour shared with the web client. The dark column is the
 * only place a colour is chosen here, and it is the same ink the brand uses.
 */

import '@/global.css';

import { Platform } from 'react-native';
import { CRMTheme } from './crmTheme';

export const Colors = {
  light: {
    text: CRMTheme.text,
    background: CRMTheme.background,
    backgroundElement: CRMTheme.surface,
    backgroundSelected: CRMTheme.surfaceAlt,
    textSecondary: CRMTheme.muted,
  },
  dark: {
    text: '#EDF1F7',
    background: CRMTheme.ink,
    backgroundElement: '#1B2735',
    backgroundSelected: '#253344',
    textSecondary: '#9AA6B8',
  },
} as const;

export type ThemeColor = keyof typeof Colors.light & keyof typeof Colors.dark;

export const Fonts = Platform.select({
  ios: {
    /** iOS `UIFontDescriptorSystemDesignDefault` */
    sans: 'system-ui',
    /** iOS `UIFontDescriptorSystemDesignSerif` */
    serif: 'ui-serif',
    /** iOS `UIFontDescriptorSystemDesignRounded` */
    rounded: 'ui-rounded',
    /** iOS `UIFontDescriptorSystemDesignMonospaced` */
    mono: 'ui-monospace',
  },
  default: {
    sans: 'normal',
    serif: 'serif',
    rounded: 'normal',
    mono: 'monospace',
  },
  web: {
    sans: 'var(--font-display)',
    serif: 'var(--font-serif)',
    rounded: 'var(--font-rounded)',
    mono: 'var(--font-mono)',
  },
});

export const Spacing = {
  half: 2,
  one: 4,
  two: 8,
  three: 16,
  four: 24,
  five: 32,
  six: 64,
} as const;

export const BottomTabInset = Platform.select({ ios: 50, android: 80 }) ?? 0;
export const MaxContentWidth = 800;
