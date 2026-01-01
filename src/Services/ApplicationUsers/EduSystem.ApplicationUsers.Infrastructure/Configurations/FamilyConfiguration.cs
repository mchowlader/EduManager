using EduSystem.ApplicationUsers.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduSystem.ApplicationUsers.Infrastructure.Configurations;

public class FamilyConfiguration : IEntityTypeConfiguration<Family>
{
    public void Configure(EntityTypeBuilder<Family> builder)
    {
        // Table
        builder.ToTable("Families");

        // Primary Key
        builder.HasKey(f => f.Id);

        // Properties
        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(f => f.Phone)
            .HasMaxLength(15);

        builder.Property(f => f.Description)
            .HasMaxLength(500);

        builder.Property(r => r.RelationId)
            .HasConversion<int>();

        // Relationship: Present Address
        builder.HasOne(f => f.PresentAddress)
            .WithMany()
            .HasForeignKey(f => f.PresentAddressId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: Permanent Address
        builder.HasOne(f => f.PermanentAddress)
            .WithMany()
            .HasForeignKey(f => f.PermanentAddressId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
