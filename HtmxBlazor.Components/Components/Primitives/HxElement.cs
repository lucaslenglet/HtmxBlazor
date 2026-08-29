using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace HtmxBlazor.Components;

/// <summary>
/// A generic htmx-enabled element with a configurable tag. The Swiss army knife:
/// every htmx attribute is available as a strongly typed parameter.
/// </summary>
/// <example>
/// <code>
/// &lt;HxElement Tag="input" type="search" name="q"
///            Get="/fragments/search"
///            Trigger="@(HxTrigger.On("input").Changed().Delay(TimeSpan.FromMilliseconds(300)))"
///            Target="#results" /&gt;
/// </code>
/// </example>
public sealed class HxElement : HxInteractiveComponentBase
{
    /// <summary>The HTML tag to render. Defaults to <c>div</c>.</summary>
    [Parameter] public string Tag { get; set; } = "div";

    [Parameter] public RenderFragment? ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, Tag);
        builder.AddMultipleAttributes(1, AllAttributes()!);
        builder.AddContent(2, ChildContent);
        builder.CloseElement();
    }
}
