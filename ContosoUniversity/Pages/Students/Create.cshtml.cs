using System.ComponentModel.DataAnnotations;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ContosoUniversity.Pages.Students;

public class Create : PageModel
{
    private readonly IMediator _mediator;

    public Create(IMediator mediator) => _mediator = mediator;

    [BindProperty]
    public required Command Data { get; set; }

    public void OnGet() => Data = new Command();

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            await _mediator.Send(Data, cancellationToken);
            return RedirectToPage(nameof(Index));
        }

        return Page();
    }

    public record Command : IRequest<int>
    {
        public string LastName { get; init; } = default!;

        [Display(Name = "First Name")]
        public string FirstMidName { get; init; } = default!;

        [DataType(DataType.Date)]
        public DateOnly? EnrollmentDate { get; init; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(m => m.LastName).NotNull().Length(1, 50);
            RuleFor(m => m.FirstMidName).NotNull().Length(1, 50);
            RuleFor(m => m.EnrollmentDate).NotNull();
        }
    }

    public class CommandHandler : IRequestHandler<Command, int>
    {
        private readonly SchoolContext _db;

        public CommandHandler(SchoolContext db) => _db = db;

        public async Task<int> Handle(Command message, CancellationToken token)
        {
            var student = new Student
            {
                FirstMidName = message.FirstMidName,
                LastName = message.LastName,
                EnrollmentDate = message.EnrollmentDate!.Value
            };

            await _db.Students.AddAsync(student, token);

            await _db.SaveChangesAsync(token);

            return student.Id;
        }
    }
}