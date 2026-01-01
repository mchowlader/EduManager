namespace EduSystem.ApplicationUsers.Domain.Entities;

public class BaseEntity
{
    public long Id { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeleteAt {  get; set; }
}
