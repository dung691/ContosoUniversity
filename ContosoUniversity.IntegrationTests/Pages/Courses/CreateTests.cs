using ContosoUniversity.IntegrationTests.Helpers;
using ContosoUniversity.Models;
using ContosoUniversity.Pages.Courses;
using ContosoUniversity.Pages.Instructors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Shouldly;
using Xunit;

namespace ContosoUniversity.IntegrationTests.Pages.Courses;

[Collection(nameof(SliceFixture))]
public class CreateTests
{
    private readonly SliceFixture _fixture;
    private readonly HttpClient _client;

    public CreateTests(SliceFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.Factory.CreateClient();
    }

    [Fact]
    public async Task Should_create_new_course()
    {
        var adminId = await _fixture.SendAsync(new CreateEdit.Command
        {
            FirstMidName = "George",
            LastName = "Costanza",
            HireDate = DateOnly.FromDateTime(DateTime.Today)
        });

        var dept = new Department
        {
            Name = "History",
            InstructorId = adminId,
            Budget = 123m,
            StartDate = DateOnly.FromDateTime(DateTime.Today)
        };

        await _fixture.InsertAsync(dept);

        Create.Command command = default!;

        await _fixture.ExecuteDbContextAsync(async (context, mediator) =>
        {
            command = new Create.Command
            {
                Credits = 4,
                DepartmentId = dept.Id,
                Number = _fixture.NextCourseNumber(),
                Title = "English 101"
            };
            await mediator.Send(command);
        });

        var created = await _fixture.ExecuteDbContextAsync(db => db.Courses.Where(c => c.Id == command.Number).SingleOrDefaultAsync());

        created.ShouldNotBeNull();
        created.DepartmentId.ShouldBe(dept.Id);
        created.Credits.ShouldBe(command.Credits);
        created.Title.ShouldBe(command.Title);
    }

    [Fact]
    public async Task Create_WhenCalled_ReturnsCreateForm()
    {
        var response = await _client.GetAsync("/Courses/Create");

        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();

        Assert.Contains("<h2>Create</h2>", responseString);
    }

    [Fact]
    public async Task Create_SentWrongModel_ReturnsViewWithErrorMessages()
    {
        var adminId = await _fixture.SendAsync(new CreateEdit.Command
        {
            FirstMidName = "George",
            LastName = "Costanza",
            HireDate = DateOnly.FromDateTime(DateTime.Today)
        });

        var dept = new Department
        {
            Name = "History",
            InstructorId = adminId,
            Budget = 123m,
            StartDate = DateOnly.FromDateTime(DateTime.Today)
        };

        await _fixture.InsertAsync(dept);

        var initResponse = await _client.GetAsync("/Courses/Create");
        var (antiForgeryFieldValue, antiForgeryCookieValue) = await AntiForgeryTokenExtractor.ExtractAntiForgeryValues(initResponse);

        var postRequest = new HttpRequestMessage(HttpMethod.Post, "/Courses/Create");
        postRequest.Headers.Add("Cookie", new CookieHeaderValue(AntiForgeryTokenExtractor.AntiForgeryCookieName, antiForgeryCookieValue).ToString());

        var formModel = new Dictionary<string, string>
        {
            { AntiForgeryTokenExtractor.AntiForgeryFieldName, antiForgeryFieldValue },
            { "Data.Credits", "4" },
            { "Data.DepartmentId", dept.Id.ToString() },
            { "Data.Number", _fixture.NextCourseNumber().ToString() }
        };

        postRequest.Content = new FormUrlEncodedContent(formModel);

        var response = await _client.SendAsync(postRequest);

        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();

        Assert.Contains("<span class=\"text-danger field-validation-error\" data-valmsg-for=\"Data.Title\" data-valmsg-replace=\"true\">&#x27;Title&#x27; must not be empty.</span>", responseString);
    }


    /// <summary>
    /// <see href="https://github.com/CodeMazeBlog/testing-aspnetcore-mvc/blob/testing-anti_forgery_token_mvc/EmployeesApp/EmployeesApp.IntegrationTests/EmployeesControllerIntegrationTests.cs#L68"/>
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task Create_WhenPOSTExecuted_ReturnsToIndexView()
    {
        var adminId = await _fixture.SendAsync(new CreateEdit.Command
        {
            FirstMidName = "George",
            LastName = "Costanza",
            HireDate = DateOnly.FromDateTime(DateTime.Today)
        });

        var dept = new Department
        {
            Name = "History",
            InstructorId = adminId,
            Budget = 123m,
            StartDate = DateOnly.FromDateTime(DateTime.Today)
        };

        await _fixture.InsertAsync(dept);

        var initResponse = await _client.GetAsync("/Courses/Create");
        var (antiForgeryFieldValue, antiForgeryCookieValue) = await AntiForgeryTokenExtractor.ExtractAntiForgeryValues(initResponse);

        var postRequest = new HttpRequestMessage(HttpMethod.Post, "/Courses/Create");
        postRequest.Headers.Add("Cookie", new CookieHeaderValue(AntiForgeryTokenExtractor.AntiForgeryCookieName, antiForgeryCookieValue).ToString());

        var modelData = new Dictionary<string, string>
        {
            { AntiForgeryTokenExtractor.AntiForgeryFieldName, antiForgeryFieldValue },
            { "Data.Credits", "4" },
            { "Data.DepartmentId", dept.Id.ToString() },
            { "Data.Number", _fixture.NextCourseNumber().ToString() },
            { "Data.Title", "English 102" }
        };

        postRequest.Content = new FormUrlEncodedContent(modelData);

        var response = await _client.SendAsync(postRequest);

        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();

        Assert.Contains("English 102", responseString);
    }
}