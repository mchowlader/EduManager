using EduSystem.ApplicationUsers.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduSystem.ApplicationUsers.Infrastructure.Configurations;

public class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> builder)
    {
        // Table
        builder.ToTable("Teachers");

        // Primary Key
        builder.HasKey(t => t.Id);

        // Properties
        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Phone)
            .HasMaxLength(15);

        builder.Property(t => t.Email)
            .HasMaxLength(100);

        builder.Property(t => t.Designation)
            .HasMaxLength(100);

        // Addresses
        builder.HasOne(t => t.PresentAddress)
            .WithMany()
            .HasForeignKey("PresentAddressId")
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.PermanentAddress)
            .WithMany()
            .HasForeignKey("PermanentAddressId")
            .OnDelete(DeleteBehavior.SetNull);

        // Family relationship
        builder.HasMany(t => t.FamilyInfos)
            .WithOne(f => f.Teacher)
            .HasForeignKey(f => f.TeacherId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
