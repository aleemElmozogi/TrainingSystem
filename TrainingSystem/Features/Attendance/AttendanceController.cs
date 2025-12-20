using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.Data;
using TrainingSystem.Shared.Domain;

namespace TrainingSystem.Features.Attendance;

[Route("api/[controller]")]
[ApiController]
public class AttendanceController : ControllerBase
{
    private readonly AppDbContext _context;

    public AttendanceController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("{courseId}/{date}")]
    public async Task<ActionResult<List<AttendanceRecord>>> GetAttendance(int courseId, DateTime date)
    {
        // 1. Get existing attendance records
        var existingRecords = await _context.AttendanceRecords
            .Include(a => a.Employee)
            .Where(a => a.CourseId == courseId && a.Date.HasValue && a.Date.Value.Date == date.Date)
            .ToListAsync();

        // 2. Get enrollments for this course
        var enrollments = await _context.Enrollments
            .Include(e => e.Employee)
            .Where(e => e.CourseId == courseId)
            .ToListAsync();

        // 3. Create missing records for enrolled employees
        foreach (var enrollment in enrollments)
        {
            if (!existingRecords.Any(r => r.EmployeeId == enrollment.EmployeeId))
            {
                var newRecord = new AttendanceRecord
                {
                    CourseId = courseId,
                    EmployeeId = enrollment.EmployeeId,
                    Employee = enrollment.Employee, // Populate navigation property for UI
                    Date = date,
                    Status = null,
                    Notes = ""
                };
                // Add to DB and to the list
                await _context.AttendanceRecords.AddAsync(newRecord);
                existingRecords.Add(newRecord);
            }
        }

        // Save changes if any new records were added
        await _context.SaveChangesAsync();

        return existingRecords.OrderBy(r => r.Employee?.FullName).ToList();
    }

    [HttpGet("by-day/{courseId}/{dayNumber}")]
    public async Task<ActionResult<List<AttendanceRecord>>> GetAttendanceByDay(int courseId, int dayNumber)
    {
        // 1. Get existing attendance records
        var existingRecords = await _context.AttendanceRecords
            .Include(a => a.Employee)
            .Where(a => a.CourseId == courseId && a.DayNumber == dayNumber)
            .ToListAsync();

        // 2. Get enrollments for this course
        var enrollments = await _context.Enrollments
            .Include(e => e.Employee)
            .Where(e => e.CourseId == courseId)
            .ToListAsync();

        // 3. Create missing records for enrolled employees
        foreach (var enrollment in enrollments)
        {
            if (!existingRecords.Any(r => r.EmployeeId == enrollment.EmployeeId))
            {
                var newRecord = new AttendanceRecord
                {
                    CourseId = courseId,
                    EmployeeId = enrollment.EmployeeId,
                    Employee = enrollment.Employee, // Populate navigation property for UI
                    Date = null,
                    DayNumber = dayNumber,
                    Status = null,
                    Notes = ""
                };
                // Add to DB and to the list
                await _context.AttendanceRecords.AddAsync(newRecord);
                existingRecords.Add(newRecord);
            }
        }

        // Save changes if any new records were added
        await _context.SaveChangesAsync();

        return existingRecords.OrderBy(r => r.Employee?.FullName).ToList();
    }

    [HttpGet("all/{courseId}")]
    public async Task<ActionResult<List<AttendanceRecord>>> GetAllCourseAttendance(int courseId)
    {
        return await _context.AttendanceRecords
            .Include(a => a.Employee)
            .Where(a => a.CourseId == courseId)
            .OrderBy(r => r.DayNumber)
            .ThenBy(r => r.Date)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult> SaveAttendance(List<AttendanceRecord> records)
    {
        if (records == null || !records.Any()) return BadRequest();

        // Optimized bulk upsert
        foreach (var record in records)
        {
            if (record.Id > 0)
            {
                var existing = await _context.AttendanceRecords.FindAsync(record.Id);
                if (existing != null)
                {
                    existing.Status = record.Status;
                    existing.Notes = record.Notes;
                    // existing.Date = record.Date; // Should not change key
                    // existing.DayNumber = record.DayNumber; // Should not change key
                    _context.Entry(existing).State = EntityState.Modified;
                }
            }
            else
            {
                // Check if exists by key (Course, Emp, Day/Date) to avoid duplicates if ID was missing
                AttendanceRecord? existing = null;
                if (record.Date.HasValue)
                {
                     existing = await _context.AttendanceRecords.FirstOrDefaultAsync(r => 
                        r.CourseId == record.CourseId && 
                        r.EmployeeId == record.EmployeeId && 
                        r.Date.HasValue && r.Date.Value.Date == record.Date.Value.Date);
                }
                else
                {
                     existing = await _context.AttendanceRecords.FirstOrDefaultAsync(r => 
                        r.CourseId == record.CourseId && 
                        r.EmployeeId == record.EmployeeId && 
                        r.DayNumber == record.DayNumber);
                }

                if (existing != null)
                {
                    existing.Status = record.Status;
                    existing.Notes = record.Notes;
                }
                else
                {
                    _context.AttendanceRecords.Add(record);
                }
            }
        }
        
        await _context.SaveChangesAsync();
        return Ok();
    }
}
