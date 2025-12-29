namespace EduSystem.ApplicationUsers.Application.DTOs;

public class SectionResponseDto
{
    public int Id { get; set; } // Section has int Id, while BaseEntity has Guid Id? 
    // Wait, let me check Section.cs again.
    public string Name { get; set; } = string.Empty;
    public int ClassCategoryId { get; set; }
}

public class SectionCreateDto
{
    public string Name { get; set; } = string.Empty;
    public int ClassCategoryId { get; set; }
}

public class SectionUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public int ClassCategoryId { get; set; }
}
