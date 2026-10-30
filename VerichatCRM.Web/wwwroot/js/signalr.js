(() => {
  const options = window.verichatSignalR;
  if (!options?.baseUrl || !options?.token || !window.signalR) return;

  const toast = document.getElementById('appointmentToast');
  const connection = new signalR.HubConnectionBuilder()
    .withUrl(`${options.baseUrl}/hubs/notifications`, { accessTokenFactory: () => options.token })
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

  const start = () => connection.start().catch(() => window.setTimeout(start, 5000));
  void start();
})();
