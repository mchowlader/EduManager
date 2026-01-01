using System;
using System.Collections.Generic;
using System.Text;
using EduSystem.ApplicationUsers.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduSystem.ApplicationUsers.Infrastructure.Configurations;

public class ClassesConfiguration : IEntityTypeConfiguration<Classes>
{
    public void Configure(EntityTypeBuilder<Classes> builder)
    {
        builder.ToTable("Classes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasMany(x => x.Students)
            .WithOne(c => c.Classes)
            .HasForeignKey(fk => fk.ClassesId)
            .OnDelete(DeleteBehavior.Restrict);

    }
}
