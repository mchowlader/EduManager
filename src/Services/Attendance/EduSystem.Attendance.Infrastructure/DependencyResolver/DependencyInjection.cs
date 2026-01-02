using EduSystem.Attendance.Application.IService;
using EduSystem.Attendance.Infrastructure.EventHandlers;
using EduSystem.Attendance.Infrastructure.Interceptors;
using EduSystem.Attendance.Infrastructure.Service;
using EduSystem.Attendance.Infrastructure.Services;
using EduSystem.Shared.Event;
using EduSystem.Shared.Messaging;
using MassTransit;
using MassTransit.DependencyInjection;
using MassTransit.RabbitMqTransport.Topology;
using EduSystem.Attendance.Infrastructure.Contexts;
using EduSystem.Shared.Infrastructure.MultiTenancy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EduSystem.Attendance.Infrastructure.DependencyResolver;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddHttpContextAccessor();
        services.AddScoped<AuditInterceptor>();
        services.AddTenantMigration<AttendanceDbContext>();

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
                    //CRITICAL: Bind to the fanout exchange
                    e.Bind<TenantDatabaseCreatedEvent>();

                    // Sequential processing
                    e.UseConcurrencyLimit(1);
                    e.PrefetchCount = 1;

                    e.UseMessageRetry(r => r.Intervals(
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(2),
                        TimeSpan.FromSeconds(5)
                    ));

                    //e.PrefetchCount = 16;
                    //e.ConcurrentMessageLimit = 1;
                    e.ConfigureConsumer<TenantDatabaseCreatedEventHandler>(context);
                });
            });
        });

        services.AddScoped<IEventBus, MassTransitEventBus>();

        return services;
    }
}
