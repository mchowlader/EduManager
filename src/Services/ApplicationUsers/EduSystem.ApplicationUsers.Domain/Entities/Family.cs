namespace EduSystem.ApplicationUsers.Domain.Entities;

public class Family
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Relation RelationWith { get; set; }
    public string? Description { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public Address? PresentAddress { get; set; }
    public Address? PermanentAddress { get; set; }
}