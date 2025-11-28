namespace EduSystem.ApplicationUsers.Domain.Entities;

public class Student : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public long PresentAddressId { get; set; }
    public Address? PresentAddress { get; set; }
    public long PermanentAddressId { get; set; }
    public Address? PermanentAddress { get; set; }
    public ClassCategory Class  { get; set; }
    public DepartmentCategory Department { get; set; }
    public IList<Family> FamilyInfos { get; set; } = new List<Family>();
    public DateTime DateOfBirth { get; set; }
    public string DateOfBirthNo { get; set; } = string.Empty;
}
