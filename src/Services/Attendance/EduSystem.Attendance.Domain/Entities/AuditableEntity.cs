using System;
using System.Collections.Generic;
using System.Text;

namespace EduSystem.Attendance.Domain.Entities;

public class AuditableEntity : BaseEntity
{
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public long? UpdatedBy { get; set; }
}
