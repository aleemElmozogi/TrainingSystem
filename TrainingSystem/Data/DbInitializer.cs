using TrainingSystem.Shared.Domain;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace TrainingSystem.Data;

public static class DbInitializer
{
    public static void Initialize(AppDbContext context)
    {
        // 1. Ensure database is created/migrated
        context.Database.Migrate();

        // 1.1 Hack to remove Unique Constraint on Enrollments if it exists (to allow duplicates)
        try
        {
            context.Database.ExecuteSqlRaw(@"
                IF EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_Enrollments_CourseId_EmployeeId' AND object_id = OBJECT_ID('Enrollments') AND is_unique = 1)
                BEGIN
                    DROP INDEX IX_Enrollments_CourseId_EmployeeId ON Enrollments;
                    CREATE INDEX IX_Enrollments_CourseId_EmployeeId ON Enrollments(CourseId, EmployeeId);
                END
            ");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating index: {ex.Message}");
        }

        // 1.2 Clear existing "Imported" notes if they exist
        try
        {
            context.Database.ExecuteSqlRaw("UPDATE AttendanceRecords SET Notes = NULL WHERE Notes = 'Imported'");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error clearing notes: {ex.Message}");
        }

        // 2. Only Seed if Employees table is empty
        if (!context.Employees.Any())
        {
            // Parse and Add New Employees from the provided CSV data
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
                    if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(empNumStr) || name.Contains("الاسم")) continue;

                    if (seenEmployeeNumbers.Contains(empNumStr)) continue; // Avoid duplicates

                    var employee = new Employee
                    {
                        FullName = name,
                        EmploymentNumber = empNumStr,
                        Department = null,
                        JobTitle = null,
                        Email = null
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
                 System.Diagnostics.Debug.WriteLine($"File not found: {filePath}");
            }
        }

        // 3. Seed Enrollments for Course 1019
        // 3. Seed Enrollments for Course 1019 (DISABLED to allow manual changes to persist)
        // SeedEnrollments1019(context);

        // 4. Seed Financial Accounting Course
        SeedFinancialAccountingCourse(context);

        // 5. Seed New Weekly Courses
        SeedMechanicalMaintenanceCourse(context);
        SeedPublicRelationsCourse(context);
        SeedInspectionMethodsCourse(context);
        SeedAccountExaminationCourse(context);
        
        // Additional Weekly Courses (14-29)
        SeedArbitration2Course(context);
        SeedOilMarketingDiplomaCourse(context);
        SeedAdvancedAccountingCourse(context);
        SeedArbitrationCourse(context);
        SeedOilSupplyMarketingCourse(context);
        SeedHumanResourcesCourse(context);
        SeedAccountingSystemsCourse(context);
        SeedAccountingSystems2Course(context);
        SeedFuelQualityControlCourse(context);
        SeedEngineMaintenanceCourse(context);
        SeedEngineMaintenance2Course(context);
        SeedOilAccountingCourse(context);
        SeedOilProductSalesCourse(context);

        // Additional Weekly Courses (30-55)
        SeedControlSystemsCourse(context);
        SeedInspectionCourse(context);
        SeedHealthSafetyDiplomaCourse(context);
        SeedHumanResources2Course(context);  // 34
        SeedHumanResources3Course(context);  // 35
        SeedItProfessionalCourse(context);    // 36
        SeedOfficeArchivesCourse(context);    // 37
        SeedHumanResources4Course(context);  // 38
        SeedAccountingDiplomaCourse(context); // 39
        SeedLegalContractsCourse(context);    // 40
        SeedSpecialCourse2Course(context);    // 41
        SeedElectricityElectronicsCourse(context); // 42
        SeedSpecialCourse1Course(context);    // 43
        SeedQualityManagementDiplomaCourse(context); // 44
        SeedManagementBasicsCourse(context);  // 45
        SeedHrMasterCourse(context);         // 46
        SeedHrMaster2Course(context);        // 47
        SeedInternationalSafetyCertCourse(context); // 48
        SeedVehicleBusinessCourse(context);   // 49
        SeedDigitalTransformationCourse(context);  // 50
        SeedHumanResources5Course(context);   // 51
        SeedAccountingDiploma2Course(context); // 52
        SeedAccountingDiploma3Course(context); // 53
        SeedPublicRelationsMgmtCourse(context); // 54
        SeedPublicRelationsMgmt2Course(context); // 55

        // New Courses from latest request
        SeedFuelQualityControlGroup2Course(context);
        SeedEngineMaintenance32ACourse(context);
        SeedHealthSafetyDiplomaA11Course(context);
        SeedOilSupplyMarketingA2Course(context);
        SeedControlSystems20ACourse(context);
        SeedIndustrialSecurityAdvancedCourse(context);
        SeedIndustrialSecurityAdvancedA4Course(context);
        SeedProfessionalMarketing3Course(context);

        // Merge "خطة التدريب اسبوعين - (4)" into "خطة التدريب اسبوعين"
        var sourceCourse = context.Courses.FirstOrDefault(c => c.Title == "خطة التدريب اسبوعين - (4)");
        var targetCourse = context.Courses.FirstOrDefault(c => c.Title == "خطة التدريب اسبوعين");

        if (sourceCourse != null && targetCourse != null)
        {
            var sourceEnrollments = context.Enrollments.Where(e => e.CourseId == sourceCourse.Id).ToList();
            var targetEmployeeIds = context.Enrollments.Where(e => e.CourseId == targetCourse.Id).Select(e => e.EmployeeId).ToHashSet();

            foreach (var enrollment in sourceEnrollments)
            {
                if (!targetEmployeeIds.Contains(enrollment.EmployeeId))
                {
                    enrollment.CourseId = targetCourse.Id;
                }
                else
                {
                    context.Enrollments.Remove(enrollment);
                }
            }

            var sourceAttendance = context.AttendanceRecords.Where(a => a.CourseId == sourceCourse.Id).ToList();
            var targetAttendanceKeys = context.AttendanceRecords
                .Where(a => a.CourseId == targetCourse.Id)
                .Select(a => new { a.EmployeeId, a.Date })
                .AsEnumerable()
                .Select(a => (a.EmployeeId, a.Date))
                .ToHashSet();

            foreach (var attendance in sourceAttendance)
            {
                if (!targetAttendanceKeys.Contains((attendance.EmployeeId, attendance.Date)))
                {
                    attendance.CourseId = targetCourse.Id;
                }
                else
                {
                    // Existing record in target wins; remove source duplicate
                    context.AttendanceRecords.Remove(attendance);
                }
            }

            context.Courses.Remove(sourceCourse);
            context.SaveChanges();
        }

        SeedShortTermPlanFromCsv(context);
    }

    private static void SeedEnrollments1019(AppDbContext context)
    {
        var targetCourseId = 1019;
        var filePath = GetFilePath("اسبوعان اسم و رقم (4)(ورقة1).csv");
        
        if (!File.Exists(filePath)) return;

        // 1. Wipe existing enrollments for this course to allow full sync (delete removed, add new)
        // This effectively "deletes the last employee" if they were removed from the file.
        var existingEnrollments = context.Enrollments.Where(e => e.CourseId == targetCourseId);
        context.Enrollments.RemoveRange(existingEnrollments);
        context.SaveChanges();

        // 2. Read CSV
        var lines = File.ReadAllLines(filePath, System.Text.Encoding.GetEncoding("windows-1256"));
        
        // Load existing employees to match by Number
        var existingEmployees = context.Employees
            .Where(e => e.EmploymentNumber != null)
            .AsEnumerable()
            .GroupBy(e => e.EmploymentNumber)
            .ToDictionary(g => g.Key, g => g.First());

        var newEnrollments = new List<Enrollment>();
        var newEmployees = new List<Employee>();

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var parts = line.Split(',');
            if (parts.Length < 2) continue;

            var name = parts[0]?.Trim().Replace("\"", "");
            var empNum = parts[1]?.Trim().Replace("\"", "");
            
            if (string.IsNullOrEmpty(empNum) || string.IsNullOrEmpty(name) || line.Contains("الاسم")) continue;

            int empId;
            if (existingEmployees.TryGetValue(empNum, out var existingEmp))
            {
                empId = existingEmp.Id;
            }
            else
            {
                // Create new Employee? Yes, user said "add new employee"
                // Check if we already staged this one
                var staged = newEmployees.FirstOrDefault(e => e.EmploymentNumber == empNum);
                if (staged != null)
                {
                    // If we are creating it, we can't easily get ID yet.
                    // We'll handle new employees in a first pass or save immediately.
                    // To keep it simple, we'll save immediately if not found.
                    // But we can't do that easily inside loop if likely to be slow? 
                    // Let's just create and SaveChanges batching? No, ID is needed.
                    continue; // Skip for now, we handle below
                }
                
                var newEmp = new Employee
                {
                    FullName = name,
                    EmploymentNumber = empNum,
                    JoinedDate = DateTime.UtcNow
                };
                newEmployees.Add(newEmp);
                continue;
            }
            
            // Allow Duplicates: Always add enrollment
            newEnrollments.Add(new Enrollment
            {
                CourseId = targetCourseId,
                EmployeeId = empId,
                EnrolledAt = DateTime.UtcNow
            });
        }

        // 2.1 Save new employees
        if (newEmployees.Any())
        {
            context.Employees.AddRange(newEmployees);
            context.SaveChanges();
            
            // Add enrollments for them
            foreach (var newEmp in newEmployees)
            {
                newEnrollments.Add(new Enrollment
                {
                    CourseId = targetCourseId,
                    EmployeeId = newEmp.Id,
                    EnrolledAt = DateTime.UtcNow
                });
            }
        }

        // 3. Save Enrollments (allowing duplicates because we removed unique index)
        if (newEnrollments.Any())
        {
            context.Enrollments.AddRange(newEnrollments);
            context.SaveChanges();
            System.Diagnostics.Debug.WriteLine($"Seeded {newEnrollments.Count} enrollments from CSV (Duplicates Allowed).");
        }
    }


    private static void SeedFinancialAccountingCourse(AppDbContext context)
    {
        var filePath = @"c:\Users\Shahed\Documents\dev\TrainingSystem\7المحاسبة_المالية_للمؤسسات_النفط(ورقة1).csv";
        if (!File.Exists(filePath)) return;

        // 1. Ensure Course Exists
        var courseTitle = "المحاسبة المالية للمؤسسات النفطية";
        var course = context.Courses.FirstOrDefault(c => c.Title == courseTitle);
        if (course == null)
        {
            course = new Course
            {
                Title = courseTitle,
                Provider = "بيت الإدارة العربي",
                StartDate = new DateTime(2025, 8, 31),
                EndDate = new DateTime(2025, 9, 11),
                TravelDate = new DateTime(2025, 8, 27),
                Duration = "أسبوعان",
                CourseType = CourseType.WeeklyPlan2021, // Changed to WeeklyPlan2021
                TargetAudience = TargetAudience.SpecificEmployees,
                Type = "External" // Likely external due to "Travel Date"
            };
            context.Courses.Add(course);
        }
        else
        {
            // Update existing course to ensure it is WeeklyPlan2021
            if (course.CourseType != CourseType.WeeklyPlan2021)
            {
                course.CourseType = CourseType.WeeklyPlan2021;
            }
        }
        context.SaveChanges();

        // 2. Read File and Process Data
        // The file format is non-standard CSV (whitespace separated in visual blocks).
        // We will read all lines and look for known employee numbers or patterns.
        var lines = File.ReadAllLines(filePath);
        
        // Dates mapping based on visual inspection of the file:
        // Week 1 Header: 2025/09/04 ... 2025/08/31 (Left to Right)
        var week1Dates = new[]
        {
            new DateTime(2025, 9, 4),
            new DateTime(2025, 9, 3),
            new DateTime(2025, 9, 2),
            new DateTime(2025, 9, 1),
            new DateTime(2025, 8, 31)
        };

        // Week 2 Header: 2025/09/11 ... 2025/09/07
        var week2Dates = new[]
        {
            new DateTime(2025, 9, 11),
            new DateTime(2025, 9, 10),
            new DateTime(2025, 9, 9),
            new DateTime(2025, 9, 8),
            new DateTime(2025, 9, 7)
        };

        // Refined loop logic
        bool isWeek1 = false;
        bool isWeek2 = false;
        
        for (int i = 0; i < lines.Length; i++)
        {
            var l = lines[i].Trim();
            if (l.Contains("لأسبوع  / 1")) { isWeek1 = true; isWeek2 = false; continue; }
            if (l.Contains("لأسبوع  /2")) { isWeek1 = false; isWeek2 = true; continue; }
            
            if (!isWeek1 && !isWeek2) continue;

            var match = Regex.Match(l, @"(\d{5,6})");
            if (!match.Success) continue;
            var empNum = match.Value;
            
            // Logic to get Employee/Enrollment (duplicated from above)
            var parts = l.Split(new[] { empNum }, StringSplitOptions.None);
            if (parts.Length < 2) continue;
            
            var attendancePart = parts[0].Trim(); // "حاضر حاضر ..."
            
            var employee = context.Employees.FirstOrDefault(e => e.EmploymentNumber == empNum);
            // If employee null, we might have created them in Week 1 pass?
            // Actually, we should ensure they exist.
            if (employee == null)
            {
                // Parse name again
                var namePart = parts[1].Trim();
                namePart = Regex.Replace(namePart, @"\s+\d+$", "").Trim();
                employee = new Employee { EmploymentNumber = empNum, FullName = namePart, JoinedDate = DateTime.UtcNow };
                context.Employees.Add(employee);
                context.SaveChanges();
            }
            
            var enrollment = context.Enrollments.FirstOrDefault(e => e.CourseId == course.Id && e.EmployeeId == employee.Id);
            if (enrollment == null)
            {
                enrollment = new Enrollment { CourseId = course.Id, EmployeeId = employee.Id, EnrolledAt = DateTime.UtcNow };
                context.Enrollments.Add(enrollment);
                context.SaveChanges();
            }
            
            // Parse Attendance Tokens
            var tokens = attendancePart.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            // Expected 5 tokens.
            var dates = isWeek1 ? week1Dates : week2Dates;
            
            for (int d = 0; d < Math.Min(tokens.Length, dates.Length); d++)
            {
                var token = tokens[d];
                var date = dates[d];
                var status = token == "حاضر" ? AttendanceStatus.Present : AttendanceStatus.Absent; // Default to Absent if not "حاضر"
                
                if (token == "حاضر")
                {
                    // Check if record exists
                    var exists = context.AttendanceRecords.Any(a => a.CourseId == course.Id && a.EmployeeId == employee.Id && a.Date == date);
                    if (!exists)
                    {
                        context.AttendanceRecords.Add(new AttendanceRecord
                        {
                            CourseId = course.Id,
                            EmployeeId = employee.Id,
                            Date = date,
                            Status = AttendanceStatus.Present,
                            Notes = null
                        });
                    }
                }
            }
            context.SaveChanges();
        }
    }

    private static void SeedMechanicalMaintenanceCourse(AppDbContext context)
    {
        var filePath = GetFilePath("10الصيانة الميكانيكية للمركبات (2) 1(ورقة1).csv");
        if (!File.Exists(filePath)) return;

        var courseTitle = "الصيانة الميكانيكيا للمركبات";
        var course = context.Courses.FirstOrDefault(c => c.Title == courseTitle);
        if (course == null)
        {
            course = new Course
            {
                Title = courseTitle,
                Provider = "بيت الإدارة العربي",
                StartDate = new DateTime(2025, 7, 15),
                EndDate = new DateTime(2025, 10, 15), // Approx 13 weeks
                TravelDate = new DateTime(2025, 7, 11),
                Duration = "13 أسبوع",
                CourseType = CourseType.WeeklyPlan2021,
                TargetAudience = TargetAudience.SpecificEmployees,
                Type = "External"
            };
            context.Courses.Add(course);
        }
        else
        {
             if (course.CourseType != CourseType.WeeklyPlan2021) course.CourseType = CourseType.WeeklyPlan2021;
        }
        context.SaveChanges();

        ProcessLayoutCsv(context, filePath, course.Id);
    }

    private static void SeedPublicRelationsCourse(AppDbContext context)
    {
        var filePath = GetFilePath("8علاقات عامة و إدارة الوقت 2 1(ورقة1).csv");
        if (!File.Exists(filePath)) return;

        var courseTitle = "علاقات عامة و إدارة الوقت";
        var course = context.Courses.FirstOrDefault(c => c.Title == courseTitle);
        if (course == null)
        {
            course = new Course
            {
                Title = courseTitle,
                Provider = "بيت الإدارة العربي",
                StartDate = new DateTime(2025, 9, 7),
                EndDate = new DateTime(2025, 12, 7), // Approx 13 weeks
                TravelDate = new DateTime(2025, 9, 2),
                Duration = "13 أسبوع",
                CourseType = CourseType.WeeklyPlan2021,
                TargetAudience = TargetAudience.SpecificEmployees,
                Type = "External"
            };
            context.Courses.Add(course);
        }
        else
        {
             if (course.CourseType != CourseType.WeeklyPlan2021) course.CourseType = CourseType.WeeklyPlan2021;
        }
        context.SaveChanges();

        ProcessLayoutCsv(context, filePath, course.Id);
    }

    private static void SeedInspectionMethodsCourse(AppDbContext context)
    {
        var filePath = GetFilePath("12الطرق_الحديثه_في_التفتيش_في_المواقع_النفطية(ورقة1).csv");
        if (!File.Exists(filePath)) return;

        var courseTitle = "الطرق الحديثة في التفتيش المواقع النفطية";
        var course = context.Courses.FirstOrDefault(c => c.Title == courseTitle);
        if (course == null)
        {
            course = new Course
            {
                Title = courseTitle,
                Provider = "بيت الإدارة العربي",
                StartDate = new DateTime(2025, 8, 24),
                EndDate = new DateTime(2025, 11, 24), // Approx 13 weeks
                Duration = "13 أسبوع",
                CourseType = CourseType.WeeklyPlan2021,
                TargetAudience = TargetAudience.SpecificEmployees,
                Type = "External"
            };
            context.Courses.Add(course);
        }
        else
        {
             if (course.CourseType != CourseType.WeeklyPlan2021) course.CourseType = CourseType.WeeklyPlan2021;
        }
        context.SaveChanges();

        ProcessLayoutCsv(context, filePath, course.Id);
    }

    private static void SeedAccountExaminationCourse(AppDbContext context)
    {
        var filePath = GetFilePath("13الاتجاهات_الحديثة_في_فحص_الحسابات(ورقة1).csv");
        if (!File.Exists(filePath)) return;

        var courseTitle = "الاتجاهات الحديثة في فحص الحسابات";
        var course = context.Courses.FirstOrDefault(c => c.Title == courseTitle);
        if (course == null)
        {
            course = new Course
            {
                Title = courseTitle,
                Provider = "بيت الإدارة العربي",
                StartDate = new DateTime(2025, 8, 10),
                EndDate = new DateTime(2025, 12, 10), // Approx 16 weeks
                Duration = "16 أسبوع",
                CourseType = CourseType.WeeklyPlan2021,
                TargetAudience = TargetAudience.SpecificEmployees,
                Type = "External"
            };
            context.Courses.Add(course);
        }
        else
        {
             if (course.CourseType != CourseType.WeeklyPlan2021) course.CourseType = CourseType.WeeklyPlan2021;
        }
        context.SaveChanges();

        ProcessLayoutCsv(context, filePath, course.Id);
    }

    private static void ProcessLayoutCsv(AppDbContext context, string filePath, int courseId)
    {
        var encoding = System.Text.Encoding.GetEncoding("windows-1256");
        var lines = File.ReadAllLines(filePath, encoding);
        if (lines.Length == 0) return;

        Dictionary<int, DateTime> currentDatesWithIndices = new Dictionary<int, DateTime>();
        var dateRegex = new Regex(@"(\d{4}/\d{1,2}/\d{1,2})|(\d{1,2}/\d{1,2}/\d{4,5})|([a-zA-Z]+,\s+[a-zA-Z]+\s+\d{1,2},\s+\d{4})"); 
        
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var trimmed = line.Trim();

            // Support both Comma Separated and Space/Tab Separated
            string[] parts;
            if (line.Contains(","))
            {
                parts = line.Split(',');
            }
            else
            {
                // Space separated logic (handle converted CSVs)
                var rawParts = Regex.Split(line, @"\s{2,}");
                var refinedParts = new List<string>();
                foreach (var rp in rawParts)
                {
                    var p = rp.Trim();
                    if (string.IsNullOrEmpty(p)) continue;
                    
                    // If this part contains multiple dates or multiple markers, split it by single space.
                    if (dateRegex.Matches(p).Count > 1 || Regex.Matches(p, @"(√|حاضر|غائب|×|TRUE|FALSE)").Count > 1)
                    {
                        refinedParts.AddRange(p.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                    }
                    else
                    {
                        refinedParts.Add(p);
                    }
                }
                parts = refinedParts.ToArray();
            }

            var dateMatches = dateRegex.Matches(trimmed);
            // If the line has many dates, it's a header line
            if (dateMatches.Count >= 3) 
            {
                currentDatesWithIndices.Clear();
                for (int j = 0; j < parts.Length; j++)
                {
                    var part = parts[j].Trim().Replace("\"", "");
                    var m = dateRegex.Match(part);
                    if (m.Success)
                    {
                        var val = m.Value;
                        // Handle typos like "26/08/20255"
                        if (val.EndsWith("55") && val.Length > 10 && !val.Contains(","))
                        {
                            val = val.Substring(0, val.Length - 1);
                        }

                        if (DateTime.TryParse(val, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var dt))
                        {
                            currentDatesWithIndices[j] = dt;
                        }
                        else if (DateTime.TryParse(val, out var dt2))
                        {
                            currentDatesWithIndices[j] = dt2;
                        }
                    }
                }
                continue;
            }

            // Check if it's a participant row
            var empMatch = Regex.Match(trimmed, @"(\d{5,6})");
            bool hasAttendance = trimmed.Contains("√") || trimmed.Contains("حاضر") || trimmed.Contains("غائب") || trimmed.ToUpper().Contains("TRUE") || trimmed.ToUpper().Contains("FALSE") || trimmed.Contains("×");
            
            if (currentDatesWithIndices.Count > 0 && (empMatch.Success || hasAttendance))
            {
                var empNum = empMatch.Success ? empMatch.Value : null;
                
                string namePart = null;
                for (int j = parts.Length - 1; j >= 0; j--)
                {
                    var p = parts[j].Trim().Replace("\"", "");
                    if (string.IsNullOrEmpty(p) || p == "√" || p == "حاضر" || p == "غائب" || p == "ت" || p == "أسم المتدرب" || p == "الاسم" || p == "رقم التوظيف" || p == "×" || p.ToUpper() == "TRUE" || p.ToUpper() == "FALSE") 
                        continue;
                    
                    if (int.TryParse(p, out _)) continue; 

                    namePart = p;
                    break;
                }

                if (string.IsNullOrEmpty(namePart)) continue;

                var employee = context.Employees.FirstOrDefault(e => (empNum != null && e.EmploymentNumber == empNum) || (e.FullName == namePart));
                
                if (employee == null)
                {
                    employee = new Employee { EmploymentNumber = empNum, FullName = namePart, JoinedDate = DateTime.UtcNow };
                    context.Employees.Add(employee);
                    context.SaveChanges();
                }
                else if (employee.EmploymentNumber == null && empNum != null)
                {
                    employee.EmploymentNumber = empNum;
                    context.SaveChanges();
                }

                if (!context.Enrollments.Any(e => e.CourseId == courseId && e.EmployeeId == employee.Id))
                {
                    context.Enrollments.Add(new Enrollment { CourseId = courseId, EmployeeId = employee.Id, EnrolledAt = DateTime.UtcNow });
                    context.SaveChanges();
                }

                // Attendance Parsing using indices
                foreach (var entry in currentDatesWithIndices)
                {
                    int idx = entry.Key;
                    DateTime date = entry.Value;

                    if (idx < parts.Length)
                    {
                        var token = parts[idx].Trim().Replace("\"", "");
                        if (string.IsNullOrEmpty(token)) continue;

                        AttendanceStatus? status = null;
                        string? note = null;
                        var tokenUpper = token.ToUpper();

                        if (token == "√" || token == "حاضر" || tokenUpper == "TRUE")
                        {
                            status = AttendanceStatus.Present;
                        }
                        else if (token == "×" || token == "غائب" || tokenUpper == "FALSE")
                        {
                            status = AttendanceStatus.Absent;
                        }
                        else if (token.Contains("وفاة") || token.Contains("عذر") || token.Contains("إجازة") || token.Contains("مريض") || token.Contains("خروج"))
                        {
                            status = AttendanceStatus.Excused;
                            note = token;
                        }

                        if (status.HasValue)
                        {
                             if (!context.AttendanceRecords.Any(a => a.CourseId == courseId && a.EmployeeId == employee.Id && a.Date == date))
                             {
                                 context.AttendanceRecords.Add(new AttendanceRecord
                                 {
                                     CourseId = courseId,
                                     EmployeeId = employee.Id,
                                     Date = date,
                                     Status = status,
                                     Notes = note
                                 });
                             }
                        }
                    }
                }
                context.SaveChanges();
            }
        }
    }

    private static void SeedArbitration2Course(AppDbContext context)
    {
        var filePath = GetFilePath("14تحكيم دولي - 2(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        SeedGenericCourse(context, filePath, "تحكيم الدولي", "EAAC", new DateTime(2025, 9, 2), "26 أسبوع");
    }

    private static void SeedOilMarketingDiplomaCourse(AppDbContext context)
    {
        var filePath = GetFilePath("15دبلوم تسويق المنتجات النفطية(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        SeedGenericCourse(context, filePath, "دبلوم تسويق المنتجات النفطية", "EAAC", new DateTime(2025, 9, 7), "26 أسبوع");
    }

    private static void SeedAdvancedAccountingCourse(AppDbContext context)
    {
        var filePath = GetFilePath("16advanced accounting 1(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        SeedGenericCourse(context, filePath, "Advance accounting", "EAAC", new DateTime(2025, 9, 23), "52 أسبوع");
    }

    private static void SeedArbitrationCourse(AppDbContext context)
    {
        var filePath = GetFilePath("17تحكيم دولي(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        SeedGenericCourse(context, filePath, "تحكيم دولي", "EAAC", new DateTime(2025, 9, 14), "26 أسبوع");
    }

    private static void SeedOilSupplyMarketingCourse(AppDbContext context)
    {
        var filePath = GetFilePath("18توريد و تسويق المنتجات النفطية(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        SeedGenericCourse(context, filePath, "توريد و تسويق المنتجات النفطية", "EAAC", new DateTime(2025, 9, 9), "13 أسبوع");
    }

    private static void SeedHumanResourcesCourse(AppDbContext context)
    {
        var filePath = GetFilePath("19تنمية الموارد البشرية(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        SeedGenericCourse(context, filePath, "تنمية الموارد البشرية", "Arb group", new DateTime(2025, 9, 8), "16 أسبوع");
    }

    private static void SeedAccountingSystemsCourse(AppDbContext context)
    {
        var filePath = GetFilePath("21Aالنظم المحاسبية(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        SeedGenericCourse(context, filePath, "النظم المحاسبية - عربي", "الاكاديمية العربية", new DateTime(2025, 9, 14), "13 أسبوع");
    }

    private static void SeedAccountingSystems2Course(AppDbContext context)
    {
        var filePath = GetFilePath("22نظم المحاسبية 2(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        SeedGenericCourse(context, filePath, "النظم المحاسبية - AAASCE", "AAASCE", new DateTime(2025, 9, 21), "2 أسبوع");
    }

    private static void SeedFuelQualityControlCourse(AppDbContext context)
    {
        var filePath = GetFilePath("25مراقبة جود الوقود(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        
        SeedGenericCourse(context, filePath, "مراقبة جودة الوقود", "AAASCE", new DateTime(2025, 7, 20), "11 أسبوع");
    }

    private static void SeedEngineMaintenanceCourse(AppDbContext context)
    {
        var filePath = GetFilePath("26تشخيص و فحص و صيانة المحركات(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        SeedGenericCourse(context, filePath, "تشخيص و فحص و صيانة المحركات", "AAASCE", new DateTime(2025, 9, 7), "4 أسبوع");
    }

    private static void SeedEngineMaintenance2Course(AppDbContext context)
    {
        var filePath = GetFilePath("27 صيانة المحركات(ورقة1).csv");
        if (!File.Exists(filePath)) 
        {
            filePath = GetFilePath("صيانة المحركات32a(ورقة1).csv");
        }
        if (!File.Exists(filePath)) return;
        SeedGenericCourse(context, filePath, "صيانة المحركات", "AAASCE", new DateTime(2025, 9, 28), "1 أسبوع");
    }

    private static void SeedOilAccountingCourse(AppDbContext context)
    {
        var filePath = GetFilePath("28المحاسبة في شركات البترول(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        SeedGenericCourse(context, filePath, "المحاسبة في شركات البترول", "Tamkeen Academy", new DateTime(2025, 9, 14), "4 أشهر");
    }

    private static void SeedOilProductSalesCourse(AppDbContext context)
    {
        var filePath = GetFilePath("29إحتساب_أرصدة_المنتجات_و_المبيعات_النفطية(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        SeedGenericCourse(context, filePath, "إحتساب أرصدة المنتجات و المبيعات النفطية", "FSC", new DateTime(2025, 9, 1), "16 أسبوع");
    }

    private static void SeedGenericCourse(AppDbContext context, string filePath, string title, string provider, DateTime startDate, string duration)
    {
        var course = context.Courses.FirstOrDefault(c => c.Title == title);
        if (course == null)
        {
            course = new Course
            {
                Title = title,
                Provider = provider,
                StartDate = startDate,
                EndDate = duration.Contains("52") ? startDate.AddDays(364) : 
                          duration.Contains("26") ? startDate.AddDays(182) : 
                          duration.Contains("16") ? startDate.AddDays(112) :
                          duration.Contains("13") ? startDate.AddDays(91) :
                          startDate.AddDays(70), 
                Duration = duration,
                CourseType = CourseType.WeeklyPlan2021,
                TargetAudience = TargetAudience.SpecificEmployees,
                Type = "External"
            };
            context.Courses.Add(course);
        }
        else
        {
            if (course.CourseType != CourseType.WeeklyPlan2021) course.CourseType = CourseType.WeeklyPlan2021;
        }
        context.SaveChanges();

        ProcessLayoutCsv(context, filePath, course.Id);
    }

    private static void SeedControlSystemsCourse(AppDbContext context)
    {
        var filePath = GetFilePath("30تشغيل_أنظمة_التحكم_في_العمليات_النفطية(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        SeedGenericCourse(context, filePath, "تشغيل أنظمة التحكم في العمليات النفطية", "شركة البريقة", new DateTime(2025, 9, 14), "13 أسبوع");
    }

    private static void SeedInspectionCourse(AppDbContext context)
    {
        var filePath = GetFilePath("31التفتيش(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        SeedGenericCourse(context, filePath, "التفتيش", "شركة البريقة", new DateTime(2025, 9, 14), "13 أسبوع");
    }

    private static void SeedHealthSafetyDiplomaCourse(AppDbContext context)
    {
        var filePath = GetFilePath("33-دبلوم في الصحة والسلامة(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        SeedGenericCourse(context, filePath, "دبلوم في الصحة والسلامة المهنية", "Arab Group", new DateTime(2025, 8, 24), "26 أسبوع");
    }

    private static void SeedHumanResources2Course(AppDbContext context)
    {
        var filePath = GetFilePath("34 -تنمية الموارد البشرية(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        SeedGenericCourse(context, filePath, "تنمية الموارد البشرية - G34", "Arab Group", new DateTime(2025, 9, 21), "16 أسبوع");
    }

    private static void SeedHumanResources3Course(AppDbContext context)
    {
        var filePath = GetFilePath("35-  B تنمية الموراد البشرية(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        SeedGenericCourse(context, filePath, "تنمية الموارد البشرية - G35", "Arab Group", new DateTime(2025, 9, 21), "26 أسبوع");
    }

    private static void SeedItProfessionalCourse(AppDbContext context)
    {
        var filePath = GetFilePath("36-It(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        SeedGenericCourse(context, filePath, "It professional certification", "EAAC", new DateTime(2025, 9, 28), "52 أسبوع");
    }

    private static void SeedOfficeArchivesCourse(AppDbContext context)
    {
        var filePath = GetFilePath("37_إدارة_المكاتب_الحديثة_و_الارشفة_الاكترونية(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        SeedGenericCourse(context, filePath, "إدارة المكاتب الحديثة و الارشفة الاكترونية", "Arab Group", new DateTime(2025, 9, 28), "13 أسبوع");
    }

    private static void SeedHumanResources4Course(AppDbContext context)
    {
        var filePath = GetFilePath("38 - تنمية الموارد البشرية(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        SeedGenericCourse(context, filePath, "تنمية الموارد البشرية - G38", "Arab Group", new DateTime(2025, 9, 28), "26 أسبوع");
    }

    private static void SeedAccountingDiplomaCourse(AppDbContext context)
    {
        var filePath = GetFilePath("39 - المحاسبة(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        SeedGenericCourse(context, filePath, "دبلوم المحاسبة - G39", "Arab Group", new DateTime(2025, 9, 21), "26 أسبوع");
    }

    private static void SeedLegalContractsCourse(AppDbContext context)
    {
        var filePath = GetFilePath("40_إدارة_العقود_و_الثوتيق_الجوانب_القانونية(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        SeedGenericCourse(context, filePath, "إدارة العقود و الثوتيق الجوانب القانونية", "EAAC", new DateTime(2025, 10, 5), "52 أسبوع");
    }

    private static void SeedElectricityElectronicsCourse(AppDbContext context)
    {
        var filePath = GetFilePath("42(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        SeedGenericCourse(context, filePath, "الكهرباء و الاكترونيات", "EAAC", new DateTime(2025, 10, 4), "Flexible");
    }

    private static void SeedSpecialCourse1Course(AppDbContext context)
    {
        var filePath = GetFilePath("43(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        SeedGenericCourse(context, filePath, "Special Course - EAAC 43", "EAAC", new DateTime(2025, 10, 6), "Flexible");
    }

    private static void SeedQualityManagementDiplomaCourse(AppDbContext context)
    {
        var filePath = GetFilePath("44(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        SeedGenericCourse(context, filePath, "دبلوم إدارة الجودة", "EAAC", new DateTime(2025, 9, 28), "52 أسبوع");
    }

    private static void SeedManagementBasicsCourse(AppDbContext context)
    {
        var filePath = GetFilePath("45(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        SeedGenericCourse(context, filePath, "أساسيات الإدارة و الاعمال", "الديار", new DateTime(2025, 10, 5), "52 أسبوع");
    }

    private static void SeedHrMasterCourse(AppDbContext context)
    {
        var filePath = GetFilePath("46(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        SeedGenericCourse(context, filePath, "ماجيستير في إدارة الموارد البشرية - G46", "Mericler", new DateTime(2025, 9, 15), "52 أسبوع");
    }

    private static void SeedHrMaster2Course(AppDbContext context)
    {
        var filePath = GetFilePath("47(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        SeedGenericCourse(context, filePath, "ماجيستير في إدارة الموارد البشرية - G47", "Mericler", new DateTime(2025, 9, 22), "52 أسبوع");
    }

    private static void SeedInternationalSafetyCertCourse(AppDbContext context)
    {
        var filePath = GetFilePath("48(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        SeedGenericCourse(context, filePath, "الشهادة الدولية في مجال السلامة المهنية", "الديار", new DateTime(2025, 11, 2), "Flexible");
    }

    private static void SeedVehicleBusinessCourse(AppDbContext context)
    {
        var filePath = GetFilePath("49(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        SeedGenericCourse(context, filePath, "اإكتشاف الاعمال بالمركبات", "الديار", new DateTime(2025, 11, 9), "4 أسابيع");
    }

    private static void SeedDigitalTransformationCourse(AppDbContext context)
    {
        var filePath = GetFilePath("50(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        SeedGenericCourse(context, filePath, "شبكات الاتصالات و التحول الرقمي", "DISC", new DateTime(2025, 9, 16), "Flexible");
    }

    private static void SeedHumanResources5Course(AppDbContext context)
    {
        var filePath = GetFilePath("51(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        SeedGenericCourse(context, filePath, "تنمية الموارد البشرية - G51", "Arab group", new DateTime(2025, 11, 2), "26 أسبوع");
    }

    private static void SeedSpecialCourse2Course(AppDbContext context)
    {
        var filePath = GetFilePath("41 1(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        SeedGenericCourse(context, filePath, "Special Course - EAAC 41", "EAAC", new DateTime(2025, 10, 6), "Flexible");
    }

    private static void SeedAccountingDiploma2Course(AppDbContext context)
    {
        var filePath = GetFilePath("52(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        SeedGenericCourse(context, filePath, "دبلوم المحاسبة - G52", "Arab group", new DateTime(2025, 11, 9), "26 أسبوع");
    }

    private static void SeedAccountingDiploma3Course(AppDbContext context)
    {
        var filePath = GetFilePath("53(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        SeedGenericCourse(context, filePath, "دبلوم المحاسبة - G53", "Arab group", new DateTime(2025, 11, 2), "26 أسبوع");
    }

    private static void SeedPublicRelationsMgmtCourse(AppDbContext context)
    {
        var filePath = GetFilePath("54(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        SeedGenericCourse(context, filePath, "إدارة العلاقات العامة وتنمية المهارات الاشرافية - G54", "Arab group", new DateTime(2025, 11, 16), "16 أسبوع");
    }

    private static void SeedPublicRelationsMgmt2Course(AppDbContext context)
    {
        var filePath = GetFilePath("55(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        SeedGenericCourse(context, filePath, "إدارة العلاقات العامة وتنمية المهارات الاشرافية - G55", "Arab group", new DateTime(2025, 11, 9), "16 أسبوع");
    }

    private static void SeedFuelQualityControlGroup2Course(AppDbContext context)
    {
        var filePath = GetFilePath("مراقبة جودة الوقود قروب 2(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        SeedGenericCourse(context, filePath, "مراقبة جودة الوقود - Group 2", "AAASCE", new DateTime(2025, 8, 10), "8 أسبوع");
    }

    private static void SeedEngineMaintenance32ACourse(AppDbContext context)
    {
        var filePath = GetFilePath("صيانة المحركات32a(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        // Seed into the main course title as well
        SeedGenericCourse(context, filePath, "صيانة المحركات", "AAASCE", new DateTime(2025, 9, 7), "4 أسبوع");
        SeedGenericCourse(context, filePath, "صيانة المحركات - 32A", "AAASCE", new DateTime(2025, 9, 7), "4 أسبوع");
    }

    private static void SeedHealthSafetyDiplomaA11Course(AppDbContext context)
    {
        var filePath = GetFilePath("دبلوم_في_الصحه_و_السلامة_المهنية_A_11(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        SeedGenericCourse(context, filePath, "دبلوم في الصحة والسلامة المهنية - A 11", "Arab Group", new DateTime(2025, 9, 9), "26 أسبوع");
    }

    private static void SeedOilSupplyMarketingA2Course(AppDbContext context)
    {
        var filePath = GetFilePath("توريد_و_تسويق_المنتجات_النفطية_A_2(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        SeedGenericCourse(context, filePath, "توريد و تسويق المنتجات النفطية - A 2", "EAAC", new DateTime(2025, 9, 28), "13 أسبوع");
    }

    private static void SeedControlSystems20ACourse(AppDbContext context)
    {
        var filePath = GetFilePath("تشغيل_أنظمة_التحكم_في_العمليات_النفطية_20A(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        SeedGenericCourse(context, filePath, "تشغيل أنظمة التحكم في العمليات النفطية - 20A", "Brega", new DateTime(2025, 7, 20), "11 أسبوع");
    }

    private static void SeedIndustrialSecurityAdvancedCourse(AppDbContext context)
    {
        var filePath = GetFilePath("الشهادة_المتقدمه_في_الامن_الصناعي(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        SeedGenericCourse(context, filePath, "الشهادة المتقدمه في الامن الصناعي", "Tamkeen", new DateTime(2025, 9, 14), "13 أسبوع");
    }

    private static void SeedIndustrialSecurityAdvancedA4Course(AppDbContext context)
    {
        var filePath = GetFilePath("الشهادة_المتقدمة_في_لامن_الصناعي_A_4(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        SeedGenericCourse(context, filePath, "الشهادة المتقدمة في لامن الصناعي - A 4", "Tamkeen", new DateTime(2025, 9, 28), "13 أسبوع");
    }

    private static void SeedProfessionalMarketing3Course(AppDbContext context)
    {
        var filePath = GetFilePath("التسويق الاحترافي 3(ورقة1).csv");
        if (!File.Exists(filePath)) return;
        SeedGenericCourse(context, filePath, "التسويق الاحترافي - 3", "Arab Group", new DateTime(2025, 10, 5), "13 أسبوع");
    }
    private static string GetFilePath(string fileName)
    {
        var workspacePath = Path.Combine(@"c:\Users\Shahed\Documents\dev\TrainingSystem", fileName);
        if (File.Exists(workspacePath)) return workspacePath;

        var downloadsPath = Path.Combine(@"c:\Users\Shahed\Downloads", fileName);
        if (File.Exists(downloadsPath)) return downloadsPath;

        return workspacePath; // Default
    }

    private static void SeedShortTermPlanFromCsv(AppDbContext context)
    {
        var filePath = GetFilePath("اسبوعان اسم و رقم (4) 1(ورقة1).csv");
        if (!File.Exists(filePath)) 
        {
            filePath = GetFilePath("اسبوعان اسم و رقم (4)(ورقة1).csv");
        }
        if (!File.Exists(filePath)) return;

        var courseTitle = "خطة التدريب اسبوعين";
        var course = context.Courses.FirstOrDefault(c => c.Title == courseTitle);
        if (course == null)
        {
            course = new Course
            {
                Title = courseTitle,
                Provider = "بيت الإدارة العربي",
                StartDate = new DateTime(2025, 12, 21),
                EndDate = new DateTime(2026, 1, 4),
                Duration = "أسبوعان",
                CourseType = CourseType.ShortTermPlan,
                TargetAudience = TargetAudience.SpecificEmployees,
                Type = "Internal"
            };
            context.Courses.Add(course);
            context.SaveChanges();
        }

        var dates = new List<DateTime>
        {
            new DateTime(2025, 12, 21), new DateTime(2025, 12, 22), new DateTime(2025, 12, 23), new DateTime(2025, 12, 24), new DateTime(2025, 12, 25),
            new DateTime(2025, 12, 28), new DateTime(2025, 12, 29), new DateTime(2025, 12, 30), new DateTime(2025, 12, 31), new DateTime(2026, 1, 1)
        };

        var encoding = System.Text.Encoding.GetEncoding("windows-1256");
        var lines = File.ReadAllLines(filePath, encoding);
        
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var trimmed = line.Trim();

            // Extract Employment Number (5 or 6 digits)
            var empMatch = Regex.Match(trimmed, @"(\d{5,6})");
            if (!empMatch.Success) continue;
            var empNum = empMatch.Value;

            // Extract Name (everything before the number)
            var namePart = trimmed.Split(new[] { empNum }, StringSplitOptions.None)[0].Trim();
            if (string.IsNullOrEmpty(namePart) || namePart.Contains("الاسم")) continue;

            var employee = context.Employees.FirstOrDefault(e => e.EmploymentNumber == empNum);
            if (employee == null)
            {
                employee = new Employee { FullName = namePart, EmploymentNumber = empNum, JoinedDate = DateTime.UtcNow };
                context.Employees.Add(employee);
                context.SaveChanges();
            }

            // Enhanced Country Detection
            string? country = null;
            if (trimmed.Contains("تركيا")) country = "تركيا";
            else if (trimmed.Contains("مصر")) country = "مصر";
            else if (trimmed.Contains("تونس")) country = "تونس";
            
            string? duration = null;
            if (country != null)
            {
                duration = "9 أيام"; // Default for country
                var durationMatch = Regex.Match(trimmed, @"(\d{1,2})\s*(أيام|يوم)");
                if (durationMatch.Success)
                {
                    duration = durationMatch.Value;
                }
            }

            // Ensure Enrollment
            var enrollment = context.Enrollments.FirstOrDefault(e => e.CourseId == course.Id && e.EmployeeId == employee.Id);
            if (enrollment == null)
            {
                enrollment = new Enrollment 
                { 
                    CourseId = course.Id, 
                    EmployeeId = employee.Id, 
                    EnrolledAt = DateTime.UtcNow,
                    Country = country,
                    Duration = duration
                };
                context.Enrollments.Add(enrollment);
            }
            else
            {
                enrollment.Duration = duration;
                enrollment.Country = country;
            }
            context.SaveChanges();

            // Attendance Parsing
            var afterNum = trimmed.Split(new[] { empNum }, StringSplitOptions.None)[1].Trim();
            var statusMarkers = new[] { "حاضر", "حضور", "غياب", "عطلة", @"لم\s*يصل", "عذر", "إجازة", "مريض", "تركيا", "مصر", "تونس" };
            var statusRegex = new Regex("(" + string.Join("|", statusMarkers) + ")");
            var statusMatches = statusRegex.Matches(afterNum);

            int dateIdx = 0;
            foreach (Match statusMatch in statusMatches)
            {
                var token = statusMatch.Value;
                if (token == "تركيا" || token == "مصر" || token == "تونس") continue; // Not a status marker
                if (dateIdx >= dates.Count) break;

                AttendanceStatus? status = null;
                if (token == "حاضر" || token == "حضور") status = AttendanceStatus.Present;
                else if (token == "غياب") status = AttendanceStatus.Absent;
                else if (token == "عطلة") status = AttendanceStatus.Holiday;
                else if (Regex.IsMatch(token, @"لم\s*يصل")) status = AttendanceStatus.NotArrived;
                else if (token == "عذر" || token == "إجازة" || token == "مريض") status = AttendanceStatus.Excused;

                if (status.HasValue)
                {
                    var date = dates[dateIdx];
                    if (!context.AttendanceRecords.Any(a => a.CourseId == course.Id && a.EmployeeId == employee.Id && a.Date == date))
                    {
                        context.AttendanceRecords.Add(new AttendanceRecord
                        {
                            CourseId = course.Id,
                            EmployeeId = employee.Id,
                            Date = date,
                            Status = status,
                            DayNumber = dateIdx + 1,
                            Notes = (status == AttendanceStatus.NotArrived || status == AttendanceStatus.Holiday) ? token : null
                        });
                    }
                    dateIdx++;
                }
            }
        }
        context.SaveChanges();
    }
}
