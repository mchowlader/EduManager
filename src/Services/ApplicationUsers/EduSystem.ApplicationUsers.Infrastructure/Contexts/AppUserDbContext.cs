using System.Linq.Expressions;
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
    public DbSet<Section> Sections { get; set; }

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
        _masterConnectionString = configuration.GetConnectionString("MasterDBConnection")
            ?? throw new InvalidOperationException("Master connection string not found");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
            return;

        string? connectionString = null;

        if (_tenantContext != null && !string.IsNullOrWhiteSpace(_tenantContext.ConnectionString))
        {
            if (_encryptor != null && _encryptor.Decrypt(_tenantContext.ConnectionString, out string decryptedConnection))
            {
                connectionString = decryptedConnection;
            }
            else
            {
                // Fallback to raw connection string if decryption fails or encryptor is missing
                connectionString = _tenantContext.ConnectionString;
            }
        }

        connectionString ??= _masterConnectionString;

        if (!string.IsNullOrEmpty(connectionString))
        {
            optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.CommandTimeout(60);
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
            });
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach(var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if(typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var isDeletedProperty = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var falseContent = Expression.Constant(false);

                var body = Expression.Equal(isDeletedProperty, falseContent);
                var lambda = Expression.Lambda(body, parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }

        //automatically apply all configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppUserDbContext).Assembly);
    }
}
