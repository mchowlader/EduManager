using Microsoft.AspNetCore.Routing;

namespace EduSystem.Shared.Infrastructure.Interfaces;

/// <summary>
/// Interface for all endpoint classes to enable automatic discovery and registration.
/// </summary>
public interface IEndpoints
{
    /// <summary>
    /// Maps the endpoints to the application route builder.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    static abstract void MapEndpoints(IEndpointRouteBuilder app);
}
