namespace EduSystem.UI.Web.Client.Models.AuthClient;

public class LoginResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public LoginData Data { get; set; } = new();
}
