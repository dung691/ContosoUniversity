using AutoMapper;
using ContosoUniversity.Data;
using ContosoUniversity.Infrastructure.TagHelpers;
using ContosoUniversity.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Pages.Courses;

public class Edit : PageModel
{
    private readonly IMediator _mediator;

    [BindProperty]
    public required Command Data { get; set; }

    public Edit(IMediator mediator) => _mediator = mediator;

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
            var updatedId = await _mediator.Send(Data, cancellationToken);
            if (updatedId is null) return NotFound();
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

    public record Command : IRequest<int?>
    {
        [Display(Name = "Number")]
        public int Id { get; init; }
        public string? Title { get; init; }
        public int? Credits { get; init; }

        [SelectListFor<Department>, DisplayName("Department")]
        public int DepartmentId { get; init; }
    }

    public class MappingProfile : Profile
    {
        public MappingProfile() => CreateProjection<Course, Command>();
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(m => m.Title).NotNull().Length(3, 50);
            RuleFor(m => m.Credits).NotNull().InclusiveBetween(0, 5);
        }
    }

    public class CommandHandler : IRequestHandler<Command, int?>
    {
        private readonly SchoolContext _db;

        public CommandHandler(SchoolContext db) => _db = db;

        public async Task<int?> Handle(Command request, CancellationToken cancellationToken)
        {
            var course = await _db.Courses.FindAsync(new object?[]{ request.Id }, cancellationToken);

            if (course is null) return default;

            course.Title = request.Title;
            course.DepartmentId = request.DepartmentId;
            course.Credits = request.Credits!.Value;

            return course.Id;
        }
    }
}