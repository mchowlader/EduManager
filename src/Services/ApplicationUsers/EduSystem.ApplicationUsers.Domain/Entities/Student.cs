namespace EduSystem.ApplicationUsers.Domain.Entities;

public class Student : AuditableEntity
{
    public string StudentCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long RollNo { get; set; }
    public string? Phone { get; set; }
    public long? PresentAddressId { get; set; }
    public Address? PresentAddress { get; set; }
    public long? PermanentAddressId { get; set; }
    public Address? PermanentAddress { get; set; }
    public long ClassesId  { get; set; }
    public Classes Classes { get; set; } = null!;
    public long? SectionId { get; set; }
    public Section? Section { get; set; }
    public long? GroupId { get; set; }
    public Group? Group { get; set; }
    public long? FamilyId { get; set; }
    public Family FamilyInfos { get; set; } = null!;
    public DateTime? DateOfBirth { get; set; }
    public string DateOfBirthNo { get; set; } = string.Empty;
}
