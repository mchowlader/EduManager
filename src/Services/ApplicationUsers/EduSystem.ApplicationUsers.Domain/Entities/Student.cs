namespace EduSystem.ApplicationUsers.Domain.Entities;

public class Student : AuditableEntity
{
    public string StudentId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long RollNo { get; set; }
    public string? Phone { get; set; }
    public long? PresentAddressId { get; set; }
    public Address? PresentAddress { get; set; }
    public long? PermanentAddressId { get; set; }
    public Address? PermanentAddress { get; set; }
    public int ClassId  { get; set; }
    public Classes Classes { get; set; } = new();
    public int SectionId { get; set; }
    public Section Section { get; set; } = new();
    public int DepartmentId { get; set; }
    public Department? Department { get; set; }
    public ICollection<Family> FamilyInfos { get; set; } = new List<Family>();
    public DateTime DateOfBirth { get; set; }
    public string DateOfBirthNo { get; set; } = string.Empty;
}
