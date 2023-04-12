using System.ComponentModel;
using AutoMapper;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ContosoUniversity.Pages.Courses;

public class Index : PageModel
{
    private readonly IMediator _mediator;

    public Index(IMediator mediator) => _mediator = mediator;

    public Result Data { get; private set; } = default!;

    public async Task OnGetAsync() => Data = await _mediator.Send(new Query());

    public record Query : IRequest<Result>;

    public record Result
    {
        public List<Course> Courses { get; init; } = default!;

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
            .ForMember(x => x.DepartmentName, o => o.MapFrom(r => r.Department.Name));
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
                .ProjectToListAsync<Result.Course>(_configuration);

            return new Result
            {
                Courses = courses
            };
        }
    }
}