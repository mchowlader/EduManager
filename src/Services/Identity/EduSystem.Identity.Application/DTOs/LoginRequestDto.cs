namespace EduSystem.Identity.Application.DTOs;

public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string TenantSlug { get; set; } = string.Empty;
}