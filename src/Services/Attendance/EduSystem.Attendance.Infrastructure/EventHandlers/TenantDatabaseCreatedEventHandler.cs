using EduSystem.Attendance.Infrastructure.Contexts;
using EduSystem.Shared.Infrastructure.MultiTenancy;
using EduSystem.Shared.Messaging;
using EduSystem.Shared.Messaging.Handlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduSystem.Attendance.Infrastructure.EventHandlers;

public class TenantDatabaseCreatedEventHandler : BaseMigrationHandler<AttendanceDbContext>
{
    protected override string ServiceName => "Attendance";

    public TenantDatabaseCreatedEventHandler(ITenantMigrationService<AttendanceDbContext> migrationService, IEventBus eventBus,
        ILogger<TenantDatabaseCreatedEventHandler> logger)
        : base(migrationService, eventBus, logger)
    {
    }

    protected override AttendanceDbContext CreateDbContext(DbContextOptions<AttendanceDbContext> options)
    {
        return new AttendanceDbContext(options);
    }
}
