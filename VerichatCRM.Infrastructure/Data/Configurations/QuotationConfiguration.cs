using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using VerichatCRM.Core.Entities;

namespace VerichatCRM.Infrastructure.Data.Configurations
{
    public class QuotationConfiguration : IEntityTypeConfiguration<Quotation>
    {
        public void Configure(EntityTypeBuilder<Quotation> builder)
        {
            builder.HasKey(q => q.QuotationId);
            builder.Property(q => q.QuoteReference).IsRequired().HasMaxLength(50);
            builder.HasIndex(q => q.QuoteReference).IsUnique();
            builder.Property(q => q.Subtotal).HasPrecision(18, 2);
            builder.Property(q => q.VatRate).HasPrecision(5, 4);
            builder.Property(q => q.VatAmount).HasPrecision(18, 2);
            builder.Property(q => q.GrandTotal).HasPrecision(18, 2);

            builder.HasOne(q => q.Deal)
                   .WithMany(d => d.Quotations)
                   .HasForeignKey(q => q.DealId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(q => q.Client)
                   .WithMany()
                   .HasForeignKey(q => q.ClientId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.OwnsMany(q => q.LineItems, li =>
            {
                li.ToTable("QuotationLineItems");
                li.WithOwner().HasForeignKey("QuotationId");
                li.HasKey(x => x.LineItemId);
                li.Property(x => x.SKU).HasMaxLength(50);
                li.Property(x => x.ProductName).HasMaxLength(200);
                li.Property(x => x.UnitPrice).HasPrecision(18, 2);
            });
        }
    }
}
