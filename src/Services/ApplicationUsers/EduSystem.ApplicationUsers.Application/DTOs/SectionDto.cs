namespace EduSystem.ApplicationUsers.Application.DTOs;

public class SectionResponseDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public long ClassesId { get; set; }
}

public class SectionCreateDto
{
    public string Name { get; set; } = string.Empty;
    public long ClassesId { get; set; }
}

public class SectionUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public long ClassesId { get; set; }
}
