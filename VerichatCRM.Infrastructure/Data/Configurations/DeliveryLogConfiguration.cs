using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using VerichatCRM.Core.Entities;

namespace VerichatCRM.Infrastructure.Data.Configurations
{
    public class DeliveryLogConfiguration : IEntityTypeConfiguration<DeliveryLog>
    {
        public void Configure(EntityTypeBuilder<DeliveryLog> builder)
        {
            builder.HasKey(d => d.DeliveryId);
            builder.Property(d => d.TrackingNumber).IsRequired().HasMaxLength(30);
            builder.HasIndex(d => d.TrackingNumber).IsUnique();
            builder.Property(d => d.Origin).HasMaxLength(200);
            builder.Property(d => d.Destination).HasMaxLength(200);
            builder.Property(d => d.Status).HasMaxLength(30);
            builder.Property(d => d.WeightKg).HasPrecision(10, 2);

            builder.OwnsMany(d => d.StatusHistory, s =>
            {
                s.WithOwner().HasForeignKey(x => x.DeliveryId);
                s.HasKey(x => x.UpdateId);
                s.Property(x => x.Status).HasMaxLength(30);
                s.Property(x => x.Location).HasMaxLength(200);
                s.Property(x => x.Notes).HasMaxLength(1000);
            });

            builder.HasIndex(d => new { d.ClientId, d.Status });
        }
    }
}
