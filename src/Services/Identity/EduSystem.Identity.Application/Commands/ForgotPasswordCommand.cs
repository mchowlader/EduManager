using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using EduSystem.Identity.Application.IService;
using EduSystem.Identity.Domain.Entities;
using EduSystem.Identity.Domain.IRepository;
using EduSystem.Identity.Shared.Common;
using MediatR;

namespace EduSystem.Identity.Application.Commands;

public class ForgotPasswordCommand : IRequest<Result<bool>>
{
    public string Email { get; set; } = string.Empty;
}

public class ForgotPasswordCommandHandler(
    IUserRepository userRepository,
    IPasswordResetTokenRepository tokenRepository,
    IEmailService emailService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ForgotPasswordCommand, Result<bool>>
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IPasswordResetTokenRepository _tokenRepository = tokenRepository;
    private readonly IEmailService _emailService = emailService;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<bool>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user == null || !user.IsActive)
            return Result<bool>.Success(true);

        var resetToken = GenerateSecureToken();
        var expiryTime = DateTime.UtcNow.AddHours(1);

        await _tokenRepository.CreateAsync(new PasswordResetToken
        {
            UserId = user.Id,
            Token = resetToken,
            ExpiresAt = expiryTime,
            CreatedAt = DateTime.UtcNow,
        });

        await _unitOfWork.CommitAsync();

        // Send email with reset link
        await _emailService.SendPasswordResetEmailAsync(
            user.Email,
            user.FullName,
            resetToken,
            null,
            cancellationToken)
        .ConfigureAwait(false);

        return Result<bool>.Success(true);
    }

    private string GenerateSecureToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
}
