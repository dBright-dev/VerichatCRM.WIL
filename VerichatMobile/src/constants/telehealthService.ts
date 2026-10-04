import { api } from './api'

export type AppointmentStatus = 'confirmed' | 'cancelled' | 'pending' | 'completed'

export type Appointment = {
  id: string
  doctorName: string // e.g. "Dr Joseph Mushawatu"
  specialty: string // e.g. "General Practitioner"
  facility: string // e.g. "Parktown Hospital"
  startsAt: string // ISO date-time, e.g. "2026-08-17T14:30:00"
  status: AppointmentStatus
}

// Flip to false to keep using the bundled mock data.
const USE_MOCK_DATA = false

const API_BASE_URL = process.env.EXPO_PUBLIC_API_URL ?? ''

// Static data for the front-end build. 
const MOCK_APPOINTMENTS: Appointment[] = [
  {
    id: '1',
    doctorName: 'Dr Joseph Mushawatu',
    specialty: 'General Practitioner',
    facility: 'Parktown Hospital',
    startsAt: '2026-08-17T14:30:00',
    status: 'confirmed',
  },
  {
    id: '2',
    doctorName: 'Dr Fungisai Hove',
    specialty: 'Gynaecologist',
    facility: 'Medical Specialist Centre',
    startsAt: '2026-08-18T15:00:00',
    status: 'cancelled',
  },
  {
    id: '3',
    doctorName: 'Dr Nyasha Mukiwa',
    specialty: 'Pharmacist',
    facility: 'MedOrange Pharmacy',
    startsAt: '2026-08-24T13:30:00',
    status: 'pending',
  },
  {
    id: '4',
    doctorName: 'Dr Sarah Naidoo',
    specialty: 'General Practitioner',
    facility: 'Parirenyatwa Hospital',
    startsAt: '2026-08-03T12:00:00',
    status: 'completed',
  },
  {
    id: '5',
    doctorName: 'Dr Lindiwe Zulu',
    specialty: 'Dermatologist',
    facility: 'Sunninghill Medical Centre',
    startsAt: '2026-08-01T09:30:00',
    status: 'completed',
  },
]

// Backend contracts: GET/POST /api/mobile/appointments (JWT).
export async function fetchAppointments(): Promise<Appointment[]> {
  if (USE_MOCK_DATA) {
    return MOCK_APPOINTMENTS
  }

  return api<Appointment[]>('/api/mobile/appointments')
}

/** Book a telemedicine visit ("Book Telemedicine" quick action). */
export async function bookAppointment(input: {
  doctorName: string
  specialty?: string
  facility?: string
  startsAt: string
}): Promise<Appointment> {
  return api<Appointment>('/api/mobile/appointments', {
    method: 'POST',
    body: JSON.stringify({
      doctorName: input.doctorName,
      specialty: input.specialty ?? '',
      facility: input.facility ?? '',
      startsAt: input.startsAt,
    }),
  })
}