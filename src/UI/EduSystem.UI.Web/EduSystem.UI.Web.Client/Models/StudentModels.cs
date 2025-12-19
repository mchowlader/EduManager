using EduSystem.UI.Web.Client.Models.Common;

namespace EduSystem.UI.Web.Client.Models;

public class StudentModel : AuditableModel
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    
    public long? PresentAddressId { get; set; }
    public AddressModel DetailsPresentAddress { get; set; } = new AddressModel(); // Renamed to avoid confusion if needed, or keeping simple
    
    public long? PermanentAddressId { get; set; }
    public AddressModel DetailsPermanentAddress { get; set; } = new AddressModel();

    public ClassCategory Class { get; set; }
    public DepartmentCategory Department { get; set; }
    
    public IList<FamilyModel> FamilyInfos { get; set; } = new List<FamilyModel>();
    
    public DateTime DateOfBirth { get; set; } = DateTime.Today;
    public string DateOfBirthNo { get; set; } = string.Empty;

    // Helper for UI
    public string PresentAddressDisplay => $"{DetailsPresentAddress?.HouseNo}, {DetailsPresentAddress?.RoadNo}, {DetailsPresentAddress?.District}";
}
