using ContosoUniversity.Data;
using ContosoUniversity.Infrastructure.TagHelpers;
using ContosoUniversity.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ContosoUniversity.Pages.Courses;

public class Create : PageModel
{
    private readonly IMediator _mediator;

    public Create(IMediator mediator) => _mediator = mediator;

    [BindProperty]
    public required Command Data { get; set; }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            await _mediator.Send(Data, cancellationToken);
            return RedirectToPage(nameof(System.Index));
        }

        return Page();
    }

    public record Command : IRequest<int>
    {
        public int Number { get; init; }
        public string? Title { get; init; }
        public int Credits { get; init; }
        [SelectListFor<Department>]
        public int DepartmentId { get; init; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(m => m.Title).NotNull().Length(3, 50);
            RuleFor(m => m.Credits).NotNull().InclusiveBetween(0, 5);
        }
    }

    public class CommandHandler : IRequestHandler<Command, int>
    {
        private readonly SchoolContext _db;

        public CommandHandler(SchoolContext db) => _db = db;

        public async Task<int> Handle(Command message, CancellationToken token)
        {
            var course = new Course
            {
                Id = message.Number,
                Credits = message.Credits,
                DepartmentId = message.DepartmentId,
                Title = message.Title
            };

            await _db.Courses.AddAsync(course, token);

            await _db.SaveChangesAsync(token);

            return course.Id;
        }
    }
}