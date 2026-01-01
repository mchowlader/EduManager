namespace EduSystem.ApplicationUsers.Domain.Entities;

public class Classes : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<Section> Sections { get; set; } = new List<Section>();
}
