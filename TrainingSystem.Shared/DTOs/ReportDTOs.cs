namespace TrainingSystem.Shared.DTOs;

public class CourseAssignmentDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmploymentNumber { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public DateTime JoinedDate { get; set; }
}

public class EmployeeAssignmentDto
{
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class AttendanceReportDto
{
    public DateTime Date { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
