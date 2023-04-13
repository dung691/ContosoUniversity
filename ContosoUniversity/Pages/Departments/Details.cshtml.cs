using System.ComponentModel.DataAnnotations;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using DelegateDecompiler.EntityFrameworkCore;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Pages.Departments;

public class Details : PageModel
{
    private readonly IMediator _mediator;

    public required Model Data { get; set; }

    public Details(IMediator mediator) => _mediator = mediator;

    public async Task<IActionResult> OnGetAsync(Query query, CancellationToken cancellationToken)
    {
        var department = await _mediator.Send(query, cancellationToken);

        if (department is null) return NotFound();

        Data = department;

        return Page();
    }

    public record Query : IRequest<Model?>
    {
        public int Id { get; init; }
    }

    public record Model
    {
        public required string Name { get; init; }

        public decimal Budget { get; init; }

        public DateOnly StartDate { get; init; }

        public int Id { get; init; }

        [Display(Name = "Administrator")]
        public string? AdministratorFullName { get; init; }
    }

    public class MappingProfile : Profile
    {
        public MappingProfile() => CreateProjection<Department, Model>()
            .ForMember(d => d.AdministratorFullName,
                opt => opt.MapFrom(s => s.Administrator == null ? null : s.Administrator.FullName));
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
            _db.Departments
                .Where(m => m.Id == message.Id)
                .ProjectTo<Model?>(_configuration)
                .DecompileAsync()
                .SingleOrDefaultAsync(token);
    }
}