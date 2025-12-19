namespace EduSystem.UI.Web.Client.Models.Common;

public class AuditableModel
{
    public long Id { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedDate { get; set; }
    public string? LastModifiedBy { get; set; }
}
