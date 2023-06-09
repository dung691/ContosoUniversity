﻿using System.ComponentModel.DataAnnotations;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Pages.Instructors;

public class CreateEdit : PageModel
{
    private readonly IMediator _mediator;

    [BindProperty]
    public required Command Data { get; set; }

    public CreateEdit(IMediator mediator) => _mediator = mediator;

    public async Task OnGetCreateAsync(CancellationToken cancellationToken)
    {
        Data = (await _mediator.Send(new Query(), cancellationToken))!;
    }

    public async Task<IActionResult> OnPostCreateAsync(CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            await _mediator.Send(Data, cancellationToken);
            return RedirectToPage(nameof(Index));
        }

        return Page();
    }

    public async Task<IActionResult> OnGetEditAsync(Query query, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest();
        var instructor = await _mediator.Send(query, cancellationToken);
        if (instructor is null) return NotFound();

        Data = instructor;

        return Page();
    }

    public async Task<IActionResult> OnPostEditAsync(CancellationToken cancellationToken)
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
        public int? Id { get; init; }
    }

    public class QueryValidator : AbstractValidator<Query>
    {
        public QueryValidator()
        {
            RuleFor(m => m.Id).NotNull();
        }
    }

    public record Command : IRequest<int>
    {
        public int? Id { get; init; }

        public string LastName { get; init; } = default!;
        [Display(Name = "First Name")]
        public string FirstMidName { get; init; } = default!;

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public DateOnly? HireDate { get; init; }

        [Display(Name = "Location")]
        public string? OfficeAssignmentLocation { get; init; }

        public string[] SelectedCourses { get; init; } = Array.Empty<string>();

        public List<AssignedCourseData> AssignedCourses { get; init; } = new();
        public List<CourseAssignment> Courses { get; init; } = new();

        public record AssignedCourseData
        {
            public int CourseId { get; init; }
            public string? Title { get; init; }
            public bool Assigned { get; init; }
        }

        public record CourseAssignment
        {
            public int CourseId { get; init; }
        }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(m => m.LastName).NotNull().Length(0, 50);
            RuleFor(m => m.FirstMidName).NotNull().Length(0, 50);
            RuleFor(m => m.HireDate).NotNull();
        }
    }

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateProjection<Instructor, Command>()
                .ForMember(d => d.SelectedCourses, opt => opt.Ignore())
                .ForMember(d => d.AssignedCourses, opt => opt.Ignore());
            CreateProjection<Course, Command.CourseAssignment>()
                .ForMember(d => d.CourseId, opt => opt.MapFrom(s => s.Id));
        }
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

        public async Task<Command?> Handle(Query message, CancellationToken token)
        {
            Command? model;
            if (message.Id == null)
            {
                model = new Command();
            }
            else
            {
                model = await _db.Instructors
                    .Include(i => i.Courses)
                    .Where(i => i.Id == message.Id)
                    .ProjectTo<Command?>(_configuration)
                    .SingleOrDefaultAsync(token);
            }

            if (model is null) return null;

            var instructorCourses = model.Courses.Select(c => c.CourseId).ToList();
            var viewModel = await _db.Courses.Select(course => new Command.AssignedCourseData
            {
                CourseId = course.Id,
                Title = course.Title,
                Assigned = instructorCourses.Count != 0 && instructorCourses.Contains(course.Id)
            }).ToListAsync(token);

            model = model with { AssignedCourses = viewModel };

            return model;
        }
    }

    public class CommandHandler : IRequestHandler<Command, int>
    {
        private readonly SchoolContext _db;

        public CommandHandler(SchoolContext db) => _db = db;

        public async Task<int> Handle(Command message, CancellationToken token)
        {
            Instructor instructor;
            if (message.Id == null)
            {
                instructor = new Instructor();
                await _db.Instructors.AddAsync(instructor, token);
            }
            else
            {
                instructor = await _db.Instructors
                    .Include(i => i.OfficeAssignment)
                    .Include(i => i.Courses)
                    .Where(i => i.Id == message.Id)
                    .SingleAsync(token);
            }

            var courses = await _db.Courses.ToListAsync(token);

            instructor.Handle(message, courses);

            await _db.SaveChangesAsync(token);

            return instructor.Id;
        }
    }
}