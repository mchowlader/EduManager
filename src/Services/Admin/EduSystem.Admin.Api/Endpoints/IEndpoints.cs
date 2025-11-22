namespace EduSystem.Admin.Api.Endpoints;


/// <summary>
/// Interface for all endpoint classes
/// </summary>
public interface IEndpoints
{
    /// <summary>
    /// Map endpoints to the application
    /// </summary>
    /// <param name="app">Endpoint route builder</param>
    static abstract void MapEndpoints(IEndpointRouteBuilder app);
}