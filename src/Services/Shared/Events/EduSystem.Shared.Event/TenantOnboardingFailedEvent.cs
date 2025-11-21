namespace EduSystem.Shared.Event;

public class TenantOnboardingFailedEvent
{
    public Guid TenantId { get; set; }
    public string TenantSlug { get; set; } = string.Empty;
    public string FaileService { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public DateTime FailedAt { get; set; }
}