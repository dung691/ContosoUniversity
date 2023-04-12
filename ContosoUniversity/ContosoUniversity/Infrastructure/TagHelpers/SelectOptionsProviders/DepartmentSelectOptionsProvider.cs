using ContosoUniversity.Data;
using ContosoUniversity.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Infrastructure.TagHelpers.SelectOptionsProviders;

public class DepartmentSelectOptionsProvider : EntitySelectOptionsProvider<Department>
{
    public DepartmentSelectOptionsProvider(SchoolContext schoolContext) : base(schoolContext)
    {
    }

    public override async ValueTask<IEnumerable<SelectListItem>> GetOptions()
    {
        var items = await SchoolContext.Departments
            .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
            .ToArrayAsync();

        return items.OrderBy(x => x.Text);
    }
}