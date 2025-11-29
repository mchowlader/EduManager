//using EduSystem.Attendance.Application.IService;
//using EduSystem.Attendance.Infrastructure.EventHandlers;
//using EduSystem.Attendance.Infrastructure.Interceptors;
//using EduSystem.Attendance.Infrastructure.Service;
//using EduSystem.Attendance.Infrastructure.Services;
//using EduSystem.Shared.Messaging;
//using MassTransit;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using EduSystem.Shared.Messaging.Handlers;

//namespace EduSystem.Attendance.Infrastructure.DependencyResolver;

//public static class DependencyInjection
//{
//    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
//    {
//        services.AddScoped<IUnitOfWork, UnitOfWork>();
//        services.AddScoped<ICurrentUserService, CurrentUserService>();
//        services.AddHttpContextAccessor();
//        services.AddScoped<AuditInterceptor>();

//        //// MassTransit with Consumer
//        //services.AddMassTransit(x =>
//        //{
//        //    x.AddConsumer<TenantDatabaseCreatedEventHandler>();

//        //    x.UsingRabbitMq((context, cfg) =>
//        //    {
//        //        var rabbitMqHost = configuration["RabbitMQ:Host"] ?? "localhost";
//        //        var rabbitMqPort = configuration.GetValue<int>("RabbitMQ:Port", 5672);
//        //        var rabbitMqUsername = configuration["RabbitMQ:Username"] ?? "guest";
//        //        var rabbitMqPassword = configuration["RabbitMQ:Password"] ?? "guest";

//        //        cfg.Host(rabbitMqHost, (ushort)rabbitMqPort, "/", h =>
//        //        {
//        //            h.Username(rabbitMqUsername);
//        //            h.Password(rabbitMqPassword);
//        //        });

//        //        cfg.UseMessageRetry(r => r.Exponential(
//        //            retryLimit: 3,
//        //            minInterval: TimeSpan.FromSeconds(1),
//        //            maxInterval: TimeSpan.FromMinutes(5),
//        //            intervalDelta: TimeSpan.FromSeconds(2)
//        //        ));

//        //        cfg.ConfigureEndpoints(context);
//        //    });
//        //});

//        //services.AddScoped<IEventBus, MassTransitEventBus>();

//        services.AddMassTransit(x =>
//        {
//            x.AddConsumer<TenantDatabaseCreatedEventHandler>();

//            x.UsingRabbitMq((context, cfg) =>
//            {
//                var rabbitMqConfig = configuration.GetSection("RabbitMQ");
//                cfg.Host(rabbitMqConfig["Host"] ?? "localhost", h =>
//                {
//                    h.Username(rabbitMqConfig["Username"] ?? "guest");
//                    h.Password(rabbitMqConfig["Password"] ?? "guest");
//                });

//                cfg.ReceiveEndpoint("attendance-tenant-database-created", e =>
//                {
//                    e.UseMessageRetry(r => r.Intervals(
//                        TimeSpan.FromSeconds(1),
//                        TimeSpan.FromSeconds(2),
//                        TimeSpan.FromSeconds(5)
//                    ));

//                    e.PrefetchCount = 16;
//                    e.UseConcurrentMessageLimit(1);
//                    e.ConfigureConsumer<TenantDatabaseCreatedEventHandler>(context);
//                });

//                cfg.SendTimeout = TimeSpan.FromMinutes(5);
//                // cfg.RequestTimeout = TimeSpan.FromMinutes(5); // এই লাইন মুছে দিন
//            });
//        });

//        // Register IEventBus
//        services.AddScoped<IEventBus, MassTransitEventBus>();

//        // Register dependencies for TenantDatabaseCreatedEventHandler

//        return services;
//    }
//}
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
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
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
