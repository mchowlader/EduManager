using System;
using System.Collections.Generic;
using System.Text;

namespace EduSystem.Shared.Infrastructure.MultiTenancy;

public class TenantContext : ITenantContext
{
    public Guid? TenantId { get; set; }
    public string? TenantSlug { get; set; }
    public string? TenantName { get; set; }
    public string? ConnectionString { get; set; }
    public bool IsSuperAdmin { get; set; }
}
