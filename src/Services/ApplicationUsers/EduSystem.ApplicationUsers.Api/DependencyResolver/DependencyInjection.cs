using Asp.Versioning;
using EduSystem.ApplicationUsers.Infrastructure.Contexts;
using EduSystem.ApplicationUsers.Infrastructure.Interceptors;
using EduSystem.Shared.Infrastructure.Security;
using EduSystem.Shared.Messaging.Extensions;
using Microsoft.EntityFrameworkCore;

namespace EduSystem.ApplicationUsers.Api.DependencyResolver;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IConnectionStringEncryptor, ConnectionStringEncryptor>();
        services.AddHealthChecks();
        services.AddEndpointsApiExplorer();
        services.AddOpenApi();
        services.AddAuthentication();
        services.AddAuthorization();
        services
        .AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = false;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        //services.AddEventBus(configuration);

        services.AddDbContext<AppUserDbContext>((serviceProvider, options) =>
        {
            var masterConnection = configuration.GetConnectionString("MasterDBConnection");

            options.UseSqlServer(masterConnection, sqlOptions =>
            {
                sqlOptions.CommandTimeout(60);
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
            });

            options.AddInterceptors(serviceProvider.GetRequiredService<AuditInterceptor>());
        });

        return services;
    }
}
