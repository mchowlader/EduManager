using EduSystem.ApplicationUsers.Infrastructure.Contexts;
using EduSystem.Shared.Infrastructure.MultiTenancy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace EduSystem.ApplicationUsers.Api.Endpoints;

public static class MigrationEndpoints
{
    public static void MapMigrationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/migrations")
            .WithTags("Migrations")
            .RequireAuthorization(policy => policy.RequireRole("SuperAdmin"));

        group.MapPost("/sync", async (
            ITenantProvider tenantProvider,
            ITenantMigrationService<AppUserDbContext> migrationService,
            CancellationToken ct) =>
        {
            var tenants = await tenantProvider.GetActiveTenantsAsync();
            await migrationService.MigrateAllTenantsAsync(tenants, ct);
            return Results.Ok(new { Message = "Migration sync triggered for all active tenants." });
        });
    }
}
