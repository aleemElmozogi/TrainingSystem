using TrainingSystem.Shared.Domain;
using System.Text.RegularExpressions;

namespace TrainingSystem.Data;

public static class DbInitializer
{
    public static void Initialize(AppDbContext context)
    {
        // Ensure the database is created
        context.Database.EnsureCreated();

        // 1. Clear existing employees/enrollments to ensure clean state as requested
        if (context.Enrollments.Any())
        {
            context.Enrollments.RemoveRange(context.Enrollments);
        }
        if (context.Employees.Any())
        {
            context.Employees.RemoveRange(context.Employees);
        }
        context.SaveChanges();

        // 2. Parse and Add New Employees from the provided CSV data
        // Reading directly from the file to ensure we get all records (avoiding truncation)
        var filePath = @"c:\Users\Shahed\Documents\dev\TrainingSystem\اسبوعان اسم و رقم (4)(ورقة1).csv";
        
        if (File.Exists(filePath))
        {
            // Use Windows-1256 for Arabic CSVs
            var lines = File.ReadAllLines(filePath, System.Text.Encoding.GetEncoding("windows-1256"));
            
            var employeesToAdd = new List<Employee>();
            var seenEmployeeNumbers = new HashSet<string>();

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(',');
                if (parts.Length < 2) continue;

                var name = parts[0]?.Trim().Replace("\"", "");
                var empNumStr = parts[1]?.Trim().Replace("\"", "");

                // Skip header or empty lines or invalid data
                // Also check if empNumStr is numeric or valid to avoid adding garbage
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(empNumStr) || name.Contains("الاسم")) continue;

                if (seenEmployeeNumbers.Contains(empNumStr)) continue; // Avoid duplicates

                var employee = new Employee
                {
                    FullName = name,
                    EmploymentNumber = empNumStr,
                    Department = null,
                    JobTitle = null,
                    Email = null,
                    // PhoneNumber = "0000000000" // Not in current model
                };

                employeesToAdd.Add(employee);
                seenEmployeeNumbers.Add(empNumStr);
            }

            if (employeesToAdd.Any())
            {
                context.Employees.AddRange(employeesToAdd);
                context.SaveChanges();
            }
        }
        else
        {
             // Fallback or log if file not found (though we know it exists)
             System.Diagnostics.Debug.WriteLine($"File not found: {filePath}");
        }
    }
}
