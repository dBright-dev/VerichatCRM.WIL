import { MaterialCommunityIcons } from '@expo/vector-icons'
import { ComponentProps, useCallback, useEffect, useState } from 'react'
import { ActivityIndicator, FlatList, StyleSheet, Text, TouchableOpacity, View } from 'react-native'
import { fetchGoals, Goal, GoalsData, GoalType, WeeklyExercise } from '../constants/goalsService'

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

  const onRefresh = async () => {
    setRefreshing(true)
    await load()
    setRefreshing(false)
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

  const renderHeader = () => <Text style={styles.section_title}>Goal Details</Text>

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
