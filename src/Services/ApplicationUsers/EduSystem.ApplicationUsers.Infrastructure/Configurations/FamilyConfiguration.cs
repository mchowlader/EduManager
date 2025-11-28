using EduSystem.ApplicationUsers.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduSystem.ApplicationUsers.Infrastructure.Configurations;

public class FamilyConfiguration : IEntityTypeConfiguration<Family>
{
    public void Configure(EntityTypeBuilder<Family> builder)
    {
        builder.ToTable("Families");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(f => f.Phone)
            .HasMaxLength(15);

        builder.Property(f => f.Description)
            .HasMaxLength(500);

        // RelationWith enum
        builder.Property(f => f.RelationWith)
            .IsRequired();

        // Optional relationship to Student
        builder.HasOne(f => f.Student)
            .WithMany(s => s.FamilyInfos)
            .HasForeignKey(f => f.StudentId)
            .OnDelete(DeleteBehavior.SetNull);

        // Optional relationship to Teacher
        builder.HasOne(f => f.Teacher)
            .WithMany(t => t.FamilyInfos)
            .HasForeignKey(f => f.TeacherId)
            .OnDelete(DeleteBehavior.SetNull);

        // Optional addresses
        builder.HasOne(f => f.PresentAddress)
            .WithMany()
            .HasForeignKey("PresentAddressId")
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(f => f.PermanentAddress)
            .WithMany()
            .HasForeignKey("PermanentAddressId")
            .OnDelete(DeleteBehavior.SetNull);
    }
}
