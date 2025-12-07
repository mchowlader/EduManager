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

        builder.Property(s => s.Class)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(s => s.Department)
            .IsRequired()
            .HasConversion<string>();

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
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.Name);
        builder.HasIndex(s => s.DateOfBirthNo).IsUnique();
    }
}
