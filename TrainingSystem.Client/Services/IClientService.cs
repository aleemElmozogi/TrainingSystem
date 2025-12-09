using TrainingSystem.Shared.Domain;

namespace TrainingSystem.Client.Services;

public interface IClientService
{
    // Employees
    Task<List<Employee>> GetEmployeesAsync();
    Task<Employee?> GetEmployeeByIdAsync(int id);
    Task<Employee?> CreateEmployeeAsync(Employee employee);
    Task UpdateEmployeeAsync(int id, Employee employee);
    Task DeleteEmployeeAsync(int id);

    // Courses
    Task<List<Course>> GetCoursesAsync();
    Task<Course?> GetCourseByIdAsync(int id);
    Task<Course?> CreateCourseAsync(Course course);
    Task UpdateCourseAsync(int id, Course course);
    Task DeleteCourseAsync(int id);

    // Enrollments
    Task EnrollEmployeeAsync(Enrollment enrollment);
    Task RemoveEnrollmentAsync(int courseId, int employeeId);
    Task<List<Enrollment>> GetEnrollmentsByCourseAsync(int courseId);

    // Attendance
    Task<List<AttendanceRecord>> GetAttendanceAsync(int courseId, DateTime date);
    Task SaveAttendanceAsync(List<AttendanceRecord> records);

    // Stats
    Task<DashboardStats> GetDashboardStatsAsync();
}
