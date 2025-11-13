using EduSystem.Identity.Application.DTOs;
using EduSystem.Identity.Application.IService;
using EduSystem.Identity.Domain.Entities;
using EduSystem.Identity.Domain.IRepository;
using EduSystem.Identity.Shared.Common;
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
    IUnitOfWork unitOfWork) : IRequestHandler<RegisterTenantCommand, Result<Guid>>
{
    private readonly ITenantRepository _tenantRepository = tenantRepository;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly ITenantDatabaseProvisioner _dbProvisioner = dbProvisioner;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<Guid>> Handle(RegisterTenantCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Registration;

        // ১. Slug uniqueness validate করুন
        if (await _tenantRepository.ExistsAsync(dto!.Slug))
            return Result<Guid>.Failure("Tenant with the same slug already exists.");

        string? connectionString = null;

        try
        {
            // ২. Database তৈরি করুন
            connectionString = await _dbProvisioner.CreateDatabaseAsync(dto.Slug);

            // ৩. Transaction শুরু করুন
            await _unitOfWork.BeginTransactionAsync();

            // ৪. Tenant তৈরি করুন
            var tenant = await CreateTenantAsync(dto, connectionString);
            if (tenant == null)
            {
                await _unitOfWork.RollbackAsync();
                await CleanupDatabaseAsync(dto.Slug);
                return Result<Guid>.Failure("Failed to create tenant.");
            }

            // ৫. Admin user তৈরি করুন
            var tenantAdmin = await CreateTenantAdminAsync(tenant, dto);
            if (tenantAdmin == null)
            {
                await _unitOfWork.RollbackAsync();
                await CleanupDatabaseAsync(dto.Slug);
                return Result<Guid>.Failure("Failed to create admin user.");
            }

            // ৬. সব ঠিক থাকলে commit করুন
            await _unitOfWork.CommitAsync();

            return Result<Guid>.Success(tenant.Id);
        }
        catch (Exception ex)
        {
            // Transaction rollback করুন
            await _unitOfWork.RollbackAsync();

            // Database drop করুন
            if (connectionString != null)
            {
                await CleanupDatabaseAsync(dto.Slug);
            }

            return Result<Guid>.Failure("Tenant registration failed: " + ex.Message);
        }
    }

    private async Task<Tenant?> CreateTenantAsync(TenantRegistrationDto dto, string connectionString)
    {
        var tenant = new Tenant
        {
            Name = dto.SchoolName,
            Slug = dto.Slug,
            ConnectionString = connectionString,
            IsActive = true,
            PrimaryColor = dto.PrimaryColor,
            SecondaryColor = dto.SecondaryColor
        };

        await _tenantRepository.CreateAsync(tenant);
        return tenant;
    }

    private async Task<User?> CreateTenantAdminAsync(Tenant tenant, TenantRegistrationDto dto)
    {
        var tenantAdmin = new User
        {
            Id = Guid.NewGuid(),
            Email = dto.AdminEmail,
            PasswordHash = _passwordHasher.Hash(dto.AdminPassword),
            FullName = dto.AdminFullName,
            PhoneNumber = dto.PhoneNumber,
            TenantId = tenant.Id,
            Role = UserRole.EduAdmin,
            IsActive = true
        };

        await _userRepository.CreateAsync(tenantAdmin);
        return tenantAdmin;
    }

    private async Task CleanupDatabaseAsync(string slug)
    {
        try
        {
            await _dbProvisioner.DropDatabaseAsync(slug);
        }
        catch
        {
            // Database cleanup এ সমস্যা হলেও ignore করুন
            // লগ করতে পারেন যদি প্রয়োজন হয়
        }
    }
}