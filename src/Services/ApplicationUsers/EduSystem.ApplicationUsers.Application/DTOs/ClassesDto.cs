using System;
using System.Collections.Generic;
using System.Text;

namespace EduSystem.ApplicationUsers.Application.DTOs;

public class ClassesCreateDto
{
    public string Name { get; set; } = string.Empty;
}

public class ClassesUpdateDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
public class ClassesResponseDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
