using EduSystem.UI.Web.Client.Models.Common;

namespace EduSystem.UI.Web.Client.Services.Base;

public interface IApiService
{
    Task<ApiResponse<T>> GetAsync<T>(string endpoint, string clientName = "GatewayClient");
    Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest data, string clientName = "GatewayClient");
    Task<ApiResponse<TResponse>> PutAsync<TRequest, TResponse>(string endpoint, TRequest data, string clientName = "GatewayClient");
    Task<ApiResponse<bool>> DeleteAsync(string endpoint, string clientName = "GatewayClient");
}
