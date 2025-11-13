namespace EduSystem.Identity.Application.IService;

public interface ITenantDatabaseProvisioner
{
    Task<string> CreateDatabaseAsync(string tenantSlug);
    Task DropDatabaseAsync(string tenantSlug);
}