using System.ComponentModel.DataAnnotations;

namespace EduSystem.UI.Web.Client.Models;

public class TenantRegistrationModel
{
    [Required(ErrorMessage = "School Name is required")]
    public string SchoolName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Slug is required")]
    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug can only contain lowercase letters, numbers, and hyphens")]
    public string Slug { get; set; } = string.Empty;

    [Required(ErrorMessage = "Login Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string AdminEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone Number is required")]
    [Phone(ErrorMessage = "Invalid phone number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string AdminPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Full Name is required")]
    public string AdminFullName { get; set; } = string.Empty;
}
