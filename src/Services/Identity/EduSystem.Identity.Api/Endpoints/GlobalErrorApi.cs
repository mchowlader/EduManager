
using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;

namespace EduSystem.Identity.Api.Endpoints;

public class GlobalErrorApi : IEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/error")
            .WithTags("Error");

        group.Map("/", (HttpContext context, ILogger<GlobalErrorApi> logger) =>
        {
            var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
            var exception = exceptionFeature?.Error;
            var traceId = context.TraceIdentifier;

            logger.LogError(exception,
                "Error ID: {TraceId} | Path: {Path} | Message: {Message}",
                traceId,
                context.Request.Path,
                exception?.Message);

            return Results.Problem(
                title: "An error occurred",
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?>
                {
                    {"traceId", context.TraceIdentifier   }
                }
            );

        });
    }
}
