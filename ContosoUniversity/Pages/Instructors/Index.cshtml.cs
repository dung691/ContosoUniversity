using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Pages.Instructors;
//public class TransactionBehavior<TRequest, TResponse>
//    : IPipelineBehavior<TRequest, TResponse>
//{
//    private readonly SchoolContext _dbContext;

//    public TransactionBehavior(SchoolContext dbContext) => _dbContext = dbContext;

//    public async Task<TResponse> Handle(TRequest request, 
//        CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
//    {
//        try
//        {
//            await _dbContext.BeginTransactionAsync();
//            var response = await next();
//            await _dbContext.CommitTransactionAsync();
//            return response;
//        }
//        catch (Exception)
//        {
//            _dbContext.RollbackTransaction();
//            throw;
//        }
//    }
//}

//public class LoggingBehavior<TRequest, TResponse>
//    : IPipelineBehavior<TRequest, TResponse>
//{
//    private readonly ILogger<TRequest> _logger;

//    public LoggingBehavior(ILogger<TRequest> logger) 
//        => _logger = logger;

//    public async Task<TResponse> Handle(
//        TRequest request, CancellationToken cancellationToken, 
//        RequestHandlerDelegate<TResponse> next)
//    {
//        using (_logger.BeginScope(request))
//        {
//            _logger.LogInformation("Calling handler...");
//            var response = await next();
//            _logger.LogInformation("Called handler with result {0}", response);
//            return response;
//        }
//    }
//}

public class Index : PageModel
{
    private readonly IMediator _mediator;

    public Index(IMediator mediator) 
        => _mediator = mediator;

    public required Model Data { get; set; }

    public async Task OnGetAsync(Query query, CancellationToken cancellationToken)
        => Data = await _mediator.Send(query, cancellationToken);

    public record Query : IRequest<Model>
    {
        public int? Id { get; init; }
        public int? CourseId { get; init; }
    }

    public record Model
    {
        public int? InstructorId { get; init; }
        public int? CourseId { get; init; }

        public IList<Instructor> Instructors { get; init; } = new List<Instructor>();
        public IList<Course> Courses { get; init; } = new List<Course>();
        public IList<Enrollment> Enrollments { get; init; } = new List<Enrollment>();

        public record Instructor
        {
            public int Id { get; init; }

            [Display(Name = "Last Name")]
            public string LastName { get; init; } = default!;

            [Display(Name = "First Name")]
            public string FirstMidName { get; init; } = default!;

            [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
            [Display(Name = "Hire Date")]
            public DateOnly HireDate { get; init; }

            [DisplayName("Office")]
            public string? OfficeAssignmentLocation { get; init; }
            [DisplayName("Courses")]
            public IEnumerable<CourseAssignment> Courses { get; init; } = Enumerable.Empty<CourseAssignment>();
        }

        public record CourseAssignment
        {
            public int CourseId { get; init; }
            public string CourseTitle { get; init; } = default!;
        }

        public record Course
        {
            [DisplayName("Number")]
            public int Id { get; init; }
            public string Title { get; init; } = default!;
            [DisplayName("Department")]
            public string? DepartmentName { get; init; }
        }

        public record Enrollment
        {
            [DisplayFormat(NullDisplayText = "No grade")]
            public Grade? Grade { get; init; }
            [DisplayName("Name")]
            public string StudentFullName { get; init; } = default!;
        }
    }

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateProjection<Instructor, Model.Instructor>();
            CreateProjection<Course, Model.CourseAssignment>()
                .ForMember(d => d.CourseId, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.CourseTitle, opt => opt.MapFrom(s => s.Title))
                ;
            CreateProjection<Course, Model.Course>();
            CreateProjection<Enrollment, Model.Enrollment>();
        }
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

        public async Task<Model> Handle(Query message, CancellationToken token)
        {
            var instructors = await _db.Instructors
                    .OrderBy(i => i.LastName)
                    .ProjectTo<Model.Instructor>(_configuration)
                    .ToListAsync(token)
                ;
            
            var courses = new List<Model.Course>();
            var enrollments = new List<Model.Enrollment>();

            if (message.Id != null)
            {
                courses = await _db.Courses
                    .Where(ci => ci.Instructors.Any(i => i.Id == message.Id))
                    .ProjectTo<Model.Course>(_configuration)
                    .ToListAsync(token);
            }

            if (message.CourseId != null)
            {
                enrollments = await _db.Enrollments
                    .Where(x => x.CourseId == message.CourseId)
                    .ProjectTo<Model.Enrollment>(_configuration)
                    .ToListAsync(token);
            }

            var viewModel = new Model
            {
                Instructors = instructors,
                Courses = courses,
                Enrollments = enrollments,
                InstructorId = message.Id,
                CourseId = message.CourseId
            };

            return viewModel;
        }
    }
}