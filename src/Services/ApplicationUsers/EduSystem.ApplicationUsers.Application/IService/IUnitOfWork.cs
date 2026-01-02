using System.Threading;
using System.Threading.Tasks;

namespace EduSystem.ApplicationUsers.Application.IService;

public interface IUnitOfWork
{
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
    Task<(bool IsValid, string Message)> ValidateAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class;
}