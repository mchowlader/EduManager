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

        builder.Property(f => f.RelationWith)
            .IsRequired();

        // FK: StudentId (Guid?)
        builder.Property(f => f.StudentId)
            .IsRequired(false); // EF maps Guid? → uniqueidentifier

        // FK: TeacherId (Guid?)
        builder.Property(f => f.TeacherId)
            .IsRequired(false);

        // FK: PresentAddressId (bigint)
        builder.Property(f => f.PresentAddressId)
            .HasColumnType("bigint")
            .IsRequired(false);

        // FK: PermanentAddressId (bigint)
        builder.Property(f => f.PermanentAddressId)
            .HasColumnType("bigint")
            .IsRequired(false);

        // Relationship: Student
        builder.HasOne(f => f.Student)
            .WithMany(s => s.FamilyInfos)
            .HasForeignKey(f => f.StudentId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // Relationship: Teacher
        builder.HasOne(f => f.Teacher)
            .WithMany(t => t.FamilyInfos)
            .HasForeignKey(f => f.TeacherId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

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
