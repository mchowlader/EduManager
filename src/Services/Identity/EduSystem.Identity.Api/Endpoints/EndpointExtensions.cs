using System.Reflection;

namespace EduSystem.Identity.Api.Endpoints;

/// <summary>
/// Extension methods for automatic endpoint registration
/// </summary>
public static class EndpointExtensions
{
    /// <summary>
    /// Automatically discovers and maps all endpoints implementing IEndpoints
    /// </summary>
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app)
    {
        var endpointTypes = GetEndpointTypes();

        foreach (var type in endpointTypes)
        {
            var mapMethod = type.GetMethod(
                nameof(IEndpoints.MapEndpoints),
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(IEndpointRouteBuilder) },
                null
            );

            mapMethod?.Invoke(null, new[] { app });
        }

        return app;
    }

    /// <summary>
    /// Automatically discovers and maps endpoints from specified assembly
    /// </summary>
    public static IEndpointRouteBuilder MapEndpoints(
        this IEndpointRouteBuilder app,
        Assembly assembly)
    {
        var endpointTypes = assembly
            .GetTypes()
            .Where(t => t.IsClass
                && !t.IsAbstract
                && t.GetInterfaces().Any(i => i == typeof(IEndpoints)));

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

    private static IEnumerable<Type> GetEndpointTypes()
    {
        var assembly = Assembly.GetExecutingAssembly();

        return assembly
            .GetTypes()
            .Where(t => t.IsClass
                && !t.IsAbstract
                && t.GetInterfaces().Any(i => i == typeof(IEndpoints)))
            .OrderBy(t => t.Name);
    }
}