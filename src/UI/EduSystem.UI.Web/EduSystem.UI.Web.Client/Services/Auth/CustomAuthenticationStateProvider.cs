using System.Security.Claims;
using System.Text.Json;
using EduSystem.UI.Web.Client.Models.AuthClient;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace EduSystem.UI.Web.Client.Services.Auth;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider, IAuthManager
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public CustomAuthenticationStateProvider(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            // Check if JSRuntime is available - we attempt to get the token.
            // During SSR/Pre-rendering this might fail or return null.

            var token = await GetTokenAsync();

            if (string.IsNullOrEmpty(token))
                return new AuthenticationState(_anonymous);

            var claims = ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "jwt", ClaimTypes.Name, ClaimTypes.Role);
            var user = new ClaimsPrincipal(identity);

            return new AuthenticationState(user);
        }
        catch (JSDisconnectedException)
        {
            // Browser disconnected - return anonymous
            return new AuthenticationState(_anonymous);
        }
        catch (TaskCanceledException)
        {
            // Task cancelled (usually during pre-rendering) - return anonymous
            return new AuthenticationState(_anonymous);
        }
        catch (Exception)
        {
            return new AuthenticationState(_anonymous);
        }
    }

    public async Task MarkUserAsAuthenticated(LoginResponse loginResponse)
    {
        try
        {
            // Store tokens
            await SetLocalStorageItemAsync("authToken", loginResponse.Data.AccessToken);
            await SetLocalStorageItemAsync("refreshToken", loginResponse.Data.RefreshToken);

            // Store user info
            var userJson = JsonSerializer.Serialize(loginResponse.Data.User);
            await SetLocalStorageItemAsync("userInfo", userJson);

            // Store tenant info
            var tenantJson = JsonSerializer.Serialize(loginResponse.Data.Tenant);
            await SetLocalStorageItemAsync("tenantInfo", tenantJson);

            // Create authenticated user
            var claims = ParseClaimsFromJwt(loginResponse.Data.AccessToken);
            var identity = new ClaimsIdentity(claims, "jwt", ClaimTypes.Name, ClaimTypes.Role);
            var user = new ClaimsPrincipal(identity);

            // Set a cookie for server-side sync (Slightly simplified for demo, in production use secure flags)
            await SetCookieAsync("edu_auth_token", loginResponse.Data.AccessToken, 12);

            // Notify authentication state changed
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AUTH] Error marking user as authenticated: {ex.Message}");
            throw;
        }
    }

    public async Task MarkUserAsLoggedOut()
    {
        try
        {
            // Clear all stored data
            await RemoveLocalStorageItemAsync("authToken");
            await RemoveLocalStorageItemAsync("refreshToken");
            await RemoveLocalStorageItemAsync("userInfo");
            await RemoveLocalStorageItemAsync("tenantInfo");

            // Remove server-side sync cookie
            await RemoveCookieAsync("edu_auth_token");

            // Notify that authentication state has changed
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AUTH] Error during logout: {ex.Message}");
        }
    }

    public async Task<string> GetTokenAsync()
    {
        try
        {
            return await GetLocalStorageItemAsync("authToken") ?? string.Empty;
        }
        catch (JSDisconnectedException)
        {
            Console.WriteLine("[AUTH] JS disconnected while getting token");
            return string.Empty;
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("[AUTH] Task cancelled while getting token");
            return string.Empty;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AUTH] Error getting token: {ex.Message}");
            return string.Empty;
        }
    }

    public async Task<UserInfo?> GetUserInfoAsync()
    {
        try
        {
            var userJson = await GetLocalStorageItemAsync("userInfo");

            if (string.IsNullOrEmpty(userJson))
                return null;

            return JsonSerializer.Deserialize<UserInfo>(userJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JSDisconnectedException)
        {
            Console.WriteLine("[AUTH] JS disconnected while getting user info");
            return null;
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("[AUTH] Task cancelled while getting user info");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AUTH] Error getting user info: {ex.Message}");
            return null;
        }
    }

    public async Task<TenantInfo?> GetTenantInfoAsync()
    {
        try
        {
            var tenantJson = await GetLocalStorageItemAsync("tenantInfo");

            if (string.IsNullOrEmpty(tenantJson))
                return null;

            return JsonSerializer.Deserialize<TenantInfo>(tenantJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JSDisconnectedException)
        {
            Console.WriteLine("[AUTH] JS disconnected while getting tenant info");
            return null;
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("[AUTH] Task cancelled while getting tenant info");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AUTH] Error getting tenant info: {ex.Message}");
            return null;
        }
    }

    #region LocalStorage Helper Methods

    private async Task<string?> GetLocalStorageItemAsync(string key)
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
        }
        catch (JSDisconnectedException)
        {
            return null;
        }
        catch (TaskCanceledException)
        {
            return null;
        }
    }

    private async Task SetLocalStorageItemAsync(string key, string value)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
        }
        catch (JSDisconnectedException)
        {
            Console.WriteLine($"[AUTH] JS disconnected while setting {key}");
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine($"[AUTH] Task cancelled while setting {key}");
        }
    }

    private async Task RemoveLocalStorageItemAsync(string key)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }
        catch (JSDisconnectedException)
        {
            Console.WriteLine($"[AUTH] JS disconnected while removing {key}");
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine($"[AUTH] Task cancelled while removing {key}");
        }
    }

    private async Task SetCookieAsync(string name, string value, int hours)
    {
        try
        {
            var expires = DateTime.Now.AddHours(hours).ToString("R");
            var cookie = $"{name}={value}; expires={expires}; path=/; SameSite=Lax; Secure";
            await _jsRuntime.InvokeVoidAsync("eval", $"document.cookie = '{cookie}'");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AUTH] Error setting cookie: {ex.Message}");
        }
    }

    private async Task RemoveCookieAsync(string name)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("eval", $"document.cookie = '{name}=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/; SameSite=Lax; Secure'");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AUTH] Error removing cookie: {ex.Message}");
        }
    }

    #endregion

    #region JWT Token Parsing

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        return JwtClaimParser.ParseClaimsFromJwt(jwt);
    }

    private byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }

        return Convert.FromBase64String(base64);
    }

    #endregion
}
