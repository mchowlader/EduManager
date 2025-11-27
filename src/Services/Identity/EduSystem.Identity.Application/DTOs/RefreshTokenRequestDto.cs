using System.ComponentModel.DataAnnotations;

namespace EduSystem.Identity.Application.DTOs;

public class RefreshTokenRequestDto
{
    [Required(ErrorMessage = "Access token is required")]
    public string AccessToken { get; set; } = string.Empty;

    [Required(ErrorMessage = "Refresh token is required")]
    public string RefreshToken { get; set; } = string.Empty;
}
