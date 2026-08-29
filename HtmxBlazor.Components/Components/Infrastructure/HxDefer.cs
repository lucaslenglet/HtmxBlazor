using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace HtmxBlazor.Components;

/// <summary>
/// Defers the rendering of its content to its own render pass, which happens
/// after sibling components declared earlier in the tree have initialized.
/// Lets a parent collect items registered by child components (e.g. tabs)
/// before rendering markup that depends on them. Same pattern as QuickGrid.
/// Infrastructure — not meant to be used directly in application markup.
/// </summary>
public sealed class HxDefer : ComponentBase
{
    [Parameter] public RenderFragment? ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
        => builder.AddContent(0, ChildContent);
}
