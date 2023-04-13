using ContosoUniversity.Models;
using ContosoUniversity.Pages.Instructors;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace ContosoUniversity.IntegrationTests.Pages.Instructors;

[Collection(nameof(SliceFixture))]
public class DeleteTests
{
    private readonly SliceFixture _fixture;

    public DeleteTests(SliceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Should_query_for_command()
    {
        var englishDept = new Department
        {
            Name = "English",
            StartDate = DateOnly.FromDateTime(DateTime.Today)
        };
        var english101 = new Course
        {
            Department = englishDept,
            Title = "English 101",
            Credits = 4,
            Id = _fixture.NextCourseNumber()
        };
        var command = new CreateEdit.Command
        {
            FirstMidName = "George",
            LastName = "Costanza",
            OfficeAssignmentLocation = "Austin",
            HireDate = DateOnly.FromDateTime(DateTime.Today),
            SelectedCourses = new []{ english101.Id.ToString()}
        };
        var instructorId = await _fixture.SendAsync(command);

        await _fixture.InsertAsync(englishDept, english101);

        var result = await _fixture.SendAsync(new Delete.Query { Id = instructorId });

        result.ShouldNotBeNull();
        result.FirstMidName.ShouldBe(command.FirstMidName);
        result.OfficeAssignmentLocation.ShouldBe(command.OfficeAssignmentLocation);
    }

    [Fact]
    public async Task Should_delete_instructor()
    {
        var instructorId = await _fixture.SendAsync(new CreateEdit.Command
        {
            FirstMidName = "George",
            LastName = "Costanza",
            OfficeAssignmentLocation = "Austin",
            HireDate = DateOnly.FromDateTime(DateTime.Today)
        });
        var englishDept = new Department
        {
            Name = "English",
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            InstructorId = instructorId
        };
        var english101 = new Course
        {
            Department = englishDept,
            Title = "English 101",
            Credits = 4,
            Id = _fixture.NextCourseNumber()
        };

        await _fixture.InsertAsync(englishDept, english101);

        await _fixture.SendAsync(new CreateEdit.Command
        {
            Id = instructorId,
            FirstMidName = "George",
            LastName = "Costanza",
            OfficeAssignmentLocation = "Austin",
            HireDate = DateOnly.FromDateTime(DateTime.Today),
            SelectedCourses = new[] { english101.Id.ToString() }
        });

        await _fixture.SendAsync(new Delete.Command { Id = instructorId });

        var instructorCount = await _fixture.ExecuteDbContextAsync(db => db.Instructors.Where(i => i.Id == instructorId).CountAsync());

        instructorCount.ShouldBe(0);

        var englishDeptId = englishDept.Id;
        englishDept = await _fixture.ExecuteDbContextAsync(db => db.Departments.FindAsync(englishDeptId));

        englishDept!.InstructorId.ShouldBeNull();

        var courseInstructorCount = await _fixture.ExecuteDbContextAsync(db => db.Courses.Where(ci => ci.Id == english101.Id)
            .Include(ci => ci.Instructors)
            .Where(r => r.Instructors.Any(x => x.Id == instructorId))
            .CountAsync());

        courseInstructorCount.ShouldBe(0);
    }

}