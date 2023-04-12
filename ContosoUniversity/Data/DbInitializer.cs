using ContosoUniversity.Models;

namespace ContosoUniversity.Data;

public static class DbInitializer
{
    public static void Initialize(SchoolContext context)
    {
        //context.Database.EnsureCreated();

        // Look for any students.
        if (context.Students.Any())
        {
            return;   // DB has been seeded
        }

        var students = new Student[]
        {
            new Student { FirstMidName = "Carson",   LastName = "Alexander",
                EnrollmentDate = DateOnly.Parse("2010-09-01") },
            new Student { FirstMidName = "Meredith", LastName = "Alonso",
                EnrollmentDate = DateOnly.Parse("2012-09-01") },
            new Student { FirstMidName = "Arturo",   LastName = "Anand",
                EnrollmentDate = DateOnly.Parse("2013-09-01") },
            new Student { FirstMidName = "Gytis",    LastName = "Barzdukas",
                EnrollmentDate = DateOnly.Parse("2012-09-01") },
            new Student { FirstMidName = "Yan",      LastName = "Li",
                EnrollmentDate = DateOnly.Parse("2012-09-01") },
            new Student { FirstMidName = "Peggy",    LastName = "Justice",
                EnrollmentDate = DateOnly.Parse("2011-09-01") },
            new Student { FirstMidName = "Laura",    LastName = "Norman",
                EnrollmentDate = DateOnly.Parse("2013-09-01") },
            new Student { FirstMidName = "Nino",     LastName = "Olivetto",
                EnrollmentDate = DateOnly.Parse("2005-09-01") }
        };

        foreach (Student s in students)
        {
            context.Students.Add(s);
        }
        context.SaveChanges();

        var instructors = new Instructor[]
        {
            new Instructor { FirstMidName = "Kim",     LastName = "Abercrombie",
                HireDate = DateOnly.Parse("1995-03-11") },
            new Instructor { FirstMidName = "Fadi",    LastName = "Fakhouri",
                HireDate = DateOnly.Parse("2002-07-06") },
            new Instructor { FirstMidName = "Roger",   LastName = "Harui",
                HireDate = DateOnly.Parse("1998-07-01") },
            new Instructor { FirstMidName = "Candace", LastName = "Kapoor",
                HireDate = DateOnly.Parse("2001-01-15") },
            new Instructor { FirstMidName = "Roger",   LastName = "Zheng",
                HireDate = DateOnly.Parse("2004-02-12") }
        };

        foreach (Instructor i in instructors)
        {
            context.Instructors.Add(i);
        }
        context.SaveChanges();

        var departments = new Department[]
        {
            new Department { Name = "English",     Budget = 350000,
                StartDate = DateOnly.Parse("2007-09-01"),
                InstructorId  = instructors.Single( i => i.LastName == "Abercrombie").Id },
            new Department { Name = "Mathematics", Budget = 100000,
                StartDate = DateOnly.Parse("2007-09-01"),
                InstructorId  = instructors.Single( i => i.LastName == "Fakhouri").Id },
            new Department { Name = "Engineering", Budget = 350000,
                StartDate = DateOnly.Parse("2007-09-01"),
                InstructorId  = instructors.Single( i => i.LastName == "Harui").Id },
            new Department { Name = "Economics",   Budget = 100000,
                StartDate = DateOnly.Parse("2007-09-01"),
                InstructorId  = instructors.Single( i => i.LastName == "Kapoor").Id }
        };

        foreach (Department d in departments)
        {
            context.Departments.Add(d);
        }
        context.SaveChanges();

        var courses = new Course[]
        {
            new Course {Id = 1050, Title = "Chemistry",      Credits = 3,
                DepartmentId = departments.Single( s => s.Name == "Engineering").Id
            },
            new Course {Id = 4022, Title = "Microeconomics", Credits = 3,
                DepartmentId = departments.Single( s => s.Name == "Economics").Id
            },
            new Course {Id = 4041, Title = "Macroeconomics", Credits = 3,
                DepartmentId = departments.Single( s => s.Name == "Economics").Id
            },
            new Course {Id = 1045, Title = "Calculus",       Credits = 4,
                DepartmentId = departments.Single( s => s.Name == "Mathematics").Id
            },
            new Course {Id = 3141, Title = "Trigonometry",   Credits = 4,
                DepartmentId = departments.Single( s => s.Name == "Mathematics").Id
            },
            new Course {Id = 2021, Title = "Composition",    Credits = 3,
                DepartmentId = departments.Single( s => s.Name == "English").Id
            },
            new Course {Id = 2042, Title = "Literature",     Credits = 4,
                DepartmentId = departments.Single( s => s.Name == "English").Id
            },
        };

        foreach (Course c in courses)
        {
            context.Courses.Add(c);
        }
        context.SaveChanges();

        var officeAssignments = new OfficeAssignment[]
        {
            new OfficeAssignment {
                InstructorId = instructors.Single( i => i.LastName == "Fakhouri").Id,
                Location = "Smith 17" },
            new OfficeAssignment {
                InstructorId = instructors.Single( i => i.LastName == "Harui").Id,
                Location = "Gowan 27" },
            new OfficeAssignment {
                InstructorId = instructors.Single( i => i.LastName == "Kapoor").Id,
                Location = "Thompson 304" },
        };

        foreach (OfficeAssignment o in officeAssignments)
        {
            context.OfficeAssignments.Add(o);
        }
        context.SaveChanges();

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

        context.SaveChanges();

        var enrollments = new Enrollment[]
        {
            new Enrollment {
                StudentId = students.Single(s => s.LastName == "Alexander").Id,
                CourseId = courses.Single(c => c.Title == "Chemistry" ).Id,
                Grade = Grade.A
            },
            new Enrollment {
                StudentId = students.Single(s => s.LastName == "Alexander").Id,
                CourseId = courses.Single(c => c.Title == "Microeconomics" ).Id,
                Grade = Grade.C
            },
            new Enrollment {
                StudentId = students.Single(s => s.LastName == "Alexander").Id,
                CourseId = courses.Single(c => c.Title == "Macroeconomics" ).Id,
                Grade = Grade.B
            },
            new Enrollment {
                StudentId = students.Single(s => s.LastName == "Alonso").Id,
                CourseId = courses.Single(c => c.Title == "Calculus" ).Id,
                Grade = Grade.B
            },
            new Enrollment {
                StudentId = students.Single(s => s.LastName == "Alonso").Id,
                CourseId = courses.Single(c => c.Title == "Trigonometry" ).Id,
                Grade = Grade.B
            },
            new Enrollment {
                StudentId = students.Single(s => s.LastName == "Alonso").Id,
                CourseId = courses.Single(c => c.Title == "Composition" ).Id,
                Grade = Grade.B
            },
            new Enrollment {
                StudentId = students.Single(s => s.LastName == "Anand").Id,
                CourseId = courses.Single(c => c.Title == "Chemistry" ).Id
            },
            new Enrollment {
                StudentId = students.Single(s => s.LastName == "Anand").Id,
                CourseId = courses.Single(c => c.Title == "Microeconomics").Id,
                Grade = Grade.B
            },
            new Enrollment {
                StudentId = students.Single(s => s.LastName == "Barzdukas").Id,
                CourseId = courses.Single(c => c.Title == "Chemistry").Id,
                Grade = Grade.B
            },
            new Enrollment {
                StudentId = students.Single(s => s.LastName == "Li").Id,
                CourseId = courses.Single(c => c.Title == "Composition").Id,
                Grade = Grade.B
            },
            new Enrollment {
                StudentId = students.Single(s => s.LastName == "Justice").Id,
                CourseId = courses.Single(c => c.Title == "Literature").Id,
                Grade = Grade.B
            }
        };

        foreach (Enrollment e in enrollments)
        {
            var enrollmentInDataBase = context.Enrollments
                .SingleOrDefault(s => s.Student.Id == e.StudentId && s.Course.Id == e.CourseId);
            if (enrollmentInDataBase == null)
            {
                context.Enrollments.Add(e);
            }
        }
        context.SaveChanges();
    }
}