namespace TrainingSystem.Shared.Domain;

public enum AttendanceStatus
{
    Present,
    Absent,
    Excused
}

public class AttendanceRecord
{
    public int Id { get; set; }
    public int CourseId { get; set; } // Ideally link to Enrollment, but linking to course/employee is fine too
    public int EmployeeId { get; set; }
    
    public DateTime Date { get; set; }
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
    public string? Notes { get; set; }
    
    public Course? Course { get; set; }
    public Employee? Employee { get; set; }
}
