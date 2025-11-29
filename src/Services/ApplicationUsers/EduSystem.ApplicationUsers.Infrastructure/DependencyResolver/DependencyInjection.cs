using EduSystem.ApplicationUsers.Application.IService;
using EduSystem.ApplicationUsers.Infrastructure.EventHandlers;
using EduSystem.ApplicationUsers.Infrastructure.Service;
using EduSystem.ApplicationUsers.Infrastructure.Services;
using EduSystem.Shared.Messaging;
using MassTransit;
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

        services.AddMassTransit(x =>
        {
            x.AddConsumer<TenantDatabaseCreatedEventHandler>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitMqConfig = configuration.GetSection("RabbitMQ");
                cfg.Host(rabbitMqConfig["Host"] ?? "localhost", h =>
                {
                    h.Username(rabbitMqConfig["Username"] ?? "guest");
                    h.Password(rabbitMqConfig["Password"] ?? "guest");
                });

                cfg.ReceiveEndpoint("attendance-tenant-database-created", e =>
                {
                    e.UseMessageRetry(r => r.Intervals(
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(2),
                        TimeSpan.FromSeconds(5)
                    ));

                    e.PrefetchCount = 16;
                    e.ConcurrentMessageLimit = 1;

                    e.ConfigureConsumer<TenantDatabaseCreatedEventHandler>(context);
                });
            });
        });

        services.AddScoped<IEventBus, MassTransitEventBus>();

        return services;
    }
}
