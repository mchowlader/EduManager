using System;
using System.Collections.Generic;
using System.Text;
using EduSystem.Identity.Domain.Entities;

namespace EduSystem.Identity.Application.DTOs;

public class LoginResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserInfoDto User { get; set; } = null!;
    public TenantInfoDto Tenant { get; set; } = null!;
}
