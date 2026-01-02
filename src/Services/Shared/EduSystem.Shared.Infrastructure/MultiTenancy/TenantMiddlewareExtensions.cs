using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using EduSystem.Shared.Infrastructure.Persistence.Interfaces;
using EduSystem.Shared.Infrastructure.Persistence;

namespace EduSystem.Shared.Infrastructure.MultiTenancy;

public static class TenantMiddlewareExtensions
{
    public static IServiceCollection AddMultiTenancy(this IServiceCollection services)
    {
        services.AddScoped<ITenantContext, TenantContext>();
        services.AddScoped<IDynamicValidationService, DynamicValidationService>();
        return services;
    }

    public static IServiceCollection AddTenantMigration<TDbContext>(this IServiceCollection services) 
        where TDbContext : DbContext
    {
        services.AddTransient<ITenantMigrationService<TDbContext>, TenantMigrationService<TDbContext>>();
        return services;
    }

    public static IApplicationBuilder UseMultiTenancy(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TenantMiddleware>();
    }
}
