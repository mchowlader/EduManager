namespace EduSystem.UI.Web.Client.Models.AuthClient;

public class TenantInfo
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string PrimaryColor { get; set; } = string.Empty;
    public string SecondaryColor { get; set; } = string.Empty;
    public string? BannerUrl { get; set; }
    public bool IsActive { get; set; }
}
