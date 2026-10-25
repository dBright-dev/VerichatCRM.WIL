import * as BackgroundFetch from 'expo-background-fetch';
import * as TaskManager from 'expo-task-manager';
import { API_BASE_URL, getAuthToken, hydrateAuthToken } from '../constants/api';

const ASSIGNMENT_SYNC_TASK = 'verichat-assignment-sync';

TaskManager.defineTask(ASSIGNMENT_SYNC_TASK, async () => {
  try {
    await hydrateAuthToken();
    const token = getAuthToken();
    if (!token) return BackgroundFetch.BackgroundFetchResult.NoData;

    // Refresh the mobile dashboard endpoint, which includes the signed-in
    // user's current CRM work/assignment summary.
    const response = await fetch(`${API_BASE_URL}/api/mobile/dashboard`, {
      headers: { Authorization: `Bearer ${token}` },
    });
    if (!response.ok) return BackgroundFetch.BackgroundFetchResult.Failed;
    await response.arrayBuffer();
    return BackgroundFetch.BackgroundFetchResult.NewData;
  } catch {
    return BackgroundFetch.BackgroundFetchResult.Failed;
  }
});

export async function registerBackgroundAssignmentSync() {
  try {
    const status = await BackgroundFetch.getStatusAsync();
    if (status !== BackgroundFetch.BackgroundFetchStatus.Available) return;
    const registered = await TaskManager.isTaskRegisteredAsync(ASSIGNMENT_SYNC_TASK);
    if (!registered) {
      await BackgroundFetch.registerTaskAsync(ASSIGNMENT_SYNC_TASK, {
        minimumInterval: 60 * 60,
        stopOnTerminate: false,
        startOnBoot: true,
      });
    }
  } catch (error) {
    console.warn('[Background sync] Could not register assignment sync:', error);
  }
}
