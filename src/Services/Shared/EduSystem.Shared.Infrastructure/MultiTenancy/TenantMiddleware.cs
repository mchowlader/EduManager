
using Microsoft.AspNetCore.Http;
using EduSystem.Shared.Infrastructure.Authentication;

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
            var tenantId = claims.FirstOrDefault(c => c.Type == CustomClaimTypes.TenantId)?.Value;
            var tenantSlug = claims.FirstOrDefault(c => c.Type == CustomClaimTypes.TenantSlug)?.Value;
            var tenantName = claims.FirstOrDefault(c => c.Type == CustomClaimTypes.TenantName)?.Value;
            var connectionString = claims.FirstOrDefault(c => c.Type == CustomClaimTypes.ConnectionString)?.Value;
            var role = claims.FirstOrDefault(c => c.Type == CustomClaimTypes.Role)?.Value;

            if (tenantContext is TenantContext mutableContext)
            {
                mutableContext.TenantId =
                    long.TryParse(tenantId, out var id) ? id : null;

                mutableContext.TenantSlug = tenantSlug;
                mutableContext.TenantName = tenantName;
                mutableContext.ConnectionString = connectionString;
                mutableContext.IsSuperAdmin = role == "SuperAdmin";
            }
        }

        await _next(context);
    }
}
