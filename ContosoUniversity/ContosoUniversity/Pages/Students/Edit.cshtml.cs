using System.ComponentModel.DataAnnotations;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Pages.Students;

public class Edit : PageModel
{
    private readonly IMediator _mediator;

    public Edit(IMediator mediator) => _mediator = mediator;

    [BindProperty]
    public Command Data { get; set; }

    public async Task OnGetAsync(Query query)
        => Data = await _mediator.Send(query);

    public async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid)
        {
            await _mediator.Send(Data);
            return RedirectToPage(nameof(Index));
        }

        return Page();
    }

    public record Query : IRequest<Command>
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

    public record Command : IRequest
    {
        public int Id { get; init; }
        public string LastName { get; init; }

        [Display(Name = "First Name")]
        public string FirstMidName { get; init; }

        public DateTime? EnrollmentDate { get; init; }
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

    public class MappingProfile : Profile
    {
        public MappingProfile() => CreateProjection<Student, Command>();
    }

    public class QueryHandler : IRequestHandler<Query, Command>
    {
        private readonly SchoolContext _db;
        private readonly AutoMapper.IConfigurationProvider _configuration;

        public QueryHandler(SchoolContext db, AutoMapper.IConfigurationProvider configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public async Task<Command> Handle(Query message, CancellationToken token) => await _db.Students
            .Where(s => s.Id == message.Id)
            .ProjectTo<Command>(_configuration)
            .SingleOrDefaultAsync(token);
    }

    public class CommandHandler : IRequestHandler<Command>
    {
        private readonly SchoolContext _db;

        public CommandHandler(SchoolContext db) => _db = db;

        public async Task Handle(Command message, CancellationToken token)
        {
            var student = await _db.Students.FindAsync(new object?[] { message.Id }, token);

            student.FirstMidName = message.FirstMidName;
            student.LastName = message.LastName;
            student.EnrollmentDate = message.EnrollmentDate!.Value;
        }
    }
}