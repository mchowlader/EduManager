namespace EduSystem.ApplicationUsers.Application.IService;

public interface ICurrentUserService
{
    Guid? GetCurrentUserId();
}
