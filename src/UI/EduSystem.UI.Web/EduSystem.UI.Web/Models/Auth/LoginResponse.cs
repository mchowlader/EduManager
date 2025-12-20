using EduSystem.UI.Web.Client.Models.AuthClient;

namespace EduSystem.UI.Web.Models.Auth;

public class LoginResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Token { get; set; }
    public UserInfoDTSs? User { get; set; }
    public DateTime? TokenExpiry { get; set; }
}
