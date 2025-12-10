using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.Data;
using TrainingSystem.Shared.Domain;

namespace TrainingSystem.Features.Courses;

[Route("api/[controller]")]
[ApiController]
public class CoursesController : ControllerBase
{
    private readonly AppDbContext _context;

    public CoursesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<Course>>> GetCourses()
    {
        return await _context.Courses.ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Course>> CreateCourse(Course course)
    {
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();
        await _context.SaveChangesAsync();
        return Ok(course);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Course>> GetCourse(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null) return NotFound();
        return course;
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCourse(int id, Course course)
    {
        if (id != course.Id) return BadRequest();
        _context.Entry(course).State = EntityState.Modified;
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Courses.Any(e => e.Id == id)) return NotFound();
            throw;
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCourse(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null) return NotFound();
        
        // Check for dependencies (Enrollments/Attendance) - For now let FK cascade or error
        // Better to check and prevent delete if data exists, but simplest is generic delete
        
        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
