namespace EduSystem.ApplicationUsers.Domain.Entities;

public class Teacher : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Address? PresentAddress { get; set; }
    public Address? PermanentAddress { get; set; }
    public string Designation { get; set; } = string.Empty;
    public IList<Family> FamilyInfos { get; set; } = new List<Family>();
}
