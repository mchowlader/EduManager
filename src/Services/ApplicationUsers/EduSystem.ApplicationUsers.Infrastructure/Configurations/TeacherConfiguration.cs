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
            .IsRequired(false)
            .HasMaxLength(100);

        builder.Property(t => t.Designation)
            .IsRequired()
            .HasMaxLength(100);

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

        builder.HasOne(s => s.FamilyInfos)
          .WithMany()
          .HasForeignKey(fk => fk.FamilyId)
          .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(t => t.Id).IsUnique();
        builder.HasIndex(t => t.Email).IsUnique();
        builder.HasIndex(t => t.Phone).IsUnique();
    }
}
