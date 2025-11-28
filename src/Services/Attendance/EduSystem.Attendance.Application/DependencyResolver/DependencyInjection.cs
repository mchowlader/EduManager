using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EduSystem.Attendance.Application.DependencyResolver;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Application service registrations go here
        return services;
    }

}
