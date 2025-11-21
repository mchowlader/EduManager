using EduSystem.Attendance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EduSystem.Attendance.Infrastructure.Contexts;

public class AttendanceDbContext : DbContext
{
    public AttendanceDbContext(DbContextOptions<AttendanceDbContext> options)
        : base(options) { }

    public DbSet<Attendances> Attendances { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Attendances>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AttendenctUserId).IsRequired();
            entity.Property(e => e.AttendanceAt).IsRequired();
        });
    }
}
