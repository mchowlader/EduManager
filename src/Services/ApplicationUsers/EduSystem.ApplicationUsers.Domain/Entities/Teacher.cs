namespace EduSystem.ApplicationUsers.Domain.Entities;

public class Teacher : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public long? PresentAddressId { get; set; }
    public Address? PresentAddress { get; set; }
    public long? PermanentAddressId { get; set; }
    public Address? PermanentAddress { get; set; }
    public string Designation { get; set; } = string.Empty;
    public long FamilyId { get; set; }
    public Family FamilyInfos { get; set; } = null!;
}
