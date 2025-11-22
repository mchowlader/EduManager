using EduSystem.Shared.Infrastructure.Security;
using EduSystem.Shared.Messaging.Extensions;

namespace EduSystem.ApplicationUsers.Api.DependencyResolver;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IConnectionStringEncryptor, ConnectionStringEncryptor>();
        services.AddHealthChecks();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddEventBus(configuration);

        return services;
    }
}
