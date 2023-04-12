using AutoMapper;
using ContosoUniversity.Data;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using AutoMapper.QueryableExtensions;
using ContosoUniversity.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Pages.Courses;

public class Details : PageModel
{
    private readonly IMediator _mediator;

    public Details(IMediator mediator) => _mediator = mediator;

    public Model? Data { get; private set; }

    public async Task<IActionResult> OnGetAsync(Query query, CancellationToken cancellationToken)
    {
        if (!query.Id.HasValue) return NotFound();
        Data = await _mediator.Send(query, cancellationToken);
        if (Data is null) return NotFound();

        return Page();
    }

    public record Query : IRequest<Model?>
    {
        public int? Id { get; init; }
    }

    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(m => m.Id).NotNull();
        }
    }

    public record Model
    {
        public int Id { get; init; }
        public string? Title { get; init; }
        public int Credits { get; init; }
        [Display(Name = "Department")]
        public string? DepartmentName { get; init; }
    }

    public class MappingProfile : Profile
    {
        public MappingProfile() => CreateProjection<Course, Model>()
            .ForMember(x => x.DepartmentName, o => o.MapFrom(r => r.Department.Name));
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

        public Task<Model?> Handle(Query message, CancellationToken token) =>
            _db.Courses
                .Where(i => i.Id == message.Id)
                .ProjectTo<Model?>(_configuration)
                .SingleOrDefaultAsync(token);
    }
}