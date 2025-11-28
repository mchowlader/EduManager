using EduSystem.Identity.Domain.Entities;
using EduSystem.Identity.Domain.IRepository;
using EduSystem.Identity.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace EduSystem.Identity.Infrastructure.Repositories;

public class PasswordResetTokenRepository(IdentityDbContext context) : IPasswordResetTokenRepository
{
    private readonly IdentityDbContext _context = context;

    public async Task<PasswordResetToken> CreateAsync(PasswordResetToken token)
    {
        token.CreatedAt = DateTime.UtcNow;
        await _context.PasswordResetTokens.AddAsync(token);
        return token;
    }

    public async Task DeleteExpiredTokensAsync()
    {
        var  expiredTokens = _context.PasswordResetTokens
            .Where(t => t.CreatedAt.AddHours(1) < DateTime.UtcNow)
            .ToListAsync();

        _context.PasswordResetTokens.RemoveRange(expiredTokens.Result);
    }

    public async Task<PasswordResetToken?> GetByTokenAsync(string token)
    {
        return await _context.PasswordResetTokens
           .Include(t => t.User)
           .FirstOrDefaultAsync(t => t.Token == token);
    }

    public async Task UpdateAsync(PasswordResetToken token)
    {
        _context.PasswordResetTokens.Update(token);
        await Task.CompletedTask;
    }
}
