using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
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

        /// <summary>
        /// Opens the connection rather than asking <c>CanConnectAsync</c> whether it
        /// could. That helper swallows the <see cref="SqlException"/> and answers
        /// <c>false</c>, so every one of the four ways a database can be unavailable
        /// - wrong password, wrong catalog, no TCP answer, refused TLS session -
        /// printed the same sentence. Both deployed APIs were reporting that sentence
        /// while their real causes were different (one a credential, the other an
        /// identity with no user in the database), and the payload could not tell them
        /// apart.
        /// <para>
        /// The connection is built from the context's connection string rather than
        /// taken from <c>GetDbConnection()</c>. That returns the context's own pooled
        /// connection, and this method's <c>await using</c> would dispose the object
        /// the context is still holding - so the migration probe underneath this one,
        /// running in the same request on the same context, threw "The ConnectionString
        /// property has not been initialized" and every healthy database reported
        /// <c>MigrationCheckFailed</c> even though it was reachable and fully migrated.
        /// </para>
        /// </summary>
        private async Task<(bool ok, string detail, long latencyMs)> CheckDatabaseAsync()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                await using var connection = new SqlConnection(_db.Database.GetConnectionString());
                await connection.OpenAsync();
                sw.Stop();
                return (true, "Database reachable.", sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                sw.Stop();
                // The server's own text names the database user, and this endpoint is
                // anonymous, so the full message is logged and only the classification
                // is returned.
                _logger.LogWarning(ex, "Health database check failed.");
                return (false, Describe(ex), sw.ElapsedMilliseconds);
            }
        }

        private static string Describe(Exception ex)
        {
            SqlException? sql = null;
            for (Exception? step = ex; step != null; step = step.InnerException)
                if (step is SqlException found) { sql = found; break; }

            if (sql is null)
                return $"Database unreachable: {ex.GetBaseException().GetType().Name}.";

            return $"Database unreachable: {DescribeFailure(sql.Number, sql.Class)}. The server's own message is in the API log.";
        }

        /// <summary>
        /// SQL Server's error number says which of the four failures happened, and the
        /// sentence that used to be returned for all of them was the reason two hosts
        /// with different causes looked identical. Only the numbers this project has
        /// actually met are claimed here; anything else reports its number and severity
        /// and leaves the reading to the log line above it.
        /// </summary>
        internal static string DescribeFailure(int number, byte severity) => number switch
        {
            // What both deployed APIs report. SQL gives no error number when the
            // session died before the engine could answer, and severity 20 is the
            // fatal-transport band, so the socket was opened and then lost during
            // login - not a wrong password, which is 18456 at severity 14.
            0 when severity >= 20 => "the connection was established and then dropped before SQL Server answered - a transport or TLS failure at the network layer, not a credential this server rejected",
            18456 => "the server answered and rejected the login - a SQL password that no longer matches the user, or an Entra token whose identity has no user in this database",
            4060 or 916 or 229 => "the login opened a session but this database cannot be opened - wrong catalog, or the user has no access to it",
            18451 => "the server named in the connection string is not one this host can resolve or reach",
            53 or 2 or 20 or 121 or 11001 or 10060 or 10054 or 258 => "no TCP answer from this host - the server is private-network only, or a firewall or egress rule blocks 1433",
            _ => $"SQL error {number} at severity {severity}"
        };

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
