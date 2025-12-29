using EduSystem.ApplicationUsers.Application.Contracts.Persistence;
using EduSystem.ApplicationUsers.Application.IService;
using EduSystem.ApplicationUsers.Shared.Common;
using MediatR;

namespace EduSystem.ApplicationUsers.Application.Features.Base.Commands;

public record BaseUpdateEntityCommand<TEntity, TUpdateDto, TResponseDto>(Guid Id, TUpdateDto Data) : IRequest<Result<TResponseDto>>
    where TEntity : class;

public class BaseUpdateEntityCommandHandler<TEntity, TUpdateDto, TResponseDto>(
    IGenericRepository<TEntity> repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<BaseUpdateEntityCommand<TEntity, TUpdateDto, TResponseDto>, Result<TResponseDto>>
    where TEntity : class
    where TResponseDto : class, new()
{
    public async Task<Result<TResponseDto>> Handle(BaseUpdateEntityCommand<TEntity, TUpdateDto, TResponseDto> request, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await repository.GetByIdAsync(request.Id, cancellationToken);
            if (entity == null) return Result<TResponseDto>.Failure("Entity not found");

            Map(request.Data, entity);
            await repository.UpdateAsync(entity, cancellationToken);
            await unitOfWork.CommitAsync();

            var result = new TResponseDto();
            Map(entity, result);
            return Result<TResponseDto>.Success(result);
        }
        catch (Exception ex) { return Result<TResponseDto>.Failure(ex.Message); }
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
