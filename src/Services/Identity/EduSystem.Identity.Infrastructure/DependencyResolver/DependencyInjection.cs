using EduSystem.Identity.Application.IService;
using EduSystem.Identity.Application.Settings;
using EduSystem.Identity.Domain.IRepository;
using EduSystem.Identity.Infrastructure.Contexts;
using EduSystem.Identity.Infrastructure.Repositories;
using EduSystem.Identity.Infrastructure.Service;
using EduSystem.Identity.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EduSystem.Identity.Infrastructure.DependencyResolver;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<IdentityDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("MasterDBConnection")));

        // JWT Settings
        var jwtSettings = new JwtSettings();
        configuration.GetSection("JwtSettings").Bind(jwtSettings);

        if (string.IsNullOrEmpty(jwtSettings.SecretKey) || jwtSettings.SecretKey.Length < 32)
            throw new InvalidOperationException("JWT SecretKey must be at least 32 characters");

        services.AddSingleton(jwtSettings);

        // Email Settings
        var emailSettings = new EmailSettings();
        configuration.GetSection("EmailSettings").Bind(emailSettings);
        services.AddSingleton(emailSettings);

        // Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();

        // Services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITenantDatabaseProvisioner, TenantDatabaseProvisioner>();

        return services;
    }
}
