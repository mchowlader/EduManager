
using Microsoft.AspNetCore.Http;

namespace EduSystem.Shared.Infrastructure.MultiTenancy;

public class TenantMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var claims = context.User.Claims;

            // Extract tenant information from JWT claims
            var tenantId = claims.FirstOrDefault(c => c.Type == "tenant_id")?.Value;
            var tenantSlug = claims.FirstOrDefault(c => c.Type == "tenant_slug")?.Value;
            var tenantName = claims.FirstOrDefault(c => c.Type == "tenant_name")?.Value;
            var connectionString = claims.FirstOrDefault(c => c.Type == "connection_string")?.Value;
            var role = claims.FirstOrDefault(c => c.Type == "role")?.Value;

            if (tenantContext is TenantContext mutableContext)
            {
                mutableContext.TenantId = Guid.TryParse(tenantId, out var id) ? id : null;
                mutableContext.TenantSlug = tenantSlug;
                mutableContext.TenantName = tenantName;
                mutableContext.ConnectionString = connectionString;
                mutableContext.IsSuperAdmin = role == "SuperAdmin";
            }
        }

        await _next(context);
    }
}
