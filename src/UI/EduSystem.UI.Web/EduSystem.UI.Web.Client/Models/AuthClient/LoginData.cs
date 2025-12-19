namespace EduSystem.UI.Web.Client.Models.AuthClient;

public class LoginData
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserInfo User { get; set; } = new();
    public TenantInfo Tenant { get; set; } = new();
}
