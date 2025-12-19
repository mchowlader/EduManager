using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

namespace EduSystem.UI.Web.Services.Auth;

/// <summary>
/// Server-side authentication state provider
/// Uses HTTP-only cookies for security
/// </summary>
public class CustomServerAuthenticationStateProvider : ServerAuthenticationStateProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CustomServerAuthenticationStateProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            return Task.FromResult(new AuthenticationState(httpContext.User));
        }

        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        return Task.FromResult(new AuthenticationState(anonymous));
    }
}
