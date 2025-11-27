using System.Security.Claims;
using EduSystem.ApplicationUsers.Application.IService;
using Microsoft.AspNetCore.Http;

namespace EduSystem.ApplicationUsers.Infrastructure.Service;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public Guid? GetCurrentUserId()
    {
        var userIdString = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(userIdString, out var userId))

            return userId;

        return Guid.Empty;
    }
}
