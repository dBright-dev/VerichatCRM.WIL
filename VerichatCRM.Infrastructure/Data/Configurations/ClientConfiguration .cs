using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VerichatCRM.Core.Entities;

namespace VerichatCRM.Infrastructure.Data.Configurations
{
    public class ClientConfiguration : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            builder.HasKey(c => c.ClientId);
            builder.Property(c => c.ClientName).IsRequired().HasMaxLength(200);
            builder.Property(c => c.Sector).HasMaxLength(100);
            builder.Property(c => c.ContactPerson).HasMaxLength(150);
            builder.Property(c => c.ContactEmail).HasMaxLength(200);
            builder.Property(c => c.ContactPhone).HasMaxLength(30);
            builder.Property(c => c.ContractStatus).HasMaxLength(50);
            builder.HasIndex(c => c.Sector);
            builder.HasIndex(c => c.ContractStatus);

            builder.OwnsMany(c => c.KeyPersonnel, kp =>
            {
                kp.ToTable("KeyPersonnel");
                kp.WithOwner().HasForeignKey("ClientId");
                kp.HasKey(x => x.Id);
            });
        }
    }
}
