namespace EduSystem.ApplicationUsers.Domain.Entities;

public class Student
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public Address? PresentAddress { get; set; }
    public Address? PermanentAddress { get; set; }
    public IList<Family> FamilyInfos { get; set; } = new List<Family>();
}