using EduSystem.ApplicationUsers.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EduSystem.ApplicationUsers.Infrastructure.Contexts;

public class AppUserDbContext : DbContext
{
    public AppUserDbContext(DbContextOptions<AppUserDbContext> options) : base(options) {}

    public DbSet<Student> Students { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<Family> Families { get; set; }
    public DbSet<Address> Address { get; set; }
}