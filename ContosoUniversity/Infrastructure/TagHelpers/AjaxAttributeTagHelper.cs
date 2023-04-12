using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ContosoUniversity.Infrastructure.TagHelpers;

/// <summary>
/// Extends <see cref="FormTagHelper"/> and <see cref="AnchorTagHelper"/> elements with ajax data-* attributes
/// Mimics BeginForm from <see href="https://github.com/mono/aspnetwebstack/blob/master/src/System.Web.Mvc/Ajax/AjaxExtensions.cs" />
/// In sync with <see href="https://github.com/aspnet/jquery-ajax-unobtrusive/blob/master/src/jquery.unobtrusive-ajax.js"/>
/// </summary>
[HtmlTargetElement("form", Attributes = AjaxAttributeName)]
[HtmlTargetElement("a", Attributes = AjaxAttributeName)]
public partial class AjaxAttributeTagHelper : TagHelper
{
    private const string AjaxAttributeName = "asp-ajax";
    public const string AjaxConfirmAttributeName = "asp-ajax-confirm";
    public const string AjaxMethodAttributeName = "asp-ajax-method";
    public const string AjaxUpdateElementAttributeName = "asp-ajax-update";
    public const string AjaxModeAttributeName = "asp-ajax-mode";
    public const string AjaxLoadingElementAttributeName = "asp-ajax-loading";
    public const string AjaxLoadingElementDurationAttributeName = "asp-ajax-loading-duration";
    public const string AjaxSuccessAttributeName = "asp-ajax-success";
    public const string AjaxFailureAttributeName = "asp-ajax-failure";
    public const string AjaxBeginAttributeName = "asp-ajax-begin";
    public const string AjaxCompleteAttributeName = "asp-ajax-complete";
    public const string AjaxUrlAttributeName = "asp-ajax-url";

    /// <summary>
    /// Must be set to true to activate unobtrusive Ajax on the target element.
    /// Has to be present because we are only extending existing form tag helper ...
    /// </summary>
    [HtmlAttributeName(AjaxAttributeName)]
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the message to display in a confirmation window before a request is submitted.
    /// </summary>
    [HtmlAttributeName(AjaxConfirmAttributeName)]
    public string? ConfirmMessage { get; set; }

    /// <summary>
    /// Gets or sets the HTTP request method ("Get" or "Post").
    /// </summary>
    [HtmlAttributeName(AjaxMethodAttributeName)]
    public string? HttpMethod { get; set; }

    /// <summary>
    /// Gets or sets the ID of the DOM element to update by using the response from the server.
    /// </summary>
    [HtmlAttributeName(AjaxUpdateElementAttributeName)]
    public string? UpdateElementId { get; set; }

    /// <summary>
    /// Gets or sets the id attribute of an HTML element that is displayed while the Ajax function is loading.
    /// </summary>
    [HtmlAttributeName(AjaxLoadingElementAttributeName)]
    public string? LoadingElementId { get; set; }

    /// <summary>
    /// Gets or sets a value, in milliseconds, that controls the duration of the animation when showing or hiding the loading element.
    /// </summary>
    [HtmlAttributeName(AjaxLoadingElementDurationAttributeName)]
    public int LoadingElementDuration { get; set; }

    /// <summary>
    /// Gets or sets the mode that specifies how to insert the response into the target DOM element. Valid values are before, after and replace. Default is replace
    /// </summary>
    [HtmlAttributeName(AjaxModeAttributeName)]
    public InsertionMode InsertionMode { get; set; } = InsertionMode.Replace;

    /// <summary>
    /// Gets or sets the JavaScript function to call after the page is successfully updated.
    /// </summary>
    [HtmlAttributeName(AjaxSuccessAttributeName)]
    public string? OnSuccessMethod { get; set; }

    /// <summary>
    /// Gets or sets the JavaScript function to call if the page update fails.
    /// </summary>
    [HtmlAttributeName(AjaxFailureAttributeName)]
    public string? OnFailureMethod { get; set; }

    /// <summary>
    /// Gets or sets the JavaScript function to call when response data has been instantiated but before the page is updated.
    /// </summary>
    [HtmlAttributeName(AjaxCompleteAttributeName)]
    public string? OnCompleteMethod { get; set; }

    /// <summary>
    /// Gets or sets the name of the JavaScript function to call immediately before the page is updated.
    /// </summary>
    [HtmlAttributeName(AjaxBeginAttributeName)]
    public string? OnBeginMethod { get; set; }

    /// <summary>
    /// Gets or sets the URL to make the request to.
    /// </summary>
    [HtmlAttributeName(AjaxUrlAttributeName)]
    public string? Url { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        base.Process(context, output);

        if (!Enabled) return;

        // Use hardcoded value to avoid string operations and culture problems("true/false" values instead of "True/False" etc.)
        output.Attributes.Add("data-ajax", "true");
        output.Attributes.AddIfSpecified("data-ajax-confirm", ConfirmMessage);
        output.Attributes.AddIfSpecified("data-ajax-method", HttpMethod);
        output.Attributes.AddIfSpecified("data-ajax-success", OnSuccessMethod);
        output.Attributes.AddIfSpecified("data-ajax-failure", OnFailureMethod);
        output.Attributes.AddIfSpecified("data-ajax-begin", OnBeginMethod);
        output.Attributes.AddIfSpecified("data-ajax-complete", OnCompleteMethod);
        output.Attributes.AddIfSpecified("data-ajax-url", Url);

        if (!string.IsNullOrEmpty(UpdateElementId))
        {
            output.Attributes.Add("data-ajax-update", EscapeIdSelector(UpdateElementId));
            // Append insertion mode only if update element id is present
            output.Attributes.Add("data-ajax-mode", InsertionMode.ToInsertionModeUnobtrusive());
        }

        if (!string.IsNullOrEmpty(LoadingElementId))
        {
            output.Attributes.Add("data-ajax-loading", EscapeIdSelector(LoadingElementId));
            output.Attributes.AddIf(LoadingElementDuration > 0, "data-ajax-loading-duration", LoadingElementDuration);
        }
    }

    private static string EscapeIdSelector(string selector)
    {
        // The string returned by this function is used as a value for jQuery's selector. The characters dot, colon and 
        // square brackets are valid id characters but need to be properly escaped since they have special meaning. For
        // e.g., for the id a.b, $('#a.b') would cause ".b" to treated as a class selector. The correct way to specify
        // this selector would be to escape the dot to get $('#a\.b').
        // See http://learn.jquery.com/using-jquery-core/faq/how-do-i-select-an-element-by-an-id-that-has-characters-used-in-css-notation/
        return '#' + IdRegex().Replace(selector, @"\$&");
    }

    [GeneratedRegex("[.:[\\]]")]
    private static partial Regex IdRegex();
}

/// <summary>
/// Used to determine where html content should be rendered
/// </summary>
public enum InsertionMode
{
    /// <summary>
    /// Replace the element.
    /// </summary>
    Replace = 0,

    /// <summary>
    /// Insert before the element.
    /// </summary>
    InsertBefore = 1,

    /// <summary>
    /// Insert after the element.
    /// </summary>
    InsertAfter = 2,

    /// <summary>
    /// Replace the entire element.
    /// </summary>
    ReplaceWith = 3
}

/// <summary>
/// Extension methods for <see cref="InsertionMode"/> used only inside <see cref="AjaxAttributeTagHelper"/>
/// </summary>
internal static class InsertionModeExtensions
{
    /// <summary>
    /// Returns proper insertion value based on enum <paramref name="value"/>
    /// </summary>
    /// <param name="value">Insertion mode</param>
    /// <returns></returns>
    public static string ToInsertionModeUnobtrusive(this InsertionMode value) =>
        value switch
        {
            InsertionMode.Replace => "replace",
            InsertionMode.InsertBefore => "before",
            InsertionMode.InsertAfter => "after",
            InsertionMode.ReplaceWith => "replace-with",
            // Default value will result with jquerys .html method
            _ => ((int)value).ToString(CultureInfo.InvariantCulture),
        };
}