using ContosoUniversity.Pages.Instructors;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoUniversity.Models;

public class Instructor
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Last Name")]
    [StringLength(50)]
    public string LastName { get; set; }

    [Required]
    [Column("FirstName")]
    [Display(Name = "First Name")]
    [StringLength(50)]
    public string FirstMidName { get; set; }

    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Display(Name = "Hire Date")]
    public DateOnly HireDate { get; set; }

    [Display(Name = "Full Name")]
    public string FullName => LastName + ", " + FirstMidName;

    public List<Course> Courses { get; set; } = new();
    public OfficeAssignment? OfficeAssignment { get; set; }

    public void Handle(CreateEdit.Command message,
        IEnumerable<Course> courses)
    {
        UpdateDetails(message);

        UpdateInstructorCourses(message.SelectedCourses, courses);
    }

    public void Handle(Delete.Command message) => OfficeAssignment = null;

    private void UpdateDetails(CreateEdit.Command message)
    {
        FirstMidName = message.FirstMidName;
        LastName = message.LastName;
        HireDate = message.HireDate.GetValueOrDefault();

        if (string.IsNullOrWhiteSpace(message.OfficeAssignmentLocation))
        {
            OfficeAssignment = null;
        }
        else if (OfficeAssignment == null)
        {
            OfficeAssignment = new OfficeAssignment
            {
                Location = message.OfficeAssignmentLocation
            };
        }
        else
        {
            OfficeAssignment.Location = message.OfficeAssignmentLocation;
        }
    }

    private void UpdateInstructorCourses(string[]? selectedCourses, IEnumerable<Course> courses)
    {
        if (selectedCourses == null)
        {
            return;
        }

        var selectedCoursesHs = new HashSet<string>(selectedCourses);
        var instructorCourses = new HashSet<int>(Courses.Select(c => c.Id));

        foreach (var course in courses)
        {
            if (selectedCoursesHs.Contains(course.Id.ToString()))
            {
                if (!instructorCourses.Contains(course.Id))
                {
                    Courses.Add(course);
                }
            }
            else
            {
                if (instructorCourses.Contains(course.Id))
                {
                    var toRemove = Courses.Single(ci => ci.Id == course.Id);
                    Courses.Remove(toRemove);
                }
            }
        }
    }
}