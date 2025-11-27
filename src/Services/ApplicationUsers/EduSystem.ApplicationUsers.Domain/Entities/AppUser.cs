namespace EduSystem.ApplicationUsers.Domain.Entities;

public class AppUser : AuditableEntity
{
    //public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string? Phone { get; set; }

    // Security
    public string? SecurityStamp { get; set; }
    public int AccessFailedCount { get; set; }
    public bool LockoutEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }

    // Status
    public bool IsActive { get; set; } = true;
    public bool EmailConfirmed { get; set; }
    public bool PhoneConfirmed { get; set; }

    // Audit
    //public DateTime CreatedAt { get; set; }
    //public DateTime? UpdatedAt { get; set; }
    //public string? CreatedBy { get; set; }
    //public string? UpdatedBy { get; set; }
}
