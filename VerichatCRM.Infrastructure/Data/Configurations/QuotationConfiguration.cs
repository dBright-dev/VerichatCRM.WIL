using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VerichatCRM.Core.Entities;

namespace VerichatCRM.Infrastructure.Data.Configurations
{
    public class QuotationConfiguration : IEntityTypeConfiguration<Quotation>
    {
        public void Configure(EntityTypeBuilder<Quotation> builder)
        {
            builder.HasKey(q => q.QuotationId);
            builder.Property(q => q.QuoteReference).IsRequired().HasMaxLength(50);
            builder.Property(q => q.Status).HasMaxLength(30);
            builder.Property(q => q.Terms).HasMaxLength(4000);
            builder.Property(q => q.Subtotal).HasPrecision(18, 2);
            builder.Property(q => q.VatRate).HasPrecision(5, 4);
            builder.Property(q => q.VatAmount).HasPrecision(18, 2);
            builder.Property(q => q.GrandTotal).HasPrecision(18, 2);

            // Two quotations generated in the same second could previously share a
            // reference; blanks are filtered so legacy drafts do not collide.
            builder.HasIndex(q => q.QuoteReference).IsUnique().HasFilter("[QuoteReference] <> N''");
            builder.HasIndex(q => q.ClientId);

            // The Client / Deal / LineItems relationships are declared in
            // AppDbContext.OnModelCreating, which owns the delete behaviour.
        }
    }
}
