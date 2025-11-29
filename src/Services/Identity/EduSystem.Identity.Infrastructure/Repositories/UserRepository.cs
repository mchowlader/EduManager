using EduSystem.Identity.Domain.Entities;
using EduSystem.Identity.Domain.IRepository;
using EduSystem.Identity.Infrastructure.Contexts;
using EduSystem.Identity.Shared.Common;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EduSystem.Identity.Infrastructure.Repositories;

public class UserRepository(IdentityDbContext context) : IUserRepository
{
    public async Task AddAsync(User user)
    {
        await context.Users.AddAsync(user);
        //await context.SaveChangesAsync();
    }

    public async Task<bool> IsEmailExistsAsync(string email) =>
        await context.Users.AnyAsync(u => u.Email == email);

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

    public Task<User?> GetByRefreshTokenAsync(string refreshToken)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<User>> GetByRoleAsync(UserRole role)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateLastLoginAsync(Guid userId, DateTime loginTime)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ClearRefreshTokenAsync(Guid userId)
    {
        throw new NotImplementedException();
    }
}
