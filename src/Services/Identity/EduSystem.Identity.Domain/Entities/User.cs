namespace EduSystem.Identity.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public Guid TenantId { get; set; } // School/College reference
    public UserRole Role { get; set; } // SuperAdmin, EduAdmin
    public bool IsActive { get; set; }
        
    // Authentication & Security
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }
    public DateTime? LastPasswordChangedAt { get; set; }

    // Tracking
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int LoginAttempts { get; set; } // For account lockout
    public DateTime? LockoutEnd { get; set; } // Account lockout until

    // Navigation
    public Tenant? Tenant { get; set; }
}
