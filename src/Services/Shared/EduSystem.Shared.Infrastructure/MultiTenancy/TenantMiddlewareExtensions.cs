using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace EduSystem.Shared.Infrastructure.MultiTenancy;

public static class TenantMiddlewareExtensions
{
    public static IServiceCollection AddMultiTenancy(this IServiceCollection services)
    {
        services.AddScoped<ITenantContext, TenantContext>();
        return services;
    }

    public static IApplicationBuilder UseMultiTenancy(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TenantMiddleware>();
    }
}
