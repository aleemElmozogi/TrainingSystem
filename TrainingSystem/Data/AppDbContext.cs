using Microsoft.EntityFrameworkCore;
using TrainingSystem.Shared.Domain;

namespace TrainingSystem.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Course> Courses { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<AttendanceRecord> AttendanceRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // AttendanceRecord Unique Constraint
        modelBuilder.Entity<AttendanceRecord>()
            .HasIndex(a => new { a.CourseId, a.EmployeeId, a.Date })
            .IsUnique();
        
        // Enrollment Unique Constraint
        modelBuilder.Entity<Enrollment>()
            .HasIndex(e => new { e.CourseId, e.EmployeeId })
            .IsUnique();
    }
}
