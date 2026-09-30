using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VerichatCRM.Core.Entities;

namespace VerichatCRM.Infrastructure.Data.AppDbContext.Configurations
{
    public class ClientConfiguration : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            builder.ToTable("Clients");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(c => c.Sector)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.ContactPerson)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(c => c.ContactEmail)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(c => c.ContactPhone)
                .HasMaxLength(30);

            builder.Property(c => c.Status)
                .IsRequired()
                .HasMaxLength(20);

            // Client query optimization explicitly: these two are the columns the Client Directory screen actually filters/searches on (sector dropdown, name/contact search).
            builder.HasIndex(c => c.Sector);
            builder.HasIndex(c => c.Name);

            // Client -> User (AssignedRep) foreign key.
            // Restrict, not Cascade.
            // Deleting a Sales Rep should never silently delete their whole client list — reassign clients first.
            builder.HasOne(c => c.AssignedRep)
                .WithMany(u => u.AssignedClients)
                .HasForeignKey(c => c.AssignedRepId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
