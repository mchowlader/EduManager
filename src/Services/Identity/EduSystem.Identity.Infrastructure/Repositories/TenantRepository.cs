using EduSystem.Identity.Domain.Entities;
using EduSystem.Identity.Domain.IRepository;
using EduSystem.Identity.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using System;

namespace EduSystem.Identity.Infrastructure.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly IdentityDbContext _context;

    public TenantRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<Tenant> CreateAsync(Tenant tenant)
    {
        tenant.Id = Guid.NewGuid();
        tenant.CreatedAt = DateTime.UtcNow;
        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();
        return tenant;
    }

    public async Task<Tenant?> GetBySlugAsync(string slug)
    {
        return await _context.Tenants
            .FirstOrDefaultAsync(t => t.Slug == slug && t.IsActive);
    }

    public async Task<bool> ExistsAsync(string slug)
    {
        return await _context.Tenants.AnyAsync(t => t.Slug == slug);
    }

    public async Task<Tenant?> GetByIdAsync(Guid id)
    {
        return await _context.Tenants.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Tenant>> GetAllAsync()
    {
        return await _context.Tenants
            .Where(t => t.IsActive)
            .ToListAsync();
    }

    public async Task UpdateAsync(Tenant tenant)
    {
        _context.Tenants.Update(tenant);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var tenant = await GetByIdAsync(id);
        if (tenant != null)
        {
            tenant.IsActive = false; // Soft delete
            await UpdateAsync(tenant);
        }
    }
}
