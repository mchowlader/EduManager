namespace EduSystem.Identity.Application.Settings;

public class SecuritySettings
{
    public int MaxLoginAttempts { get; set; } = 5;
    public int AccountLockoutMinutes { get; set; } = 30;
    public int PasswordResetTokenExpiryHours { get; set; } = 1;
    public int MinPasswordLength { get; set; } = 8;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireDigit { get; set; } = true;
    public bool RequireSpecialCharacter { get; set; } = true;
    public int SessionTimeoutMinutes { get; set; } = 60;
}
