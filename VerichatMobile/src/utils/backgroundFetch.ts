import * as BackgroundTask from 'expo-background-task';
import * as TaskManager from 'expo-task-manager';
import { API_BASE_URL, getAuthToken, hydrateAuthToken } from '../constants/api';

const ASSIGNMENT_SYNC_TASK = 'verichat-assignment-sync-v2';

TaskManager.defineTask(ASSIGNMENT_SYNC_TASK, async () => {
  try {
    await hydrateAuthToken();
    const token = getAuthToken();
    if (!token) return BackgroundTask.BackgroundTaskResult.Success;

    // Refresh the mobile dashboard endpoint, which includes the signed-in
    // user's current CRM work/assignment summary.
    const response = await fetch(`${API_BASE_URL}/api/mobile/dashboard`, {
      headers: { Authorization: `Bearer ${token}` },
    });
    if (!response.ok) return BackgroundTask.BackgroundTaskResult.Failed;
    await response.arrayBuffer();
    return BackgroundTask.BackgroundTaskResult.Success;
  } catch {
    return BackgroundTask.BackgroundTaskResult.Failed;
  }
});

export async function registerBackgroundAssignmentSync() {
  try {
    const status = await BackgroundTask.getStatusAsync();
    if (status !== BackgroundTask.BackgroundTaskStatus.Available) return;
    const registered = await TaskManager.isTaskRegisteredAsync(ASSIGNMENT_SYNC_TASK);
    if (!registered) {
      await BackgroundTask.registerTaskAsync(ASSIGNMENT_SYNC_TASK, {
        minimumInterval: 60,
      });
    }
  } catch (error) {
    console.warn('[Background sync] Could not register assignment sync:', error);
  }
}
