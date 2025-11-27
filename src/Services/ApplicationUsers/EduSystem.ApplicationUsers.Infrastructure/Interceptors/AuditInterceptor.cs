using System;
using System.Collections.Generic;
using System.Text;
using EduSystem.ApplicationUsers.Application.IService;
using EduSystem.ApplicationUsers.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EduSystem.ApplicationUsers.Infrastructure.Interceptors;

public class AuditInterceptor(ICurrentUserService currentUser) : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUser = currentUser;

    private void UpdateAuditFields(DbContext? context)
    {
        if (context == null) return;

        var entries = context.ChangeTracker.Entries<AuditableEntity>();
        var currentUser = _currentUser.GetCurrentUserId();
        var now = DateTime.UtcNow;

        foreach(var entry in entries)
        {
            if(entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.CreatedBy = currentUser;
            }
            else if(entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedBy = currentUser;
            }
        }
    }
}
