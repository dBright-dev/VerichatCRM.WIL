using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using VerichatCRM.Infrastructure.Data;

namespace VerichatCRM.API;

/// <summary>
/// Creates the EF Core context for migration and publish tooling without
/// booting the web host (which intentionally requires runtime secrets such
/// as the JWT signing key). Database operations still require a real
/// ConnectionStrings:DefaultConnection at runtime.
/// </summary>
public sealed class DesignTimeAppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddUserSecrets<DesignTimeAppDbContextFactory>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        // EF script generation only needs provider/model metadata. A configured
        // connection string is preferred; the fallback is never used by the
        // running API and lets publish generate scripts without deployment secrets.
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=VerichatCRM_DesignTime;Trusted_Connection=True;TrustServerCertificate=True";
        }

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new AppDbContext(options);
    }
}
