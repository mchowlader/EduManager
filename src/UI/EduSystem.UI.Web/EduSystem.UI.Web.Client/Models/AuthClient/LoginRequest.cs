namespace EduSystem.UI.Web.Client.Models.AuthClient;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? TenantSlug { get; set; }
}
