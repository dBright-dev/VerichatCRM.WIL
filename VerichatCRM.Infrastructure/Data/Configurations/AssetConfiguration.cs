using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using VerichatCRM.Core.Entities;

namespace VerichatCRM.Infrastructure.Data.Configurations
{
    public class AssetConfiguration : IEntityTypeConfiguration<Asset>
    {
        public void Configure(EntityTypeBuilder<Asset> builder)
        {
            builder.HasKey(a => a.AssetId);
            builder.Property(a => a.SerialNumber).IsRequired().HasMaxLength(100);
            builder.HasIndex(a => a.SerialNumber).IsUnique();
            builder.Property(a => a.Model).HasMaxLength(200);
            builder.Property(a => a.Manufacturer).HasMaxLength(200);
            builder.Property(a => a.Status).HasMaxLength(30);

            builder.OwnsMany(a => a.ServiceLogs, l =>
            {
                l.WithOwner().HasForeignKey(x => x.AssetId);
                l.HasKey(x => x.LogId);
                l.Property(x => x.Description).HasMaxLength(2000);
                l.Property(x => x.Outcome).HasMaxLength(500);
                l.Property(x => x.PartsReplaced).HasMaxLength(500);
            });

            builder.HasIndex(a => a.ClientId);
            builder.HasIndex(a => a.WarrantyExpiry);
        }
    }
}
