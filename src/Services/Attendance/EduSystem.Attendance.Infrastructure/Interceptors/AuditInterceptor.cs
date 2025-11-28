using EduSystem.Attendance.Application.IService;
using EduSystem.Attendance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EduSystem.Attendance.Infrastructure.Interceptors;

public class AuditInterceptor(ICurrentUserService currentUser) : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUser = currentUser;

    private void UpdateAuditFields(DbContext? context)
    {
        if (context == null) return;

        var entries = context.ChangeTracker.Entries<AuditableEntity>();
        var currentUser = _currentUser.GetCurrentUserId();
        var now = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.CreatedBy = currentUser;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedBy = currentUser;
            }
        }
    }
}
