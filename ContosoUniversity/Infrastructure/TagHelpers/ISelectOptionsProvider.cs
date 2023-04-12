using Microsoft.AspNetCore.Mvc.Rendering;

namespace ContosoUniversity.Infrastructure.TagHelpers;

public interface ISelectOptionsProvider
{
    bool Match(Type type);
    ValueTask<IEnumerable<SelectListItem>> GetOptions();
}