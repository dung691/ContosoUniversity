using System.ComponentModel;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using DelegateDecompiler.EntityFrameworkCore;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Pages.Departments;

public class Index : PageModel
{
    private readonly IMediator _mediator;

    public Index(IMediator mediator) => _mediator = mediator;

    public required List<Model> Data { get; set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
        => Data = await _mediator.Send(new Query(), cancellationToken);

    public record Query : IRequest<List<Model>>;

    public record Model
    {
        public required string Name { get; init; }

        public decimal Budget { get; init; }

        public DateOnly StartDate { get; init; }

        public int Id { get; init; }
        [DisplayName("Administrator")]
        public string? AdministratorFullName { get; init; }
    }

    public class MappingProfile : Profile
    {
        public MappingProfile() => CreateProjection<Department, Model>()
            .ForMember(d => d.AdministratorFullName,
                opt => opt.MapFrom(s => s.Administrator == null ? null : s.Administrator.FullName));
    }

    public class QueryHandler : IRequestHandler<Query, List<Model>>
    {
        private readonly SchoolContext _db;
        private readonly AutoMapper.IConfigurationProvider _configuration;

        public QueryHandler(SchoolContext db, AutoMapper.IConfigurationProvider configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public Task<List<Model>> Handle(Query message, 
            CancellationToken token) => _db
            .Departments
            .ProjectTo<Model>(_configuration)
            .DecompileAsync()
            .ToListAsync(token);
    }
}