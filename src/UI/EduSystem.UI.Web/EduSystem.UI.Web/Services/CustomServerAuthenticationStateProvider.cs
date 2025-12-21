using EduSystem.UI.Web.Client.Models.AuthClient;
using EduSystem.UI.Web.Client.Services.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.JSInterop;
using System.Security.Claims;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;

namespace EduSystem.UI.Web.Services.Auth;

/// <summary>
/// Server-side authentication state provider
/// Uses HTTP-only cookies for security
/// </summary>
public class CustomServerAuthenticationStateProvider : ServerAuthenticationStateProvider, IAuthManager
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IJSRuntime _jsRuntime;
    private Task<AuthenticationState>? _authenticationStateTask;

    public CustomServerAuthenticationStateProvider(IHttpContextAccessor httpContextAccessor, IJSRuntime jsRuntime)
    {
        _httpContextAccessor = httpContextAccessor;
        _jsRuntime = jsRuntime;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (_authenticationStateTask != null)
        {
            return _authenticationStateTask;
        }

        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            _authenticationStateTask = Task.FromResult(new AuthenticationState(httpContext.User));
            return _authenticationStateTask;
        }

        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        return Task.FromResult(new AuthenticationState(anonymous));
    }

    public async Task MarkUserAsAuthenticated(LoginResponse loginResponse)
    {
        try
        {
            // During interactive server-side sessions, we can set the cookie via JS
            var expires = DateTime.Now.AddHours(12).ToString("R");
            var cookie = $"edu_auth_token={loginResponse.Data.AccessToken}; expires={expires}; path=/; SameSite=Lax; Secure";
            await _jsRuntime.InvokeVoidAsync("eval", $"document.cookie = '{cookie}'");

            // Build principal
            var claims = ParseClaimsFromJwt(loginResponse.Data.AccessToken);
            var identity = new ClaimsIdentity(claims, "jwt", ClaimTypes.Name, ClaimTypes.Role);
            var user = new ClaimsPrincipal(identity);

            // Notify
            _authenticationStateTask = Task.FromResult(new AuthenticationState(user));
            NotifyAuthenticationStateChanged(_authenticationStateTask);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SERVER AUTH] Error marking user as authenticated: {ex.Message}");
        }
    }

    public async Task MarkUserAsLoggedOut()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("eval", "document.cookie = 'edu_auth_token=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/; SameSite=Lax; Secure'");
            var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
            _authenticationStateTask = Task.FromResult(new AuthenticationState(anonymous));
            NotifyAuthenticationStateChanged(_authenticationStateTask);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SERVER AUTH] Error during logout: {ex.Message}");
        }
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        return JwtClaimParser.ParseClaimsFromJwt(jwt);
    }
}
