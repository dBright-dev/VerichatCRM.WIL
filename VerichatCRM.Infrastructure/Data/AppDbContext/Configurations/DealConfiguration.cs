using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VerichatCRM.Core.Entities;

namespace VerichatCRM.Infrastructure.Data.AppDbContext.Configurations
{
    public class DealConfiguration : IEntityTypeConfiguration<Deal>
    {
        public void Configure(EntityTypeBuilder<Deal> builder)
        {
            builder.ToTable("Deals");
            builder.HasKey(d => d.Id);

            builder.Property(d => d.Value)
                .HasPrecision(18, 2);

            builder.Property(d => d.Stage)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.HasIndex(d => d.Stage);
            builder.HasIndex(d => d.AssignedRepId);
            builder.HasIndex(d => d.ExpectedCloseDate);

            // Deal -> Client foreign key.
            builder.HasOne(d => d.Client)
                .WithMany(c => c.Deals)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.AssignedRep)
                .WithMany(u => u.AssignedDeals)
                .HasForeignKey(d => d.AssignedRepId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
