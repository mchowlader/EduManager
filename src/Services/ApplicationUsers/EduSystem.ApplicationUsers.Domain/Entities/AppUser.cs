namespace EduSystem.ApplicationUsers.Domain.Entities;

public class AppUser
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserEmail { get; set; }
    public string UserPassword { get; set; } = string.Empty;
    public string? UserPhone { get; set; }
}
