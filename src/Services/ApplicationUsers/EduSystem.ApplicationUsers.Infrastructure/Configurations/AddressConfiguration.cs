using EduSystem.ApplicationUsers.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduSystem.ApplicationUsers.Infrastructure.Configurations;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        // Table
        builder.ToTable("Addresses");

        // Primary Key
        builder.HasKey(a => a.Id);

        // Properties
        builder.Property(a => a.Division)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.District)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Thana)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Village)
            .IsRequired()
            .HasMaxLength(150);
    }
}
