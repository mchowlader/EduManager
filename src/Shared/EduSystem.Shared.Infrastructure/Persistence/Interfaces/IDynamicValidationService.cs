using Microsoft.EntityFrameworkCore;

namespace EduSystem.Shared.Infrastructure.Persistence.Interfaces;

public interface IDynamicValidationService
{
    Task<(bool IsValid, string Message)> ValidateEntityAsync<TEntity>(DbContext dbContext, TEntity entity, CancellationToken cancellationToken = default) where TEntity : class;
}
