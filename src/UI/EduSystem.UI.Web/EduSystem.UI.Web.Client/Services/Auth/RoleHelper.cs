using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace EduSystem.UI.Web.Client.Services.Auth;

public class RoleHelper
{
    private readonly AuthenticationStateProvider _authStateProvider;

    public RoleHelper(AuthenticationStateProvider authStateProvider)
    {
        _authStateProvider = authStateProvider;
    }

    /// <summary>
    /// Check if current user has specific role
    /// </summary>
    public async Task<bool> IsInRoleAsync(string role)
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        return user.Identity?.IsAuthenticated == true &&
               user.IsInRole(role);
    }

    /// <summary>
    /// Check if current user has any of the specified roles
    /// </summary>
    public async Task<bool> IsInAnyRoleAsync(params string[] roles)
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user.Identity?.IsAuthenticated != true)
            return false;

        return roles.Any(role => user.IsInRole(role));
    }

    /// <summary>
    /// Get current user's role
    /// </summary>
    public async Task<string?> GetCurrentRoleAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user.Identity?.IsAuthenticated != true)
            return null;

        return user.FindFirst(ClaimTypes.Role)?.Value;
    }

    /// <summary>
    /// Get all roles for current user
    /// </summary>
    public async Task<IEnumerable<string>> GetAllRolesAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user.Identity?.IsAuthenticated != true)
            return Enumerable.Empty<string>();

        return user.FindAll(ClaimTypes.Role).Select(c => c.Value);
    }

    /// <summary>
    /// Get current user ID
    /// </summary>
    public async Task<string?> GetCurrentUserIdAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    /// <summary>
    /// Get current user email
    /// </summary>
    public async Task<string?> GetCurrentUserEmailAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        return user.FindFirst(ClaimTypes.Email)?.Value;
    }

    /// <summary>
    /// Check if user is authenticated
    /// </summary>
    public async Task<bool> IsAuthenticatedAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        return authState.User.Identity?.IsAuthenticated == true;
    }
}

// Extension method for easy registration
public static class RoleHelperExtensions
{
    public static IServiceCollection AddRoleHelper(this IServiceCollection services)
    {
        services.AddScoped<RoleHelper>();
        return services;
    }
}
