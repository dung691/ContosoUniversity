﻿using System.ComponentModel.DataAnnotations;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Pages.Instructors;

public class Details : PageModel
{
    private readonly IMediator _mediator;

    public Details(IMediator mediator) => _mediator = mediator;

    public Model Data { get; private set; }

    public async Task OnGetAsync(Query query) => Data = await _mediator.Send(query);

    public record Query : IRequest<Model>
    {
        public int? Id { get; init; }
    }

    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(m => m.Id).NotNull();
        }
    }

    public record Model
    {
        public int? Id { get; init; }

        public string LastName { get; init; }
        [Display(Name = "First Name")]
        public string FirstMidName { get; init; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public DateOnly? HireDate { get; init; }

        [Display(Name = "Location")]
        public string OfficeAssignmentLocation { get; init; }
    }

    public class MappingProfile : Profile
    {
        public MappingProfile() => CreateProjection<Instructor, Model>();
    }

    public class QueryHandler : IRequestHandler<Query, Model>
    {
        private readonly SchoolContext _db;
        private readonly AutoMapper.IConfigurationProvider _configuration;

        public QueryHandler(SchoolContext db, AutoMapper.IConfigurationProvider configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public Task<Model> Handle(Query message, CancellationToken token) => _db
            .Instructors
            .Where(i => i.Id == message.Id)
            .ProjectTo<Model>(_configuration)
            .SingleOrDefaultAsync(token);
    }
}