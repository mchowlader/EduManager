using EduSystem.ApplicationUsers.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduSystem.ApplicationUsers.Infrastructure.Configurations;

public class SectionConfiguration : IEntityTypeConfiguration<Section>
{
    public void Configure(EntityTypeBuilder<Section> builder)
    {
        builder.ToTable("Sections");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasOne(c => c.Classes)
            .WithMany(s => s.Sections)
            .HasForeignKey(fk => fk.ClassesId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.ClassesId, x.Name})
            .IsUnique();
    }
}
