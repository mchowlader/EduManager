using System.Security.Claims;
using EduSystem.Identity.Application.DTOs;
using EduSystem.Identity.Application.IService;
using EduSystem.Identity.Application.Settings;
using EduSystem.Identity.Domain.IRepository;
using EduSystem.Identity.Shared.Common;
using MediatR;

namespace EduSystem.Identity.Application.Commands;

public class RefreshTokenCommand : IRequest<Result<LoginResponseDto>>
{
    public RefreshTokenRequestDto RefreshTokenRequest { get; set; } = null!;
}

public class RefreshTokenCommandHandler(
    ITokenService tokenService,
    JwtSettings jwtSettings,
    IUnitOfWork unitOfWork,
    IUserRepository userRepository,
    ITenantRepository tenantRepository)
    : IRequestHandler<RefreshTokenCommand, Result<LoginResponseDto>>
{
    private readonly ITokenService _tokenService = tokenService;
    private readonly JwtSettings _jwtSettings = jwtSettings;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly ITenantRepository _tenantRepository = tenantRepository;

    public async Task<Result<LoginResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var dto = request.RefreshTokenRequest;

        // 1. Validate and extract claims from expired token
        var principal = _tokenService.GetPrincipalFromExpiredToken(dto.AccessToken);
        if(principal == null)
            return Result<LoginResponseDto>.Failure("Invalid access token.");

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Result<LoginResponseDto>.Failure("Invalid token claims.");

        // 2. Get user from database
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return Result<LoginResponseDto>.Failure("User not found.");

        // 3. Validate refresh token
        if (user.RefreshToken != dto.RefreshToken)
            return Result<LoginResponseDto>.Failure("Invalid refresh token.");

        if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            return Result<LoginResponseDto>.Failure("Refresh token has expired.");

        // 4. Check if user is still active
        if(!user.IsActive)
            return Result<LoginResponseDto>.Failure("User account is inactive.");

        // 5. Get tenant
        var tenant = await _tenantRepository.GetByIdAsync(user.TenantId);
        if (tenant == null)
            return Result<LoginResponseDto>.Failure("Tenant is not found.");

        if (!tenant.IsActive)
            return Result<LoginResponseDto>.Failure("Tenant is inactive.");

        // 6. Generate new tokens
        var newAccessToken = _tokenService.GenerateAccessToken(user, tenant);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        // 7. Update refresh token in database
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);

        // 8. Return response
        var response = new LoginResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            User = new UserInfoDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role.ToString()
            },
            Tenant = new TenantInfoDto
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Slug = tenant.Slug ?? string.Empty,
                LogoUrl = tenant.LogoUrl,
                PrimaryColor = tenant.PrimaryColor,
                SecondaryColor = tenant.SecondaryColor
            }

        };

        return Result<LoginResponseDto>.Success(response);
    }
}
