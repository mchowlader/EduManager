using System;
using System.Collections.Generic;
using System.Text;
using EduSystem.ApplicationUsers.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduSystem.ApplicationUsers.Infrastructure.Configurations;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.HasKey(e => e.Id);

        // Indexes
        builder.HasIndex(e => e.Email).IsUnique();
        builder.HasIndex(e => e.UserName).IsUnique();

        // Properties
        builder.Property(e => e.Email).HasMaxLength(256).IsRequired();
        builder.Property(e => e.UserName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.PasswordHash).IsRequired();
        builder.Property(e => e.Phone).HasMaxLength(10);
        builder.Property(e => e.SecurityStamp).HasMaxLength(100);

        // Defaults
        builder.Property(e => e.IsActive).HasDefaultValue(true);
        builder.Property(e => e.EmailConfirmed).HasDefaultValue(false);
        builder.Property(e => e.PhoneConfirmed).HasDefaultValue(false);
        builder.Property(e => e.AccessFailedCount).HasDefaultValue(0);
        builder.Property(e => e.LockoutEnabled).HasDefaultValue(true);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
    }
}
