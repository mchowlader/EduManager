using System.Reflection;

namespace EduSystem.Attendance.Api.EndPoints;

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
        var endPointTypes = GetEndPointType();

        foreach(var type in endPointTypes)
        {
            var mapMethod = type.GetMethod(
                nameof(IEndpoints.MapEndPoints),
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(IEndpointRouteBuilder)},
                null
            );

            mapMethod?.Invoke(null, new[] {app});
        }

        return app;
    }

    /// <summary>
    /// Automatically discovers and maps endpoints from specified assembly
    /// </summary>
    public static IEndpointRouteBuilder MapEndPoints(
        this IEndpointRouteBuilder app, Assembly assembly)
    {
        var endPointType = assembly
            .GetTypes()
            .Where(t => t.IsClass
                && !t.IsAbstract
                && t.GetInterfaces().Any(i => i == typeof(IEndpoints)));

        foreach (var type in endPointType)
        {
            var mapMethod = type.GetMethod(
                nameof(IEndpoints.MapEndPoints),
                BindingFlags.Public | BindingFlags.Static
            );

            mapMethod?.Invoke(null, new[] { app });
        }

        return app;
    }

    public static IEnumerable<Type> GetEndPointType()
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
