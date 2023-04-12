using ContosoUniversity.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ContosoUniversity.Infrastructure.TagHelpers.SelectOptionsProviders;

public abstract class EntitySelectOptionsProvider<T> : ISelectOptionsProvider where T : class
{
    protected EntitySelectOptionsProvider(SchoolContext schoolContext)
    {
        SchoolContext = schoolContext;
    }

    protected SchoolContext SchoolContext { get; }

    public bool Match(Type type) => typeof(T) == type;
    public abstract ValueTask<IEnumerable<SelectListItem>> GetOptions();
}