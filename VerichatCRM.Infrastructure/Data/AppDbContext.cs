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
        public DbSet<Inquiry> Inquiries => Set<Inquiry>();
        public DbSet<Quotation> Quotations => Set<Quotation>();
        public DbSet<Ticket> Tickets => Set<Ticket>();
        public DbSet<Asset> Assets => Set<Asset>();
        public DbSet<DeliveryLog> DeliveryLogs => Set<DeliveryLog>();
        public DbSet<User> Users => Set<User>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<MobileDevice> MobileDevices => Set<MobileDevice>();
        public DbSet<VitalReading> VitalReadings => Set<VitalReading>();
        public DbSet<WellnessGoal> WellnessGoals => Set<WellnessGoal>();
        public DbSet<WeeklyExerciseLog> WeeklyExerciseLogs => Set<WeeklyExerciseLog>();
        public DbSet<TelehealthAppointment> TelehealthAppointments => Set<TelehealthAppointment>();
        public DbSet<LabResultRecord> LabResultRecords => Set<LabResultRecord>();
        public DbSet<MobilePrescription> MobilePrescriptions => Set<MobilePrescription>();
        public DbSet<MobileProfile> MobileProfiles => Set<MobileProfile>();
        public DbSet<MobileSettings> MobileSettings => Set<MobileSettings>();
        public DbSet<HealthMetric> HealthMetrics => Set<HealthMetric>();
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

            // Client -> Deals (one cascade path Client -> Deal is kept)
            modelBuilder.Entity<Deal>(entity =>
            {
                entity.HasOne(e => e.Client)
                      .WithMany(c => c.Deals)
                      .HasForeignKey(e => e.ClientId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Quotation links to both Client and Deal. SQL Server forbids
            // two cascade paths Client -> Quotations (direct + via Deals),
            // so both are Restrict: deleting a Client/Deal with Quotations
            // is blocked instead of cascading.
            modelBuilder.Entity<Quotation>(entity =>
            {
                entity.HasOne(e => e.Client)
                      .WithMany()
                      .HasForeignKey(e => e.ClientId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Deal)
                      .WithMany(d => d.Quotations)
                      .HasForeignKey(e => e.DealId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(e => e.LineItems)
                      .WithOne()
                      .HasForeignKey(l => l.QuotationId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
