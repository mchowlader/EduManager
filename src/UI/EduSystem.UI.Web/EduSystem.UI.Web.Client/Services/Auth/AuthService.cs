using EduSystem.UI.Web.Client.Models.AuthClient;
using EduSystem.UI.Web.Client.Models.Common;
using EduSystem.UI.Web.Client.Services.Base;

namespace EduSystem.UI.Web.Client.Services.Auth;

public class AuthService : IAuthService
{
    private readonly IApiService _apiService;
    private readonly IAuthManager _authManager;

    public AuthService(IApiService apiService, IAuthManager authManager)
    {
        _apiService = apiService;
        _authManager = authManager;
    }

    public async Task<ApiResponse<LoginData>> LoginAsync(LoginRequest request)
    {
        var result = await _apiService.PostAsync<LoginRequest, LoginData>("/api/identity/v1/auth/login", request, allowAnonymous: true);

        if (result.Success && result.Data != null)
        {
            await _authManager.MarkUserAsAuthenticated(new LoginResponse
            {
                Success = true,
                Message = result.Message,
                Data = result.Data
            });
        }

        return result;
    }

    public async Task<ApiResponse<LoginData>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        return await _apiService.PostAsync<RefreshTokenRequest, LoginData>("/api/identity/v1/auth/refresh-token", request, allowAnonymous: true);
    }

    public async Task<ApiResponse<string>> RegisterAsync(RegisterRequest request)
    {
         var result = await _apiService.PostAsync<RegisterRequest, string>("/api/identity/v1/tenants/register", request, allowAnonymous: true);

        return result;
    }

    public async Task LogoutAsync()
    {
        await _authManager.MarkUserAsLoggedOut();
    }
}
