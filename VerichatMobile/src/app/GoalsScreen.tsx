import React, { useCallback, useEffect, useState } from "react"
import {
  View,
  Text,
  StyleSheet,
  TouchableOpacity,
  TextInput,
  Alert,
} from "react-native"
import { api } from "../constants/api"
import { CRMTheme } from '../constants/crmTheme'
import Svg, { Circle } from 'react-native-svg'
import Animated, { useAnimatedProps, useSharedValue, withTiming } from 'react-native-reanimated'
import SkeletonCard from '../components/ui/SkeletonCard'

const AnimatedCircle = Animated.createAnimatedComponent(Circle)

function GoalRing({ percentage, label }: { percentage: number; label: string }) {
  const progress = useSharedValue(0)
  const radius = 43
  const circumference = 2 * Math.PI * radius
  useEffect(() => { progress.value = withTiming(Math.min(Math.max(percentage, 0), 100), { duration: 900 }) }, [percentage, progress])
  const animatedProps = useAnimatedProps(() => ({ strokeDashoffset: circumference * (1 - progress.value / 100) }))
  return (
    <View style={styles.ringContainer}>
      <Svg width={100} height={100} viewBox="0 0 100 100" style={{ transform: [{ rotate: '-90deg' }] }}>
        <Circle cx={50} cy={50} r={radius} stroke={CRMTheme.border} strokeWidth={8} fill="transparent" />
        <AnimatedCircle cx={50} cy={50} r={radius} stroke={CRMTheme.primary} strokeWidth={8} fill="transparent" strokeLinecap="round" strokeDasharray={`${circumference} ${circumference}`} animatedProps={animatedProps} />
      </Svg>
      <Text style={styles.progressText}>{label}: {percentage}%</Text>
    </View>
  )
}

type SalesGoal = {
  id: string
  title: string
  target: number
  achieved: number
  period: "monthly" | "quarterly" | "yearly"
  periodLabel: string
}

type Props = {
  userId?: string
}

const SalesGoalCard = ({ userId }: Props) => {
  const [goals, setGoals] = useState<SalesGoal[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [progressDrafts, setProgressDrafts] = useState<Record<string, string>>({})
  const [savingId, setSavingId] = useState<string | null>(null)

  const loadGoals = useCallback(async () => {
    setLoading(true)
    try {
      const data = await api<SalesGoal[]>("/api/salesgoals/my-goals")
      setGoals(data || [])
      setError(null)
    } catch (e) {
      setError(e instanceof Error ? e.message : "Failed to load goals.")
    } finally {
      setLoading(false)
    }
  }, [])

  const saveProgress = async (goal: SalesGoal) => {
    const achieved = Number(progressDrafts[goal.id] ?? goal.achieved)
    if (!Number.isFinite(achieved) || achieved < 0) { Alert.alert('Check achieved amount', 'Enter a non-negative amount.'); return }
    setSavingId(goal.id)
    try {
      const updated = await api<SalesGoal>(`/api/salesgoals/${encodeURIComponent(goal.id)}/progress`, { method: 'PATCH', body: JSON.stringify({ achieved }) })
      setGoals(current => current.map(item => item.id === goal.id ? updated : item))
      setProgressDrafts(current => ({ ...current, [goal.id]: String(updated.achieved) }))
    } catch (e) { Alert.alert('Could not update quota progress', e instanceof Error ? e.message : 'Please try again.') }
    finally { setSavingId(null) }
  }

  useEffect(() => {
    // This effect starts an async load; state updates happen after the request settles.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    void loadGoals()
  }, [loadGoals, userId])

  if (loading) {
    return (
      <View style={styles.container}>
        <SkeletonCard />
      </View>
    )
  }

  if (error) {
    return (
      <View style={styles.errorContainer}>
        <Text style={styles.errorText}>{error}</Text>
        <TouchableOpacity style={styles.retryButton} onPress={() => void loadGoals()}>
          <Text style={styles.retryText}>Retry</Text>
        </TouchableOpacity>
      </View>
    )
  }

  return (
    <View style={styles.container}>
      {goals.map((goal) => {
        const percentage = Math.round((goal.achieved / goal.target) * 100)
        return (
          <View key={goal.id} style={styles.goalCard}>
            <GoalRing percentage={percentage} label={goal.periodLabel} />
            <Text style={styles.goalTitle}>{goal.title}</Text>
            <Text style={styles.goalDetails}>Target: R {goal.target.toLocaleString()} • Achieved: R {goal.achieved.toLocaleString()}</Text>
            <View style={styles.progressEdit}><TextInput style={styles.progressInput} value={progressDrafts[goal.id] ?? String(goal.achieved)} onChangeText={value => setProgressDrafts(current => ({ ...current, [goal.id]: value }))} keyboardType="decimal-pad" accessibilityLabel={`Achieved amount for ${goal.title}`} /><TouchableOpacity style={styles.progressButton} disabled={savingId === goal.id} onPress={() => void saveProgress(goal)}><Text style={styles.progressButtonText}>{savingId === goal.id ? 'Saving…' : 'Update achieved'}</Text></TouchableOpacity></View>
          </View>
        )
      })}
      {goals.length === 0 && (
        <Text style={styles.emptyText}>No sales goals found</Text>
      )}
    </View>
  )
}

const styles = StyleSheet.create({
  container: {
    padding: 20,
    backgroundColor: CRMTheme.surface,
    borderRadius: 12,
    marginBottom: 16,
  },
  errorContainer: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
    padding: 20,
  },
  errorText: {
    color: "#b3261e",
    fontSize: 14,
    textAlign: "center",
  },
  retryButton: {
    marginTop: 8,
    backgroundColor: CRMTheme.primary,
    paddingVertical: 8,
    paddingHorizontal: 16,
    borderRadius: 6,
  },
  retryText: {
    color: "#fff",
    fontWeight: "bold",
  },
  goalCard: {
    backgroundColor: "#f8f9fa",
    borderRadius: 12,
    padding: 20,
    marginBottom: 12,
  },
  ringContainer: {
    height: 100,
    width: 100,
    justifyContent: "center",
    alignItems: "center",
    marginBottom: 12,
    position: "relative",
  },
  progressText: {
    position: 'absolute',
    fontSize: 12,
    fontWeight: "700",
    color: CRMTheme.primary,
    textAlign: "center",
  },
  goalTitle: {
    fontSize: 16,
    fontWeight: "600",
    color: "#333",
    textAlign: "center",
    marginBottom: 4,
  },
  goalDetails: {
    fontSize: 12,
    color: "#666",
    textAlign: "center",
  },
  progressEdit: { flexDirection: 'row', gap: 8, marginTop: 12 },
  progressInput: { flex: 1, borderWidth: 1, borderColor: '#ccd7e2', borderRadius: 8, paddingHorizontal: 10, color: '#17324d' },
  progressButton: { backgroundColor: CRMTheme.primary, paddingHorizontal: 12, justifyContent: 'center', borderRadius: 8 },
  progressButtonText: { color: '#fff', fontWeight: '800', fontSize: 12 },
  emptyText: {
    fontSize: 14,
    color: "#757575",
    textAlign: "center",
    marginTop: 20,
  },
})

export default SalesGoalCard

