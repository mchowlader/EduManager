namespace EduSystem.Attendance.Application.IService;

public interface ICurrentUserService
{
    Guid? GetCurrentUserId();
}
