using EduSystem.UI.Web.Client.Models.Common;

namespace EduSystem.UI.Web.Client.Models;

public class TeacherModel : AuditableModel
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public long? PresentAddressId { get; set; }
    public AddressModel? PresentAddress { get; set; }
    public long? PermanentAddressId { get; set; }
    public AddressModel? PermanentAddress { get; set; }
    public string Designation { get; set; } = string.Empty;
    public IList<FamilyModel> FamilyInfos { get; set; } = new List<FamilyModel>();
}
