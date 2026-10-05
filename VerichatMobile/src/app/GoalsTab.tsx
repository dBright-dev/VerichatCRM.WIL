import { MaterialCommunityIcons } from '@expo/vector-icons'
import { ComponentProps, useCallback, useEffect, useState } from 'react'
import { ActivityIndicator, Alert, FlatList, Modal, StyleSheet, Text, TextInput, TouchableOpacity, View } from 'react-native'
import { fetchGoals, Goal, GoalsData, GoalType, logExercise, updateGoal, WeeklyExercise } from '../constants/goalsService'

/* ------------------------------ Constants ------------------------------- */

type IconName = ComponentProps<typeof MaterialCommunityIcons>['name']

const GOAL_STYLES: Record<
  GoalType,
  { icon: IconName; iconColor: string; barColor: string; textColor: string }
> = {
  steps: { icon: 'walk', iconColor: '#37474f', barColor: '#2e9e2e', textColor: '#2e7d32' },
  water: { icon: 'water', iconColor: '#29b6f6', barColor: '#29b6d8', textColor: '#2f8cff' },
  sleep: { icon: 'sleep', iconColor: '#5c6bc0', barColor: '#ff7a1a', textColor: '#ef6c00' },
  exercise: { icon: 'dumbbell', iconColor: '#d86a7f', barColor: '#b71c3c', textColor: '#b71c3c' },
}

const CHART_HEIGHT = 44

/* ------------------------------- Helpers -------------------------------- */

const formatNumber = (value: number) =>
  value.toLocaleString('en-US', { maximumFractionDigits: 2 })

const getPercent = (goal: Goal) =>
  goal.target > 0 ? Math.min(100, Math.round((goal.current / goal.target) * 100)) : 0

/* ------------------------------ Weekly chart ---------------------------- */

const WeeklyExerciseCard = ({ weekly }: { weekly: WeeklyExercise }) => {
  const total = weekly.days.reduce((sum, day) => sum + day.minutes, 0)
  const weeklyTarget = weekly.targetPerDayMin * weekly.days.length
  const remaining = Math.max(0, weeklyTarget - total)
  const maxMinutes = Math.max(weekly.targetPerDayMin, ...weekly.days.map((d) => d.minutes), 1)

  return (
    <View style={styles.weekly_card}>
      <View style={styles.weekly_top}>
        <Text style={styles.weekly_title}>Weekly Exercise</Text>
        <Text style={styles.weekly_total}>
          {formatNumber(total)}/{formatNumber(weeklyTarget)} min
        </Text>
      </View>

      <View style={styles.chart}>
        {weekly.days.map((day, index) => {
          const height =
            day.minutes <= 0 ? 6 : Math.max(8, (day.minutes / maxMinutes) * CHART_HEIGHT)
          const isToday = weekly.todayIndex === index

          return (
            <View key={`${day.label}-${index}`} style={styles.chart_column}>
              <View style={styles.chart_bar_area}>
                <View style={[styles.chart_bar, { height }]} />
              </View>
              <Text style={styles.chart_label}>{day.label}</Text>
              <View style={[styles.today_line, isToday && styles.today_line_active]} />
            </View>
          )
        })}
      </View>

      <View style={styles.weekly_bottom}>
        <Text style={styles.weekly_note}>Target: {weekly.targetPerDayMin} min/day</Text>
        <Text style={styles.weekly_note}>
          {remaining > 0 ? `${formatNumber(remaining)} min to weekly goal` : 'Weekly goal reached'}
        </Text>
      </View>
    </View>
  )
}

/* -------------------------------- Component ----------------------------- */

const GoalsTab = () => {
  const [data, setData] = useState<GoalsData | null>(null)
  const [loading, setLoading] = useState(true)
  const [refreshing, setRefreshing] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [editingGoal, setEditingGoal] = useState<Goal | null>(null)
  const [targetValue, setTargetValue] = useState('')
  const [exerciseOpen, setExerciseOpen] = useState(false)
  const [exerciseMinutes, setExerciseMinutes] = useState('')
  const [saving, setSaving] = useState(false)

  const load = useCallback(async () => {
    try {
      const data = await fetchGoals()
      setError(null)
      setData(data)
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Something went wrong.')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    // This effect starts an async fetch; its state updates run after the request settles.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    void load()
  }, [load])

  useEffect(() => {
    const timer = setInterval(() => void load(), 30_000)
    return () => clearInterval(timer)
  }, [load])

  const onRefresh = async () => {
    setRefreshing(true)
    await load()
    setRefreshing(false)
  }

  const saveGoal = async () => {
    const target = Number(targetValue)
    if (!editingGoal || !Number.isFinite(target) || target <= 0) {
      Alert.alert('Check target', 'Enter a target greater than zero.')
      return
    }
    setSaving(true)
    try {
      const updated = await updateGoal({ ...editingGoal, target })
      setData((current) => current ? { ...current, goals: current.goals.map((goal) => goal.id === updated.id ? updated : goal) } : current)
      setEditingGoal(null)
    } catch (cause) {
      Alert.alert('Could not save goal', cause instanceof Error ? cause.message : 'Please try again.')
    } finally {
      setSaving(false)
    }
  }

  const saveExercise = async () => {
    const minutes = Number(exerciseMinutes)
    if (!Number.isInteger(minutes) || minutes <= 0) {
      Alert.alert('Check exercise time', 'Enter a whole number of minutes greater than zero.')
      return
    }
    setSaving(true)
    try {
      const today = (new Date().getDay() + 6) % 7
      const currentMinutes = data?.weeklyExercise?.days[today]?.minutes ?? 0
      const weeklyExercise = await logExercise(today, currentMinutes + minutes)
      setData((current) => current ? { ...current, weeklyExercise } : current)
      setExerciseMinutes('')
      setExerciseOpen(false)
    } catch (cause) {
      Alert.alert('Could not save exercise', cause instanceof Error ? cause.message : 'Please try again.')
    } finally {
      setSaving(false)
    }
  }

  const renderGoal = ({ item }: { item: Goal }) => {
    const look = GOAL_STYLES[item.type]
    const percent = getPercent(item)
    const remaining = Math.max(0, item.target - item.current)
    const reached = item.target > 0 && item.current >= item.target

    return (
      <View style={styles.card}>
        <View style={styles.icon_box}>
          <MaterialCommunityIcons name={look.icon} size={48} color={look.iconColor} />
        </View>

        <View style={styles.card_body}>
          <View style={styles.top_row}>
            <View style={styles.name_block}>
              <Text style={styles.goal_name}>{item.name}</Text>
              <Text style={styles.goal_target}>
                Goal: {formatNumber(item.target)} {item.unit}
              </Text>
            </View>

            <View style={styles.value_block}>
              <Text style={styles.goal_value}>{formatNumber(item.current)}</Text>
              <Text style={styles.goal_unit}>{item.unit}</Text>
            </View>
            <TouchableOpacity accessibilityLabel={`Edit ${item.name} target`} onPress={() => { setEditingGoal(item); setTargetValue(String(item.target)) }}>
              <MaterialCommunityIcons name="pencil-outline" size={21} color="#0057a8" />
            </TouchableOpacity>
          </View>

          <View style={styles.track}>
            <View
              style={[styles.fill, { width: `${percent}%`, backgroundColor: look.barColor }]}
            />
          </View>

          <View style={styles.bottom_row}>
            <Text style={styles.percent_text}>{percent}% complete</Text>
            <Text style={[styles.remaining_text, { color: look.textColor }]}>
              {reached ? 'Goal reached' : `${formatNumber(remaining)} ${item.unit} to go`}
            </Text>
          </View>
        </View>
      </View>
    )
  }

  const renderHeader = () => <View><Text style={styles.section_title}>Goal Details</Text><TouchableOpacity style={styles.exercise_button} onPress={() => setExerciseOpen(true)}><MaterialCommunityIcons name="dumbbell" size={18} color="#ffffff" /><Text style={styles.exercise_button_text}>Log exercise</Text></TouchableOpacity></View>

  const renderFooter = () =>
    data?.weeklyExercise ? <WeeklyExerciseCard weekly={data.weeklyExercise} /> : null

  const renderEmpty = () => {
    if (loading) {
      return (
        <View style={styles.empty}>
          <ActivityIndicator size="large" color="#004e7a" />
        </View>
      )
    }

    if (error) {
      return (
        <View style={styles.empty}>
          <Text style={styles.error_text}>{error}</Text>
          <TouchableOpacity style={styles.retry_button} onPress={onRefresh}>
            <Text style={styles.retry_text}>Try again</Text>
          </TouchableOpacity>
        </View>
      )
    }

    return (
      <View style={styles.empty}>
        <MaterialCommunityIcons name="flag-outline" size={48} color="#9e9e9e" />
        <Text style={styles.empty_text}>No goals set yet</Text>
      </View>
    )
  }

  return (
    <>
    <FlatList
      style={styles.flex}
      data={data?.goals ?? []}
      keyExtractor={(item) => item.id}
      renderItem={renderGoal}
      ListHeaderComponent={renderHeader}
      ListFooterComponent={renderFooter}
      ListEmptyComponent={renderEmpty}
      contentContainerStyle={styles.list}
      showsVerticalScrollIndicator={false}
      refreshing={refreshing}
      onRefresh={onRefresh}
    />
    <Modal visible={editingGoal !== null} transparent animationType="slide" onRequestClose={() => setEditingGoal(null)}>
      <View style={styles.modal_backdrop}><View style={styles.modal_card}><Text style={styles.modal_title}>Update {editingGoal?.name ?? 'goal'} target</Text><TextInput style={styles.modal_input} value={targetValue} onChangeText={setTargetValue} keyboardType="decimal-pad" placeholder="Daily target"/><TouchableOpacity style={styles.modal_save} disabled={saving} onPress={() => void saveGoal()}><Text style={styles.modal_save_text}>{saving ? 'Saving…' : 'Save to account'}</Text></TouchableOpacity><TouchableOpacity onPress={() => setEditingGoal(null)}><Text style={styles.modal_cancel}>Cancel</Text></TouchableOpacity></View></View>
    </Modal>
    <Modal visible={exerciseOpen} transparent animationType="slide" onRequestClose={() => setExerciseOpen(false)}>
      <View style={styles.modal_backdrop}><View style={styles.modal_card}><Text style={styles.modal_title}>Log today’s exercise</Text><TextInput style={styles.modal_input} value={exerciseMinutes} onChangeText={setExerciseMinutes} keyboardType="number-pad" placeholder="Minutes"/><TouchableOpacity style={styles.modal_save} disabled={saving} onPress={() => void saveExercise()}><Text style={styles.modal_save_text}>{saving ? 'Saving…' : 'Save exercise'}</Text></TouchableOpacity><TouchableOpacity onPress={() => setExerciseOpen(false)}><Text style={styles.modal_cancel}>Cancel</Text></TouchableOpacity></View></View>
    </Modal>
    </>
  )
}

export default GoalsTab

/* -------------------------------- Styles -------------------------------- */

const styles = StyleSheet.create({
  flex: {
    flex: 1,
  },
  list: {
    paddingHorizontal: 16,
    paddingTop: 18,
    paddingBottom: 24,
    gap: 14,
    flexGrow: 1,
  },
  section_title: {
    fontSize: 18,
    fontWeight: '900',
    color: '#000000',
    marginLeft: 24,
    marginBottom: 2,
  },
  exercise_button: { alignSelf: 'flex-start', flexDirection: 'row', gap: 8, alignItems: 'center', backgroundColor: '#0057a8', paddingHorizontal: 14, paddingVertical: 9, borderRadius: 10, marginBottom: 8 },
  exercise_button_text: { color: '#ffffff', fontWeight: '800' },
  modal_backdrop: { flex: 1, justifyContent: 'center', backgroundColor: 'rgba(0,0,0,0.45)', padding: 20 },
  modal_card: { backgroundColor: '#ffffff', borderRadius: 18, padding: 20, gap: 12 },
  modal_title: { color: '#0a2f55', fontSize: 19, fontWeight: '900' },
  modal_input: { height: 46, borderWidth: 1, borderColor: '#aab2b9', borderRadius: 10, paddingHorizontal: 12, color: '#111111' },
  modal_save: { backgroundColor: '#0057a8', padding: 13, borderRadius: 12, alignItems: 'center' },
  modal_save_text: { color: '#ffffff', fontWeight: '900' },
  modal_cancel: { textAlign: 'center', color: '#59636e', padding: 6 },

  // Goal card
  card: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 12,
    backgroundColor: '#efefef',
    borderRadius: 14,
    paddingVertical: 14,
    paddingHorizontal: 14,
  },
  icon_box: {
    width: 52,
    justifyContent: 'center',
    alignItems: 'center',
  },
  card_body: {
    flex: 1,
    gap: 8,
  },
  top_row: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'flex-start',
  },
  name_block: {
    gap: 2,
  },
  goal_name: {
    fontSize: 13,
    fontWeight: '900',
    color: '#000000',
  },
  goal_target: {
    fontSize: 9,
    color: '#757575',
  },
  value_block: {
    alignItems: 'center',
  },
  goal_value: {
    fontSize: 18,
    fontWeight: '900',
    color: '#000000',
  },
  goal_unit: {
    fontSize: 9,
    fontStyle: 'italic',
    color: '#757575',
  },
  track: {
    height: 14,
    borderRadius: 7,
    backgroundColor: '#d0d0d0',
    overflow: 'hidden',
  },
  fill: {
    height: '100%',
    borderRadius: 7,
  },
  bottom_row: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  percent_text: {
    fontSize: 11,
    color: '#424242',
  },
  remaining_text: {
    fontSize: 11,
    fontWeight: '900',
  },

  // Weekly exercise
  weekly_card: {
    backgroundColor: '#efefef',
    borderRadius: 14,
    paddingVertical: 14,
    paddingHorizontal: 16,
    gap: 10,
  },
  weekly_top: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  weekly_title: {
    fontSize: 13,
    fontWeight: '900',
    color: '#000000',
  },
  weekly_total: {
    fontSize: 13,
    fontWeight: '900',
    color: '#000000',
  },
  chart: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    paddingHorizontal: 4,
  },
  chart_column: {
    alignItems: 'center',
    gap: 4,
  },
  chart_bar_area: {
    height: CHART_HEIGHT,
    justifyContent: 'flex-end',
  },
  chart_bar: {
    width: 28,
    borderRadius: 8,
    backgroundColor: '#bdbdbd',
  },
  chart_label: {
    fontSize: 10,
    color: '#424242',
  },
  today_line: {
    width: 28,
    height: 3,
    borderRadius: 2,
    backgroundColor: 'transparent',
  },
  today_line_active: {
    backgroundColor: '#1565c0',
  },
  weekly_bottom: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  weekly_note: {
    fontSize: 11,
    color: '#424242',
  },

  // Empty / error states
  empty: {
    alignItems: 'center',
    justifyContent: 'center',
    paddingTop: 40,
    gap: 12,
  },
  empty_text: {
    fontSize: 16,
    fontWeight: '600',
    color: '#757575',
  },
  error_text: {
    fontSize: 16,
    color: '#333333',
    textAlign: 'center',
    paddingHorizontal: 24,
  },
  retry_button: {
    backgroundColor: '#0057a8',
    paddingVertical: 12,
    paddingHorizontal: 30,
    borderRadius: 20,
  },
  retry_text: {
    color: '#ffffff',
    fontWeight: 'bold',
    fontSize: 16,
  },
})
