using Microsoft.AspNetCore.Components;

namespace HtmxBlazor.Components;

/// <summary>
/// Declares one step inside an <see cref="HxWizard"/>. Renders nothing by itself:
/// the parent renders the progress marker, and the content of the current step only.
/// </summary>
public sealed class HxWizardStep : ComponentBase
{
    [CascadingParameter] internal HxWizard? Parent { get; set; }

    /// <summary>Text shown in the progress marker.</summary>
    [Parameter, EditorRequired] public string Title { get; set; } = default!;

    /// <summary>
    /// Names of the fields owned by this step's inputs. The wizard skips them when
    /// re-rendering the carried values as hidden fields, so a value is never submitted
    /// twice when its input is visible.
    /// </summary>
    [Parameter] public string[]? Fields { get; set; }

    /// <summary>Content of the step, typically form fields. Only rendered when the step is current.</summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    protected override void OnInitialized()
    {
        if (Parent is null)
        {
            throw new InvalidOperationException($"{nameof(HxWizardStep)} must be used inside an {nameof(HxWizard)}.");
        }

        Parent.AddStep(this);
    }
}
