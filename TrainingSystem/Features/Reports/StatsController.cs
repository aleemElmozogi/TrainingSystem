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
        // 1. Attendance Percentage
        // 1. Attendance Percentage (Today)
        var today = DateTime.Today;
        var todayRecords = await _context.AttendanceRecords
            .Where(a => a.Date.Date == today)
            .ToListAsync();

        double attendancePct = 0;
        if (todayRecords.Any())
        {
            var presentCount = todayRecords.Count(a => a.Status == AttendanceStatus.Present);
            attendancePct = Math.Round((double)presentCount / todayRecords.Count * 100, 1);
        }
        else 
        {
            // Optional: Fallback to yesterday or all-time if no data for today? 
            // For now, let's keep it 0 but maybe check if there are any active courses today?
            // If active courses exist but no attendance taken, 0 is technically correct (or "Not Taken").
            // User complained it's "Always 0". If they are testing, they might not have added today's attendance.
            // Let's fallback to "All Time" if today is empty? No, that's confusing with the label "Today".
            // Let's keep it Today but ensure the logic is correct.
            // Maybe they want "All Time" but the label says "Today"? 
            // I'll stick to "Today" logic as it matches the UI "اليوم".
        }

        // 2. Monthly Course Chart (Last 6 Months)
        var sixMonthsAgo = DateTime.Now.AddMonths(-5).Date;
        var courseData = await _context.Courses
            .Where(c => c.StartDate >= sixMonthsAgo)
            .GroupBy(c => new { c.StartDate.Year, c.StartDate.Month })
            .Select(g => new { Date = new DateTime(g.Key.Year, g.Key.Month, 1), Count = g.Count() })
            .ToListAsync();

        // Fill gaps if needed, or just send raw list. Using arrays for MudChart.
        // We'll simplisticly map the result to arrays.
        var labels = new List<string>();
        var data = new List<double>();

        for (int i = 5; i >= 0; i--)
        {
            var d = DateTime.Now.AddMonths(-i);
            var monthName = d.ToString("MMM", new System.Globalization.CultureInfo("ar-SA"));
            labels.Add(monthName);
            
            var existing = courseData.FirstOrDefault(x => x.Date.Month == d.Month && x.Date.Year == d.Year);
            data.Add(existing?.Count ?? 0);
        }

        // 3. Employee Growth
        var lastMonthDate = DateTime.Now.AddMonths(-1);
        var employeesLastMonth = await _context.Employees.CountAsync(e => e.JoinedDate < lastMonthDate);
        var employeesNow = await _context.Employees.CountAsync();
        double empGrowth = 0;
        if (employeesLastMonth > 0)
            empGrowth = Math.Round(((double)(employeesNow - employeesLastMonth) / employeesLastMonth) * 100, 1);
        else if (employeesNow > 0)
            empGrowth = 100;

        // 4. Course Growth (Active courses now vs last month)
        // correct logic: created in last 30 days? or just active count?
        // Let's go with "New Courses this month" vs "New Courses last month" to show activity.
        // Or simple count growth. Let's do Count Growth.
        var coursesLastMonth = await _context.Courses.CountAsync(c => c.StartDate < lastMonthDate);
        var coursesNow = await _context.Courses.CountAsync();
        double courseGrowth = 0;
        if (coursesLastMonth > 0)
            courseGrowth = Math.Round(((double)(coursesNow - coursesLastMonth) / coursesLastMonth) * 100, 1);
        else if (coursesNow > 0)
            courseGrowth = 100;

        var stats = new DashboardStats
        {
            EmployeeCount = employeesNow,
            CourseCount = coursesNow,
            EnrollmentCount = await _context.Enrollments.CountAsync(),
            LatestCourses = await _context.Courses.OrderByDescending(c => c.StartDate).Take(5).ToListAsync(),
            AttendancePercentage = attendancePct,
            MonthlyCourseCounts = data.ToArray(),
            MonthlyLabels = labels.ToArray(),
            EmployeeGrowth = empGrowth,
            CourseGrowth = courseGrowth
        };
        return stats;
    }
}
