(() => {
  // ---------------------------------------------------------------------------
  // Global live-notification client, loaded on every page by _Layout.
  //
  // Origin: window.verichatApiUrl (emitted by _Layout from the server's
  // ApiBaseUrl). It used to fall back to window.location.origin, which meant the
  // hub path was requested from the web app - a host that runs no hub - and the
  // socket retried against a 404 every five seconds for the whole session.
  //
  // Auth: the HttpOnly JwtToken cookie, forwarded by withCredentials and read
  // server-side by the API's bearer events. Reading the token from
  // localStorage/document.cookie is gone, because the cookie is no longer
  // script-readable.
  // ---------------------------------------------------------------------------
  const API_BASE = (window.verichatApiUrl || '').replace(/\/$/, '');
  if (!API_BASE || !window.signalR) return;

  // ---------------------------------------------------------------------------
  // Toast notification markup
  // ---------------------------------------------------------------------------
  const createToast = () => {
    const toast = document.createElement('div');
    toast.id = 'verichat-notification-toast';
    toast.className = 'vc-notification-toast vc-toast-hidden';
    toast.innerHTML = `
      <div class="vc-toast-content">
        <span class="vc-toast-icon">🔔</span>
        <div class="vc-toast-body">
          <div class="vc-toast-title" id="verichat-notification-title">New Notification</div>
          <div class="vc-toast-message" id="verichat-notification-message"></div>
        </div>
        <button class="vc-toast-dismiss" aria-label="Dismiss" onclick="this.parentElement.parentElement.classList.add('vc-toast-hidden')">&times;</button>
      </div>
    `;
    document.body.appendChild(toast);
    return toast;
  };

  const toast = createToast();
  const toastTitle = toast.querySelector('#verichat-notification-title');
  const toastMessage = toast.querySelector('#verichat-notification-message');

  const showToast = (title, message) => {
    if (toastTitle) toastTitle.textContent = title || 'New Notification';
    if (toastMessage) toastMessage.textContent = message || '';
    toast.classList.remove('vc-toast-hidden');
    setTimeout(() => toast.classList.add('vc-toast-hidden'), 6000);
  };

  // ---------------------------------------------------------------------------
  // Connect to SignalR Notification Hub
  // ---------------------------------------------------------------------------
  const connection = new signalR.HubConnectionBuilder()
    .withUrl(`${API_BASE}/hubs/notifications`, { withCredentials: true })
    .withAutomaticReconnect({ nextRetryDelayInMilliseconds: (c) => (c.previousRetryCount < 5 ? 2000 * (c.previousRetryCount + 1) : null) })
    .build();

  connection.on('notification', (notice) => {
    if (!notice) return;

    const title = notice.title || notice.message || 'CRM Update';
    const message = notice.message || notice.title || '';

    showToast(title, message);

    // Auto-reload page for critical updates
    if (notice?.title?.includes('Appointment') ||
        notice?.title?.includes('Quote Request') ||
        notice?.title?.includes('Quotation')) {
      setTimeout(() => window.location.reload(), 900);
    }
  });

  connection.on('reconnecting', () => console.log('[SignalR] Reconnecting...'));
  connection.on('reconnected', (connectionId) => console.log('[SignalR] Reconnected:', connectionId));

  // Initial connect: retry a bounded number of times with backoff, then stop.
  // The previous implementation re-scheduled itself every 5 seconds forever, so a
  // misconfigured origin produced an endless request loop on every staff session.
  const MAX_ATTEMPTS = 5;
  let attempts = 0;

  const start = () => {
    attempts += 1;
    return connection.start()
      .then(() => console.log('[SignalR] Connected to notification hub'))
      .catch((err) => {
        if (attempts >= MAX_ATTEMPTS) {
          console.warn('[SignalR] Giving up after ' + attempts + ' attempts:', err);
          return;
        }
        window.setTimeout(start, 2000 * attempts);
      });
  };

  void start();

  // ---------------------------------------------------------------------------
  // Expose connection for other modules (e.g. bell badge counter)
  // ---------------------------------------------------------------------------
  window.verichatNotifications = { connection, showToast };
})();
