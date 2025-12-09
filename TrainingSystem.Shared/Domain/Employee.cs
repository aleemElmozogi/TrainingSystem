namespace TrainingSystem.Shared.Domain;

public class Employee
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public string? Department { get; set; }
    public string? JobTitle { get; set; }

    // Navigation
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
