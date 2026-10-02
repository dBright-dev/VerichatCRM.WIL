using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
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
            builder.HasIndex(d => d.ClientId);
            builder.HasIndex(d => d.Stage);

            builder.HasOne(d => d.Client)
                   .WithMany(c => c.Deals)
                   .HasForeignKey(d => d.ClientId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
