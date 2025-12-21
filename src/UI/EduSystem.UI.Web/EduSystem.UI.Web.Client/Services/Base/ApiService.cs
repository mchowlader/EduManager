using System.Net.Http.Json;
using System.Text.Json;
using EduSystem.UI.Web.Client.Models.Common;
using Microsoft.Extensions.Logging;

namespace EduSystem.UI.Web.Client.Services.Base;

public class ApiService : IApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ApiService> _logger;

    public ApiService(IHttpClientFactory httpClientFactory, ILogger<ApiService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<ApiResponse<T>> GetAsync<T>(string endpoint, string clientName = "GatewayClient", bool allowAnonymous = false)
    {
        try
        {
            var client = _httpClientFactory.CreateClient(clientName);
            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            if (allowAnonymous) request.Headers.Add("X-Allow-Anonymous", "true");
            
            var response = await client.SendAsync(request);
            return await HandleResponse<T>(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during GET request to {Endpoint}", endpoint);
            return ApiResponse<T>.Failure($"Connection error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest data, string clientName = "GatewayClient", bool allowAnonymous = false)
    {
        try
        {
            var client = _httpClientFactory.CreateClient(clientName);
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Content = JsonContent.Create(data);
            if (allowAnonymous) request.Headers.Add("X-Allow-Anonymous", "true");

            var response = await client.SendAsync(request);
            return await HandleResponse<TResponse>(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during POST request to {Endpoint}", endpoint);
            return ApiResponse<TResponse>.Failure($"Connection error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<TResponse>> PutAsync<TRequest, TResponse>(string endpoint, TRequest data, string clientName = "GatewayClient", bool allowAnonymous = false)
    {
        try
        {
            var client = _httpClientFactory.CreateClient(clientName);
            var request = new HttpRequestMessage(HttpMethod.Put, endpoint);
            request.Content = JsonContent.Create(data);
            if (allowAnonymous) request.Headers.Add("X-Allow-Anonymous", "true");

            var response = await client.SendAsync(request);
            return await HandleResponse<TResponse>(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during PUT request to {Endpoint}", endpoint);
            return ApiResponse<TResponse>.Failure($"Connection error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(string endpoint, string clientName = "GatewayClient", bool allowAnonymous = false)
    {
        try
        {
            var client = _httpClientFactory.CreateClient(clientName);
            var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
            if (allowAnonymous) request.Headers.Add("X-Allow-Anonymous", "true");

            var response = await client.SendAsync(request);
            return await HandleResponse<bool>(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during DELETE request to {Endpoint}", endpoint);
            return ApiResponse<bool>.Failure($"Connection error: {ex.Message}");
        }
    }

    private async Task<ApiResponse<T>> HandleResponse<T>(HttpResponseMessage response)
    {
        var statusCode = (int)response.StatusCode;
        
        if (response.IsSuccessStatusCode)
        {
            try
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<T>>();
                if (result != null)
                {
                    result.StatusCode = statusCode;
                    return result;
                }
                
                // If the response is success but doesn't match ApiResponse<T> format exactly, 
                // try to read the direct object (for flexibility with some APIs)
                var fallbackData = await response.Content.ReadFromJsonAsync<T>();
                return ApiResponse<T>.Successful(fallbackData!, "Success", statusCode);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize success response body");
                return ApiResponse<T>.Successful(default!, "Success with no content", statusCode);
            }
        }

        // Handle Errors
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return ApiResponse<T>.Failure(error?.Message ?? "Request failed", error?.Errors, statusCode);
        }
        catch
        {
            var rawError = await response.Content.ReadAsStringAsync();
            return ApiResponse<T>.Failure($"Request failed with status {statusCode}: {rawError}", null, statusCode);
        }
    }
}
