using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EduSystem.Admin.Api.Endpoints;

public class HealthApi : IEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/health").WithTags("Health");

        // Health check endpoint
        group.MapGet("/health", async (HealthCheckService healthCheckService) =>
        {
            var report = await healthCheckService.CheckHealthAsync();

            return Results.Ok(new
            {
                status = report.Status.ToString(),
                duration = report.TotalDuration.TotalMilliseconds + "ms",
                timestamp = DateTime.UtcNow,
                info = report.Entries.Select(e => new
                {
                    key = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description ?? "No description"
                })
            });
        })
        .WithName("HealthCheck")
        .WithTags("Health")
        .WithSummary("Get health status")
        .Produces<object>(StatusCodes.Status200OK);

        // Ping endpoint
        group.MapGet("/ping", () =>
        {
            return Results.Ok(new
            {
                message = "pong",
                timestamp = DateTime.UtcNow,
                service = "Admin API"
            });
        })
        .WithName("Ping")
        .WithTags("Health")
        .WithSummary("Simple ping test")
        .Produces<object>(StatusCodes.Status200OK);

    }
}
