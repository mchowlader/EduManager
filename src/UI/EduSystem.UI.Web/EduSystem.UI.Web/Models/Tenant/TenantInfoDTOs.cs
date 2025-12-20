namespace EduSystem.UI.Web.Models.Tenant;

public class TenantInfoDTOs
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string PrimaryColor { get; set; } = "#667eea";
    public string SecondaryColor { get; set; } = "#764ba2";
    public bool IsActive { get; set; }
}
