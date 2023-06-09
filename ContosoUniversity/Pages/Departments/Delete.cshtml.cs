﻿using System.ComponentModel.DataAnnotations;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Pages.Departments;

public class Delete : PageModel
{
    private readonly IMediator _mediator;

    public Delete(IMediator mediator) => _mediator = mediator;

    [BindProperty]
    public required Command Data { get; set; }

    public async Task<IActionResult> OnGetAsync(Query query, CancellationToken cancellationToken)
    {
        var department = await _mediator.Send(query, cancellationToken);
        if (department is null) return NotFound();

        Data = department;

        return Page();
    }

    public async Task<ActionResult> OnPostAsync(CancellationToken cancellationToken)
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
        public int Id { get; init; }
        public string Name { get; init; } = default!;
        public decimal Budget { get; init; }
        public DateOnly StartDate { get; init; }
        [Display(Name = "Administrator")]
        public string? AdministratorFullName { get; init; }
    }

    public class MappingProfile : Profile
    {
        public MappingProfile() => CreateProjection<Department, Command>()
            .ForMember(d => d.AdministratorFullName, 
                opt => opt.MapFrom(x => x.Administrator == null ? null : x.Administrator.FullName));
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
            var department = await _db.Departments.FindAsync(new object?[] { message.Id }, token);
            if (department is not null)
                _db.Departments.Remove(department);
        }
    }
}