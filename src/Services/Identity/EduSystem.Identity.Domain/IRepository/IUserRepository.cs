using EduSystem.Identity.Domain.Entities;

namespace EduSystem.Identity.Domain.IRepository;

public
    interface IUserRepository
{
    Task<User> GetByIdAsync(Guid id);
    Task<User> GetByEmailAsync(string email);
    Task<User> CreateAsync(User user);
    Task UpdateAsync(User user);
    Task<IEnumerable<User>> GetByTenantIdAsync(Guid tenantId);
}
