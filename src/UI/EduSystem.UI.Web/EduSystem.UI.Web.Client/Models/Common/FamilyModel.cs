namespace EduSystem.UI.Web.Client.Models.Common;

public class FamilyModel : AuditableModel
{
    public string Name { get; set; } = string.Empty;
    public string Relation { get; set; } = string.Empty; // e.g., Father, Mother
    public string ContactNo { get; set; } = string.Empty;
    public string Occupation { get; set; } = string.Empty;
}
