using Asp.Versioning;
using Asp.Versioning.Builder;
using EduSystem.ApplicationUsers.Application.Contracts.Persistence;
using EduSystem.ApplicationUsers.Application.Features.Base.Commands;
using EduSystem.ApplicationUsers.Application.Features.Base.Queries;
using EduSystem.ApplicationUsers.Domain.Entities;
using EduSystem.ApplicationUsers.Shared.Common;
using EduSystem.Shared.Infrastructure.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EduSystem.ApplicationUsers.Api.EndPoints;

public abstract class BaseEndPoints<TEntity, TCreateDto, TUpdateDto, TResponseDto>
    where TEntity : BaseEntity, new()
    where TCreateDto : class
    where TUpdateDto : class
    where TResponseDto : class, new()
{
    protected static string EntityName => typeof(TEntity).Name;
    protected static string Route => ToKebabCase(typeof(TEntity).Name);
    protected const double DefaultApiVersion = 1.0;

    protected static RouteGroupBuilder MapBaseEndpoints(IEndpointRouteBuilder app, double version = DefaultApiVersion)
    {
        var versionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(version))
            .ReportApiVersions()
            .Build();

        var group = app.MapGroup($"/api/v{{version:apiVersion}}/{Route}")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(version)
            .WithTags(EntityName);

        // GET Paged (GetAll internally)
        group.MapGet("/", GetAllV1)
        .WithName($"GetAll{EntityName}")
        .WithSummary($"Get all {EntityName}")
        .WithDescription($"Retrieves all {ToFriendlyName(EntityName)} records with internal pagination support.")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest);

        // GET Single
        group.MapGet("/{id:guid}", GetByIdV1)
        .WithName($"Get{EntityName}ById")
        .WithSummary($"Get {EntityName} by ID")
        .WithDescription($"Retrieves a single {ToFriendlyName(EntityName)} record by its unique identifier.")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status404NotFound);

        // POST Create
        group.MapPost("/", CreateV1)
        .WithName($"Create{EntityName}")
        .WithSummary($"Create a new {EntityName}")
        .WithDescription($"Adds a new {ToFriendlyName(EntityName)} to the system.")
        .Produces<object>(StatusCodes.Status201Created)
        .Produces<object>(StatusCodes.Status400BadRequest);

        // PUT Update
        group.MapPut("/{id:guid}", UpdateV1)
        .WithName($"Update{EntityName}")
        .WithSummary($"Update existing {EntityName}")
        .WithDescription($"Updates an existing {ToFriendlyName(EntityName)} record identified by ID.")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest);

        // DELETE
        group.MapDelete("/{id:guid}", DeleteV1)
        .WithName($"Delete{EntityName}")
        .WithSummary($"Delete {EntityName}")
        .WithDescription($"Performs a soft delete of a {ToFriendlyName(EntityName)} from the system.")
        .Produces<object>(StatusCodes.Status204NoContent)
        .Produces<object>(StatusCodes.Status400BadRequest);

        return group;
    }

    private static async Task<IResult> GetAllV1(
        [FromServices] IMediator mediator,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await mediator.Send(new BaseGetAllEntitiesQuery<TEntity, TResponseDto>(pageNumber, pageSize));
        if (!result.IsSuccess)
        {
            return Results.BadRequest(new { success = false, message = result.ErrorMessage, errors = new[] { result.ErrorMessage } });
        }
        return Results.Ok(new { success = true, message = $"{EntityName} list retrieved successfully", data = result.Data });
    }

    private static async Task<IResult> GetByIdV1(
        Guid id,
        [FromServices] IMediator mediator)
    {
        var result = await mediator.Send(new BaseGetEntityByIdQuery<TEntity, TResponseDto>(id));
        if (!result.IsSuccess)
        {
            return Results.NotFound(new { success = false, message = result.ErrorMessage });
        }
        return Results.Ok(new { success = true, message = $"{EntityName} retrieved successfully", data = result.Data });
    }

    private static async Task<IResult> CreateV1(
        TCreateDto dto,
        [FromServices] IMediator mediator)
    {
        var result = await mediator.Send(new BaseCreateEntityCommand<TEntity, TCreateDto, TResponseDto>(dto));
        if (!result.IsSuccess)
        {
            return Results.BadRequest(new { success = false, message = result.ErrorMessage, errors = new[] { result.ErrorMessage } });
        }
        // In a real generic scenario, we might not have the version here easily, but we can default to 1 for standard responses
        return Results.Created($"/api/v1/{Route}/{result.Data}", new { success = true, message = $"{EntityName} created successfully", data = result.Data });
    }

    private static async Task<IResult> UpdateV1(
        Guid id,
        TUpdateDto dto,
        [FromServices] IMediator mediator)
    {
        var result = await mediator.Send(new BaseUpdateEntityCommand<TEntity, TUpdateDto, TResponseDto>(id, dto));
        if (!result.IsSuccess)
        {
            return Results.BadRequest(new { success = false, message = result.ErrorMessage, errors = new[] { result.ErrorMessage } });
        }
        return Results.Ok(new { success = true, message = $"{EntityName} updated successfully", data = result.Data });
    }

    private static async Task<IResult> DeleteV1(
        Guid id,
        [FromServices] IMediator mediator)
    {
        var result = await mediator.Send(new BaseDeleteEntityCommand<TEntity>(id));
        if (!result.IsSuccess)
        {
            return Results.BadRequest(new { success = false, message = result.ErrorMessage });
        }
        return Results.Ok(new { success = true, message = $"{EntityName} deleted successfully" });
    }

    private static string ToFriendlyName(string name)
    {
        return string.Concat(name.Select((x, i) => i > 0 && char.IsUpper(x) ? " " + x.ToString() : x.ToString())).ToLower();
    }

    private static string ToKebabCase(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return string.Concat(value.Select((x, i) => i > 0 && char.IsUpper(x) ? "-" + x.ToString() : x.ToString())).ToLower();
    }
}
