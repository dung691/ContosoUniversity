using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoUniversity.Models;

public class Student
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = default!;

    [Required]
    [StringLength(50)]
    [Column("FirstName")]
    [Display(Name = "First Name")]
    public string FirstMidName { get; set; } = default!;
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Display(Name = "Enrollment Date")]
    public DateOnly EnrollmentDate { get; set; }
    [Display(Name = "Full Name")]
    public string FullName => LastName + ", " + FirstMidName;

    public List<Enrollment> Enrollments { get; set; } = new();
}