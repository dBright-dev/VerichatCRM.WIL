import { api } from './api'

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

// Flip to false to keep using the bundled mock data.
const USE_MOCK_DATA = false

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

// Backend contracts: GET/PATCH /api/mobile/profile (JWT).
export async function fetchProfile(): Promise<Profile> {
  if (USE_MOCK_DATA) {
    return MOCK_PROFILE
  }

  return api<Profile>('/api/mobile/profile')
}

/** Persists profile + address edits. */
export async function updateProfile(changes: {
  displayName?: string
  phone?: string
  company?: string
  vatNumber?: string | null
  addressName?: string
  street?: string
  city?: string
  country?: string
}): Promise<Profile> {
  return api<Profile>('/api/mobile/profile', {
    method: 'PATCH',
    body: JSON.stringify(changes),
  })
}