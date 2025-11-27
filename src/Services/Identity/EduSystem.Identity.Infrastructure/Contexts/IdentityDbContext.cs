using EduSystem.Identity.Domain.Entities;
using EduSystem.Identity.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace EduSystem.Identity.Infrastructure.Contexts;

public class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Tenant> Tenants { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new TenantConfiguration());

    }
}
