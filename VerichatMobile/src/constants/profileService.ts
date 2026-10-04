export type Address = {
  name: string // e.g. "Parirenyatwa Hospital"
  street: string // e.g. "15 Mazowe Street, Avondale"
  city: string // e.g. "Harare"
  country: string // e.g. "Zimbabwe"
}

export type Profile = {
  fullName: string
  orderCount: number
  memberSince: string // ISO date, e.g. "2021-03-01"
  email: string
  phone: string
  company: string
  vatNumber: string | null
  defaultAddress: Address | null
}

// Flip to false once the backend endpoint is ready.
const USE_MOCK_DATA = true

const API_BASE_URL = process.env.EXPO_PUBLIC_API_URL ?? ''

// Static data for the front-end build. Remove when the endpoint is live.
const MOCK_PROFILE: Profile = {
  fullName: 'Julia Caeser',
  orderCount: 48,
  memberSince: '2021-03-01',
  email: 'juliacaeser@parihospital.co.zw',
  phone: '+263 77 153 9824',
  company: 'Parirenyatwa Hospital',
  vatNumber: '225876943',
  defaultAddress: {
    name: 'Parirenyatwa Hospital',
    street: '15 Mazowe Street, Avondale',
    city: 'Harare',
    country: 'Zimbabwe',
  },
}

// TODO: confirm the endpoint path and response shape with your backend,
// and attach the logged-in user's auth token to the request headers.
export async function fetchProfile(): Promise<Profile> {
  if (USE_MOCK_DATA) {
    return MOCK_PROFILE
  }

  const response = await fetch(`${API_BASE_URL}/user/account`, {
    headers: {
      'Content-Type': 'application/json',
      // Authorization: `Bearer ${token}`,
    },
  })

  if (!response.ok) {
    throw new Error(`Could not load your profile (${response.status}).`)
  }

  return response.json()
}