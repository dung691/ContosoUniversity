using ContosoUniversity.Models;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Data;

public static class SchoolContextSeed
{
    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SchoolContext>();
        await dbContext.Database.MigrateAsync();

        await SeedAsync(dbContext);
    }

    private static async Task SeedAsync(SchoolContext context)
    {
        //context.Database.EnsureCreated();

        // Look for any GetPreconfiguredStudents().
        if (context.Students.Any())
        {
            return; // DB has been seeded
        }

        var students = new Student[]
        {
            new()
            {
                FirstMidName = "Carson", LastName = "Alexander",
                EnrollmentDate = DateOnly.Parse("2010-09-01")
            },
            new()
            {
                FirstMidName = "Meredith", LastName = "Alonso",
                EnrollmentDate = DateOnly.Parse("2012-09-01")
            },
            new()
            {
                FirstMidName = "Arturo", LastName = "Anand",
                EnrollmentDate = DateOnly.Parse("2013-09-01")
            },
            new()
            {
                FirstMidName = "Gytis", LastName = "Barzdukas",
                EnrollmentDate = DateOnly.Parse("2012-09-01")
            },
            new()
            {
                FirstMidName = "Yan", LastName = "Li",
                EnrollmentDate = DateOnly.Parse("2012-09-01")
            },
            new()
            {
                FirstMidName = "Peggy", LastName = "Justice",
                EnrollmentDate = DateOnly.Parse("2011-09-01")
            },
            new()
            {
                FirstMidName = "Laura", LastName = "Norman",
                EnrollmentDate = DateOnly.Parse("2013-09-01")
            },
            new()
            {
                FirstMidName = "Nino", LastName = "Olivetto",
                EnrollmentDate = DateOnly.Parse("2005-09-01")
            }
        };

        foreach (Student s in students)
        {
            context.Students.Add(s);
        }

        await context.SaveChangesAsync();

        var instructors = new Instructor[]
        {
            new()
            {
                FirstMidName = "Kim", LastName = "Abercrombie",
                HireDate = DateOnly.Parse("1995-03-11")
            },
            new()
            {
                FirstMidName = "Fadi", LastName = "Fakhouri",
                HireDate = DateOnly.Parse("2002-07-06")
            },
            new()
            {
                FirstMidName = "Roger", LastName = "Harui",
                HireDate = DateOnly.Parse("1998-07-01")
            },
            new()
            {
                FirstMidName = "Candace", LastName = "Kapoor",
                HireDate = DateOnly.Parse("2001-01-15")
            },
            new()
            {
                FirstMidName = "Roger", LastName = "Zheng",
                HireDate = DateOnly.Parse("2004-02-12")
            }
        };

        foreach (Instructor i in instructors)
        {
            context.Instructors.Add(i);
        }

        await context.SaveChangesAsync();

        var departments = new Department[]
        {
            new()
            {
                Name = "English", Budget = 350000,
                StartDate = DateOnly.Parse("2007-09-01"),
                InstructorId = instructors.Single(i => i.LastName == "Abercrombie").Id
            },
            new()
            {
                Name = "Mathematics", Budget = 100000,
                StartDate = DateOnly.Parse("2007-09-01"),
                InstructorId = instructors.Single(i => i.LastName == "Fakhouri").Id
            },
            new()
            {
                Name = "Engineering", Budget = 350000,
                StartDate = DateOnly.Parse("2007-09-01"),
                InstructorId = instructors.Single(i => i.LastName == "Harui").Id
            },
            new()
            {
                Name = "Economics", Budget = 100000,
                StartDate = DateOnly.Parse("2007-09-01"),
                InstructorId = instructors.Single(i => i.LastName == "Kapoor").Id
            }
        };

        foreach (Department d in departments)
        {
            context.Departments.Add(d);
        }

        await context.SaveChangesAsync();

        var courses = new Course[]
        {
            new()
            {
                Id = 1050, Title = "Chemistry", Credits = 3,
                DepartmentId = departments.Single(s => s.Name == "Engineering").Id
            },
            new()
            {
                Id = 4022, Title = "Microeconomics", Credits = 3,
                DepartmentId = departments.Single(s => s.Name == "Economics").Id
            },
            new()
            {
                Id = 4041, Title = "Macroeconomics", Credits = 3,
                DepartmentId = departments.Single(s => s.Name == "Economics").Id
            },
            new()
            {
                Id = 1045, Title = "Calculus", Credits = 4,
                DepartmentId = departments.Single(s => s.Name == "Mathematics").Id
            },
            new()
            {
                Id = 3141, Title = "Trigonometry", Credits = 4,
                DepartmentId = departments.Single(s => s.Name == "Mathematics").Id
            },
            new()
            {
                Id = 2021, Title = "Composition", Credits = 3,
                DepartmentId = departments.Single(s => s.Name == "English").Id
            },
            new()
            {
                Id = 2042, Title = "Literature", Credits = 4,
                DepartmentId = departments.Single(s => s.Name == "English").Id
            },
        };

        foreach (Course c in courses)
        {
            context.Courses.Add(c);
        }

        await context.SaveChangesAsync();

        var officeAssignments = new OfficeAssignment[]
        {
            new()
            {
                InstructorId = instructors.Single(i => i.LastName == "Fakhouri").Id,
                Location = "Smith 17"
            },
            new()
            {
                InstructorId = instructors.Single(i => i.LastName == "Harui").Id,
                Location = "Gowan 27"
            },
            new()
            {
                InstructorId = instructors.Single(i => i.LastName == "Kapoor").Id,
                Location = "Thompson 304"
            },
        };

        foreach (OfficeAssignment o in officeAssignments)
        {
            context.OfficeAssignments.Add(o);
        }

        await context.SaveChangesAsync();

        var chemistryCourse = courses.Single(c => c.Title == "Chemistry");
        chemistryCourse.Instructors.Add(instructors.Single(i => i.LastName == "Kapoor"));
        chemistryCourse.Instructors.Add(instructors.Single(i => i.LastName == "Harui"));

        var microeconomicsCourse = courses.Single(c => c.Title == "Microeconomics");
        microeconomicsCourse.Instructors.Add(instructors.Single(i => i.LastName == "Zheng"));

        var calculusCourse = courses.Single(c => c.Title == "Calculus");
        calculusCourse.Instructors.Add(instructors.Single(i => i.LastName == "Fakhouri"));

        var trigonometryCourse = courses.Single(c => c.Title == "Trigonometry");
        trigonometryCourse.Instructors.Add(instructors.Single(i => i.LastName == "Harui"));

        var compositionCourse = courses.Single(c => c.Title == "Composition");
        compositionCourse.Instructors.Add(instructors.Single(i => i.LastName == "Abercrombie"));

        var literatureCourse = courses.Single(c => c.Title == "Literature");
        literatureCourse.Instructors.Add(instructors.Single(i => i.LastName == "Abercrombie"));

        await context.SaveChangesAsync();

        var enrollments = new Enrollment[]
        {
            new()
            {
                StudentId = students.Single(s => s.LastName == "Alexander").Id,
                CourseId = courses.Single(c => c.Title == "Chemistry").Id,
                Grade = Grade.A
            },
            new()
            {
                StudentId = students.Single(s => s.LastName == "Alexander").Id,
                CourseId = courses.Single(c => c.Title == "Microeconomics").Id,
                Grade = Grade.C
            },
            new()
            {
                StudentId = students.Single(s => s.LastName == "Alexander").Id,
                CourseId = courses.Single(c => c.Title == "Macroeconomics").Id,
                Grade = Grade.B
            },
            new()
            {
                StudentId = students.Single(s => s.LastName == "Alonso").Id,
                CourseId = courses.Single(c => c.Title == "Calculus").Id,
                Grade = Grade.B
            },
            new()
            {
                StudentId = students.Single(s => s.LastName == "Alonso").Id,
                CourseId = courses.Single(c => c.Title == "Trigonometry").Id,
                Grade = Grade.B
            },
            new()
            {
                StudentId = students.Single(s => s.LastName == "Alonso").Id,
                CourseId = courses.Single(c => c.Title == "Composition").Id,
                Grade = Grade.B
            },
            new()
            {
                StudentId = students.Single(s => s.LastName == "Anand").Id,
                CourseId = courses.Single(c => c.Title == "Chemistry").Id
            },
            new()
            {
                StudentId = students.Single(s => s.LastName == "Anand").Id,
                CourseId = courses.Single(c => c.Title == "Microeconomics").Id,
                Grade = Grade.B
            },
            new()
            {
                StudentId = students.Single(s => s.LastName == "Barzdukas").Id,
                CourseId = courses.Single(c => c.Title == "Chemistry").Id,
                Grade = Grade.B
            },
            new()
            {
                StudentId = students.Single(s => s.LastName == "Li").Id,
                CourseId = courses.Single(c => c.Title == "Composition").Id,
                Grade = Grade.B
            },
            new()
            {
                StudentId = students.Single(s => s.LastName == "Justice").Id,
                CourseId = courses.Single(c => c.Title == "Literature").Id,
                Grade = Grade.B
            }
        };

        foreach (Enrollment e in enrollments)
        {
            var enrollmentInDataBase = context.Enrollments
                .SingleOrDefault(s => s.StudentId == e.StudentId && s.CourseId == e.CourseId);
            if (enrollmentInDataBase == null)
            {
                context.Enrollments.Add(e);
            }
        }

        await context.SaveChangesAsync();
    }
}