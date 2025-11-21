namespace EduSystem.Identity.Application.IService;

public interface ITenantDatabaseProvisioner
{
    //Task<bool> CreateDatabaseAsync(string tenantSlug, out string connectionString);
    Task<(bool Success, string EncryptedConnectionString)> CreateDatabaseAsync(string tenantSlug);
    Task<bool> DropDatabaseAsync(string tenantSlug);
}