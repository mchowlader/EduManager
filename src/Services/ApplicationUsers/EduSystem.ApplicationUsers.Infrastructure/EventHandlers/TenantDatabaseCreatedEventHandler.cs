using EduSystem.ApplicationUsers.Infrastructure.Contexts;
using EduSystem.Shared.Infrastructure.MultiTenancy;
using EduSystem.Shared.Messaging;
using EduSystem.Shared.Messaging.Handlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduSystem.ApplicationUsers.Infrastructure.EventHandlers;

public class TenantDatabaseCreatedEventHandler : BaseMigrationHandler<AppUserDbContext>
{
    public TenantDatabaseCreatedEventHandler(ITenantMigrationService<AppUserDbContext> migrationService, IEventBus eventBus,
        ILogger<TenantDatabaseCreatedEventHandler> logger)
        : base(migrationService, eventBus, logger)
    {
    }

    protected override string ServiceName => "ApplicationUsers";

    protected override AppUserDbContext CreateDbContext(DbContextOptions<AppUserDbContext> options)
    {
        return new AppUserDbContext(options);
    }
}
