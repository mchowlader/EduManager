using EduSystem.ApplicationUsers.Application.IService;
using EduSystem.ApplicationUsers.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore.Storage;
using EduSystem.Shared.Infrastructure.Persistence.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace EduSystem.ApplicationUsers.Infrastructure.Services;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppUserDbContext _context;
    private readonly IDynamicValidationService _validationService;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(AppUserDbContext context, IDynamicValidationService validationService)
    {
        _context = context;
        _validationService = validationService;
    }

    public async Task<(bool IsValid, string Message)> ValidateAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class
    {
        return await _validationService.ValidateEntityAsync(_context, entity, cancellationToken);
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
        catch
        {
            await RollbackAsync();
            throw;
        }
    }

    public async Task RollbackAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}