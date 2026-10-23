import React, { useEffect, useState } from "react"
import {
  View,
  Text,
  StyleSheet,
  ActivityIndicator,
  TouchableOpacity,
} from "react-native"

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

  useEffect(() => {
    ;(async () => {
      try {
        const resp = await fetch(`/api/salesgoals/my-goals`, {
          method: "GET",
          headers: { "Content-Type": "application/json" },
        })
        const data = await resp.json()
        if (resp.ok) {
          setGoals(data || [])
        } else {
          setError(data.message || "Failed to load goals.")
        }
      } catch (e) {
        setError(e instanceof Error ? e.message : "Failed to load goals.")
      } finally {
        setLoading(false)
      }
    })()
  }, [userId])

  if (loading) {
    return (
      <View style={styles.container}>
        <ActivityIndicator size="large" color="#0057a8" />
      </View>
    )
  }

  if (error) {
    return (
      <View style={styles.errorContainer}>
        <Text style={styles.errorText}>{error}</Text>
        <TouchableOpacity style={styles.retryButton} onPress={() => window.location.reload()}>
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
            <View style={styles.progressRing}>
              <Text style={styles.progressText}>
                {goal.periodLabel}: {percentage}% achieved
              </Text>
            </View>
            <Text style={styles.goalTitle}>{goal.title}</Text>
            <Text style={styles.goalDetails}>
              Target: {goal.target} • Achieved: {goal.achieved}
            </Text>
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
    backgroundColor: "#ffffff",
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
    backgroundColor: "#0057a8",
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
  progressRing: {
    height: 100,
    width: 100,
    borderRadius: 50,
    backgroundColor: "#e0e0e0",
    justifyContent: "center",
    alignItems: "center",
    overflow: "hidden",
    marginBottom: 12,
    position: "relative",
  },
  progressText: {
    fontSize: 24,
    fontWeight: "700",
    color: "#0057a8",
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
  emptyText: {
    fontSize: 14,
    color: "#757575",
    textAlign: "center",
    marginTop: 20,
  },
})

export default SalesGoalCard