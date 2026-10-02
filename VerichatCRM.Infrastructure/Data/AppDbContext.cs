using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using VerichatCRM.Core.Entities;

namespace VerichatCRM.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Inventory> Inventory => Set<Inventory>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
        public DbSet<WarehouseAlert> WarehouseAlerts => Set<WarehouseAlert>();
        public DbSet<Client> Clients => Set<Client>();
        public DbSet<Deal> Deals => Set<Deal>();
        public DbSet<Quotation> Quotations => Set<Quotation>();
        public DbSet<Ticket> Tickets => Set<Ticket>();
        public DbSet<Asset> Assets => Set<Asset>();
        public DbSet<DeliveryLog> DeliveryLogs => Set<DeliveryLog>();
        public DbSet<User> Users => Set<User>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Product configuration
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.SKU);
                entity.Property(e => e.ProductName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Price).HasPrecision(18, 2);
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.StockQty);
            });

            // Order configuration
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.OrderId);
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
                entity.OwnsMany(e => e.LineItems, li =>
                {
                    li.Property(l => l.UnitPrice).HasPrecision(18, 2);
                });
                entity.HasIndex(e => e.ClientId);
                entity.HasIndex(e => e.Status);
            });

            // PurchaseOrder configuration
            modelBuilder.Entity<PurchaseOrder>(entity =>
            {
                entity.HasKey(e => e.PurchaseOrderId);
                entity.Property(e => e.TotalCost).HasPrecision(18, 2);
                entity.OwnsMany(e => e.LineItems, li =>
                {
                    li.Property(l => l.UnitCost).HasPrecision(18, 2);
                });
            });

            // WarehouseAlert configuration
            modelBuilder.Entity<WarehouseAlert>(entity =>
            {
                entity.HasKey(e => e.AlertId);
                entity.HasIndex(e => new { e.SKU, e.IsResolved });
            });

            // Inventory configuration
            modelBuilder.Entity<Inventory>(entity =>
            {
                entity.HasKey(e => e.InventoryId);
                entity.HasOne(e => e.Product)
                      .WithMany()
                      .HasForeignKey(e => e.SKU)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
