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

// Backend contracts: GET/POST /api/mobile/appointments (JWT).
export async function fetchAppointments(): Promise<Appointment[]> {
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
