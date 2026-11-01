using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VerichatCRM.Core.Entities;

namespace VerichatCRM.Infrastructure.Data.Configurations
{
    /// <summary>
    /// The quotation's line items had no configuration class at all, which left
    /// UnitPrice with no store type: EF logged "values will be silently truncated if
    /// they do not fit in the default precision and scale" and moved on. A quote line
    /// that loses its decimals is a quote that does not match its own total, so the
    /// price column is given the same 18,2 the parent quotation's money columns use.
    /// </summary>
    public class QuotationLineItemConfiguration : IEntityTypeConfiguration<QuotationLineItem>
    {
        public void Configure(EntityTypeBuilder<QuotationLineItem> builder)
        {
            builder.HasKey(l => l.LineItemId);
            builder.Property(l => l.SKU).IsRequired().HasMaxLength(50);
            builder.Property(l => l.ProductName).IsRequired().HasMaxLength(200);
            builder.Property(l => l.UnitPrice).HasPrecision(18, 2);
        }
    }
}
