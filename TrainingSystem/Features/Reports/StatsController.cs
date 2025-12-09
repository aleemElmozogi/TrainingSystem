using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.Data;
using TrainingSystem.Shared.Domain;

namespace TrainingSystem.Features.Reports;

[Route("api/[controller]")]
[ApiController]
public class StatsController : ControllerBase
{
    private readonly AppDbContext _context;

    public StatsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<DashboardStats>> GetStats()
    {
        var stats = new DashboardStats
        {
            EmployeeCount = await _context.Employees.CountAsync(),
            CourseCount = await _context.Courses.CountAsync(),
            EnrollmentCount = await _context.Enrollments.CountAsync()
        };
        return stats;
    }
}
