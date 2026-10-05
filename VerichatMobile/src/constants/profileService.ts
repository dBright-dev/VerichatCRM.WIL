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

// Backend contracts: GET/PATCH /api/mobile/profile (JWT).
export async function fetchProfile(): Promise<Profile> {
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
