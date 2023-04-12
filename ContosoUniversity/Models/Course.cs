using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoUniversity.Models;

public class Course
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Display(Name = "Number")]
    public int Id { get; set; }

    [StringLength(50, MinimumLength = 3)]
    public string? Title { get; set; }

    [Range(0, 5)]
    public int Credits { get; set; }

    public int DepartmentId { get; set; }

    public Department Department { get; set; }
    public List<Enrollment> Enrollments { get; set; } = new();
    public List<Instructor> Instructors { get; set; } = new();
}