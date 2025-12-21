namespace EduSystem.Shared.Infrastructure.Utilities;

public static class DateTimeHelper
{
    public static DateTime Now => DateTime.UtcNow.ToLocalTime();
}
