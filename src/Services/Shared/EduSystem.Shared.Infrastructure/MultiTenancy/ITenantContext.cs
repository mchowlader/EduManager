using System;
using System.Collections.Generic;
using System.Text;

namespace EduSystem.Shared.Infrastructure.MultiTenancy;

public interface ITenantContext
{
    Guid? TenantId { get; }
    string? TenantSlug { get; }
    string? TenantName { get; }
    string? ConnectionString { get; }
    bool IsSuperAdmin { get; }
}
