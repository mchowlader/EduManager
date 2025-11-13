using EduSystem.Identity.Domain.Entities;

namespace EduSystem.Identity.Domain.IRepository;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id);
    Task<Tenant?> GetBySlugAsync(string slug);
    Task<IEnumerable<Tenant>> GetAllAsync();
    Task<Tenant> CreateAsync(Tenant tenant);
    Task UpdateAsync(Tenant tenant);
    Task<bool> ExistsAsync(string slug);
    Task DeleteAsync(Guid id);
}