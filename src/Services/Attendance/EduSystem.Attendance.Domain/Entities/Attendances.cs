namespace EduSystem.Attendance.Domain.Entities;

public class Attendances
{
    public int Id { get; set; }
    public int AttendenctUserId { get; set; }
    public DateTime AttendanceAt { get; set; }
}