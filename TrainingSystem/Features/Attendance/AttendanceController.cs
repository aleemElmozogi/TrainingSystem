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
        // Return existing records for that course and date
        return await _context.AttendanceRecords
            .Include(a => a.Employee)
            .Where(a => a.CourseId == courseId && a.Date.Date == date.Date)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult> SaveAttendance(List<AttendanceRecord> records)
    {
        if (records == null || !records.Any()) return BadRequest();

        var courseId = records.First().CourseId;
        var date = records.First().Date.Date;

        // Remove existing records for this day/course to replace them (or update them)
        // Simple approach: delete all for that day/course and insert new ones (careful with IDs if needed elsewhere, but for attendance log it's okay usually, or simpler: upsert)
        
        // Let's try upsert logic or just find existing and update
        var existingRecords = await _context.AttendanceRecords
            .Where(a => a.CourseId == courseId && a.Date.Date == date)
            .ToListAsync();

        foreach (var record in records)
        {
            var existing = existingRecords.FirstOrDefault(r => r.EmployeeId == record.EmployeeId);
            if (existing != null)
            {
                existing.IsPresent = record.IsPresent;
                existing.Notes = record.Notes;
            }
            else
            {
                _context.AttendanceRecords.Add(record);
            }
        }
        
        await _context.SaveChangesAsync();
        return Ok();
    }
}
