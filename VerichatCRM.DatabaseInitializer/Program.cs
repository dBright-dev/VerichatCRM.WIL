using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using VerichatCRM.Infrastructure.Data;
using VerichatCRM.Infrastructure.Services;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("🔄 VerichatCRM Database Initialization Tool");
            Console.WriteLine("==========================================\n");

            // Set up configuration
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            // Set up DI container
            var services = new ServiceCollection();

            // Add DbContext
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Add logging
            services.AddLogging();

            // Add DatabaseInitializer
            services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();

            var serviceProvider = services.BuildServiceProvider();

            // Get the initializer and run it
            using (var scope = serviceProvider.CreateScope())
            {
                var initializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
                Console.WriteLine("📊 Connection String: Server=db70543.databaseasp.net; Database=db70543");
                Console.WriteLine("🔐 Attempting to connect and apply migrations...\n");

                await initializer.InitializeAsync();

                Console.WriteLine("\n✅ Database initialization completed successfully!");
                Console.WriteLine("✨ Your database should now be visible in SQL Server Management Studio");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Error: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"   Inner: {ex.InnerException.Message}");
            Environment.Exit(1);
        }
    }
}
