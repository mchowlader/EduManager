using EduSystem.ApplicationUsers.Application.IService;
using EduSystem.ApplicationUsers.Infrastructure.Contexts;
using EduSystem.ApplicationUsers.Infrastructure.Interceptors;
using EduSystem.ApplicationUsers.Infrastructure.Service;
using EduSystem.ApplicationUsers.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EduSystem.ApplicationUsers.Infrastructure.DependencyResolver;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddHttpContextAccessor();

        services.AddDbContext<AppUserDbContext>((serviceProvider, option) =>
        {
            option.UseSqlServer(configuration.GetConnectionString("MasterDBConnection"));
            option.AddInterceptors(serviceProvider.GetRequiredService<AuditInterceptor>());
        });

        services.AddScoped<AuditInterceptor>();

        return services;
    }
}
