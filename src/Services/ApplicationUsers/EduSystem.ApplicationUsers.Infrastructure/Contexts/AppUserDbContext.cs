using EduSystem.ApplicationUsers.Domain.Entities;
using EduSystem.Shared.Infrastructure.MultiTenancy;
using EduSystem.Shared.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EduSystem.ApplicationUsers.Infrastructure.Contexts;

public class AppUserDbContext : DbContext
{
    public DbSet<Student> Students { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<Family> Families { get; set; }
    public DbSet<Address> Address { get; set; }
    public DbSet<AppUser> AppUser { get; set; }

    private readonly ITenantContext? _tenantContext;
    private readonly string? _masterConnectionString;
    private readonly IConnectionStringEncryptor? _encryptor;

    public AppUserDbContext(DbContextOptions<AppUserDbContext> options) : base(options) { }

    public AppUserDbContext(
        DbContextOptions<AppUserDbContext> options,
        ITenantContext tenantContext,
        IConfiguration configuration,
        IConnectionStringEncryptor encryptor) : base(options)
    {
        _tenantContext = tenantContext;
        _encryptor = encryptor;
        _masterConnectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Master connection string not found");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
            return;

        if (_tenantContext == null || _encryptor == null)
            return; // Skip if tenant context or encryptor is not available

        string? connectionString = null;

        if (!string.IsNullOrWhiteSpace(_tenantContext.ConnectionString))
        {
            if (_encryptor.Decrypt(_tenantContext.ConnectionString, out string decryptedConnection))
                connectionString = decryptedConnection;
        }

        connectionString ??= _masterConnectionString;

        if (!string.IsNullOrWhiteSpace(connectionString))
            optionsBuilder.UseSqlServer(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //automatically apply all configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppUserDbContext).Assembly);
    }
}
