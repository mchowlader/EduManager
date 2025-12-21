using EduSystem.UI.Web.Client.Models.AuthClient;

namespace EduSystem.UI.Web.Client.Services.Auth;

public interface IAuthManager
{
    Task MarkUserAsAuthenticated(LoginResponse loginResponse);
    Task MarkUserAsLoggedOut();
    Task<string?> GetRefreshTokenAsync();
    Task<string?> GetAccessTokenAsync();
    Task UpdateTokensAsync(string accessToken, string refreshToken);
}
