using EduSystem.ApplicationUsers.Domain.IRepository;
using EduSystem.ApplicationUsers.Application.IService;
using EduSystem.ApplicationUsers.Shared.Common;
using MediatR;

namespace EduSystem.ApplicationUsers.Application.Features.Base.Commands;

public record BaseDeleteEntityCommand<TEntity>(long Id) : IRequest<Result>
    where TEntity : class;

public class BaseDeleteEntityCommandHandler<TEntity>(
    IGenericRepository<TEntity> repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<BaseDeleteEntityCommand<TEntity>, Result>
    where TEntity : class
{
    public async Task<Result> Handle(BaseDeleteEntityCommand<TEntity> request, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await repository.GetByIdAsync(request.Id, cancellationToken);
            if (entity == null) return Result.Failure("Entity not found");

            await repository.DeleteAsync(entity, cancellationToken);
            await unitOfWork.CommitAsync();
            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}
