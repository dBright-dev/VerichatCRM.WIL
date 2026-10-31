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

            // Filtered so legacy rows that never received a tracking number do not
            // collide with each other; a real tracking number still has to be unique.
            builder.HasIndex(d => d.TrackingNumber).IsUnique().HasFilter("[TrackingNumber] <> N''");
            builder.HasIndex(d => new { d.ClientId, d.Status });
        }
    }
}
