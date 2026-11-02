/**
 * The Verichat colour tokens, shared with the web client.
 *
 * Every value here is the same colour the web tier declares as a CSS custom
 * property in `VerichatCRM.Web/wwwroot/css/verichat-theme.css`. That duplication
 * is deliberate: a client reading a ticket on the phone and a support agent
 * reading the same ticket in the browser should see the same colour mean the
 * same thing. Change a value on one side and the two clients drift.
 *
 * `primary` was #17689b and `primaryDark` #123c61 in the mobile client, while
 * the web client used #2F6FED on #101B33. The web values won because the web
 * tier is the larger surface and the landing page, auth screens and staff
 * dashboards were all already built on them.
 */
export const CRMTheme = {
  /** Action colour: buttons, links, prices, selected state. */
  primary: '#2F6FED',
  /** Deep navy: headers, dark full-screen backgrounds, headings on light. */
  primaryDark: '#101B33',
  /** The plate the Verichat logo is drawn on in the company profile. */
  ink: '#0D141D',

  text: '#182233',
  muted: '#6B7385',
  background: '#F4F6FA',
  surface: '#FFFFFF',
  /** Pale tint for a selected or hovered fill, and for soft brand panels. */
  surfaceAlt: '#E8EFFE',
  border: '#E3E7EF',

  success: '#1FA971',
  successSoft: '#E4F7EE',
  warning: '#E8940C',
  warningSoft: '#FDF1DC',
  danger: '#E24C4C',
  dangerSoft: '#FBE6E6',
} as const

export const CRMGradient = [CRMTheme.primaryDark, CRMTheme.primary] as const
