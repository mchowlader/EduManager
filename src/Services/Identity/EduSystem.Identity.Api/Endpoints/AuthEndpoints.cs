
using System.Security.Claims;
using Asp.Versioning;
using EduSystem.Identity.Application.Commands;
using EduSystem.Identity.Application.DTOs;
using MediatR;

namespace EduSystem.Identity.Api.Endpoints;

public class AuthEndpoints : IEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder app)
    {
        // ✅ Version set - শুধু v1
        var versionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        // ✅ Authentication endpoints - শুধু v1 এ available
        var auth = app.MapGroup("/api/v{version:apiVersion}/auth")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1, 0)
            .WithTags("Authentication");

        // Login endpoint
        auth.MapPost("/login", LoginV1)
            .WithName("Login")
            .WithSummary("User login")
            .WithDescription("Authenticate user and return JWT tokens")
            .AllowAnonymous()
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        // Refresh token endpoint
        auth.MapPost("/refresh-token", RefreshTokenV1)
            .WithName("RefreshToken")
            .WithSummary("Refresh JWT tokens")
            .WithDescription("Refresh JWT access and refresh tokens")
            .AllowAnonymous()
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        // Logout endpoint
        auth.MapPost("/logout", LogoutV1)
            .WithName("Logout")
            .WithSummary("User Logout")
            .WithDescription("Invalidate user's refresh token")
            .RequireAuthorization()
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status401Unauthorized);

        // Get current user info
        auth.MapPost("/me", GetCurrentUserV1)
            .WithName("GetCurrentUser")
            .WithSummary("Get current authenticated user")
            .WithDescription("Retrieve current user information from JWT token")
            .RequireAuthorization()
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status401Unauthorized);

        // Change password
        auth.MapPost("/change-password", ChangePasswordV1)
            .WithName("ChangePassword")
            .WithSummary("Change user password")
            .WithDescription("Change password for authenticated user")
            .RequireAuthorization()
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest)
            .Produces<object>(StatusCodes.Status401Unauthorized);

        // Forgot password
        auth.MapPost("/forgot-password", ForgotPasswordV1)
            .WithName("ForgotPassword")
            .WithSummary("Request password reset")
            .WithDescription("Send password reset link to user's email")
            .AllowAnonymous()
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        // Reset password
        auth.MapPost("/reset-password", ResetPasswordV1)
            .WithName("ResetPassword")
            .WithSummary("Reset password")
            .WithDescription("Reset password using token from email")
            .AllowAnonymous()
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        // Verify token
        auth.MapGet("/verify-token", VerifyTokenV1)
            .WithName("VerifyToken")
            .WithSummary("Verify JWT token")
            .WithDescription("Check if current JWT token is valid")
            .RequireAuthorization()
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status401Unauthorized);
    }


    // Login handler
    private static async Task<IResult> LoginV1(
        LoginRequestDto request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand { LoginRequest = request };
        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return Results.BadRequest(new
            {
                success = false,
                message = result.ErrorMessage,
                errors = new[] { result.ErrorMessage }
            });
        }

        return Results.Ok(new
        {
            success = true,
            message = "Login successful",
            data = result.Data
        });
    }

    // Refresh token handler
    private static async Task<IResult> RefreshTokenV1(RefreshTokenRequestDto requestDto,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCommand { RefreshTokenRequest = requestDto };
        var result = await mediator.Send(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return Results.BadRequest(new
            {
                success = false,
                message = result.ErrorMessage,
                errors = new[] { result.ErrorMessage }
            });
        }

        return Results.Ok(new
        {
            success = true,
            message = "Token refreshed successfully",
            data = result.Data
        });
    }

    private static async Task<IResult> LogoutV1(HttpContext context,
       IMediator mediator,
       CancellationToken cancellationToken)
    {
        var userIdClaims = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaims) || !Guid.TryParse(userIdClaims, out var userId))
            return Results.Unauthorized();

        var command = new LogoutCommand { UserId = userId };
        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return Results.BadRequest(new
            {
                success = false,
                message = result.ErrorMessage
            });
        }

        return Results.Ok(new
        {
            success = true,
            message = "Logged out successfully"
        });
    }

    // Get current user handler
    private static IResult GetCurrentUserV1(HttpContext context)
    {
        var claims = context.User.Claims.ToDictionary(
            c => c.Type.Split('/').Last(),
            c => c.Value
        );

        var userData = new
        {
            userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            email = context.User.FindFirst(ClaimTypes.Email)?.Value,
            fullName = context.User.FindFirst(ClaimTypes.Name)?.Value,
            phoneNumber = context.User.FindFirst("phone_number")?.Value,
            role = context.User.FindFirst(ClaimTypes.Role)?.Value,
            isActive = context.User.FindFirst("is_active")?.Value,
            tenant = new
            {
                id = context.User.FindFirst("tenant_id")?.Value,
                slug = context.User.FindFirst("tenant_slug")?.Value,
                name = context.User.FindFirst("tenant_name")?.Value
            }
        };

        return Results.Ok(new
        {
            success = true,
            message = "User information retrieved successfully",
            data = userData,
            claims = claims.Where(c => !c.Key.Contains("tenant_connection")) // Hide connection string
        });
    }

    // Change password handler
    private static async Task<IResult> ChangePasswordV1(
        ChangePasswordRequestDto request,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        var command = new ChangePasswordCommand
        {
            UserId = userId,
            CurrentPassword = request.CurrentPassword,
            NewPassword = request.NewPassword
        };

        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return Results.BadRequest(new
            {
                success = false,
                message = result.ErrorMessage
            });
        }

        return Results.Ok(new
        {
            success = true,
            message = "Password changed successfully"
        });
    }

    // Forgot password handler
    private static async Task<IResult> ForgotPasswordV1(
        ForgotPasswordRequestDto request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new ForgotPasswordCommand
        {
            Email = request.Email
        };

        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);

        // Always return success for security (don't reveal if email exists)
        return Results.Ok(new
        {
            success = true,
            message = "If the email exists, a password reset link has been sent."
        });
    }

    // Reset password handler
    private static async Task<IResult> ResetPasswordV1(
        ResetPasswordRequestDto request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new ResetPasswordCommand
        {
            Token = request.Token,
            NewPassword = request.NewPassword
        };

        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            return Results.BadRequest(new
            {
                success = false,
                message = result.ErrorMessage,
                errors = new[] { result.ErrorMessage }
            });
        }

        return Results.Ok(new
        {
            success = true,
            message = "Password reset successfully"
        });
    }

    // Verify token handler
    private static IResult VerifyTokenV1(HttpContext context)
    {
        return Results.Ok(new
        {
            success = true,
            message = "Token is valid",
            data = new
            {
                isAuthenticated = context.User.Identity?.IsAuthenticated ?? false,
                userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                role = context.User.FindFirst(ClaimTypes.Role)?.Value
            }
        });
    }
}
