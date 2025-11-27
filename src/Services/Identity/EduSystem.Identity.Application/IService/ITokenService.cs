using System.Security.Claims;
using EduSystem.Identity.Domain.Entities;
using EduSystem.Identity.Shared.Common;
using EduSystem.Shared.Infrastructure.Authentication;

namespace EduSystem.Identity.Application.IService;

public interface ITokenService
{
    string GenerateAccessToken(User user, Tenant tenant);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    Task<bool> ValidateRefreshTokenAsync(Guid userId, string refreshToken);
}
