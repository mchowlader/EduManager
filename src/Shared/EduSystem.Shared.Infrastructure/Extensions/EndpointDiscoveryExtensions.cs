using System.Reflection;
using EduSystem.Shared.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Routing;

namespace EduSystem.Shared.Infrastructure.Extensions;

/// <summary>
/// Extension methods for automatic endpoint discovery and registration.
/// </summary>
public static class EndpointDiscoveryExtensions
{
    /// <summary>
    /// Automatically discovers and maps all classes implementing IEndpoints in the calling assembly.
    /// </summary>
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app)
    {
        return app.MapEndpoints(Assembly.GetCallingAssembly());
    }

    /// <summary>
    /// Automatically discovers and maps all classes implementing IEndpoints in the specified assembly.
    /// </summary>
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app, Assembly assembly)
    {
        var endpointTypes = assembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsAssignableTo(typeof(IEndpoints)));

        foreach (var type in endpointTypes)
        {
            var mapMethod = type.GetMethod(
                nameof(IEndpoints.MapEndpoints),
                BindingFlags.Public | BindingFlags.Static
            );

            mapMethod?.Invoke(null, new[] { app });
        }

        return app;
    }
}
