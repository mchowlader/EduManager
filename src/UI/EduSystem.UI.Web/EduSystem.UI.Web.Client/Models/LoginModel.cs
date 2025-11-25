using System.ComponentModel.DataAnnotations;

namespace EduSystem.UI.Web.Client.Models;

public class LoginModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "School Slug is required")]
    public string TenantSlug { get; set; } = string.Empty;
}
