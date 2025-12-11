namespace EduSystem.UI.Web.Client.Models
{
    public class AuditableEntity
    {
        public long Id { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? LastModifiedDate { get; set; }
        public string? LastModifiedBy { get; set; }
    }

    public class Address : AuditableEntity
    {
        public string HouseNo { get; set; } = string.Empty;
        public string RoadNo { get; set; } = string.Empty;
        public string Village { get; set; } = string.Empty;
        public string PostOffice { get; set; } = string.Empty;
        public string Thana { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
    }

    public class Family : AuditableEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Relation { get; set; } = string.Empty; // e.g., Father, Mother
        public string ContactNo { get; set; } = string.Empty;
        public string Occupation { get; set; } = string.Empty;
    }

    public class Student : AuditableEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        
        public long? PresentAddressId { get; set; }
        public Address DetailsPresentAddress { get; set; } = new Address(); // Renamed to avoid confusion if needed, or keeping simple
        
        public long? PermanentAddressId { get; set; }
        public Address DetailsPermanentAddress { get; set; } = new Address();

        public ClassCategory Class { get; set; }
        public DepartmentCategory Department { get; set; }
        
        public IList<Family> FamilyInfos { get; set; } = new List<Family>();
        
        public DateTime DateOfBirth { get; set; } = DateTime.Today;
        public string DateOfBirthNo { get; set; } = string.Empty;

        // Helper for UI
        public string PresentAddressDisplay => $"{DetailsPresentAddress?.HouseNo}, {DetailsPresentAddress?.RoadNo}, {DetailsPresentAddress?.District}";
    }
}
