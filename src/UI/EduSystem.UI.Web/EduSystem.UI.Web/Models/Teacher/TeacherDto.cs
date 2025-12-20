namespace EduSystem.UI.Web.Models.Teacher;

public class TeacherDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Designation { get; set; }
    public DateTime JoiningDate { get; set; }
    public bool IsActive { get; set; }
}
