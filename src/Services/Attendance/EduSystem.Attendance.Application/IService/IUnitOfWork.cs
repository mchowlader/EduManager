namespace EduSystem.Attendance.Application.IService;

public interface IUnitOfWork
{
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}
