import { api } from './api'

export type GoalType = 'steps' | 'water' | 'sleep' | 'exercise'

export type Goal = {
  id: string
  type: GoalType
  name: string // e.g. "Steps"
  current: number // progress so far today
  target: number // daily goal
  unit: string // e.g. "steps", "L", "hrs", "min"
}

export type WeeklyExerciseDay = {
  label: string // e.g. "M", "T", "W"
  minutes: number
}

export type WeeklyExercise = {
  targetPerDayMin: number // e.g. 30
  days: WeeklyExerciseDay[] // Monday to Sunday
  todayIndex: number | null // index into days, null if the week is not current
}

export type GoalsData = {
  goals: Goal[]
  weeklyExercise: WeeklyExercise | null
}

// Flip to false to keep using the bundled mock data.
const USE_MOCK_DATA = false

const API_BASE_URL = process.env.EXPO_PUBLIC_API_URL ?? ''

// Static data for the front-end build. Remove when the endpoint is live.
const MOCK_GOALS: GoalsData = {
  goals: [
    { id: '1', type: 'steps', name: 'Steps', current: 724, target: 1000, unit: 'steps' },
    { id: '2', type: 'water', name: 'Water', current: 1.4, target: 2, unit: 'L' },
    { id: '3', type: 'sleep', name: 'Sleep', current: 6.5, target: 8, unit: 'hrs' },
    { id: '4', type: 'exercise', name: 'Exercise', current: 25, target: 30, unit: 'min' },
  ],
  weeklyExercise: {
    targetPerDayMin: 30,
    todayIndex: 6,
    days: [
      { label: 'M', minutes: 20 },
      { label: 'T', minutes: 40 },
      { label: 'W', minutes: 15 },
      { label: 'T', minutes: 35 },
      { label: 'F', minutes: 30 },
      { label: 'S', minutes: 10 },
      { label: 'S', minutes: 25 },
    ],
  },
}

// Backend contract: GET /api/mobile/goals (JWT).

export async function fetchGoals(): Promise<GoalsData> {
  if (USE_MOCK_DATA) {
    return MOCK_GOALS
  }

  return api<GoalsData>('/api/mobile/goals')
}

/** Log minutes of exercise for one day of the current week (0=Monday). */
export async function logExercise(dayIndex: number, minutes: number): Promise<GoalsData['weeklyExercise']> {
  const weekly = await api<GoalsData['weeklyExercise']>('/api/mobile/goals/exercise', {
    method: 'POST',
    body: JSON.stringify({ dayIndex, minutes }),
  })
  return weekly
}

// TODO: add updateGoal(id, target) when the "edit goal" flow is designed.