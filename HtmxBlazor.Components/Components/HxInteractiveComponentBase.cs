using HtmxBlazor.Components.Htmx;
using Microsoft.AspNetCore.Components;

namespace HtmxBlazor.Components;

/// <summary>
/// Base class for components that issue htmx requests. Exposes the core htmx
/// attributes as strongly typed parameters and merges them with any extra
/// attributes captured by <see cref="HxComponentBase.AdditionalAttributes"/>.
/// </summary>
public abstract class HxInteractiveComponentBase : HxComponentBase
{
    /// <summary>URL for a GET request (<c>hx-get</c>).</summary>
    [Parameter] public string? Get { get; set; }

    /// <summary>URL for a POST request (<c>hx-post</c>).</summary>
    [Parameter] public string? Post { get; set; }

    /// <summary>URL for a PUT request (<c>hx-put</c>).</summary>
    [Parameter] public string? Put { get; set; }

    /// <summary>URL for a PATCH request (<c>hx-patch</c>).</summary>
    [Parameter] public string? Patch { get; set; }

    /// <summary>URL for a DELETE request (<c>hx-delete</c>).</summary>
    [Parameter] public string? Delete { get; set; }

    /// <summary>What triggers the request (<c>hx-trigger</c>). Accepts an <see cref="HxTrigger"/> builder.</summary>
    [Parameter] public string? Trigger { get; set; }

    /// <summary>How the response is swapped in (<c>hx-swap</c>).</summary>
    [Parameter] public HxSwap? Swap { get; set; }

    /// <summary>Extra <c>hx-swap</c> modifiers, e.g. <c>transition:true</c> or <c>scroll:bottom</c>.</summary>
    [Parameter] public string? SwapModifiers { get; set; }

    /// <summary>Which element receives the response (<c>hx-target</c>, extended CSS selector).</summary>
    [Parameter] public string? Target { get; set; }

    /// <summary>Which part of the response to swap in (<c>hx-select</c>, CSS selector).</summary>
    [Parameter] public string? Select { get; set; }

    /// <summary>Element(s) flagged with the <c>htmx-request</c> class during the request (<c>hx-indicator</c>).</summary>
    [Parameter] public string? Indicator { get; set; }

    /// <summary>Element(s) disabled during the request (<c>hx-disabled-elt</c>).</summary>
    [Parameter] public string? DisabledElt { get; set; }

    /// <summary>Extra values submitted with the request, as a JSON object (<c>hx-vals</c>).</summary>
    [Parameter] public string? Vals { get; set; }

    /// <summary>Additional elements whose values are included in the request (<c>hx-include</c>).</summary>
    [Parameter] public string? Include { get; set; }

    /// <summary>Confirmation message shown before issuing the request (<c>hx-confirm</c>).</summary>
    [Parameter] public string? Confirm { get; set; }

    /// <summary>URL pushed into the browser history, or <c>true</c>/<c>false</c> (<c>hx-push-url</c>).</summary>
    [Parameter] public string? PushUrl { get; set; }

    /// <summary>How this request synchronizes with others (<c>hx-sync</c>).</summary>
    [Parameter] public string? Sync { get; set; }

    /// <summary>The merged htmx + additional attributes to splat onto the root element.</summary>
    protected IReadOnlyDictionary<string, object> AllAttributes()
    {
        var attributes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        void Add(string name, string? value)
        {
            if (value is not null)
            {
                attributes[name] = value;
            }
        }

        Add("hx-get", Get);
        Add("hx-post", Post);
        Add("hx-put", Put);
        Add("hx-patch", Patch);
        Add("hx-delete", Delete);
        Add("hx-trigger", Trigger);
        Add("hx-swap", Swap is null
            ? null
            : SwapModifiers is null
                ? Swap.Value.ToAttributeValue()
                : $"{Swap.Value.ToAttributeValue()} {SwapModifiers}");
        Add("hx-target", Target);
        Add("hx-select", Select);
        Add("hx-indicator", Indicator);
        Add("hx-disabled-elt", DisabledElt);
        Add("hx-vals", Vals);
        Add("hx-include", Include);
        Add("hx-confirm", Confirm);
        Add("hx-push-url", PushUrl);
        Add("hx-sync", Sync);

        if (AdditionalAttributes is not null)
        {
            foreach (var (key, value) in AdditionalAttributes)
            {
                attributes[key] = value;
            }
        }

        return attributes;
    }
}
