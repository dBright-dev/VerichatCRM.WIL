using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VerichatCRM.Infrastructure.Data;

namespace VerichatCRM.Infrastructure.Services
{
    public interface IDatabaseInitializer
    {
        Task InitializeAsync();
    }

    public class DatabaseInitializer : IDatabaseInitializer
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<DatabaseInitializer> _logger;

        public DatabaseInitializer(AppDbContext dbContext, ILogger<DatabaseInitializer> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            try
            {
                _logger.LogInformation("Starting database migrations...");
                await _dbContext.Database.MigrateAsync();
                _logger.LogInformation("✅ Database migrations applied successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error applying migrations: {ex.Message}");
                throw;
            }
        }
    }
}
