using Asp.Versioning;
using Asp.Versioning.Builder;
using EduSystem.Identity.Application.Commands;
using EduSystem.Identity.Application.DTOs;
using MediatR;

namespace EduSystem.Identity.Api.Endpoints;

public class TenantEndpoints : IEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder app)
    {
        // ✅ Version set - শুধু Tenant এর v1 deprecated
        var versionSet = app.NewApiVersionSet()
            .HasDeprecatedApiVersion(new ApiVersion(1, 0))  // v1 deprecated
            .HasApiVersion(new ApiVersion(2, 0))             // v2 current
            .ReportApiVersions()
            .Build();

        // ============ V1 Tenant Endpoints (Deprecated) ============
        var groupV1 = app.MapGroup("/api/v{version:apiVersion}/tenants")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1, 0)
            .WithTags("Tenants (v1)");

        groupV1.MapPost("/register", RegisterTenantV1)
            .WithName("RegisterTenantV1")
            .WithSummary("Register a new tenant")
            .WithDescription("Creates a new tenant in the system")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        // ============ V2 Tenant Endpoints (Current) ============
        var groupV2 = app.MapGroup("/api/v{version:apiVersion}/tenants")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(2, 0)
            .WithTags("Tenants (v2)");

        groupV2.MapPost("/register", RegisterTenantV2)
            .WithName("RegisterTenantV2")
            .WithSummary("Register a new tenant")
            .WithDescription("Creates a new tenant in the system with enhanced validation")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> RegisterTenantV1(TenantRegistrationDto dto, IMediator mediator)
    {
        var command = new RegisterTenantCommand { Registration = dto };
        var result = await mediator.Send(command);
        return result.IsSuccess
            ? Results.Ok(new { success = true, message = "Tenant registered successfully (v1)", data = result.Data })
            : Results.BadRequest(new { success = false, message = result.ErrorMessage, errors = result.Errors });
    }

    private static async Task<IResult> RegisterTenantV2(TenantRegistrationDto dto, IMediator mediator)
    {
        var command = new RegisterTenantCommand { Registration = dto };
        var result = await mediator.Send(command);
        return result.IsSuccess
            ? Results.Ok(new
            {
                success = true,
                message = result.ErrorMessage ?? "Tenant registered successfully (v2)",
                data = result.Data
            })
            : Results.BadRequest(new { success = false, message = result.ErrorMessage, errors = result.Errors });
    }
}
