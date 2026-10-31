using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VerichatCRM.Core.Entities;

namespace VerichatCRM.Infrastructure.Data.Configurations
{
    public class DealConfiguration : IEntityTypeConfiguration<Deal>
    {
        public void Configure(EntityTypeBuilder<Deal> builder)
        {
            builder.HasKey(d => d.DealId);
            builder.Property(d => d.Stage).IsRequired().HasMaxLength(50);
            builder.Property(d => d.Revenue).HasPrecision(18, 2);
            builder.Property(d => d.Notes).HasMaxLength(4000);
            builder.HasIndex(d => d.ClientId);
            builder.HasIndex(d => d.Stage);

            // The Client -> Deal delete behaviour is decided once, in
            // AppDbContext.OnModelCreating, not here.
        }
    }
}
