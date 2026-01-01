namespace EduSystem.ApplicationUsers.Domain.Entities;

public class Group : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public ICollection<Student> Student { get; set; } = null!;
}
