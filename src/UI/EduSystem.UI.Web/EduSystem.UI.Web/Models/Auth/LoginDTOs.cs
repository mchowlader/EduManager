namespace EduSystem.UI.Web.Models.Auth;

public class LoginDTOs
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? TenantSlug { get; set; }
    public bool RememberMe { get; set; }
}
