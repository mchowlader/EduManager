namespace EduSystem.ApplicationUsers.Application.IService;

public interface IUnitOfWork
{
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}