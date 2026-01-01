using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace EduSystem.Shared.Infrastructure.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static long GetUserId(this ClaimsPrincipal principal)
    {
        var userIdClaim = principal
            .FindFirst(ClaimTypes.NameIdentifier)?
            .Value;

        return long.TryParse(userIdClaim, out var userId)
            ? userId
            : 0;
    }
    public static string GetEmail(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
    }

    public static string GetFullName(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
    }

    public static string GetRole(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
    }

    public static long GetTenantId(this ClaimsPrincipal principal)
    {
        var tenantIdClaim = principal.FindFirst("tenant_id")?.Value;
        return long.TryParse(tenantIdClaim, out var tenantId)
            ? tenantId
            : 0;
    }

    public static string GetTenantSlug(this ClaimsPrincipal principal)
    {
        return principal.FindFirst("tenant_slug")?.Value ?? string.Empty;
    }

    public static string GetTenantName(this ClaimsPrincipal principal)
    {
        return principal.FindFirst("tenant_name")?.Value ?? string.Empty;
    }

    public static bool IsSuperAdmin(this ClaimsPrincipal principal)
    {
        return principal.IsInRole("SuperAdmin");
    }

    public static bool IsEduAdmin(this ClaimsPrincipal principal)
    {
        return principal.IsInRole("EduAdmin");
    }

    public static bool IsTeacher(this ClaimsPrincipal principal)
    {
        return principal.IsInRole("Teacher");
    }

    public static bool IsStudent(this ClaimsPrincipal principal)
    {
        return principal.IsInRole("Student");
    }

    public static bool HasAnyRole(this ClaimsPrincipal principal, params string[] roles)
    {
        return roles.Any(role => principal.IsInRole(role));
    }
}
