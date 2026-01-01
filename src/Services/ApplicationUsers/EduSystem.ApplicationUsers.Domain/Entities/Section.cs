namespace EduSystem.ApplicationUsers.Domain.Entities;

public class Section : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public long ClassesId { get; set; }
    public Classes Classes { get; set; } = null!;
    public ICollection<Student> Students { get; set; } = new List<Student>();
}
