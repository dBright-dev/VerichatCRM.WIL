import { useCallback, useEffect, useMemo, useState } from 'react'
import { ActivityIndicator, FlatList, KeyboardAvoidingView, Platform, StyleSheet, Text, TextInput, TouchableOpacity, View } from 'react-native'
import { useLocalSearchParams } from 'expo-router'
import { useSafeAreaInsets } from 'react-native-safe-area-context'
import { CRMTheme } from '../constants/crmTheme'
import { api } from '../constants/api'
import { subscribeGlobalRefresh } from '../utils/refreshEvents'

type TimelineItem = { id: string; type: string; title: string; text: string; createdAt: string; createdByUserId: string }

export default function ClientTimelineScreen() {
  const { clientId } = useLocalSearchParams<{ clientId: string }>()
  const insets = useSafeAreaInsets()
  const [items, setItems] = useState<TimelineItem[]>([])
  const [note, setNote] = useState('')
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    if (!clientId) return
    try {
      const result = await api<TimelineItem[]>(`/api/client/${encodeURIComponent(clientId)}/timeline`)
      setItems(result ?? [])
      setError(null)
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : 'Could not load client history.')
    } finally {
      setLoading(false)
    }
  }, [clientId])

  useEffect(() => {
    // Initial network load is an external synchronization task.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    void load()
  }, [load])
  useEffect(() => subscribeGlobalRefresh(() => { void load() }), [load])

  const groupedItems = useMemo(() => {
    const groups = new Map<string, TimelineItem[]>()
    for (const item of [...items].sort((a, b) => Date.parse(b.createdAt) - Date.parse(a.createdAt))) {
      const key = new Date(item.createdAt).toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' })
      groups.set(key, [...(groups.get(key) ?? []), item])
    }
    return [...groups.entries()]
  }, [items])

  const saveNote = async () => {
    const text = note.trim()
    if (!text || !clientId || saving) return
    setSaving(true)
    try {
      await api(`/api/client/${encodeURIComponent(clientId)}/notes`, { method: 'POST', body: JSON.stringify({ text }) })
      setNote('')
      await load()
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : 'Could not save the note.')
    } finally {
      setSaving(false)
    }
  }

  return (
    <KeyboardAvoidingView style={styles.screen} behavior={Platform.OS === 'ios' ? 'padding' : undefined}>
      <Text style={[styles.heading, { paddingTop: insets.top + 14 }]}>Client timeline</Text>
      {error && <Text style={styles.error}>{error}</Text>}
      {loading ? <ActivityIndicator style={styles.loader} color={CRMTheme.primary} /> : (
        <FlatList
          style={styles.list}
          contentContainerStyle={styles.listContent}
          data={groupedItems}
          keyExtractor={([day]) => day}
          keyboardShouldPersistTaps="handled"
          ListEmptyComponent={<Text style={styles.empty}>No history yet. Add a note to start the conversation.</Text>}
          renderItem={({ item: [day, dayItems] }) => (
            <View>
              <Text style={styles.day}>{day}</Text>
              {dayItems.map((entry) => (
                <View key={entry.id} style={[styles.bubble, entry.type === 'note' ? styles.noteBubble : styles.eventBubble]}>
                  <Text style={styles.eventTitle}>{entry.title}</Text>
                  <Text style={styles.eventText}>{entry.text}</Text>
                  <Text style={styles.time}>{new Date(entry.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</Text>
                </View>
              ))}
            </View>
          )}
        />
      )}
      <View style={[styles.composer, { paddingBottom: Math.max(insets.bottom, 12) }]}>
        <TextInput value={note} onChangeText={setNote} placeholder="Write a quick note…" multiline maxLength={4000} style={styles.input} accessibilityLabel="Quick note" />
        <TouchableOpacity onPress={() => void saveNote()} disabled={!note.trim() || saving} style={[styles.send, (!note.trim() || saving) && styles.sendDisabled]} accessibilityRole="button" accessibilityLabel="Save note">
          <Text style={styles.sendText}>{saving ? '…' : 'Send'}</Text>
        </TouchableOpacity>
      </View>
    </KeyboardAvoidingView>
  )
}

const styles = StyleSheet.create({
  screen: { flex: 1, backgroundColor: CRMTheme.background },
  heading: { paddingHorizontal: 18, paddingBottom: 14, color: '#172b4d', fontSize: 22, fontWeight: '700' },
  list: { flex: 1 },
  listContent: { paddingHorizontal: 16, paddingBottom: 20 },
  day: { alignSelf: 'center', marginVertical: 12, paddingHorizontal: 12, paddingVertical: 5, overflow: 'hidden', borderRadius: 12, backgroundColor: '#e1e8f0', color: '#53657b', fontSize: 12, fontWeight: '600' },
  bubble: { maxWidth: '88%', marginBottom: 10, padding: 12, borderRadius: 14 },
  noteBubble: { alignSelf: 'flex-end', backgroundColor: '#dceeff', borderBottomRightRadius: 4 },
  eventBubble: { alignSelf: 'flex-start', backgroundColor: '#fff', borderBottomLeftRadius: 4, borderWidth: 1, borderColor: '#e3e9f0' },
  eventTitle: { marginBottom: 3, color: CRMTheme.primary, fontSize: 12, fontWeight: '700' },
  eventText: { color: '#24354b', fontSize: 15, lineHeight: 21 },
  time: { alignSelf: 'flex-end', marginTop: 5, color: '#7b8797', fontSize: 10 },
  empty: { paddingTop: 30, color: '#64748b', textAlign: 'center' },
  loader: { marginTop: 24 },
  error: { paddingHorizontal: 16, paddingVertical: 8, color: '#a02d2d' },
  composer: { flexDirection: 'row', alignItems: 'flex-end', gap: 10, paddingHorizontal: 14, paddingTop: 10, backgroundColor: '#fff', borderTopWidth: 1, borderTopColor: '#e3e9f0' },
  input: { flex: 1, maxHeight: 120, minHeight: 44, paddingHorizontal: 14, paddingVertical: 11, borderWidth: 1, borderColor: '#d5deea', borderRadius: 22, color: '#172b4d', backgroundColor: '#f9fbfd' },
  send: { justifyContent: 'center', alignItems: 'center', height: 44, minWidth: 64, borderRadius: 22, backgroundColor: CRMTheme.primary },
  sendDisabled: { opacity: 0.5 },
  sendText: { color: '#fff', fontWeight: '700' },
})


