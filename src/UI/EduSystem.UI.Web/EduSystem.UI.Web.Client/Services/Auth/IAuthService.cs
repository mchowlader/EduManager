using EduSystem.UI.Web.Client.Models.AuthClient;
using EduSystem.UI.Web.Client.Models.Common;

namespace EduSystem.UI.Web.Client.Services.Auth;

public interface IAuthService
{
    Task<ApiResponse<LoginData>> LoginAsync(LoginRequest request);
    Task LogoutAsync();
}
