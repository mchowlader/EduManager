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
            .IsRequired()
            .HasMaxLength(15);

        builder.Property(t => t.Email)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Designation)
            .IsRequired()
            .HasMaxLength(100);

        // ✅ Foreign Key Properties (Explicitly nullable)
        builder.Property(t => t.PresentAddressId)
            .HasColumnName("PresentAddressId") // ✅ Column name fix
            .HasColumnType("bigint")
            .IsRequired(false);

        builder.Property(t => t.PermanentAddressId)
            .HasColumnType("bigint")
            .IsRequired(false);

        // ✅ Address Relationships (Restrict instead of SetNull)
        builder.HasOne(t => t.PresentAddress)
            .WithMany()
            .HasForeignKey(t => t.PresentAddressId) // ✅ Use property instead of string
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.PermanentAddress)
            .WithMany()
            .HasForeignKey(t => t.PermanentAddressId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // ✅ Family Relationship (Cascade for dependent entities)
        builder.HasMany(t => t.FamilyInfos)
            .WithOne(f => f.Teacher)
            .HasForeignKey(f => f.TeacherId)
            .OnDelete(DeleteBehavior.Cascade); // ✅ Cascade instead of SetNull

        // Indexes
        builder.HasIndex(t => t.Email).IsUnique();
        builder.HasIndex(t => t.Phone);
        builder.HasIndex(t => t.Name);
    }
}
