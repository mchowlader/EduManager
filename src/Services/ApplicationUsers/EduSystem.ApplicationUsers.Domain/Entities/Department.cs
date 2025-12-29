namespace EduSystem.ApplicationUsers.Domain.Entities;

public class Department : AuditableEntity
{
    public new int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<Student> Student { get; set; } = new List<Student>();
}
