namespace EduSystem.UI.Web.Client.Models.Common;

public class AddressModel : AuditableModel
{
    public string HouseNo { get; set; } = string.Empty;
    public string RoadNo { get; set; } = string.Empty;
    public string Village { get; set; } = string.Empty;
    public string PostOffice { get; set; } = string.Empty;
    public string Thana { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
}
