using EduSystem.ApplicationUsers.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduSystem.ApplicationUsers.Infrastructure.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        // Table
        builder.ToTable("Students");

        // Primary Key
        builder.HasKey(s => s.Id);

        // Properties
        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Phone)
            .HasMaxLength(15);

        builder.Property(s => s.DateOfBirth)
            .IsRequired();

        builder.Property(s => s.DateOfBirthNo)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Class)
            .IsRequired();

        builder.Property(s => s.Department)
            .IsRequired();

        // Addresses
        builder.HasOne(s => s.PresentAddress)
            .WithMany()
            .HasForeignKey(s => s.PresentAddressId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(s => s.PermanentAddress)
            .WithMany()
            .HasForeignKey(s => s.PermanentAddressId)
            .OnDelete(DeleteBehavior.SetNull);

        // Family relationship
        builder.HasMany(s => s.FamilyInfos)
            .WithOne(f => f.Student)
            .HasForeignKey(f => f.StudentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
