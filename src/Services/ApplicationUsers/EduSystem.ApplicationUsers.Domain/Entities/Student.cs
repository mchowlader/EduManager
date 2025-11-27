namespace EduSystem.ApplicationUsers.Domain.Entities;

public class Student : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public Address? PresentAddress { get; set; }
    public Address? PermanentAddress { get; set; }
    public ClassCategory Class  { get; set; }
    public DepartmentCategory Department { get; set; }
    public IList<Family> FamilyInfos { get; set; } = new List<Family>();
}
