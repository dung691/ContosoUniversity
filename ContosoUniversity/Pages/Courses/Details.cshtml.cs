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

    public required Model Data { get; set; }

    public async Task<IActionResult> OnGetAsync(Query query, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return NotFound();
        var course = await _mediator.Send(query, cancellationToken);
        if (course is null) return NotFound();

        Data = course;

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
            .ForMember(d => d.DepartmentName, 
                opt => opt.MapFrom(s => s.Department == null ? null : s.Department.Name));
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
                .Where(c => c.Id == message.Id)
                .ProjectTo<Model?>(_configuration)
                .SingleOrDefaultAsync(token);
    }
}