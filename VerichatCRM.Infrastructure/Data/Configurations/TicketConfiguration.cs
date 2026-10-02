using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using VerichatCRM.Core.Entities;

namespace VerichatCRM.Infrastructure.Data.Configurations
{
    public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
    {
        public void Configure(EntityTypeBuilder<Ticket> builder)
        {
            builder.HasKey(t => t.TicketId);
            builder.Property(t => t.TrackingNumber).IsRequired().HasMaxLength(30);
            builder.HasIndex(t => t.TrackingNumber).IsUnique();
            builder.Property(t => t.Title).IsRequired().HasMaxLength(200);
            builder.Property(t => t.Description).HasMaxLength(4000);
            builder.Property(t => t.Priority).HasMaxLength(20);
            builder.Property(t => t.Status).HasMaxLength(30);
            builder.Property(t => t.Category).HasMaxLength(50);

            // Store PhotoUrls as JSON column
            builder.Property(t => t.PhotoUrls)
                .HasConversion(
                    v => string.Join(';', v),
                    v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList())
                .Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<string>>(
                    (a, b) => a!.SequenceEqual(b!),
                    a => a.Aggregate(0, (h, s) => HashCode.Combine(h, s.GetHashCode())),
                    a => a.ToList()));

            // Store Activities as owned collection
            builder.OwnsMany(t => t.Activities, a =>
            {
                a.WithOwner().HasForeignKey(x => x.TicketId);
                a.HasKey(x => x.ActivityId);
                a.Property(x => x.Action).HasMaxLength(50);
                a.Property(x => x.FromValue).HasMaxLength(100);
                a.Property(x => x.ToValue).HasMaxLength(100);
                a.Property(x => x.Notes).HasMaxLength(2000);
            });

            builder.HasIndex(t => new { t.ClientId, t.Status });
            builder.HasIndex(t => new { t.AssignedToUserId, t.Status });
            builder.HasIndex(t => t.SlaDeadline);
        }
    }
}
