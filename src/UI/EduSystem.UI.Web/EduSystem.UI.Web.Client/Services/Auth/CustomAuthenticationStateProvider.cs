//using System.Security.Claims;
//using System.Text.Json;
//using EduSystem.UI.Web.Client.Models.AuthClient;
//using Microsoft.AspNetCore.Components.Authorization;
//using Microsoft.JSInterop;

//namespace EduSystem.UI.Web.Client.Services.Auth;

//public class CustomAuthenticationStateProvider : AuthenticationStateProvider
//{
//    private readonly IJSRuntime _runtime;
//    private ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

//    public CustomAuthenticationStateProvider(IJSRuntime runtime)
//    {
//        _runtime = runtime;
//    }

//    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
//    {
//        try
//        {
//            var token = await _runtime.InvokeAsync<string>("localStorage.getItem", "authToken");

//            if (string.IsNullOrEmpty(token))
//                return new AuthenticationState(_anonymous);

//            var claims = ParseClaimsFromJwt(token);
//            var identity = new ClaimsIdentity(claims, "jwt");
//            var user = new ClaimsPrincipal(identity);

//            return new AuthenticationState(user);

//        }
//        catch (Exception)
//        {
//            return new AuthenticationState(_anonymous); ;
//        }
//    }
//    public async Task MarkUserAsAuthenticated(LoginResponse loginResponse)
//    {
//        // Store tokens
//        await _runtime.InvokeVoidAsync("localStorate.setItem", "authToken", loginResponse.Data.AccessToken);
//        await _runtime.InvokeVoidAsync("localStorage.setItem", "refreshToken", loginResponse.Data.RefreshToken);

//        // Store user info
//        var userJson = JsonSerializer.Serialize(loginResponse.Data.User);
//        await _runtime.InvokeVoidAsync("localStorage.setItem", "userInfo", userJson);

//        // Store tenant info
//        var tenantJson = JsonSerializer.Serialize(loginResponse.Data.Tenant);
//        await _runtime.InvokeVoidAsync("localStorage.setItem", "tenantInfo", tenantJson);

//        var claims = ParseClaimsFromJwt(userJson);
//        var identity = new ClaimsIdentity(claims, "jwt");
//        var user = new ClaimsPrincipal(identity);

//        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
//    }
//    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
//    {
//        var claims = new List<Claim>();
//        var payload = jwt.Split('.')[1];

//        var jsonBytes = ParseBase64WithoutPadding(payload);
//        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

//        if(keyValuePairs != null)
//        {
//            // Handle roles - can be string or array
//            if (keyValuePairs.TryGetValue("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", out var roles))
//            {
//                if(roles is JsonElement element)
//                {
//                    if(element.ValueKind == JsonValueKind.String)
//                    {
//                        claims.Add(new Claim(ClaimTypes.Role, element.GetString()!));
//                    }
//                    else if(element.ValueKind == JsonValueKind.Array)
//                    {
//                        foreach (var role in element.EnumerateArray())
//                        {
//                            claims.Add(new Claim(ClaimTypes.Role, role.GetString()!));
//                        }
//                    }
//                }
//            }
//        }

//        // Map standard claims
//        var claimMappings = new Dictionary<string, string>
//        {
//            { "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", ClaimTypes.NameIdentifier },
//            { "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", ClaimTypes.Email },
//            { "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", ClaimTypes.Name },
//        };

//        foreach (var kvp in keyValuePairs)
//        {
//            if (claimMappings.TryGetValue(kvp.Key, out var claimType))
//            {
//                claims.Add(new Claim(claimType, kvp.Value?.ToString() ?? string.Empty));
//            }
//            else if (kvp.Key != "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
//            {
//                claims.Add(new Claim(kvp.Key, kvp.Value?.ToString() ?? string.Empty));
//            }
//        }

//        return claims;
//    }
//    private byte[] ParseBase64WithoutPadding(string base64)
//    {
//        switch(base64.Length % 4)
//        {
//            case 2: base64 += "=="; break;
//            case 3: base64 += "="; break;
//        }

//        return Convert.FromBase64String(base64);
//    }

//    public async Task<string> GetTokenAsync()
//    {
//        try
//        {
//            return await _runtime.InvokeAsync<string>("localStorage.getItem", "authToken");
//        }
//        catch (Exception)
//        {
//            return string.Empty; ;
//        }
//    }

//    public async Task<UserInfo?> GetUserInfoAsync()
//    {
//        try
//        {
//            var userJson = await _runtime.InvokeAsync<string>("localStorage.getItem", "userInfo");
//            if (string.IsNullOrEmpty(userJson)) return null;
//            return JsonSerializer.Deserialize<UserInfo?>(userJson);
//        }
//        catch (Exception)
//        {
//            return null;
//        }
//    }

//    public async Task<TenantInfo?> GetTenantInfoAsync()
//    {
//        try
//        {
//            var tenantJson = await _runtime.InvokeAsync<string>("localStorage.getItem", "tenantInfo");
//            if (string.IsNullOrEmpty(tenantJson)) return null;
//            return JsonSerializer.Deserialize<TenantInfo>(tenantJson);
//        }
//        catch
//        {
//            return null;
//        }
//    }
//}


using System.Security.Claims;
using System.Text.Json;
using EduSystem.UI.Web.Client.Models.AuthClient;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace EduSystem.UI.Web.Client.Services.Auth;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
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
            var identity = new ClaimsIdentity(claims, "jwt");
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
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

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

    #endregion

    #region JWT Token Parsing

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();

        try
        {
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            if (keyValuePairs == null)
                return claims;

            // Handle roles - can be string or array
            if (keyValuePairs.TryGetValue("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", out var roles))
            {
                if (roles is JsonElement element)
                {
                    if (element.ValueKind == JsonValueKind.String)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, element.GetString()!));
                    }
                    else if (element.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var role in element.EnumerateArray())
                        {
                            claims.Add(new Claim(ClaimTypes.Role, role.GetString()!));
                        }
                    }
                }
            }

            // Map standard claims
            var claimMappings = new Dictionary<string, string>
            {
                { "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", ClaimTypes.NameIdentifier },
                { "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", ClaimTypes.Email },
                { "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", ClaimTypes.Name },
                { "sub", ClaimTypes.NameIdentifier },
                { "email", ClaimTypes.Email },
                { "name", ClaimTypes.Name }
            };

            foreach (var kvp in keyValuePairs)
            {
                if (claimMappings.TryGetValue(kvp.Key, out var claimType))
                {
                    claims.Add(new Claim(claimType, kvp.Value?.ToString() ?? string.Empty));
                }
                else if (kvp.Key != "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                {
                    claims.Add(new Claim(kvp.Key, kvp.Value?.ToString() ?? string.Empty));
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AUTH] Error parsing JWT claims: {ex.Message}");
        }

        return claims;
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
