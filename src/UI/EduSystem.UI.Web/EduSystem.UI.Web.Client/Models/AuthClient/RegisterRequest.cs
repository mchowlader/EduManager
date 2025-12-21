namespace EduSystem.UI.Web.Client.Models.AuthClient;

public class RegisterRequest
{
    public string SchoolName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string AdminFullName { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? PrimaryColor { get; set; } = "#667eea";
    public string? SecondaryColor { get; set; } = "#764ba2";
}
