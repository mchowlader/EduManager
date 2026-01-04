namespace EduSystem.Shared.Event;

public class TenantOnboardingCompletedEvent
{
    public long TenantId { get; set; }
    public string TenantSlug { get; set; } = string.Empty;
    public List<string> CompleteServie { get; set; } = new();
    public DateTime CompletedAt { get; set; }
}
