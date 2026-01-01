namespace EduSystem.ApplicationUsers.Domain.Entities;

public class Address : AuditableEntity
{
    public new long Id { get; set; }
    public string Village { get; set; } = string.Empty;
    public string Thana { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Division { get; set; } = string.Empty;
}
