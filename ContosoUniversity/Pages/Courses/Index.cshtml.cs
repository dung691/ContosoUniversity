using System.ComponentModel;
using AutoMapper;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Pages.Courses;

public class Index : PageModel
{
    private readonly IMediator _mediator;

    public Index(IMediator mediator) => _mediator = mediator;

    public required Result Data { get; set; }

    public async Task OnGetAsync(CancellationToken cancellationToken) => Data = await _mediator.Send(new Query(), cancellationToken);

    public record Query : IRequest<Result>;

    public record Result
    {
        public List<Course> Courses { get; init; } = new();

        public record Course
        {
            public int Id { get; init; }
            public string? Title { get; init; }
            public int Credits { get; init; }
            [DisplayName("Department")]
            public string? DepartmentName { get; init; }
        }
    }

    public class MappingProfile : Profile
    {
        public MappingProfile() => CreateProjection<Course, Result.Course>()
            .ForMember(
                d => d.DepartmentName, 
                opt => opt.MapFrom(s => s.Department != null ? s.Department.Name : null));
    }

    public class QueryHandler : IRequestHandler<Query, Result>
    {
        private readonly SchoolContext _db;
        private readonly AutoMapper.IConfigurationProvider _configuration;

        public QueryHandler(SchoolContext db, AutoMapper.IConfigurationProvider configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public async Task<Result> Handle(Query message, CancellationToken token)
        {
            var courses = await _db.Courses
                .OrderBy(d => d.Id)
                .Include(d => d.Department)
                .ProjectToListAsync<Result.Course>(_configuration);

            return new Result
            {
                Courses = courses
            };
        }
    }
}