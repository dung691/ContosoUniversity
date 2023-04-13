using AutoMapper;
using ContosoUniversity.Data;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using AutoMapper.QueryableExtensions;
using ContosoUniversity.Models;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Pages.Courses;

public class Delete : PageModel
{
    private readonly IMediator _mediator;

    public Delete(IMediator mediator) => _mediator = mediator;

    [BindProperty]
    public required Command Data { get; set; }

    public async Task<IActionResult> OnGetAsync(Query query, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return NotFound();

        var course = await _mediator.Send(query, cancellationToken);

        if (course is null) return NotFound();

        Data = course;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            await _mediator.Send(Data!, cancellationToken);
            return RedirectToPage(nameof(System.Index));
        }

        return Page();
    }

    public record Query : IRequest<Command?>
    {
        public int? Id { get; init; }
    }

    public class QueryValidator : AbstractValidator<Query>
    {
        public QueryValidator()
        {
            RuleFor(m => m.Id).NotNull();
        }
    }

    public class MappingProfile : Profile
    {
        public MappingProfile() => CreateProjection<Course, Command>()
            .ForMember(d => d.DepartmentName, 
                opt => opt.MapFrom(s => s.Department == null ? null : s.Department.Name));
    }

    public class QueryHandler : IRequestHandler<Query, Command?>
    {
        private readonly SchoolContext _db;
        private readonly AutoMapper.IConfigurationProvider _configuration;

        public QueryHandler(SchoolContext db, AutoMapper.IConfigurationProvider configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public Task<Command?> Handle(Query message, CancellationToken token) =>
            _db.Courses
                .Where(c => c.Id == message.Id)
                .ProjectTo<Command?>(_configuration)
                .SingleOrDefaultAsync(token);
    }

    public record Command : IRequest
    {
        [Display(Name = "Number")]
        public int Id { get; init; }
        public string? Title { get; init; }
        public int Credits { get; init; }
        [Display(Name = "Department")]
        public string? DepartmentName { get; init; }
    }

    public class CommandHandler : IRequestHandler<Command>
    {
        private readonly SchoolContext _db;

        public CommandHandler(SchoolContext db) => _db = db;

        public async Task Handle(Command message, CancellationToken token)
        {
            var course = await _db.Courses.FindAsync(new object?[] { message.Id }, token);
            if (course is not null)
                _db.Courses.Remove(course);
        }
    }
}