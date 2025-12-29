namespace EduSystem.ApplicationUsers.Domain.Entities;

public class BaseEntity
{
    public Guid Id { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeleteAt {  get; set; }
}
