using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.Authorization;
using EduSystem.UI.Web.Client.Services.Auth;
using EduSystem.UI.Web.Client.Models.AuthClient;

namespace EduSystem.UI.Web.Client.HttpHandlers;

/// <summary>
/// HTTP Message Handler যা প্রতিটি request এ automatically Bearer token add করে
/// DelegatingHandler inherit করে - এটা generic AuthenticationHandler নয়
/// </summary>
public class AuthenticationHandler : DelegatingHandler
{
    private readonly IServiceProvider _serviceProvider;
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    public AuthenticationHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var isAnonymous = request.Headers.Contains("X-Allow-Anonymous");
        
        if (isAnonymous)
        {
            request.Headers.Remove("X-Allow-Anonymous");
        }
        else
        {
            var authManager = _serviceProvider.GetService<IAuthManager>();
            if (authManager != null)
            {
                var token = await authManager.GetAccessTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && !isAnonymous)
        {
            try
            {
                await _semaphore.WaitAsync(cancellationToken);

                // Check again if token was refreshed by another thread
                var authManager = _serviceProvider.GetRequiredService<IAuthManager>();
                var currentToken = await authManager.GetAccessTokenAsync();
                
                // If the token in the request is different from current, it was already refreshed
                var requestToken = request.Headers.Authorization?.Parameter;
                
                if (currentToken != requestToken && !string.IsNullOrEmpty(currentToken))
                {
                    // Token already refreshed, retry with new token
                    return await RetryRequest(request, currentToken, cancellationToken);
                }

                // Try to refresh
                var authService = _serviceProvider.GetRequiredService<IAuthService>();
                var refreshToken = await authManager.GetRefreshTokenAsync();

                if (!string.IsNullOrEmpty(refreshToken) && !string.IsNullOrEmpty(currentToken))
                {
                    var refreshResult = await authService.RefreshTokenAsync(new RefreshTokenRequest 
                    { 
                        AccessToken = currentToken, 
                        RefreshToken = refreshToken 
                    });

                    if (refreshResult.Success && refreshResult.Data != null)
                    {
                        await authManager.UpdateTokensAsync(refreshResult.Data.AccessToken, refreshResult.Data.RefreshToken);
                        return await RetryRequest(request, refreshResult.Data.AccessToken, cancellationToken);
                    }
                }

                // Refresh failed or no refresh token, logout
                await authManager.MarkUserAsLoggedOut();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        return response;
    }

    private async Task<HttpResponseMessage> RetryRequest(HttpRequestMessage request, string newToken, CancellationToken cancellationToken)
    {
        // Clone the request as it might have been sent already
        var newRequest = await CloneRequest(request);
        newRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
        return await base.SendAsync(newRequest, cancellationToken);
    }

    private async Task<HttpRequestMessage> CloneRequest(HttpRequestMessage request)
    {
        var newRequest = new HttpRequestMessage(request.Method, request.RequestUri);
        
        // Copy content
        if (request.Content != null)
        {
            var contentBytes = await request.Content.ReadAsByteArrayAsync();
            newRequest.Content = new ByteArrayContent(contentBytes);
            
            foreach (var header in request.Content.Headers)
            {
                newRequest.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        // Copy generic headers
        foreach (var header in request.Headers)
        {
            newRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return newRequest;
    }
}
