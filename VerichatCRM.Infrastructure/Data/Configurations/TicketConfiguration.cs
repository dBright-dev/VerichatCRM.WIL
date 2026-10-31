using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VerichatCRM.Core.Entities;

namespace VerichatCRM.Infrastructure.Data.Configurations
{
    public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
    {
        public void Configure(EntityTypeBuilder<Ticket> builder)
        {
            builder.HasKey(t => t.TicketId);
            builder.Property(t => t.TrackingNumber).IsRequired().HasMaxLength(30);
            builder.Property(t => t.Title).IsRequired().HasMaxLength(200);
            builder.Property(t => t.Description).HasMaxLength(4000);
            builder.Property(t => t.Priority).HasMaxLength(20);
            builder.Property(t => t.Status).HasMaxLength(30);
            builder.Property(t => t.Category).HasMaxLength(50);
            builder.Property(t => t.ResolutionNotes).HasMaxLength(4000);

            // PhotoUrls stays a nvarchar(max) JSON column, which is how EF Core 10
            // maps List<string> by default and how the deployed rows are stored.
            // Do not add the old semicolon value converter here: it would reinterpret
            // every existing JSON array as one long string.

            builder.HasIndex(t => t.TrackingNumber).IsUnique();
            builder.HasIndex(t => new { t.ClientId, t.Status });
            builder.HasIndex(t => new { t.AssignedToUserId, t.Status });
            builder.HasIndex(t => t.SlaDeadline);
        }
    }
}
