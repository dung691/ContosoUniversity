using System.ComponentModel.DataAnnotations;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Pages.Students;

public class Details : PageModel
{
    private readonly IMediator _mediator;

    public Details(IMediator mediator) => _mediator = mediator;

    public required Model Data { get; set; }

    public async Task<IActionResult> OnGetAsync(Query query, CancellationToken cancellationToken)
    {
        var student = await _mediator.Send(query, cancellationToken);
        if (student is null) return NotFound();

        Data = student;

        return Page();
    }

    public record Query : IRequest<Model?>
    {
        public int Id { get; init; }
    }

    public record Model
    {
        public int Id { get; init; }
        [Display(Name = "First Name")]
        public string FirstMidName { get; init; }
        public string LastName { get; init; }
        public DateOnly EnrollmentDate { get; init; }
        public List<Enrollment> Enrollments { get; init; } = new();

        public record Enrollment
        {
            public string CourseTitle { get; init; }
            public Grade? Grade { get; init; }
        }
    }

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateProjection<Student, Model>();
            CreateProjection<Enrollment, Model.Enrollment>();
        }
    }

    public class QueryHandler : IRequestHandler<Query, Model?>
    {
        private readonly SchoolContext _db;
        private readonly AutoMapper.IConfigurationProvider _configuration;

        public QueryHandler(SchoolContext db, AutoMapper.IConfigurationProvider configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public Task<Model?> Handle(Query message, CancellationToken token) => _db
            .Students
            .Where(s => s.Id == message.Id)
            .ProjectTo<Model?>(_configuration)
            .SingleOrDefaultAsync(token);
    }
}