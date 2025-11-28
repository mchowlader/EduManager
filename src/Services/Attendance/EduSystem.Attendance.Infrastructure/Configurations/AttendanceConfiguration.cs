using EduSystem.Attendance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduSystem.ApplicationUsers.Infrastructure.Configurations;

public class AttendancesConfiguration : IEntityTypeConfiguration<Attendances>
{
    public void Configure(EntityTypeBuilder<Attendances> builder)
    {
        // Table
        builder.ToTable("Attendances");

        // Primary Key
        builder.HasKey(a => a.Id);

        // Properties
        builder.Property(a => a.AttendenctUserId)
            .IsRequired();

        builder.Property(a => a.AttendanceAt)
            .IsRequired();

        // Index for quick lookup by user and date (optional but recommended)
        builder.HasIndex(a => new { a.AttendenctUserId, a.AttendanceAt })
            .HasDatabaseName("IX_Attendances_User_Date");
    }
}
