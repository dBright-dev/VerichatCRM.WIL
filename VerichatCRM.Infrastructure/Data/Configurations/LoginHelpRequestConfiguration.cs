using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VerichatCRM.Core.Entities;

namespace VerichatCRM.Infrastructure.Data.Configurations
{
    public class LoginHelpRequestConfiguration : IEntityTypeConfiguration<LoginHelpRequest>
    {
        public void Configure(EntityTypeBuilder<LoginHelpRequest> builder)
        {
            builder.HasKey(h => h.Id);
            builder.Property(h => h.Name).IsRequired().HasMaxLength(150);
            builder.Property(h => h.Email).IsRequired().HasMaxLength(200);
            builder.Property(h => h.ExpectedRole).HasMaxLength(50);
            builder.Property(h => h.Message).IsRequired().HasMaxLength(2000);
            builder.Property(h => h.Channel).IsRequired().HasMaxLength(40);
            builder.Property(h => h.Status).IsRequired().HasMaxLength(20);

            // The queue is always read as "what is still open, newest first", so the
            // two columns it is filtered and sorted on are indexed together.
            builder.HasIndex(h => new { h.Status, h.CreatedAt });
        }
    }
}
