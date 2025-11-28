using Asp.Versioning;
using EduSystem.Attendance.Infrastructure.Contexts;
using EduSystem.Attendance.Infrastructure.EventHandlers;
using EduSystem.Attendance.Infrastructure.Interceptors;
using EduSystem.Shared.Infrastructure.Security;
using EduSystem.Shared.Messaging;
using EduSystem.Shared.Messaging.Extensions;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EduSystem.Attendance.Api.DependencyResolver;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks();
        services.AddEndpointsApiExplorer();
        services.AddOpenApi();
        services.AddSingleton<IConnectionStringEncryptor, ConnectionStringEncryptor>();
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


        services.AddDbContext<AttendanceDbContext>((serviceProvider, option) =>
        {
            option.UseSqlServer(configuration.GetConnectionString("MasterDBConnection"));
            option.AddInterceptors(serviceProvider.GetRequiredService<AuditInterceptor>());
        });

        return services;
    }
}
