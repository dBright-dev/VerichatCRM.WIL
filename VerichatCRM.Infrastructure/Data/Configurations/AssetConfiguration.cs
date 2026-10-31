using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VerichatCRM.Core.Entities;

namespace VerichatCRM.Infrastructure.Data.Configurations
{
    /// <summary>
    /// AssetServiceLog, KeyPersonnel, DeliveryStatusUpdate, TicketActivity and
    /// QuotationLineItem are deliberately NOT declared here with OwnsMany. They are
    /// independent entities in the deployed schema, and redeclaring them as owned
    /// would rewrite those tables.
    /// </summary>
    public class AssetConfiguration : IEntityTypeConfiguration<Asset>
    {
        public void Configure(EntityTypeBuilder<Asset> builder)
        {
            builder.HasKey(a => a.AssetId);
            builder.Property(a => a.SerialNumber).IsRequired().HasMaxLength(100);
            builder.Property(a => a.Model).HasMaxLength(200);
            builder.Property(a => a.Manufacturer).HasMaxLength(200);
            builder.Property(a => a.Status).HasMaxLength(30);

            // Assets are sometimes registered before the serial number is known, so
            // blank rows are filtered out of the unique index instead of colliding.
            builder.HasIndex(a => a.SerialNumber).IsUnique().HasFilter("[SerialNumber] <> N''");
            builder.HasIndex(a => a.ClientId);
            builder.HasIndex(a => a.WarrantyExpiry);
        }
    }
}
