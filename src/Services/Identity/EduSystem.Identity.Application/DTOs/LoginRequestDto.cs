using System.ComponentModel.DataAnnotations;

namespace EduSystem.Identity.Application.DTOs;

public class LoginRequestDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Optional: For SuperAdmin to login to specific tenant
    /// </summary>
    public string? TenantSlug { get; set; }
}
