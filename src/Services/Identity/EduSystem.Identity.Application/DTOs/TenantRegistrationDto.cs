namespace EduSystem.Identity.Application.DTOs;

public class TenantRegistrationDto
{
    public string SchoolName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty; // e.g., "dhaka-high-school"
    public string AdminEmail { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
    public string AdminFullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
}