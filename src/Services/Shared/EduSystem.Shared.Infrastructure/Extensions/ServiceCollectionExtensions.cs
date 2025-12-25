using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace EduSystem.Shared.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCentralizedLoggin(this IServiceCollection services, string serviceName)
    {
        var helper = new LogHelper.LoggingHelper(serviceName);

        services.AddSingleton(helper);
        services.AddSingleton<ILogger>(helper.Logger);

        return services;
    }
}
