namespace EduSystem.ApplicationUsers.Domain.Entities;

public class Family : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public Relation RelationId { get; set; }
    public string? Description { get; set; }
    public string Phone { get; set; } = string.Empty;
    public long? PresentAddressId { get; set; }
    public Address? PresentAddress { get; set; }
    public long? PermanentAddressId { get; set; }
    public Address? PermanentAddress { get; set; }
}
