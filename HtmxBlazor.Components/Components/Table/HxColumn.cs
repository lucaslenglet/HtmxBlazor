using Microsoft.AspNetCore.Components;

namespace HtmxBlazor.Components;

/// <summary>
/// Declares one column inside an <see cref="HxTable{TItem}"/>. Renders nothing by
/// itself: the parent renders the header and, for each item, the cell template.
/// </summary>
public sealed class HxColumn<TItem> : ComponentBase
{
    [CascadingParameter] internal HxTable<TItem>? Parent { get; set; }

    /// <summary>Text shown in the column header.</summary>
    [Parameter, EditorRequired] public string Title { get; set; } = default!;

    /// <summary>
    /// Sort key sent in the fragment URL (<c>?sort=key</c>) when the header is clicked.
    /// When <c>null</c> the column is not sortable.
    /// </summary>
    [Parameter] public string? SortKey { get; set; }

    /// <summary>Cell template; the item is available as <c>@context</c>.</summary>
    [Parameter] public RenderFragment<TItem>? ChildContent { get; set; }

    protected override void OnInitialized()
    {
        if (Parent is null)
        {
            throw new InvalidOperationException("HxColumn must be used inside an HxTable.");
        }

        Parent.AddColumn(this);
    }
}
