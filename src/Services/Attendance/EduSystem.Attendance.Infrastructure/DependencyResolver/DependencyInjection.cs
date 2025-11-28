using EduSystem.Attendance.Application.IService;
using EduSystem.Attendance.Infrastructure.EventHandlers;
using EduSystem.Attendance.Infrastructure.Interceptors;
using EduSystem.Attendance.Infrastructure.Service;
using EduSystem.Attendance.Infrastructure.Services;
using EduSystem.Shared.Messaging;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EduSystem.Attendance.Infrastructure.DependencyResolver;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddHttpContextAccessor();
        services.AddScoped<AuditInterceptor>();

        // MassTransit with Consumer
        services.AddMassTransit(x =>
        {
            x.AddConsumer<TenantDatabaseCreatedEventHandler>();

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

                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddScoped<IEventBus, MassTransitEventBus>();

        return services;
    }
}
