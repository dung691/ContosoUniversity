using System.ComponentModel;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using ContosoUniversity.Data;
using ContosoUniversity.Infrastructure.TagHelpers;
using ContosoUniversity.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Pages.Departments;

public class Edit : PageModel
{
    private readonly IMediator _mediator;

    [BindProperty]
    public required Command Data { get; set; }

    public Edit(IMediator mediator) => _mediator = mediator;

    public async Task<IActionResult> OnGetAsync(Query query, CancellationToken cancellationToken)
    {
        var department = await _mediator.Send(query, cancellationToken);

        if (department is null) return NotFound();

        Data = department;

        return Page();
    }

    public async Task<ActionResult> OnPostAsync(int id, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            await _mediator.Send(Data, cancellationToken);
            return RedirectToPage(nameof(Index));
        }

        return Page();
    }

    public record Query : IRequest<Command?>
    {
        public int Id { get; init; }
    }

    public record Command : IRequest
    {
        public string Name { get; init; }

        public decimal? Budget { get; init; }

        public DateOnly? StartDate { get; init; }
        [SelectListFor<Instructor>, DisplayName("Administrator")]
        public int InstructorId { get; set; }
        public int Id { get; init; }
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

    public class MappingProfile : Profile
    {
        public MappingProfile() => CreateProjection<Department, Command>();
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

        public async Task<Command?> Handle(Query message, 
            CancellationToken token) => await _db
            .Departments
            .Where(d => d.Id == message.Id)
            .ProjectTo<Command?>(_configuration)
            .SingleOrDefaultAsync(token);
    }

    public class CommandHandler : IRequestHandler<Command>
    {
        private readonly SchoolContext _db;

        public CommandHandler(SchoolContext db) => _db = db;

        public async Task Handle(Command message, CancellationToken token)
        {
            var dept = await _db.Departments.FindAsync(new object?[] { message.Id }, token);

            dept.Name = message.Name;
            dept.StartDate = message.StartDate!.Value;
            dept.Budget = message.Budget!.Value;
            dept.InstructorId = message.InstructorId;
        }
    }
}