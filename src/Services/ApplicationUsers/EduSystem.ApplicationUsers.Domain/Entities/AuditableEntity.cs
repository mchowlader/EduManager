using System;
using System.Collections.Generic;
using System.Text;

namespace EduSystem.ApplicationUsers.Domain.Entities;

public class AuditableEntity : BaseEntity
{
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
}
