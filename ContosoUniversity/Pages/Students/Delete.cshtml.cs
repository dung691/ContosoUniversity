using System.ComponentModel.DataAnnotations;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Pages.Students;

public class Delete : PageModel
{
    private readonly IMediator _mediator;

    public Delete(IMediator mediator) => _mediator = mediator;

    [BindProperty]
    public required Command Data { get; set; }

    public async Task<IActionResult> OnGetAsync(Query query, CancellationToken cancellationToken)
    {
        var student = await _mediator.Send(query, cancellationToken);

        if (student is null) return NotFound();

        Data = student;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid)
        {
            await _mediator.Send(Data);
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
        public int Id { get; init; }
        [Display(Name = "First Name")]
        public string FirstMidName { get; init; } = default!;
        public string LastName { get; init; } = default!;
        public DateOnly EnrollmentDate { get; init; }
    }

    public class MappingProfile : Profile
    {
        public MappingProfile() => CreateProjection<Student, Command>();
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

        public async Task<Command?> Handle(Query message, CancellationToken token) => await _db
            .Students
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
            if (student is not null)
                _db.Students.Remove(student);
        }
    }

}