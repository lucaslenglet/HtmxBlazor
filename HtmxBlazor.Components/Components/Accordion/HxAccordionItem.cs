using Microsoft.AspNetCore.Components;

namespace HtmxBlazor.Components;

/// <summary>
/// Declares one panel inside an <see cref="HxAccordion"/>. Renders nothing by itself:
/// the parent renders the panel header, and the content of the open panel only.
/// </summary>
public sealed class HxAccordionItem : ComponentBase
{
    [CascadingParameter] internal HxAccordion? Parent { get; set; }

    /// <summary>Stable identifier of the panel, used in the fragment URL (<c>?open=key</c>).</summary>
    [Parameter, EditorRequired] public string Key { get; set; } = default!;

    /// <summary>Text shown in the panel header.</summary>
    [Parameter, EditorRequired] public string Title { get; set; } = default!;

    /// <summary>Content of the panel. Only rendered when the panel is open.</summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    protected override void OnInitialized()
    {
        if (Parent is null)
        {
            throw new InvalidOperationException($"{nameof(HxAccordionItem)} must be used inside an {nameof(HxAccordion)}.");
        }

        Parent.AddItem(this);
    }
}
