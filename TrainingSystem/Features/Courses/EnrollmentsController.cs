using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.Data;
using TrainingSystem.Shared.Domain;

namespace TrainingSystem.Features.Courses;

[Route("api/[controller]")]
[ApiController]
public class EnrollmentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public EnrollmentsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<Enrollment>> EnrollEmployee(Enrollment enrollment)
    {
        // Check if already enrolled
        var exists = await _context.Enrollments
            .AnyAsync(e => e.CourseId == enrollment.CourseId && e.EmployeeId == enrollment.EmployeeId);

        if (exists)
        {
            return Conflict("Employee is already enrolled in this course.");
        }

        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();
        return Ok(enrollment);
    }
    
    [HttpGet("course/{courseId}")]
    public async Task<ActionResult<List<Enrollment>>> GetEnrollmentsByCourse(int courseId)
    {
         return await _context.Enrollments
            .Include(e => e.Employee)
            .Where(e => e.CourseId == courseId)
            .ToListAsync();
    }

    [HttpDelete("{courseId}/{employeeId}")]
    public async Task<IActionResult> RemoveEnrollment(int courseId, int employeeId)
    {
        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.CourseId == courseId && e.EmployeeId == employeeId);
            
        if (enrollment == null) return NotFound();

        _context.Enrollments.Remove(enrollment);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
