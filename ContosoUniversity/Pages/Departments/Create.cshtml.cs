using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ContosoUniversity.Data;
using ContosoUniversity.Infrastructure.TagHelpers;
using ContosoUniversity.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ContosoUniversity.Pages.Departments;

public class Create : PageModel
{
    private readonly IMediator _mediator;

    [BindProperty]
    public required Command Data { get; set; }

    public Create(IMediator mediator) => _mediator = mediator;

    public async Task<ActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            await _mediator.Send(Data, cancellationToken);
            return RedirectToPage(nameof(Index));
        }

        return Page();
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(m => m.Name).NotNull().Length(3, 50);
            RuleFor(m => m.Budget).NotNull();
            RuleFor(m => m.StartDate).NotNull();
            RuleFor(m => m.InstructorId).NotNull();
        }
    }

    public record Command : IRequest<int>
    {
        public string Name { get; init; } = default!;

        [DataType(DataType.Currency)]
        [Column(TypeName = "money")]
        public decimal? Budget { get; init; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateOnly? StartDate { get; init; }

        [SelectListFor<Instructor>, DisplayName("Administrator")]
        public int InstructorId { get; init; }
    }

    public class CommandHandler : IRequestHandler<Command, int>
    {
        private readonly SchoolContext _db;

        public CommandHandler(SchoolContext db) => _db = db;

        public async Task<int> Handle(Command message, CancellationToken token)
        {
            var department = new Department
            {
                InstructorId = message.InstructorId,
                Budget = message.Budget!.Value,
                Name = message.Name,
                StartDate = message.StartDate!.Value
            };

            await _db.Departments.AddAsync(department, token);

            await _db.SaveChangesAsync(token);

            return department.Id;
        }
    }
}