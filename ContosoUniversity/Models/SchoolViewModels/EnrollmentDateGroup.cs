using System.ComponentModel.DataAnnotations;

namespace ContosoUniversity.Models.SchoolViewModels;

public record EnrollmentDateGroup
{
    [DataType(DataType.Date)]
    public DateOnly? EnrollmentDate { get; init; }

    public int StudentCount { get; init; }
}