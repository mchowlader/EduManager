using EduSystem.ApplicationUsers.Application.Contracts.Persistence;
using EduSystem.ApplicationUsers.Domain.Entities;
using EduSystem.ApplicationUsers.Infrastructure.Contexts;
using EduSystem.Shared.Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;

namespace EduSystem.ApplicationUsers.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    private readonly AppUserDbContext _dbContext;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(AppUserDbContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = dbContext.Set<T>();
    }

    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.IsDeleted = true;
        entity.DeleteAt = DateTimeHelper.Now;

        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var dbResult = await _dbSet.AsNoTracking()
            .Where(e => !e.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return dbResult;
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var dbResult = await _dbSet.FindAsync(new object[] { id }, cancellationToken);
        return dbResult;
    }

    public async Task<PagedList<T>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var totalCount = await _dbSet.CountAsync(e => !e.IsDeleted);
        var items = await _dbSet
            .Skip((pageNumber-1)*pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var pagedList = new PagedList<T>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return pagedList;
    }

    public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }
}
