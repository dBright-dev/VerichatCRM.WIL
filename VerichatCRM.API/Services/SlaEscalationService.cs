namespace VerichatCRM.API.Services
{
    public class SlaEscalationService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<SlaEscalationService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

        public SlaEscalationService(IServiceProvider services, ILogger<SlaEscalationService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SLA Escalation Service started. Interval: {Interval}", _interval);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _services.CreateScope();
                    var ticketService = scope.ServiceProvider.GetRequiredService<ITicketService>();

                    var escalated = await ticketService.RunSlaEscalationCheckAsync();
                    if (escalated > 0)
                    {
                        _logger.LogWarning("SLA check complete — {Count} ticket(s) escalated.", escalated);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "SLA escalation check failed.");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}
