using EduSystem.ApplicationUsers.Application.IService;
using EduSystem.ApplicationUsers.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore.Storage;
namespace EduSystem.ApplicationUsers.Infrastructure.Services;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppUserDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(AppUserDbContext context)
    {
        _context = context;
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