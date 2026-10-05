using Microsoft.EntityFrameworkCore;
using VerichatCRM.Core.Entities;

namespace VerichatCRM.Infrastructure.Data
{
    /// <summary>
    /// Seeds a small starting catalogue so a freshly created database is not
    /// completely empty. Both blocks are guarded by AnyAsync, so they only run
    /// once and never overwrite anything a user has since added.
    ///
    /// This deliberately does NOT create users - the first System Administrator
    /// comes from AdminBootstrapper via configuration.
    /// </summary>
    public static class DataSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (!await context.Clients.AnyAsync())
            {
                var clients = new List<Client>
                {
                    new()
                    {
                        ClientId = Guid.NewGuid().ToString(),
                        ClientName = "Anglo American",
                        Sector = "Mining",
                        ContactPerson = "Naledi Mokoena",
                        ContactEmail = "n.mokoena@angloamerican.example",
                        ContactPhone = "+27 11 555 0142",
                        ContractStatus = "Active",
                        Address = "44 Main Street, Johannesburg",
                        LastContactDate = DateTime.UtcNow.AddDays(-2)
                    },
                    new()
                    {
                        ClientId = Guid.NewGuid().ToString(),
                        ClientName = "Nexam Holdings",
                        Sector = "Manufacturing",
                        ContactPerson = "Ryan Fisher",
                        ContactEmail = "r.fisher@nexam.example",
                        ContactPhone = "+27 21 555 0198",
                        ContractStatus = "Prospect",
                        Address = "12 Harbour Road, Cape Town",
                        LastContactDate = DateTime.UtcNow.AddDays(-6)
                    },
                    new()
                    {
                        ClientId = Guid.NewGuid().ToString(),
                        ClientName = "Siemens Healthineers",
                        Sector = "Clinical",
                        ContactPerson = "Anele Dube",
                        ContactEmail = "a.dube@siemens-health.example",
                        ContactPhone = "+27 12 555 0177",
                        ContractStatus = "Active",
                        Address = "88 Health Avenue, Pretoria",
                        LastContactDate = DateTime.UtcNow.AddDays(-1)
                    },
                    new()
                    {
                        ClientId = Guid.NewGuid().ToString(),
                        ClientName = "Landfill Solutions SA",
                        Sector = "Waste Management",
                        ContactPerson = "Karabo Sithole",
                        ContactEmail = "k.sithole@landfillsa.example",
                        ContactPhone = "+27 31 555 0163",
                        ContractStatus = "Inactive",
                        Address = "5 Industrial Park, Durban",
                        LastContactDate = DateTime.UtcNow.AddDays(-30)
                    }
                };

                context.Clients.AddRange(clients);
            }

            if (!await context.Products.AnyAsync())
            {
                var products = new List<Product>
                {
                    new() { SKU = "VC-AM-001", ProductName = "Anoscopy Model - Adult Medium", Category = "Clinical", Price = 12500m, StockQty = 20, LowStockThreshold = 5 },
                    new() { SKU = "VC-CM-003", ProductName = "Crutch - Adjustable Forearm", Category = "Rehab", Price = 1100m, StockQty = 8, LowStockThreshold = 10 },
                    new() { SKU = "VC-TB-005", ProductName = "IV Giving Set - Sterile", Category = "Clinical", Price = 28m, StockQty = 3500, LowStockThreshold = 2000 },
                    new() { SKU = "VC-LB-006", ProductName = "Lab Coat - White Unisex", Category = "Education", Price = 380m, StockQty = 24, LowStockThreshold = 15 },
                    new() { SKU = "VC-SM-013", ProductName = "Surgical Mask - Type IIR Box 50", Category = "Clinical", Price = 180m, StockQty = 180, LowStockThreshold = 500 },
                    new() { SKU = "VC-WA-015", ProductName = "Wheelchair - Lightweight Aluminium", Category = "Rehab", Price = 8900m, StockQty = 22, LowStockThreshold = 25 }
                };

                context.Products.AddRange(products);
            }

            await context.SaveChangesAsync();
        }
    }
}