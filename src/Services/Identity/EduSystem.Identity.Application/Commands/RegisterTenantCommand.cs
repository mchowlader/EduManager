using EduSystem.Identity.Application.DTOs;
using EduSystem.Identity.Application.IService;
using EduSystem.Identity.Domain.Entities;
using EduSystem.Identity.Domain.IRepository;
using EduSystem.Identity.Shared.Common;
using EduSystem.Shared.Event;
using EduSystem.Shared.Messaging;
using MediatR;

namespace EduSystem.Identity.Application.Commands;

public class RegisterTenantCommand : IRequest<Result<Guid>>
{
    public TenantRegistrationDto? Registration { get; set; }
}

public class RegisterTenantCommandHandler(
    ITenantRepository tenantRepository,
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITenantDatabaseProvisioner dbProvisioner,
    IUnitOfWork unitOfWork,
    IEventBus eventBus) : IRequestHandler<RegisterTenantCommand, Result<Guid>>
{
    private readonly ITenantRepository _tenantRepository = tenantRepository;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly ITenantDatabaseProvisioner _dbProvisioner = dbProvisioner;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IEventBus _eventBus = eventBus;

    public async Task<Result<Guid>> Handle(RegisterTenantCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Registration;
        if (dto == null)
            return Result<Guid>.Failure("Invalid registration data.");

        if (await _tenantRepository.ExistsAsync(dto.Slug))
            return Result<Guid>.Failure("Tenant with the same slug already exists.");

        string? connectionString = null;
        bool success = false;
        Result<User>? tenantAdminResult = null;

        try
        {
            // 1. Create Database
            var dbResult = await _dbProvisioner.CreateDatabaseAsync(dto.Slug);
            if (!dbResult.Success)
                return Result<Guid>.Failure("Failed to create tenant database.");

            connectionString = dbResult.EncryptedConnectionString;

            // 2. Begin transaction
            await _unitOfWork.BeginTransactionAsync();

            // 3. Create Tenant
            var tenantResult = await CreateTenantAsync(dto, connectionString);
            if (!tenantResult.IsSuccess)
            {
                await HandleFailureAsync(dto.Slug, tenantResult.ErrorMessage);
                return Result<Guid>.Failure(tenantResult.ErrorMessage!);
            }
            var tenant = tenantResult.Data!;

            // 4. Create Tenant Admin with EduAdmin role
            tenantAdminResult = await CreateTenantAdminAsync(tenant, dto);
            if (!tenantAdminResult.IsSuccess)
            {
                await HandleFailureAsync(dto.Slug, tenantAdminResult.ErrorMessage);
                return Result<Guid>.Failure(tenantAdminResult.ErrorMessage!);
            }
            var tenantAdmin = tenantAdminResult.Data!;

            // 5. Commit transaction
            await _unitOfWork.CommitAsync();

            // 6. Publish Event
            await _eventBus.PublishAsync(new TenantDatabaseCreatedEvent
            {
                TenantId = tenant.Id,
                TenantSlug = tenant.Slug!,
                EncryptedConnectionString = connectionString,
                AdminUserId = tenantAdmin.Id,
                AdminEmail = tenantAdmin.Email,
                CreatedAt = DateTime.UtcNow
            }, cancellationToken).ConfigureAwait(false);

            success = true;
            return Result<Guid>.Success(tenant.Id);
        }
        catch (Exception ex)
        {
            await HandleFailureAsync(dto.Slug, ex.Message);
            return Result<Guid>.Failure("Tenant registration failed: " + ex.Message);
        }
        finally
        {
            // Ensure cleanup only if something failed
            if (!success || connectionString == null || (tenantAdminResult == null || !tenantAdminResult.IsSuccess))
                await HandleFailureAsync(dto.Slug);
        }
    }

    private async Task<Result<Tenant>> CreateTenantAsync(TenantRegistrationDto dto, string connectionString)
    {
        if (dto == null)
            return Result<Tenant>.Failure("Tenant registration data is null.");

        try
        {
            var tenant = new Tenant
            {
                Name = dto.SchoolName,
                Slug = dto.Slug,
                ConnectionString = connectionString,
                IsActive = true,
                PrimaryColor = dto.PrimaryColor,
                SecondaryColor = dto.SecondaryColor,
                LogoUrl = null,
                BannerUrl = null,
                CreatedAt = DateTime.UtcNow
            };

            var createdTenant = await _tenantRepository.CreateAsync(tenant);

            if (createdTenant == null)
                return Result<Tenant>.Failure("Failed to create tenant.");

            return Result<Tenant>.Success(createdTenant);
        }
        catch (Exception ex)
        {
            return Result<Tenant>.Failure("Failed to create tenant: " + ex.Message);
        }
    }

    private async Task<Result<User>> CreateTenantAdminAsync(Tenant tenant, TenantRegistrationDto dto)
    {
        if (tenant == null)
            return Result<User>.Failure("Tenant cannot be null.");
        
        try
        {
            // Check if email already exists
            var isEmailExists = await _userRepository.IsEmailExistsAsync(dto.AdminEmail);
            if (isEmailExists)
                return Result<User>.Failure("Email already exists.");

            var tenantAdmin = new User
            {
                Id = Guid.NewGuid(),
                Email = dto.AdminEmail,
                PasswordHash = _passwordHasher.Hash(dto.AdminPassword),
                FullName = dto.AdminFullName,
                PhoneNumber = dto.PhoneNumber,
                TenantId = tenant.Id,
                Role = UserRole.EduAdmin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            //var isEmailExits = await _userRepository.IsEmailExistsAsync(tenantAdmin.Email);

            //if(isEmailExits)
            //    return Result<User>.Failure("Email already exits.");

            await _userRepository.AddAsync(tenantAdmin);

            return Result<User>.Success(tenantAdmin);
        }
        catch (Exception ex)
        {
            // Optional: log exception
            return Result<User>.Failure("Failed to create tenant admin: " + ex.Message);
        }
    }

    private async Task<bool> CleanupDatabaseAsync(string slug)
    {
        try
        {
            return await _dbProvisioner.DropDatabaseAsync(slug);
        }
        catch (Exception)
        {
            // implement logging here
            return false;
        }
    }

    private async Task HandleFailureAsync(string tenantSlug, string? errorMessage = null)
    {
        try
        {
            // Rollback transaction
            await _unitOfWork.RollbackAsync();

            // Cleanup database
            if (!string.IsNullOrWhiteSpace(tenantSlug))
                await CleanupDatabaseAsync(tenantSlug);

            // Optional: log failure
            if (!string.IsNullOrWhiteSpace(errorMessage))
                // e.g., _logger.LogError(errorMessage);
                Console.WriteLine($"Tenant cleanup: {errorMessage}");
        }
        catch (Exception ex)
        {
            // Optional: log cleanup failure
            Console.WriteLine($"Failed to cleanup tenant '{tenantSlug}': {ex.Message}");
        }
    }
}
