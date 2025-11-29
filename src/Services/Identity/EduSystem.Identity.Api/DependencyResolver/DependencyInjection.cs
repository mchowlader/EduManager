using Asp.Versioning;
using EduSystem.Identity.Infrastructure.EventHandlers;
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
        // API Versioning Configuration
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        // CORS Configuration
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });

        // MassTransit with Consumer
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

                cfg.UseMessageRetry(r => r.Exponential(
                    retryLimit: 3,
                    minInterval: TimeSpan.FromSeconds(1),
                    maxInterval: TimeSpan.FromMinutes(5),
                    intervalDelta: TimeSpan.FromSeconds(2)
                ));

            });
        });

        services.AddScoped<IEventBus, MassTransitEventBus>();

        return services;
    }
}
