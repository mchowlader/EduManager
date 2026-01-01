using EduSystem.ApplicationUsers.Domain.IRepository;
using EduSystem.ApplicationUsers.Application.IService;
using EduSystem.ApplicationUsers.Shared.Common;
using MediatR;

namespace EduSystem.ApplicationUsers.Application.Features.Base.Commands;

public record BaseCreateEntityCommand<TEntity, TCreateDto, TResponseDto>(TCreateDto Data) : IRequest<Result<TResponseDto>>
    where TEntity : class, new()
    where TResponseDto : class, new();

public class BaseCreateEntityCommandHandler<TEntity, TCreateDto, TResponseDto>(
    IGenericRepository<TEntity> repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<BaseCreateEntityCommand<TEntity, TCreateDto, TResponseDto>, Result<TResponseDto>>
    where TEntity : class, new()
    where TResponseDto : class, new()
{
    public async Task<Result<TResponseDto>> Handle(BaseCreateEntityCommand<TEntity, TCreateDto, TResponseDto> request, CancellationToken cancellationToken)
    {
        try
        {
            var entity = new TEntity();
            Map(request.Data!, entity);

            await repository.AddAsync(entity, cancellationToken);
            await unitOfWork.CommitAsync();

            var result = new TResponseDto();
            Map(entity, result);

            return Result<TResponseDto>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<TResponseDto>.Failure(ex.Message);
        }
    }

    private static void Map(object source, object target)
    {
        var sourceProps = source.GetType().GetProperties();
        var targetProps = target.GetType().GetProperties();

        foreach (var sourceProp in sourceProps)
        {
            if (sourceProp.Name == "Id") continue;

            var value = sourceProp.GetValue(source);
            var targetProp = targetProps.FirstOrDefault(p => p.Name.Equals(sourceProp.Name, StringComparison.OrdinalIgnoreCase) && p.PropertyType == sourceProp.PropertyType);
            
            if (targetProp != null && targetProp.CanWrite)
            {
                targetProp.SetValue(target, value);
            }
        }
    }
}
