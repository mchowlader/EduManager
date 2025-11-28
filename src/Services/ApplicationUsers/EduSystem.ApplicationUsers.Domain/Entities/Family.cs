namespace EduSystem.ApplicationUsers.Domain.Entities;

public class Family : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public Relation RelationWith { get; set; }
    public string? Description { get; set; }
    public string Phone { get; set; } = string.Empty;
    public Address? PresentAddress { get; set; }
    public Address? PermanentAddress { get; set; }
    public Guid? StudentId { get; set; }
    public Student? Student { get; set; }
    public Guid? TeacherId { get; set; }
    public Teacher? Teacher { get; set; }
}
