using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoUniversity.Models;

public class Department
{
    public int Id { get; set; }

    [StringLength(50, MinimumLength = 3)]
    public string? Name { get; set; }

    [DataType(DataType.Currency)]
    [Column(TypeName = "money")]
    public decimal Budget { get; set; }

    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Display(Name = "Start Date")]
    public DateOnly StartDate { get; set; }

    public int? InstructorId { get; set; }

    public Instructor Administrator { get; set; }
    public List<Course> Courses { get; set; } = new();
}