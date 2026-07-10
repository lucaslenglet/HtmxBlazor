using Microsoft.AspNetCore.Components;

namespace HtmxBlazor.Components;

/// <summary>
/// Declares one tab inside an <see cref="HxTabs"/>. Renders nothing by itself:
/// the parent renders the tab header, and the content of the active tab only.
/// </summary>
public sealed class HxTab : ComponentBase
{
    [CascadingParameter] internal HxTabs? Parent { get; set; }

    /// <summary>Stable identifier of the tab, used in the fragment URL (<c>?tab=key</c>).</summary>
    [Parameter, EditorRequired] public string Key { get; set; } = default!;

    /// <summary>Text shown in the tab header.</summary>
    [Parameter, EditorRequired] public string Title { get; set; } = default!;

    /// <summary>Content of the tab panel. Only rendered when the tab is active.</summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    protected override void OnInitialized()
    {
        if (Parent is null)
        {
            throw new InvalidOperationException($"{nameof(HxTab)} must be used inside an {nameof(HxTabs)}.");
        }

        Parent.AddTab(this);
    }
}
