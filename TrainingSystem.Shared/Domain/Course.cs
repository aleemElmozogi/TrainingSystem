namespace TrainingSystem.Shared.Domain;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public enum CourseType
{
    ShortTerm,
    LongTerm
}

public class Course
{
    public int Id { get; set; }

    [Required(ErrorMessage = "عنوان الدورة مطلوب")]
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "تاريخ البدء مطلوب")]
    public DateTime StartDate { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "تاريخ الانتهاء مطلوب")]
    public DateTime EndDate { get; set; } = DateTime.Now.AddDays(5);

    public DateTime? TravelDate { get; set; }

    [Required(ErrorMessage = "مدة الدورة مطلوبة")]
    public string Duration { get; set; } = "5 أيام";

    [Required(ErrorMessage = "جهة التنفيذ مطلوبة")]
    public string Provider { get; set; } = string.Empty;

    public string Type { get; set; } = "Internal"; // Internal, External
    
    // Navigation
    [JsonIgnore]
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
