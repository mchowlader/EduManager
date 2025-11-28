using EduSystem.Identity.Infrastructure.Contexts;
using EduSystem.Shared.Infrastructure.Security;
using EduSystem.Shared.Messaging;
using EduSystem.Shared.Messaging.Handlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduSystem.Identity.Infrastructure.EventHandlers;

public class TenantDatabaseCreatedEventHandler : BaseMigrationHandler<IdentityDbContext>
{
    protected override string ServiceName => "Attendance";

    public TenantDatabaseCreatedEventHandler(IConnectionStringEncryptor encryptor, IEventBus eventBus,
        ILogger<TenantDatabaseCreatedEventHandler> logger)
        : base(encryptor, eventBus, logger)
    {
    }

    protected override IdentityDbContext CreateDbContext(DbContextOptions<IdentityDbContext> options)
    {
        return new IdentityDbContext(options);
    }
}
