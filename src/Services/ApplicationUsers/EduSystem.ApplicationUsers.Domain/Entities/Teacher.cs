namespace EduSystem.ApplicationUsers.Domain.Entities;

public class Teacher
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Address? PresentAddress { get; set; }
    public Address? PermanentAddress { get; set; }
    public IList<Family> FamilyInfos { get; set; } = new List<Family>();
}