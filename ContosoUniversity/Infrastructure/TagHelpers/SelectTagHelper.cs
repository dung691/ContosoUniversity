using System.Reflection;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ContosoUniversity.Infrastructure.TagHelpers;

[HtmlTargetElement("select", Attributes = ForAttributeName)]
[HtmlTargetElement("select", Attributes = ItemsAttributeName)]
public class SelectTagHelper : Microsoft.AspNetCore.Mvc.TagHelpers.SelectTagHelper
{
    private const string ForAttributeName = "asp-for";
    private const string ItemsAttributeName = "asp-items";

    private readonly IHtmlHelper _htmlHelper;

    public SelectTagHelper(IHtmlGenerator generator, IHtmlHelper htmlHelper) : base(generator) 
        => _htmlHelper = htmlHelper;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (Items is not null && Items.Any()) return;

        var modelType = For.ModelExplorer.ModelType;
        var containerType = For.Metadata.ContainerType;

        if (!string.IsNullOrEmpty(For.Metadata.PropertyName))
        {
            var selectListForAttribute = containerType?
                .GetProperty(For.Metadata.PropertyName)?
                .GetCustomAttribute(typeof(SelectListForAttribute<>));

            if (selectListForAttribute is ISelectListProviderFactory selectListProviderFactory)
            {
                var selectListOptionsProvider = selectListProviderFactory.Create(ViewContext.HttpContext.RequestServices);
                Items = await selectListOptionsProvider.GetOptions();
            }
        }

        if (typeof(Enum).IsAssignableFrom(modelType))
        {
            Items = _htmlHelper.GetEnumSelectList(modelType);
        }

        await base.ProcessAsync(context, output);
    }
}