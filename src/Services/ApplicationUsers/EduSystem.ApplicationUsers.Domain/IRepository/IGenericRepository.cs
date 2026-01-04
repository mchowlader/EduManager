using EduSystem.ApplicationUsers.Shared.Common;

namespace EduSystem.ApplicationUsers.Domain.IRepository;

public interface IGenericRepository<T> where T : class
{
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    Task<PagedList<T>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}
