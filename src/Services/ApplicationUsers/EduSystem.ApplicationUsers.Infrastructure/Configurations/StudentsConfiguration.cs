using EduSystem.ApplicationUsers.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduSystem.ApplicationUsers.Infrastructure.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Phone)
            .HasMaxLength(15);

        // ✅ EXPLICIT NULLABLE CONFIGURATION
        builder.Property(s => s.PresentAddressId)
            .HasColumnType("bigint")
            .IsRequired(false);

        builder.Property(s => s.PermanentAddressId)
            .HasColumnType("bigint")
            .IsRequired(false);

        builder.Property(s => s.DateOfBirth)
            .IsRequired();

        builder.Property(s => s.DateOfBirthNo)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasOne(c => c.Classes)
            .WithMany(s => s.Student)
            .HasForeignKey(c => c.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Department)
            .WithMany(s => s.Student)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationships
        builder.HasOne(s => s.PresentAddress)
            .WithMany()
            .HasForeignKey(s => s.PresentAddressId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.PermanentAddress)
            .WithMany()
            .HasForeignKey(s => s.PermanentAddressId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.FamilyInfos)
            .WithOne(f => f.Student)
            .HasForeignKey(f => f.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.StudentId);
        builder.HasIndex(s => s.DateOfBirthNo).IsUnique();
    }
}
