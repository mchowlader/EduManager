using System;
using System.Collections.Generic;
using System.Text;
using EduSystem.Identity.Application.IService;
using EduSystem.Identity.Domain.IRepository;
using EduSystem.Identity.Shared.Common;
using MediatR;

namespace EduSystem.Identity.Application.Commands;

public class ResetPasswordCommand : IRequest<Result<bool>>
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}


public class ResetPasswordCommandHandler(
    IUserRepository userRepository,
    IPasswordResetTokenRepository tokenRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ResetPasswordCommand, Result<bool>>
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IPasswordResetTokenRepository _tokenRepository = tokenRepository;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<bool>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        // Get token from database
        var resetToken = await _tokenRepository.GetByTokenAsync(request.Token);

        if (resetToken == null)
            return Result<bool>.Failure("Invalid or expired reset token");

        // Check if token is expired
        if(resetToken.ExpiresAt < DateTime.UtcNow)
            return Result<bool>.Failure("Reset token has expired.");

        // Check if token is already used
        if (resetToken.IsUsed)
            return Result<bool>.Failure("Reset token has already been used.");

        // Get user
        var user = await _userRepository.GetByIdAsync(resetToken.UserId);
        if (user == null)
            return Result<bool>.Failure("User not found.");

        // Update password
        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        // Invalidate all refresh tokens
        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;

        // Mark token as used
        resetToken.IsUsed = true;
        resetToken.UsedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        await _tokenRepository.UpdateAsync(resetToken);
        await _unitOfWork.CommitAsync();

        return Result<bool>.Success(true);
    }
}
