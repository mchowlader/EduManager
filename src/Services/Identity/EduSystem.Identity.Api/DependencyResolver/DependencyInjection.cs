using Asp.Versioning;
using EduSystem.Shared.Event;
using EduSystem.Shared.Infrastructure.Security;
using EduSystem.Shared.Messaging;
using MassTransit;

namespace EduSystem.Identity.Api.DependencyResolver;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IConnectionStringEncryptor, ConnectionStringEncryptor>();
        services.AddHealthChecks();
        services.AddAuthentication();
        services.AddAuthorization();

        // ✅ API Versioning with proper configuration
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(2, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("x-api-version")
            );
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        // CORS
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            });
        });

        // MassTransit
        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitMqHost = configuration["RabbitMQ:Host"] ?? "localhost";
                var rabbitMqPort = configuration.GetValue<int>("RabbitMQ:Port", 5672);
                var rabbitMqUsername = configuration["RabbitMQ:Username"] ?? "guest";
                var rabbitMqPassword = configuration["RabbitMQ:Password"] ?? "guest";

                cfg.Host(rabbitMqHost, (ushort)rabbitMqPort, "/", h =>
                {
                    h.Username(rabbitMqUsername);
                    h.Password(rabbitMqPassword);
                });

                cfg.Publish<TenantDatabaseCreatedEvent>(p => { p.ExchangeType = "fanout"; });
                cfg.UseMessageRetry(r => r.Exponential(3, TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(2)));
            });
        });

        services.AddScoped<IEventBus, MassTransitEventBus>();

        return services;
    }
}
