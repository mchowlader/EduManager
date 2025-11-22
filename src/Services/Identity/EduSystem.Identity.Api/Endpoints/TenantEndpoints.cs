using EduSystem.Identity.Application.Commands;
using EduSystem.Identity.Application.DTOs;
using MediatR;

namespace EduSystem.Identity.Api.Endpoints;

public class TenantEndpoints : IEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tenants").WithTags("Tenants");
        group.MapPost("/register", RegisterTenant)
            .WithName("RegisterTenant");

    }

    private static async Task<IResult> RegisterTenant(TenantRegistrationDto dto, IMediator mediator)
    {
        var command = new RegisterTenantCommand { Registration = dto };
        var result = await mediator.Send(command);

        return result.IsSuccess
            ? Results.Ok(new { tenantId = result.Data })
            : Results.BadRequest(new { errors = result.Errors });
    }
}
