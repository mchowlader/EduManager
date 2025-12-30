using EduSystem.ApplicationUsers.Application.Contracts.Persistence;
using EduSystem.ApplicationUsers.Application.IService;
using EduSystem.ApplicationUsers.Infrastructure.EventHandlers;
using EduSystem.ApplicationUsers.Infrastructure.Interceptors;
using EduSystem.ApplicationUsers.Infrastructure.Service;
using EduSystem.ApplicationUsers.Infrastructure.Services;
using EduSystem.ApplicationUsers.Infrastructure.Repositories;
using EduSystem.Shared.Event;
using EduSystem.Shared.Messaging;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EduSystem.ApplicationUsers.Infrastructure.DependencyResolver;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddHttpContextAccessor();
        services.AddScoped<AuditInterceptor>();
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

                cfg.ReceiveEndpoint("applicationusers-tenant-database-created", e =>
                {
                    // CRITICAL: Bind to the fanout exchange
                    e.Bind<TenantDatabaseCreatedEvent>();

                    // Sequential processing - one message at a time
                    e.UseConcurrencyLimit(1);
                    e.PrefetchCount = 1;
                    e.UseMessageRetry(r => r.Intervals(
                        TimeSpan.FromSeconds(2),
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(10)
                    ));

                    e.ConfigureConsumer<TenantDatabaseCreatedEventHandler>(context);
                });
            });
        });

        services.AddScoped<IEventBus, MassTransitEventBus>();

        return services;
    }
}
