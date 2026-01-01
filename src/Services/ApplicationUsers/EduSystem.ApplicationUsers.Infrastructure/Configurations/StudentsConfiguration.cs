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

        builder.Property(s => s.StudentCode)
          .IsRequired()
          .HasMaxLength(100);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.RollNo)
         .IsRequired()
         .HasMaxLength(100);

        builder.Property(s => s.Phone)
            .IsRequired(false)
            .HasMaxLength(15);

        builder.Property(s => s.DateOfBirth)
            .IsRequired(false);

        builder.Property(s => s.DateOfBirthNo)
            .IsRequired(false)
            .HasMaxLength(50);

        builder.HasOne(c => c.Classes)
            .WithMany(s => s.Students)
            .HasForeignKey(c => c.ClassesId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Section)
            .WithMany(se => se.Students)
            .HasForeignKey(s => s.SectionId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

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

        builder.HasOne(s => s.Group)
            .WithMany(s => s.Student)
            .HasForeignKey(s => s.GroupId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.FamilyInfos)
          .WithMany()
          .HasForeignKey(fk => fk.FamilyId)
          .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => t.Id).IsUnique();
        builder.HasIndex(s => s.StudentCode).IsUnique();
        builder.HasIndex(s => s.DateOfBirthNo).IsUnique();
    }
}
