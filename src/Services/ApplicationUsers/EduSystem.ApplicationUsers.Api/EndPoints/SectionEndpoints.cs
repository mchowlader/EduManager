using EduSystem.ApplicationUsers.Application.DTOs;
using EduSystem.ApplicationUsers.Domain.Entities;
using EduSystem.Shared.Infrastructure.Interfaces;

namespace EduSystem.ApplicationUsers.Api.EndPoints;

public class SectionEndpoints : BaseEndPoints<Section, SectionCreateDto, SectionUpdateDto, SectionResponseDto>, IEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder app)
    {
        //1. map base endpoints
        var group = MapBaseEndpoints(app);

        // 2.add custom endpoint for searching sections by name
        group.MapGet("/search", SearchByNameV1)
            .WithName("SearchSectionsByName")
            .WithSummary("Search sections by name")
            .Produces<object>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> SearchByNameV1(
        string name,
        CancellationToken cancellationToken)
    {
        //implement your search logic here. For demonstration, returning a dummy response.
        await Task.CompletedTask;
        return Results.Ok(new
        {
            success = true,
            message = $"Searching for sections with name: {name}",
            data = new[] { new { id = Guid.NewGuid(), name = name } }
        });
    }
}
