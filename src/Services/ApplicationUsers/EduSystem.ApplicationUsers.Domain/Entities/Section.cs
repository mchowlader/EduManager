using System;
using System.Collections.Generic;
using System.Text;

namespace EduSystem.ApplicationUsers.Domain.Entities;

public class Section : AuditableEntity
{
    public new int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ClassCategoryId { get; set; }
    public Classes ClassCategory { get; set; } = new Classes();
}
