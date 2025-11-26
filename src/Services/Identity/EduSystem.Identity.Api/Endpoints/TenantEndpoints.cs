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
        var versionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .HasDeprecatedApiVersion(new ApiVersion(1, 0))
            .HasApiVersion(new ApiVersion(2, 0))
            .ReportApiVersions()
            .Build();

        var groupV1 = app.MapGroup("/api/v{version:apiVersion}/tenants")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(1, 0)
            .WithTags("Tenants");

        groupV1.MapPost("/register", RegisterTenantV1)
            .WithName("RegisterTenantV1")
            .WithSummary("Register a new tenant (V1)")
            .WithDescription("⚠️ This version is deprecated. Please use V2.")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        var groupV2 = app.MapGroup("/api/v{version:apiVersion}/tenants")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(2, 0)
            .WithTags("Tenants");

        groupV2.MapPost("/register", RegisterTenantV2)
            .WithName("RegisterTenantV2")
            .WithSummary("Register a new tenant (V2)")
            .WithDescription("Creates a new tenant in the system - Version 2")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> RegisterTenantV1(TenantRegistrationDto dto, IMediator mediator)
    {
        var command = new RegisterTenantCommand { Registration = dto };
        var result = await mediator.Send(command);
        return result.IsSuccess
            ? Results.Ok(new { tenantId = result.Data, version = "v1" })
            : Results.BadRequest(new { errors = result.Errors });
    }

    private static async Task<IResult> RegisterTenantV2(TenantRegistrationDto dto, IMediator mediator)
    {
        var command = new RegisterTenantCommand { Registration = dto };
        var result = await mediator.Send(command);
        return result.IsSuccess
            ? Results.Ok(new
            {
                tenantId = result.Data,
                version = "v2",
                message = result.ErrorMessage ?? "Tenant registered successfully."
            })
            : Results.BadRequest(new { errors = result.Errors });
    }
}
