using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VerichatCRM.Core.Entities;

namespace VerichatCRM.Infrastructure.Data.Configurations
{
    public class DeliveryLogConfiguration : IEntityTypeConfiguration<DeliveryLog>
    {
        public void Configure(EntityTypeBuilder<DeliveryLog> builder)
        {
            builder.HasKey(d => d.DeliveryId);
            builder.Property(d => d.TrackingNumber).IsRequired().HasMaxLength(30);
            builder.Property(d => d.Origin).HasMaxLength(200);
            builder.Property(d => d.Destination).HasMaxLength(200);
            builder.Property(d => d.Status).HasMaxLength(30);
            builder.Property(d => d.WeightKg).HasPrecision(10, 2);

            builder.HasIndex(d => d.TrackingNumber).IsUnique();
            builder.HasIndex(d => new { d.ClientId, d.Status });
        }
    }
}
