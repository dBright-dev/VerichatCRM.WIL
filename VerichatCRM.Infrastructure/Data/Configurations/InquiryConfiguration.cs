using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VerichatCRM.Core.Entities;

namespace VerichatCRM.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Only the columns the enquiry queue filters and sorts on, plus the money
    /// column. The text columns are left at nvarchar(max): their length is
    /// enforced by the request DTOs, and narrowing them here would be a second
    /// schema change bundled into a feature the hosted database did not ask for.
    /// </summary>
    public class InquiryConfiguration : IEntityTypeConfiguration<Inquiry>
    {
        public void Configure(EntityTypeBuilder<Inquiry> builder)
        {
            builder.HasKey(i => i.InquiryId);
            builder.Property(i => i.Urgency).HasMaxLength(30);
            builder.Property(i => i.EstimatedValue).HasPrecision(18, 2);

            // SQL Server refuses to index an nvarchar(max) column, so the status
            // has to be narrowed before the queue's sort index can exist. The
            // values come from InquiryStatuses and the longest is nine characters.
            builder.Property(i => i.Status).HasMaxLength(30);

            // The queue is read as "open enquiries, best first", so the status
            // filter and the score sort want the composite index.
            builder.HasIndex(i => new { i.Status, i.Score });
        }
    }
}
