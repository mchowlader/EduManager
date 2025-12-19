namespace EduSystem.UI.Web.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private CurrentUser? _currentUser;

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CurrentUser?> GetCurrentUserAsync()
    {
        if (_currentUser != null)
            return _currentUser;

        try
        {
            _currentUser = await _httpClient.GetFromJsonAsync<CurrentUser>("api/auth/current-user");
            return _currentUser;
        }
        catch
        {
            return null;
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            await _httpClient.PostAsync("api/auth/logout", null);
            _currentUser = null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Logout error: {ex.Message}");
        }
    }
}

public class CurrentUser
{
    public string Id { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
    public string TenantId { get; set; } = "";
}
