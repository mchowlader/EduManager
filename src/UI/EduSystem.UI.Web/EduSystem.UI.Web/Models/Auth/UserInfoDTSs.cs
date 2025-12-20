namespace EduSystem.UI.Web.Models.Auth;

public class UserInfoDTSs
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string[] Roles { get; set; } = Array.Empty<string>();
    public string? TenantId { get; set; }
}
