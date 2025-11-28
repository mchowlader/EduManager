namespace EduSystem.Shared.Event;

public class TenantDatabaseCreatedEvent
{
    public Guid TenantId {  get; set; }
    public string TenantSlug {  get; set; } = string.Empty;
    public string EncryptedConnectionString {  get; set; } = string.Empty;
    public Guid AdminUserId { get; set; }
    public string AdminEmail { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
