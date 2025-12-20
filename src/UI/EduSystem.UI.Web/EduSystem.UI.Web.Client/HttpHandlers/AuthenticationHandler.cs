using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.Authorization;
using EduSystem.UI.Web.Client.Services.Auth;

namespace EduSystem.UI.Web.Client.HttpHandlers;

/// <summary>
/// HTTP Message Handler যা প্রতিটি request এ automatically Bearer token add করে
/// DelegatingHandler inherit করে - এটা generic AuthenticationHandler নয়
/// </summary>
public class AuthenticationHandler : DelegatingHandler
{
    private readonly IServiceProvider _serviceProvider;

    public AuthenticationHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Login/Register endpoint এ token লাগবে না
        var isAuthEndpoint = request.RequestUri?.PathAndQuery.Contains("/auth/login") == true ||
                            request.RequestUri?.PathAndQuery.Contains("/tenants/register") == true ||
                            request.RequestUri?.PathAndQuery.Contains("/auth/refresh") == true;

        if (!isAuthEndpoint)
        {
            // Service Provider থেকে AuthenticationStateProvider get করুন
            // Scoped service তাই constructor এ inject করা যায় না
            var authStateProvider = _serviceProvider.GetService<AuthenticationStateProvider>();

            if (authStateProvider is CustomAuthenticationStateProvider customProvider)
            {
                try
                {
                    var token = await customProvider.GetTokenAsync();

                    if (!string.IsNullOrEmpty(token))
                    {
                        // Bearer token add করুন
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        Console.WriteLine($"[AUTH HANDLER] Token added to: {request.RequestUri?.PathAndQuery}");
                    }
                    else
                    {
                        Console.WriteLine($"[AUTH HANDLER] No token for: {request.RequestUri?.PathAndQuery}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[AUTH HANDLER] Error getting token: {ex.Message}");
                }
            }
        }
        else
        {
            Console.WriteLine($"[AUTH HANDLER] Auth endpoint, skipping token: {request.RequestUri?.PathAndQuery}");
        }

        // Request পাঠান
        var response = await base.SendAsync(request, cancellationToken);

        // 401 Unauthorized হলে user logout করে দিন
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            Console.WriteLine("[AUTH HANDLER] 401 Unauthorized - Logging out user");

            var authStateProvider = _serviceProvider.GetService<AuthenticationStateProvider>();
            if (authStateProvider is CustomAuthenticationStateProvider customProvider)
            {
                await customProvider.MarkUserAsLoggedOut();
            }
        }

        return response;
    }
}
