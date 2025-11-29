//using EduSystem.Attendance.Domain.Entities;
//using EduSystem.Shared.Infrastructure.MultiTenancy;
//using EduSystem.Shared.Infrastructure.Security;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Configuration;

//namespace EduSystem.Attendance.Infrastructure.Contexts;

//public class AttendanceDbContext : DbContext
//{
//    public DbSet<Attendances> Attendances { get; set; }

//    private readonly ITenantContext? _tenantContext;
//    private readonly string? _masterConnectionString;
//    private readonly IConnectionStringEncryptor? _encryptor;

//    public AttendanceDbContext(DbContextOptions<AttendanceDbContext> options) : base(options) { }

//    public AttendanceDbContext(
//        DbContextOptions<AttendanceDbContext> options,
//        ITenantContext tenantContext,
//        IConfiguration configuration,
//        IConnectionStringEncryptor encryptor) : base(options)
//    {
//        _tenantContext = tenantContext;
//        _encryptor = encryptor;
//        _masterConnectionString = configuration.GetConnectionString("DefaultConnection")
//            ?? throw new InvalidOperationException("Master connection string not found");
//    }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//    {
//        if (optionsBuilder.IsConfigured)
//            return;

//        if (_tenantContext == null || _encryptor == null)
//            return; // Skip if tenant context or encryptor is not available

//        string? connectionString = null;

//        if (!string.IsNullOrWhiteSpace(_tenantContext.ConnectionString))
//        {
//            if (_encryptor.Decrypt(_tenantContext.ConnectionString, out string decryptedConnection))
//                connectionString = decryptedConnection;
//        }

//        connectionString ??= _masterConnectionString;

//        if (!string.IsNullOrWhiteSpace(connectionString))
//            optionsBuilder.UseSqlServer(connectionString);
//    }



//    protected override void OnModelCreating(ModelBuilder modelBuilder)
//    {
//        base.OnModelCreating(modelBuilder);

//        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AttendanceDbContext).Assembly);
//        //// Apply tenant isolation filter
//        //modelBuilder.Entity<Teacher>().HasQueryFilter(t =>
//        //    _tenantContext.IsSuperAdmin || t.TenantId == _tenantContext.TenantId);

//        //modelBuilder.Entity<Student>().HasQueryFilter(s =>
//        //    _tenantContext.IsSuperAdmin || s.TenantId == _tenantContext.TenantId);
//    }
//}
using EduSystem.Attendance.Domain.Entities;
using EduSystem.Shared.Infrastructure.MultiTenancy;
using EduSystem.Shared.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

public class AttendanceDbContext : DbContext
{
    public DbSet<Attendances> Attendances { get; set; }

    private readonly ITenantContext? _tenantContext;
    private readonly string? _masterConnectionString;
    private readonly IConnectionStringEncryptor? _encryptor;

    public AttendanceDbContext(DbContextOptions<AttendanceDbContext> options)
        : base(options)
    {
    }

    public AttendanceDbContext(
        DbContextOptions<AttendanceDbContext> options,
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
        {
            return;
        }

        string? connectionString = null;

        if (!string.IsNullOrWhiteSpace(_tenantContext.ConnectionString))
        {
            if (_encryptor.Decrypt(_tenantContext.ConnectionString, out string decryptedConnection))
            {
                connectionString = decryptedConnection;
            }
        }

        connectionString ??= _masterConnectionString;

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            optionsBuilder.UseSqlServer(connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AttendanceDbContext).Assembly);

        if (_tenantContext != null)
        {
            // Uncomment when needed
            // modelBuilder.Entity<Teacher>().HasQueryFilter(t =>
            //     _tenantContext.IsSuperAdmin || t.TenantId == _tenantContext.TenantId);
        }
    }
}
