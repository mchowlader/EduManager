namespace EduSystem.Identity.Application.DTOs;

public class TenantInfoDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? BannerUrl { get; set; }
    public bool IsActive { get; set; }
}
