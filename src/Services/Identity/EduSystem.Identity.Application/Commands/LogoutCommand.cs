using EduSystem.Identity.Application.IService;
using EduSystem.Identity.Domain.IRepository;
using EduSystem.Identity.Shared.Common;
using MediatR;

namespace EduSystem.Identity.Application.Commands;

public class LogoutCommand : IRequest<Result<bool>>
{
    public Guid UserId { get; set; }
}

public class LogoutCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<LogoutCommand, Result<bool>>
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<bool>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
            return Result<bool>.Failure("User not found.");

        // Clear refresh token to invalidate all sessions
        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.CommitAsync();

        return Result<bool>.Success(true);
    }
}
