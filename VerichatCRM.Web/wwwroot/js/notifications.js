(() => {
  // ---------------------------------------------------------------------------
  // Configure SignalR connection details.
  // Read JWT from localStorage (set during login) or fall back to cookie.
  // ---------------------------------------------------------------------------
  const API_BASE = (window.verichatApiUrl || '').replace(/\/$/, '') || window.location.origin;
  const storedToken = localStorage.getItem('verichat_jwt_token') || document.cookie.match(/(?:^|; )JwtToken=([^;]*)/)?.[1];
  const TOKEN = storedToken;

  if (!TOKEN || !window.signalR) return;

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
    .withUrl(`${API_BASE}/hubs/notifications`, {
      accessTokenFactory: () => TOKEN,
      withCredentials: true,
    })
    .withAutomaticReconnect()
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

  connection.on('reconnecting', () => {
    console.log('[SignalR] Reconnecting...');
  });

  connection.on('reconnected', (connectionId) => {
    console.log('[SignalR] Reconnected:', connectionId);
  });

  const start = () =>
    connection.start()
      .then(() => console.log('[SignalR] Connected to notification hub'))
      .catch((err) => {
        console.error('[SignalR] Connection failed:', err);
        setTimeout(start, 5000);
      });

  void start();

  // ---------------------------------------------------------------------------
  // Expose connection for other modules (e.g. bell badge counter)
  // ---------------------------------------------------------------------------
  window.verichatNotifications = { connection, showToast };
})();
