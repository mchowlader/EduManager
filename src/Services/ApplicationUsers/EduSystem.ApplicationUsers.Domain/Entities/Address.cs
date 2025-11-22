namespace EduSystem.ApplicationUsers.Domain.Entities;

public class Address
{
    public long Id { get; set; }
    public string Division { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Thana { get; set; } = string.Empty;
    public string Village { get; set; } = string.Empty ;
}
