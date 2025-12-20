using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.Data;
using TrainingSystem.Shared.Domain;
using TrainingSystem.Shared.DTOs;

namespace TrainingSystem.Features.Reports;

[Route("api/[controller]")]
[ApiController]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReportsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("course-assignments/{courseId}")]
    public async Task<ActionResult<List<CourseAssignmentDto>>> GetCourseAssignments(int courseId)
    {
        var assignments = await _context.Enrollments
            .Where(e => e.CourseId == courseId)
            .Include(e => e.Employee)
            .Select(e => new CourseAssignmentDto
            {
                EmployeeId = e.EmployeeId,
                EmployeeName = e.Employee!.FullName,
                EmploymentNumber = e.Employee.EmploymentNumber,
                Department = e.Employee.Department,
                JobTitle = e.Employee.JobTitle,
                JoinedDate = e.Employee.JoinedDate,
                Country = e.Country,
                Duration = e.Duration
            })
            .ToListAsync();

        return assignments;
    }

    [HttpGet("employee-assignments/{employeeId}")]
    public async Task<ActionResult<List<EmployeeAssignmentDto>>> GetEmployeeAssignments(int employeeId)
    {
        var assignments = await _context.Enrollments
            .Where(e => e.EmployeeId == employeeId)
            .Include(e => e.Course)
            .Select(e => new EmployeeAssignmentDto
            {
                CourseId = e.CourseId,
                CourseTitle = e.Course!.Title,
                StartDate = e.Course.StartDate,
                EndDate = e.Course.EndDate,
                Status = (e.Course.EndDate.HasValue && DateTime.Now > e.Course.EndDate) ? "Completed" : "Active",
                Country = e.Country,
                Duration = e.Duration
            })
            .ToListAsync();

        return assignments;
    }

    [HttpGet("attendance")]
    public async Task<ActionResult<List<AttendanceReportDto>>> GetAttendanceReport(
        [FromQuery] int? courseId, 
        [FromQuery] int? employeeId, 
        [FromQuery] DateTime? startDate, 
        [FromQuery] DateTime? endDate)
    {
        var attendanceRecords = await _context.AttendanceRecords
            .Include(a => a.Course)
            .Include(a => a.Employee)
            .AsNoTracking() // added performance
            .ToListAsync();

        // Performance Note: Doing join in memory for simpler LINQ translation if huge data isn't expected, 
        // OR better: use LINQ Query Syntax to join Enrollments.
        // For now, let's pre-fetch Enrollments if courseId is filtered, or use sub-select if small.
        // Given existing structure, let's use a separate lookup for efficiency if needed.

        IEnumerable<AttendanceRecord> query = attendanceRecords;

        if (courseId.HasValue)
            query = query.Where(a => a.CourseId == courseId.Value);

        if (employeeId.HasValue)
            query = query.Where(a => a.EmployeeId == employeeId.Value);

        if (startDate.HasValue)
        {
            var start = startDate.Value.Date;
            query = query.Where(a => a.Date.HasValue && a.Date.Value.Date >= start);
        }

        if (endDate.HasValue)
        {
            var end = endDate.Value.Date;
            query = query.Where(a => a.Date.HasValue && a.Date.Value.Date <= end);
        }

        // Fetch Enrollments to map details
        // We get distinct pairs of CourseId/EmployeeId from the result
        var pairs = query.Select(x => new { x.CourseId, x.EmployeeId }).Distinct().ToList();
        
        // This is not efficient for ALL records, but okay for report scope usually.
        // Better: Fetch all enrollments matching the criteria.
        var enrollments = await _context.Enrollments.ToListAsync(); 
        // Optimization: if courseId is passed, filter.
        
        var report = query
            .OrderByDescending(a => a.Date)
            .Select(a => 
            {
                var enroll = enrollments.FirstOrDefault(e => e.CourseId == a.CourseId && e.EmployeeId == a.EmployeeId);
                return new AttendanceReportDto
                {
                    Date = a.Date,
                    DayNumber = a.DayNumber,
                    EmployeeName = a.Employee?.FullName ?? string.Empty,
                    CourseTitle = a.Course?.Title ?? string.Empty,
                    Status = a.Status.ToString(),
                    Notes = a.Notes,
                    Country = enroll?.Country
                };
            })
            .ToList();

        return report;
    }

    [HttpGet("export/course-assignments/{courseId}")]
    public async Task<IActionResult> ExportCourseAssignments(int courseId)
    {
        var course = await _context.Courses.FindAsync(courseId);
        if (course == null) return NotFound();

        var assignments = await GetCourseAssignments(courseId);
        var list = assignments.Value ?? new List<CourseAssignmentDto>();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Assignments");
        
        // Right-to-Left for Arabic
        worksheet.RightToLeft = true;

        // Header
        worksheet.Cell(1, 1).Value = "اسم الموظف";
        worksheet.Cell(1, 2).Value = "الرقم الوظيفي";
        worksheet.Cell(1, 3).Value = "القسم";
        worksheet.Cell(1, 4).Value = "المسمى الوظيفي";
        worksheet.Cell(1, 5).Value = "تاريخ الانضمام";
        worksheet.Cell(1, 6).Value = "الدولة";
        worksheet.Cell(1, 7).Value = "المدة";

        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

        int row = 2;
        foreach (var item in list)
        {
            worksheet.Cell(row, 1).Value = item.EmployeeName;
            worksheet.Cell(row, 2).Value = item.EmploymentNumber;
            worksheet.Cell(row, 3).Value = item.Department;
            worksheet.Cell(row, 4).Value = item.JobTitle;
            worksheet.Cell(row, 5).Value = item.JoinedDate.ToString("yyyy/MM/dd");
            worksheet.Cell(row, 6).Value = item.Country;
            worksheet.Cell(row, 7).Value = item.Duration;
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();

        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Assignments_{course.Title}.xlsx");
    }

    [HttpGet("export/employee-assignments/{employeeId}")]
    public async Task<IActionResult> ExportEmployeeAssignments(int employeeId)
    {
        var employee = await _context.Employees.FindAsync(employeeId);
        if (employee == null) return NotFound();

        var assignments = await GetEmployeeAssignments(employeeId);
        var list = assignments.Value ?? new List<EmployeeAssignmentDto>();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Courses");
        worksheet.RightToLeft = true;

        worksheet.Cell(1, 1).Value = "عنوان الدورة";
        worksheet.Cell(1, 2).Value = "تاريخ البدء";
        worksheet.Cell(1, 3).Value = "تاريخ الانتهاء";
        worksheet.Cell(1, 4).Value = "الحالة";
        worksheet.Cell(1, 5).Value = "الدولة";
        worksheet.Cell(1, 6).Value = "المدة";

        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

        int row = 2;
        foreach (var item in list)
        {
            worksheet.Cell(row, 1).Value = item.CourseTitle;
            worksheet.Cell(row, 2).Value = item.StartDate?.ToString("yyyy/MM/dd") ?? "-";
            worksheet.Cell(row, 3).Value = item.EndDate?.ToString("yyyy/MM/dd") ?? "-";
            worksheet.Cell(row, 4).Value = item.Status; // Translate if needed
            worksheet.Cell(row, 5).Value = item.Country;
            worksheet.Cell(row, 6).Value = item.Duration;
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();

        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Courses_{employee.FullName}.xlsx");
    }

    [HttpGet("export/attendance")]
    public async Task<IActionResult> ExportAttendance(
        [FromQuery] int? courseId, 
        [FromQuery] int? employeeId, 
        [FromQuery] DateTime? startDate, 
        [FromQuery] DateTime? endDate)
    {
        var reportResult = await GetAttendanceReport(courseId, employeeId, startDate, endDate);
        var list = reportResult.Value ?? new List<AttendanceReportDto>();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Attendance");
        worksheet.RightToLeft = true;

        worksheet.Cell(1, 1).Value = "اليوم / التاريخ";
        worksheet.Cell(1, 2).Value = "الموظف";
        worksheet.Cell(1, 3).Value = "الدورة";
        worksheet.Cell(1, 4).Value = "الحالة";
        worksheet.Cell(1, 5).Value = "ملاحظات";
        worksheet.Cell(1, 6).Value = "الدولة";

        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

        int row = 2;
        foreach (var item in list)
        {
            worksheet.Cell(row, 1).Value = item.Date.HasValue 
                ? item.Date.Value.ToString("yyyy/MM/dd") 
                : $"اليوم {item.DayNumber}";
            worksheet.Cell(row, 2).Value = item.EmployeeName;
            worksheet.Cell(row, 3).Value = item.CourseTitle;
            string statusText = item.Status switch
            {
                "Present" => "حاضر",
                "Absent" => "غائب",
                "Excused" => "غائب بعذر",
                "NotArrived" => "لم يصل",
                "Holiday" => "عطلة",
                "EmergencyHoliday" => "عطلة طارئة",
                _ => item.Status
            };
            worksheet.Cell(row, 4).Value = statusText;
            worksheet.Cell(row, 5).Value = item.Notes;
            worksheet.Cell(row, 6).Value = item.Country; // Export Country
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();

        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Attendance_Report.xlsx");
    }
}
