using EduSystem.Identity.Domain.Entities;
using EduSystem.Identity.Domain.IRepository;
using EduSystem.Identity.Infrastructure.Contexts;
using EduSystem.Identity.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace EduSystem.Identity.Infrastructure.Repositories;

public class UserRepository(IdentityDbContext context) : IUserRepository
{
    public async Task<Result<User>> CreateAsync(User user)
    {
        if (user == null)
            return Result<User>.Failure("User cannot be null.");

        try
        {
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            return Result<User>.Success(user);
        }
        catch (DbUpdateException dbEx)
        {
            // Optional: log dbEx here
            return Result<User>.Failure("Database update failed: " + dbEx.Message);
        }
        catch (Exception ex)
        {
            // Optional: log ex here
            return Result<User>.Failure("An unexpected error occurred: " + ex.Message);
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