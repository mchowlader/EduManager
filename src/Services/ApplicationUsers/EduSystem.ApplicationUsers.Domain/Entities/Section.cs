namespace EduSystem.ApplicationUsers.Domain.Entities;

public class Section : AuditableEntity
{
    public new int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ClassesId { get; set; }
    public Classes Classes { get; set; } = new Classes();
}
