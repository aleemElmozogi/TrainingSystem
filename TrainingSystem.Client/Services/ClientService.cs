using System.Net.Http.Json;
using TrainingSystem.Shared.Domain;

namespace TrainingSystem.Client.Services;

public class ClientService : IClientService
{
    private readonly HttpClient _httpClient;

    public ClientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // Employees
    public async Task<List<Employee>> GetEmployeesAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<Employee>>("api/employees") ?? new List<Employee>();
    }

    public async Task<Employee?> GetEmployeeByIdAsync(int id)
    {
        try 
        {
            return await _httpClient.GetFromJsonAsync<Employee>($"api/employees/{id}");
        }
        catch(HttpRequestException) { return null; }
    }

     public async Task<Employee?> CreateEmployeeAsync(Employee employee)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/employees", employee);
            if (response.IsSuccessStatusCode)
            {
                 return await response.Content.ReadFromJsonAsync<Employee>();
            }
            else
            {
                 Console.WriteLine($"Error creating employee: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                 return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception creating employee: {ex.Message}");
            return null;
        }
    }

    public async Task UpdateEmployeeAsync(int id, Employee employee)
    {
        await _httpClient.PutAsJsonAsync($"api/employees/{id}", employee);
    }

    public async Task DeleteEmployeeAsync(int id)
    {
        await _httpClient.DeleteAsync($"api/employees/{id}");
    }

    // Courses
    public async Task<List<Course>> GetCoursesAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<Course>>("api/courses") ?? new List<Course>();
    }

    public async Task<Course?> GetCourseByIdAsync(int id)
    {
         try 
        {
            return await _httpClient.GetFromJsonAsync<Course>($"api/courses/{id}");
        }
        catch(HttpRequestException) { return null; }
    }

    public async Task<Course?> CreateCourseAsync(Course course)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/courses", course);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Course>();
            }
            Console.WriteLine($"Error creating course: {response.StatusCode}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception creating course: {ex.Message}");
            return null;
        }
    }

    public async Task UpdateCourseAsync(int id, Course course)
    {
        await _httpClient.PutAsJsonAsync($"api/courses/{id}", course);
    }

    public async Task DeleteCourseAsync(int id)
    {
        await _httpClient.DeleteAsync($"api/courses/{id}");
    }

    // Enrollments
    public async Task EnrollEmployeeAsync(Enrollment enrollment)
    {
        await _httpClient.PostAsJsonAsync("api/enrollments", enrollment);
    }

    public async Task RemoveEnrollmentAsync(int courseId, int employeeId)
    {
        await _httpClient.DeleteAsync($"api/enrollments/{courseId}/{employeeId}");
    }

    public async Task<List<Enrollment>> GetEnrollmentsByCourseAsync(int courseId)
    {
        return await _httpClient.GetFromJsonAsync<List<Enrollment>>($"api/enrollments/course/{courseId}") ?? new List<Enrollment>();
    }

    // Attendance
    public async Task<List<AttendanceRecord>> GetAttendanceAsync(int courseId, DateTime date)
    {
        // Format date universally or just use standard ToString("O") or simple yyyy-MM-dd
        // Passing date in URL segment might be tricky with special chars, query string is safer or specific format
        // The controller uses [HttpGet("{courseId}/{date}")], so date comes from route.
        // dotnet Date binding Usually works with ISO-8601
        
        var dateStr = date.ToString("yyyy-MM-dd");
        try 
        {
             return await _httpClient.GetFromJsonAsync<List<AttendanceRecord>>($"api/attendance/{courseId}/{dateStr}") ?? new List<AttendanceRecord>();
        }
        catch(HttpRequestException) // 404 or empty
        {
             return new List<AttendanceRecord>();
        }
    }

    public async Task SaveAttendanceAsync(List<AttendanceRecord> records)
    {
        await _httpClient.PostAsJsonAsync("api/attendance", records);
    }

    // Stats
    public async Task<DashboardStats> GetDashboardStatsAsync()
    {
        return await _httpClient.GetFromJsonAsync<DashboardStats>("api/stats") ?? new DashboardStats();
    }

    // Reports
    public async Task<List<TrainingSystem.Shared.DTOs.CourseAssignmentDto>> GetCourseAssignmentsReportAsync(int courseId)
    {
         return await _httpClient.GetFromJsonAsync<List<TrainingSystem.Shared.DTOs.CourseAssignmentDto>>($"api/reports/course-assignments/{courseId}") ?? new List<TrainingSystem.Shared.DTOs.CourseAssignmentDto>();
    }

    public async Task<List<TrainingSystem.Shared.DTOs.EmployeeAssignmentDto>> GetEmployeeAssignmentsReportAsync(int employeeId)
    {
        return await _httpClient.GetFromJsonAsync<List<TrainingSystem.Shared.DTOs.EmployeeAssignmentDto>>($"api/reports/employee-assignments/{employeeId}") ?? new List<TrainingSystem.Shared.DTOs.EmployeeAssignmentDto>();
    }

    public async Task<List<TrainingSystem.Shared.DTOs.AttendanceReportDto>> GetAttendanceReportAsync(int? courseId, int? employeeId, DateTime? startDate, DateTime? endDate)
    {
        var query = System.Web.HttpUtility.ParseQueryString(string.Empty);
        if (courseId.HasValue) query["courseId"] = courseId.Value.ToString();
        if (employeeId.HasValue) query["employeeId"] = employeeId.Value.ToString();
        if (startDate.HasValue) query["startDate"] = startDate.Value.ToString("yyyy-MM-dd");
        if (endDate.HasValue) query["endDate"] = endDate.Value.ToString("yyyy-MM-dd");

        var url = $"api/reports/attendance?{query}";
        return await _httpClient.GetFromJsonAsync<List<TrainingSystem.Shared.DTOs.AttendanceReportDto>>(url) ?? new List<TrainingSystem.Shared.DTOs.AttendanceReportDto>();
    }
}
