using System.Text.Json;
using EduSystem.UI.Web.Models.Common;

namespace EduSystem.UI.Web.Services.Common;

public abstract class BaseHttpService
{
    protected readonly IHttpClientFactory _httpClientFactory;
    protected readonly ILogger _logger;
    private readonly JsonSerializerOptions _jsonSerializer;

    protected BaseHttpService(IHttpClientFactory httpClientFactory, ILogger logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _jsonSerializer = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    protected HttpClient CreateClient(string clientName)
    {
        return _httpClientFactory.CreateClient(clientName);
    }

    
}
