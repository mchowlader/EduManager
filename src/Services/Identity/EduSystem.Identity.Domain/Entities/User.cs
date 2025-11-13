namespace EduSystem.Identity.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public Guid TenantId { get; set; } // School/College reference
    public UserRole Role { get; set; } // Admin, Teacher, Student
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public Tenant? Tenant { get; set; }
}