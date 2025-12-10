namespace TrainingSystem.Shared.Domain;

public class Enrollment
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public Course? Course { get; set; }
    
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
}
