using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using VerichatCRM.Core.Entities;

namespace VerichatCRM.Infrastructure.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.UserId);
            builder.Property(u => u.DisplayName).IsRequired().HasMaxLength(150);
            builder.Property(u => u.Email).IsRequired().HasMaxLength(200);
            builder.HasIndex(u => u.Email).IsUnique();
            builder.Property(u => u.PasswordHash).IsRequired();
            builder.Property(u => u.Role).IsRequired().HasMaxLength(50);

            builder.Property(u => u.ApprovalStatus).IsRequired().HasMaxLength(20);
            builder.Property(u => u.RejectionReason).HasMaxLength(1000);
            builder.Property(u => u.PasswordResetToken).HasMaxLength(256);
            builder.Property(u => u.EmailVerificationToken).HasMaxLength(256);
            builder.Property(u => u.DeviceToken).HasMaxLength(512);

            // The approval queue is one query, run by every reviewer who opens the
            // users screen: the accounts still waiting, newest first.
            builder.HasIndex(u => new { u.ApprovalStatus, u.CreatedAt });
        }
    }
}
