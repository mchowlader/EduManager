using EduSystem.ApplicationUsers.Application.DTOs;
using EduSystem.ApplicationUsers.Domain.Entities;
using EduSystem.Shared.Infrastructure.Interfaces;

namespace EduSystem.ApplicationUsers.Api.EndPoints;

public class ClassesEndPoints
    : BaseEndPoints<Classes, ClassesCreateDto, ClassesUpdateDto, ClassesResponseDto>, IEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = MapBaseEndpoints(app);
    }
}
