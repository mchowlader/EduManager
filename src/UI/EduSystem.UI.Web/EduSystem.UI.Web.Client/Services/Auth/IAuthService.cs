using EduSystem.UI.Web.Client.Models.AuthClient;
using EduSystem.UI.Web.Client.Models.Common;

namespace EduSystem.UI.Web.Client.Services.Auth;

public interface IAuthService
{
    Task<ApiResponse<LoginData>> LoginAsync(LoginRequest request);
    Task<ApiResponse<LoginData>> RefreshTokenAsync(RefreshTokenRequest request);
    Task<ApiResponse<string>> RegisterAsync(RegisterRequest request);
    Task LogoutAsync();
}
