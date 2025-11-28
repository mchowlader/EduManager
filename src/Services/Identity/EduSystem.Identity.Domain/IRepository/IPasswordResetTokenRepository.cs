using EduSystem.Identity.Domain.Entities;

namespace EduSystem.Identity.Domain.IRepository;

public interface IPasswordResetTokenRepository
{
    Task<PasswordResetToken?> GetByTokenAsync(string token);
    Task<PasswordResetToken> CreateAsync(PasswordResetToken token);
    Task UpdateAsync(PasswordResetToken token);
    Task DeleteExpiredTokensAsync();
}
