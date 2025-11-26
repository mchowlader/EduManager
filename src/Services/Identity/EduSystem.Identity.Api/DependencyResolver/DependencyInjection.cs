using Asp.Versioning;
using EduSystem.Shared.Infrastructure.Security;
using EduSystem.Shared.Messaging.Extensions;
using Microsoft.OpenApi;

namespace EduSystem.Identity.Api.DependencyResolver;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IConnectionStringEncryptor, ConnectionStringEncryptor>();
        services.AddHealthChecks();
        services.AddEndpointsApiExplorer();

        services
        .AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true; // true করুন
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        services.AddSwaggerGen(option =>
        {
            option.CustomSchemaIds(type => type.FullName);

            option.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "EduSystem Identity API",
                Version = "v1"
            });

            option.SwaggerDoc("v2", new OpenApiInfo
            {
                Title = "EduSystem Identity API",
                Version = "v2"
            });
        });

        services.AddEventBus(configuration);
        return services;
    }
}
