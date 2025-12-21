namespace EduSystem.UI.Web.Client.Models.AuthClient;

public class RefreshTokenRequest
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
