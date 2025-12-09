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
        var response = await _httpClient.PostAsJsonAsync("api/employees", employee);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Employee>();
        }
        return null;
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
        var response = await _httpClient.PostAsJsonAsync("api/courses", course);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Course>();
        }
        return null;
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
}
