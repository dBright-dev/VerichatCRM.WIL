(() => {
  // SignalR for pages that show a live toast (pipeline board, appointments).
  // The origin comes from window.verichatApiUrl, emitted by _Layout from the
  // server's ApiBaseUrl setting. Authentication is the HttpOnly JwtToken cookie,
  // forwarded by withCredentials and read server-side in the API's bearer events,
  // so the JWT is never written into page script.
  const API_BASE = (window.verichatApiUrl || '').replace(/\/$/, '');
  if (!API_BASE || !window.signalR) return;

  const toast = document.getElementById('appointmentToast');
  const connection = new signalR.HubConnectionBuilder()
    .withUrl(`${API_BASE}/hubs/notifications`, { withCredentials: true })
    .withAutomaticReconnect()
    .build();

  connection.on('notification', (notice) => {
    if (toast) {
      toast.textContent = notice?.message || notice?.title || 'A CRM update arrived.';
      toast.style.display = 'block';
      window.setTimeout(() => { toast.style.display = 'none'; }, 6000);
    }
    if (notice?.title?.includes('Appointment') || notice?.title?.includes('Quote Request') || notice?.title?.includes('Quotation')) {
      window.setTimeout(() => window.location.reload(), 900);
    }
  });

  void connection.start().catch(() => {
    // A dead socket should not spam the network; the page still works without it.
    console.warn('[SignalR] Notification hub unavailable at ' + API_BASE);
  });
})();
