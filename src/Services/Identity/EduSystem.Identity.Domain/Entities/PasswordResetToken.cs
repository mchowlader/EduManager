using System;
using System.Collections.Generic;
using System.Text;

namespace EduSystem.Identity.Domain.Entities;

public class PasswordResetToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public User? User { get; set; }
}
