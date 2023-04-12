namespace ContosoUniversity.Infrastructure.TagHelpers;

public interface ISelectListProviderFactory
{
    ISelectOptionsProvider Create(IServiceProvider serviceProvider);
}

[AttributeUsage(AttributeTargets.Property)]
public class SelectListForAttribute<T> : Attribute, ISelectListProviderFactory
{
    public ISelectOptionsProvider Create(IServiceProvider serviceProvider)
    {
        var providers = serviceProvider.GetServices<ISelectOptionsProvider>();
        return providers.Single(x => x.Match(typeof(T)));
    }
}