using ContosoUniversity.Data;
using ContosoUniversity.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Infrastructure.TagHelpers.SelectOptionsProviders;

public class InstructorSelectOptionsProvider : EntitySelectOptionsProvider<Instructor>
{
    public InstructorSelectOptionsProvider(SchoolContext schoolContext) : base(schoolContext)
    {
    }

    public override async ValueTask<IEnumerable<SelectListItem>> GetOptions()
    {
        var items = await SchoolContext.Instructors
            .Select(x => new SelectListItem(x.FullName, x.Id.ToString()))
            .ToArrayAsync();

        return items.OrderBy(x => x.Text);
    }
}