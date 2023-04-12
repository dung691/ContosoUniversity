using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ContosoUniversity.Infrastructure.TagHelpers;

/// <summary>
/// Extension methods for <see cref="TagHelperAttributeList"/> collection
/// </summary>
public static class TagHelperAttributeListExtensions
{
    /// <summary>
    /// Shorthand method to add attribute to collection if condition is satisfied
    /// </summary>
    public static void AddIf(this TagHelperAttributeList attributes, bool condition, string name, object? value)
    {
        // Framework internally invokes Add overload with TagHelperAttribute argument
        AddIf(attributes, condition, new TagHelperAttribute(name, value));
    }

    public static void AddIfSpecified(this TagHelperAttributeList attributes, string name, string? value)
    {
        attributes.AddIf(!string.IsNullOrWhiteSpace(value), name, value);
    }

    public static void AddIf(this TagHelperAttributeList attributes, bool condition, TagHelperAttribute attribute)
    {
        ArgumentNullException.ThrowIfNull(attributes);

        if (condition)
        {
            attributes.Add(attribute);
        }
    }
}