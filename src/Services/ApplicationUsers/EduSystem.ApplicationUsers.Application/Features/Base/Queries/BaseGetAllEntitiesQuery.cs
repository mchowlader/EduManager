using EduSystem.ApplicationUsers.Application.Contracts.Persistence;
using EduSystem.ApplicationUsers.Shared.Common;
using MediatR;

namespace EduSystem.ApplicationUsers.Application.Features.Base.Queries;

public record BaseGetAllEntitiesQuery<TEntity, TResponseDto>(int PageNumber, int PageSize) : IRequest<Result<PagedList<TResponseDto>>>
    where TEntity : class
    where TResponseDto : class, new();

public class BaseGetAllEntitiesQueryHandler<TEntity, TResponseDto>(IGenericRepository<TEntity> repository)
    : IRequestHandler<BaseGetAllEntitiesQuery<TEntity, TResponseDto>, Result<PagedList<TResponseDto>>>
    where TEntity : class
    where TResponseDto : class, new()
{
    public async Task<Result<PagedList<TResponseDto>>> Handle(BaseGetAllEntitiesQuery<TEntity, TResponseDto> request, CancellationToken cancellationToken)
    {
        var pagedList = await repository.GetPagedAsync(request.PageNumber, request.PageSize, cancellationToken);
        
        var items = pagedList.Items.Select(entity => 
        {
            var dto = new TResponseDto();
            Map(entity, dto);
            return dto;
        }).ToList();

        return Result<PagedList<TResponseDto>>.Success(new PagedList<TResponseDto>
        {
            Items = items,
            TotalCount = pagedList.TotalCount,
            PageNumber = pagedList.PageNumber,
            PageSize = pagedList.PageSize
        });
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
