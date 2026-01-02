using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EduSystem.Shared.Infrastructure.MultiTenancy;

public interface ITenantMigrationService<TDbContext> where TDbContext : DbContext
{
    Task MigrateAsync(string encryptedConnectionString, string tenantSlug, CancellationToken cancellationToken = default);
    Task MigrateAllTenantsAsync(IEnumerable<(long Id, string Slug, string EncryptedConnectionString)> tenants, CancellationToken cancellationToken = default);
}
