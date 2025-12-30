using EduSystem.ApplicationUsers.Application.Contracts.Persistence;
using EduSystem.ApplicationUsers.Shared.Common;
using MediatR;

namespace EduSystem.ApplicationUsers.Application.Features.Base.Queries;

public record BaseGetEntityByIdQuery<TEntity, TResponseDto>(Guid Id) : IRequest<Result<TResponseDto>>
    where TEntity : class
    where TResponseDto : class, new();

public class BaseGetEntityByIdQueryHandler<TEntity, TResponseDto>(IGenericRepository<TEntity> repository)
    : IRequestHandler<BaseGetEntityByIdQuery<TEntity, TResponseDto>, Result<TResponseDto>>
    where TEntity : class
    where TResponseDto : class, new()
{
    public async Task<Result<TResponseDto>> Handle(BaseGetEntityByIdQuery<TEntity, TResponseDto> request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity == null) return Result<TResponseDto>.Failure("Entity not found");

        var result = new TResponseDto();
        Map(entity, result);
        return Result<TResponseDto>.Success(result);
    }

    private static void Map(object source, object target)
    {
        var sourceProps = source.GetType().GetProperties();
        var targetProps = target.GetType().GetProperties();

        foreach (var sourceProp in sourceProps)
        {
            var targetProp = targetProps.FirstOrDefault(p => p.Name == sourceProp.Name && p.PropertyType == sourceProp.PropertyType);
            if (targetProp != null && targetProp.CanWrite)
            {
                targetProp.SetValue(target, sourceProp.GetValue(source));
            }
        }
    }
}
