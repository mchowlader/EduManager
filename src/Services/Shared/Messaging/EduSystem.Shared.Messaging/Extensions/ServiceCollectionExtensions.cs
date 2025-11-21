using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace EduSystem.Shared.Messaging.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventBus(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator>? configure = null)
    {
        services.AddMassTransit(x =>
        {
            //conjume configuration if any
            configure?.Invoke(x);
            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitMqHost = configuration["RabbitMQ:Host"] ?? "localhost";
                var rabbitMqUsername = configuration["RabbitMQ:Username"] ?? "guest";
                var rabbitMqPort = int.TryParse(configuration["RabbitMQ:Port"], out var port)? port : 5732;
                var rabbitMqPassword = configuration["RabbitMQ:Password"] ?? "guest";

                cfg.Host(rabbitMqHost, (ushort)rabbitMqPort, "/", h =>
                {
                    h.Username(rabbitMqUsername);
                    h.Password(rabbitMqPassword);
                });

                // Retry policy
                cfg.UseMessageRetry(r => r.Exponential(
                    retryLimit: 3,
                    minInterval: TimeSpan.FromSeconds(1),
                    maxInterval: TimeSpan.FromSeconds(5),
                    intervalDelta: TimeSpan.FromSeconds(2)
                ));

                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddScoped<IEventBus, MassTransitEventBus>();

        return services;
    }
}