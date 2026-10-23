import React from "react"
import { View, Text, StyleSheet, ScrollView } from "react-native"
import SalesGoalCard from "./GoalsScreen"

const Dashboard = () => {
  return (
    <ScrollView
      style={[
        { flex: 1 },
        { padding: 20 },
        { backgroundColor: "#f5f7fa" },
      ]}
    >
      <Text style={styles.title}>Dashboard</Text>
      <Text style={styles.subtitle}>Client Communication & Goals</Text>

      <SalesGoalCard />

      {/* Other dashboard cards would go here */}
      <View style={styles.placeholder} />
    </ScrollView>
  )
}

const styles = StyleSheet.create({
  title: {
    fontSize: 24,
    fontWeight: "900",
    color: "#0a2f55",
    marginBottom: 8,
  },
  subtitle: {
    fontSize: 14,
    color: "#666",
    marginBottom: 20,
  },
  placeholder: {
    height: 100,
    backgroundColor: "#e0e0e0",
    borderRadius: 8,
    marginVertical: 16,
  },
})

export default Dashboard