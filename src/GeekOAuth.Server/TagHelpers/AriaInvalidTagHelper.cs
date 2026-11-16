using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GeekOAuth.Server.TagHelpers;

/// <summary>
/// Marks every <c>&lt;input asp-for&gt;</c> whose model-state entry has errors with <c>aria-invalid="true"</c>,
/// so each page does not have to repeat the attribute field by field.
/// </summary>
[HtmlTargetElement("input", Attributes = "asp-for")]
public sealed class AriaInvalidTagHelper : TagHelper
{
    [HtmlAttributeName("asp-for")]
    public ModelExpression For { get; set; } = default!;

    [ViewContext, HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = default!;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var key = ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(For.Name);
        if (ViewContext.ModelState.TryGetValue(key, out var entry) && entry.Errors.Count > 0)
            output.Attributes.SetAttribute("aria-invalid", "true");
    }
}
