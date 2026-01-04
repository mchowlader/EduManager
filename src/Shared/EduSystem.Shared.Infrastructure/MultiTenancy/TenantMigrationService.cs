using EduSystem.Shared.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduSystem.Shared.Infrastructure.MultiTenancy;

public class TenantMigrationService<TDbContext>(
    IConnectionStringEncryptor encryptor,
    ILogger<TenantMigrationService<TDbContext>> logger)
    : ITenantMigrationService<TDbContext> where TDbContext : DbContext
{
    private readonly IConnectionStringEncryptor _encryptor = encryptor;
    private readonly ILogger<TenantMigrationService<TDbContext>> _logger = logger;
    private const int MaxRetries = 3;

    public async Task MigrateAsync(string encryptedConnectionString, string tenantSlug, CancellationToken cancellationToken = default)
    {
        var retryCount = 0;
        Exception? lastException = null;

        while (retryCount < MaxRetries)
        {
            if (cancellationToken.IsCancellationRequested) return;

            try
            {
                _logger.LogInformation("🔄 Applying migrations for tenant: {TenantSlug} (Attempt {Attempt}/{MaxRetries})",
                    tenantSlug, retryCount + 1, MaxRetries);

                if (!_encryptor.Decrypt(encryptedConnectionString, out var connectionString))
                {
                    throw new InvalidOperationException($"Failed to decrypt connection string for tenant: {tenantSlug}");
                }

                var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();
                optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.CommandTimeout(300);
                    sqlOptions.EnableRetryOnFailure();
                });

                // We need a way to create the DbContext. 
                // Since this is a generic service, we might need a factory or Activator.
                // However, most DbContexts in this project have a constructor that takes DbContextOptions.
                using var dbContext = (TDbContext)Activator.CreateInstance(typeof(TDbContext), optionsBuilder.Options)!;

                await dbContext.Database.MigrateAsync(cancellationToken);

                _logger.LogInformation("✅ Migrations applied successfully for tenant: {TenantSlug}", tenantSlug);
                return;
            }
            catch (Exception ex)
            {
                lastException = ex;
                retryCount++;
                _logger.LogWarning(ex, "❌ Migration failed for tenant: {TenantSlug}. Retry {Attempt}/{MaxRetries}",
                    tenantSlug, retryCount, MaxRetries);

                if (retryCount < MaxRetries)
                {
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount)), cancellationToken);
                }
            }
        }

        _logger.LogError(lastException, "❌❌ FAILED: Could not apply migrations for tenant: {TenantSlug} after {MaxRetries} attempts",
            tenantSlug, MaxRetries);
        throw lastException!;
    }

    public async Task MigrateAllTenantsAsync(IEnumerable<(long Id, string Slug, string EncryptedConnectionString)> tenants, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("🚀 Starting bulk migration for {Count} tenants...", tenants.Count());

        foreach (var tenant in tenants)
        {
            try
            {
                await MigrateAsync(tenant.EncryptedConnectionString, tenant.Slug, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "⚠️ Skipping tenant {TenantSlug} due to migration failure.", tenant.Slug);
                // Continue with other tenants
            }
        }

        _logger.LogInformation("🏁 Bulk migration completed.");
    }
}
