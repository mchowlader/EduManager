using EduSystem.Identity.Domain.Entities;
using EduSystem.Identity.Domain.IRepository;
using EduSystem.Identity.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace EduSystem.Identity.Infrastructure.Repositories;

public class UserRepository(IdentityDbContext context) : IUserRepository
{
    public async Task<User> CreateAsync(User user)
    {
        try
        {
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();
            return user;
        }
        catch (Exception)
        {
           return null!;
        }
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await context.Users
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await context.Users
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<IEnumerable<User>> GetByTenantIdAsync(Guid tenantId)
    {
        return await context.Users
            .Where(u => u.TenantId == tenantId)
            .Include(u => u.Tenant)
            .ToListAsync();
    }

    public async Task UpdateAsync(User user)
    {
        context.Users.Update(user);
        await context.SaveChangesAsync();
    }
}