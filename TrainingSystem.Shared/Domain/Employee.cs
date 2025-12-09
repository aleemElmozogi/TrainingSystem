namespace TrainingSystem.Shared.Domain;

using System.ComponentModel.DataAnnotations;

public class Employee
{
    public int Id { get; set; }

    [Required(ErrorMessage = "الاسم الكامل مطلوب")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "رقم التوظيف مطلوب")]
    public string EmploymentNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
    [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "القسم مطلوب")]
    public string Department { get; set; } = string.Empty;

    [Required(ErrorMessage = "المسمى الوظيفي مطلوب")]
    public string JobTitle { get; set; } = string.Empty;

    public DateTime JoinedDate { get; set; } = DateTime.Now;

    // Navigation
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
