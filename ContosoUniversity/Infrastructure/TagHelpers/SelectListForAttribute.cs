using Microsoft.AspNetCore.Mvc.Rendering;

namespace ContosoUniversity.Infrastructure.TagHelpers;

public interface ISelectListProviderFactory
{
    ISelectOptionsProvider Create(IServiceProvider serviceProvider);
}

public class NullSelectListProvider : ISelectOptionsProvider
{
    public bool Match(Type type) => false;
    public ValueTask<IEnumerable<SelectListItem>> GetOptions() => new(Enumerable.Empty<SelectListItem>());
}

[AttributeUsage(AttributeTargets.Property)]
public class SelectListForAttribute<T> : Attribute, ISelectListProviderFactory
{
    public ISelectOptionsProvider Create(IServiceProvider serviceProvider)
    {
        var providers = serviceProvider.GetServices<ISelectOptionsProvider>();
        return providers.SingleOrDefault(x => x.Match(typeof(T))) ?? new NullSelectListProvider();
    }
}