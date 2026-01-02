namespace EduSystem.Shared.Infrastructure.MultiTenancy;

public interface ITenantProvider
{
    Task<IEnumerable<(long Id, string Slug, string EncryptedConnectionString)>> GetActiveTenantsAsync();
}
