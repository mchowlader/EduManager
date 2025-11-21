namespace EduSystem.Shared.Event;

public class ServiceMigrationCompletedEvent
{
    public Guid TenantId {  get; set; }
    public string TenantSlug { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public DateTime CompletedAt { get; set; }
}
