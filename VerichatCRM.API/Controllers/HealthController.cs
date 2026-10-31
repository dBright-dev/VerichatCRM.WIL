using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using VerichatCRM.Infrastructure.Data;

namespace VerichatCRM.API.Controllers
{
    /// <summary>
    /// Unauthenticated health + discovery endpoints. Keep these dependency-light
    /// so they answer even when the database is down.
    /// </summary>
    [ApiController]
    [AllowAnonymous]
    public class HealthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly ILogger<HealthController> _logger;

        public HealthController(AppDbContext db, IConfiguration config, ILogger<HealthController> logger)
        {
            _db = db;
            _config = config;
            _logger = logger;
        }

        private async Task<(bool ok, string detail, long latencyMs)> CheckDatabaseAsync()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var ok = await _db.Database.CanConnectAsync();
                sw.Stop();
                return ok
                    ? (true, "Database reachable.", sw.ElapsedMilliseconds)
                    : (false, "CanConnectAsync returned false. Check connection string / firewall.", sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogWarning(ex, "Health database check failed.");
                return (false, ex.GetBaseException().Message, sw.ElapsedMilliseconds);
            }
        }

        /// <summary>Liveness probe — always 200 when the app is running. Never touches the DB.</summary>
        [HttpGet("/health")]
        [HttpGet("/api/health/live")]
        public IActionResult Live() => Ok(new
        {
            status = "Alive",
            service = "VerichatCRM.API",
            timestamp = DateTime.UtcNow
        });

        /// <summary>
        /// Readiness probe — 200 Healthy, 503 Degraded/Unhealthy when the DB is unreachable.
        /// Load balancers / smoke tests should hit this.
        /// </summary>
        [HttpGet("/api/health")]
        public async Task<IActionResult> Health()
        {
            var (dbOk, dbDetail, latencyMs) = await CheckDatabaseAsync();
            var pendingMigrations = Array.Empty<string>();
            string? migrationCheckError = null;
            if (dbOk)
            {
                try
                {
                    pendingMigrations = (await _db.Database.GetPendingMigrationsAsync()).ToArray();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Health migration check failed.");
                    migrationCheckError = "Unable to inspect applied database migrations.";
                }
            }
            var schemaOk = dbOk && migrationCheckError == null && pendingMigrations.Length == 0;
            var jwtConfigured = !string.IsNullOrWhiteSpace(_config["Jwt:Key"]);
            var status = schemaOk && jwtConfigured ? "Healthy" : "Degraded";

            var payload = new
            {
                status,
                service = "VerichatCRM.API",
                timestamp = DateTime.UtcNow,
                version = typeof(Program).Assembly.GetName().Version?.ToString(),
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                checks = new
                {
                    database = new
                    {
                        status = !dbOk ? "Unhealthy"
                            : migrationCheckError != null ? "MigrationCheckFailed"
                            : pendingMigrations.Length > 0 ? "MigrationsPending" : "Healthy",
                        detail = migrationCheckError ?? (pendingMigrations.Length > 0
                            ? $"Database reachable; {pendingMigrations.Length} migration(s) pending: {string.Join(", ", pendingMigrations)}"
                            : dbDetail),
                        latencyMs
                    },
                    jwt = new { status = jwtConfigured ? "Healthy" : "Unhealthy", detail = jwtConfigured ? "Jwt:Key present." : "Jwt:Key MISSING — auth will fail." }
                }
            };
            return schemaOk && jwtConfigured
                ? Ok(payload)
                : StatusCode(StatusCodes.Status503ServiceUnavailable, payload);
        }

        /// <summary>Machine-readable list of every API route. Handy when Swagger UI is disabled in production.</summary>
        [HttpGet("/api")]
        [HttpGet("/api/info")]
        public IActionResult ApiList([FromServices] EndpointDataSource endpoints) => Ok(new
        {
            service = "VerichatCRM.API",
            timestamp = DateTime.UtcNow,
            endpoints = endpoints.Endpoints
                .OfType<RouteEndpoint>()
                .Select(e => new
                {
                    route = e.RoutePattern.RawText,
                    methods = string.Join(",", e.Metadata.OfType<HttpMethodMetadata>().SelectMany(m => m.HttpMethods).Distinct()),
                    requiresAuth = e.Metadata.OfType<AuthorizeAttribute>().Any()
                        && !e.Metadata.OfType<AllowAnonymousAttribute>().Any()
                })
                .Where(e => e.route != null)
                .OrderBy(e => e.route)
                .ToList()
        });
    }
}
