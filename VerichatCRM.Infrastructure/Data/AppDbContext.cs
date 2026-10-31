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
        public DbSet<ClientAssignment> ClientAssignments => Set<ClientAssignment>();
        public DbSet<CalibrationSchedule> CalibrationSchedules => Set<CalibrationSchedule>();
        public DbSet<SalesGoal> SalesGoals => Set<SalesGoal>();
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
        public DbSet<ClientTimelineNote> ClientTimelineNotes => Set<ClientTimelineNote>();
        public DbSet<Appointment> Appointments => Set<Appointment>();
        public DbSet<SalesGoal> SalesGoals => Set<SalesGoal>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ClientTimelineNote>(entity =>
            {
                entity.HasKey(note => note.NoteId);
                entity.Property(note => note.Text).IsRequired().HasMaxLength(4000);
                entity.Property(note => note.ClientId).IsRequired();
                entity.Property(note => note.CreatedByUserId).IsRequired();
                entity.HasIndex(note => new { note.ClientId, note.CreatedAt });
            });

            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasKey(appointment => appointment.Id);
                entity.Property(appointment => appointment.ClientId).IsRequired();
                entity.Property(appointment => appointment.RequestedByUserId).IsRequired();
                entity.Property(appointment => appointment.Topic).IsRequired().HasMaxLength(160);
                entity.Property(appointment => appointment.Status).IsRequired().HasMaxLength(24);
                entity.Property(appointment => appointment.Notes).HasMaxLength(4000);
                entity.HasIndex(appointment => new { appointment.ClientId, appointment.ScheduledDate });
                entity.HasIndex(appointment => new { appointment.Status, appointment.ScheduledDate });
            });

            modelBuilder.Entity<SalesGoal>(entity =>
            {
                entity.HasKey(goal => goal.Id);
                entity.Property(goal => goal.UserId).IsRequired();
                entity.Property(goal => goal.Title).IsRequired().HasMaxLength(160);
                entity.Property(goal => goal.Period).IsRequired().HasMaxLength(24);
                entity.Property(goal => goal.PeriodLabel).HasMaxLength(80);
                entity.Property(goal => goal.Target).HasPrecision(18, 2);
                entity.Property(goal => goal.Achieved).HasPrecision(18, 2);
                entity.HasIndex(goal => new { goal.UserId, goal.Period });
            });

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
            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasKey(e => e.ClientId);
                entity.Property(e => e.ClientName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Sector).IsRequired().HasMaxLength(100);
                entity.Property(e => e.SectorType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ContactEmail).HasMaxLength(256);
                entity.HasIndex(e => e.SectorType);
            });

            modelBuilder.Entity<Deal>(entity =>
            {
                entity.HasOne(e => e.Client)
                      .WithMany(c => c.Deals)
                      .HasForeignKey(e => e.ClientId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ClientAssignment>(entity =>
            {
                entity.HasKey(e => e.ClientAssignmentId);
                entity.Property(e => e.SalesRepId).IsRequired().HasMaxLength(64);
                entity.HasIndex(e => new { e.ClientId, e.SalesRepId, e.AssignedDate });

                entity.HasOne(e => e.Client)
                      .WithMany(c => c.Assignments)
                      .HasForeignKey(e => e.ClientId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.SalesRep)
                      .WithMany()
                      .HasForeignKey(e => e.SalesRepId)
                      .HasPrincipalKey(u => u.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<CalibrationSchedule>(entity =>
            {
                entity.HasKey(e => e.ScheduleId);
                entity.Property(e => e.EquipmentId).IsRequired().HasMaxLength(64);
                entity.Property(e => e.AssignedTechId).IsRequired().HasMaxLength(64);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => new { e.ClientId, e.ScheduledDate });
                entity.HasIndex(e => e.Status);

                entity.HasOne(e => e.Client)
                      .WithMany(c => c.CalibrationSchedules)
                      .HasForeignKey(e => e.ClientId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Equipment)
                      .WithMany()
                      .HasForeignKey(e => e.EquipmentId)
                      .HasPrincipalKey(a => a.AssetId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.AssignedTech)
                      .WithMany()
                      .HasForeignKey(e => e.AssignedTechId)
                      .HasPrincipalKey(u => u.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<SalesGoal>(entity =>
            {
                entity.HasKey(e => e.GoalId);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(64);
                entity.Property(e => e.TargetAmount).HasPrecision(18, 2);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => new { e.UserId, e.StartDate, e.EndDate });

                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .HasPrincipalKey(u => u.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Inquiry>(entity =>
            {
                entity.Property(e => e.FollowUpStatus).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.NextFollowUpDate);
                entity.HasIndex(e => e.FollowUpStatus);
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
