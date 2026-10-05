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

// Backend contract: GET /api/mobile/goals (JWT).

export async function fetchGoals(): Promise<GoalsData> {
  return api<GoalsData>('/api/mobile/goals')
}

/** Persist edits to a goal the API has already assigned to this account. */
export async function updateGoal(goal: Pick<Goal, 'id'> & Partial<Omit<Goal, 'id'>>): Promise<Goal> {
  return api<Goal>(`/api/mobile/goals/${encodeURIComponent(goal.id)}`, {
    method: 'PATCH',
    body: JSON.stringify({
      current: goal.current,
      target: goal.target,
      name: goal.name,
      unit: goal.unit,
      type: goal.type,
    }),
  })
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
