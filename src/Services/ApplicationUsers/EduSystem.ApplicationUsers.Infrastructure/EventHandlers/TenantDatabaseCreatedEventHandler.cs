using EduSystem.ApplicationUsers.Infrastructure.Contexts;
using EduSystem.Shared.Infrastructure.Security;
using EduSystem.Shared.Messaging;
using EduSystem.Shared.Messaging.Handlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduSystem.ApplicationUsers.Infrastructure.EventHandlers;

public class TenantDatabaseCreatedEventHandler : BaseMigrationHandler<AppUserDbContext>
{
    public TenantDatabaseCreatedEventHandler(IConnectionStringEncryptor encryptor, IEventBus eventBus, 
        ILogger<TenantDatabaseCreatedEventHandler> logger)
        : base(encryptor, eventBus, logger)
    {
    }

    protected override string ServiceName => "ApplicationUser";

    protected override AppUserDbContext CreateDbContext(DbContextOptions<AppUserDbContext> options)
    {
        return new AppUserDbContext(options);
    }
}
