using EduSystem.Identity.Application.DTOs;
using EduSystem.Identity.Application.IService;
using EduSystem.Identity.Application.Settings;
using EduSystem.Identity.Domain.Entities;
using EduSystem.Identity.Domain.IRepository;
using EduSystem.Identity.Shared.Common;
using MediatR;
using Microsoft.Extensions.Options;

namespace EduSystem.Identity.Application.Commands;

public class LoginCommand : IRequest<Result<LoginResponseDto>>
{
    public LoginRequestDto LoginRequest { get; set; } = null!;
}

public class LoginCommandHandler(
    IUserRepository userRepository,
    ITenantRepository tenantRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IUnitOfWork unitOfWork,
    IOptions<JwtSettings> jwtSettings,
    IOptions<SecuritySettings> securitySettings) : IRequestHandler<LoginCommand, Result<LoginResponseDto>>
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly ITenantRepository _tenantRepository = tenantRepository;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;
    private readonly SecuritySettings _securitySettings = securitySettings.Value;

    public async Task<Result<LoginResponseDto>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var dto = request.LoginRequest;

        // 1. Find user by email
        var user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user == null)
        {
            return Result<LoginResponseDto>.Failure("Invalid email or password.");
        }

        // 2. Check account lockout
        if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
        {
            var remainingMinutes = (int)(user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes;
            return Result<LoginResponseDto>.Failure(
                $"Account is locked due to multiple failed login attempts. Please try again in {remainingMinutes} minutes.");
        }

        // 3. Check if user is active
        if (!user.IsActive)
        {
            return Result<LoginResponseDto>.Failure("Your account has been deactivated.");
        }

        // 4. Verify password
        if (!_passwordHasher.Verify(dto.Password, user.PasswordHash))
        {
            // Increment login attempts
            user.LoginAttempts++;
            user.UpdatedAt = DateTime.UtcNow;

            // Lock account after max attempts (using config value)
            if (user.LoginAttempts >= _securitySettings.MaxLoginAttempts)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(_securitySettings.AccountLockoutMinutes);
                await _userRepository.UpdateAsync(user);
                await _unitOfWork.CommitAsync();

                return Result<LoginResponseDto>.Failure(
                    $"Account locked due to {_securitySettings.MaxLoginAttempts} failed login attempts. " +
                    $"Please try again after {_securitySettings.AccountLockoutMinutes} minutes.");
            }

            await _userRepository.UpdateAsync(user);
            await _unitOfWork.CommitAsync();

            var remainingAttempts = _securitySettings.MaxLoginAttempts - user.LoginAttempts;
            return Result<LoginResponseDto>.Failure(
                $"Invalid email or password. {remainingAttempts} attempt(s) remaining.");
        }

        // 5. Get tenant information
        var tenant = await _tenantRepository.GetByIdAsync(user.TenantId);
        if (tenant == null)
        {
            return Result<LoginResponseDto>.Failure("Tenant not found.");
        }

        // 6. Check if tenant is active
        if (!tenant.IsActive)
        {
            return Result<LoginResponseDto>.Failure(
                "Your organization's account is currently inactive.");
        }

        // 7. Special handling for SuperAdmin (optional multi-tenant access)
        if (user.Role == UserRole.SuperAdmin && !string.IsNullOrEmpty(dto.TenantSlug))
        {
            var targetTenant = await _tenantRepository.GetBySlugAsync(dto.TenantSlug);
            if (targetTenant != null && targetTenant.IsActive)
            {
                tenant = targetTenant;
            }
        }

        // 8. Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user, tenant);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // 9. Update user login info
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow
            .AddDays(_jwtSettings.RefreshTokenExpirationDays);
        user.LastLoginAt = DateTime.UtcNow;
        user.LoginAttempts = 0; // Reset login attempts on successful login
        user.LockoutEnd = null; // Clear lockout
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.CommitAsync();

        // 10. Prepare response
        var response = new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow
                .AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            User = new UserInfoDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role.ToString(),
                IsActive = user.IsActive
            },
            Tenant = new TenantInfoDto
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Slug = tenant.Slug ?? string.Empty,
                LogoUrl = tenant.LogoUrl,
                BannerUrl = tenant.BannerUrl,
                PrimaryColor = tenant.PrimaryColor,
                SecondaryColor = tenant.SecondaryColor
            }
        };

        return Result<LoginResponseDto>.Success(response);
    }
}
