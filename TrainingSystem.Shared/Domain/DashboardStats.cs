namespace TrainingSystem.Shared.Domain;

public class DashboardStats
{
    public int EmployeeCount { get; set; }
    public int CourseCount { get; set; }
    public int EnrollmentCount { get; set; }
    public List<Course> LatestCourses { get; set; } = new();

    public double EmployeeGrowth { get; set; }
    public double CourseGrowth { get; set; }
    
    // Chart / Additional Data
    public double AttendancePercentage { get; set; }
    public double[] MonthlyCourseCounts { get; set; } = Array.Empty<double>();
    public string[] MonthlyLabels { get; set; } = Array.Empty<string>();
}
