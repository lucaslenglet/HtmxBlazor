using HtmxBlazor.Components.Htmx;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;

namespace HtmxBlazor.Components;

/// <summary>
/// Base class for server side rendered components that participate in an htmx page.
/// Gives access to the ambient <see cref="HttpContext"/> so components can inspect
/// the htmx request headers and set htmx response headers (events, redirects, ...).
/// </summary>
public abstract class HxComponentBase : ComponentBase
{
    /// <summary>The current request context. Only available with Blazor static SSR.</summary>
    [CascadingParameter]
    public HttpContext? HttpContext { get; set; }

    /// <summary>Extra attributes are forwarded to the root element of the component.</summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>Whether the component is being rendered for an htmx request (fragment) rather than a full page.</summary>
    protected bool IsHtmxRequest => HttpContext?.Request.IsHtmx() ?? false;

    /// <summary>Triggers a client side event through the <c>HX-Trigger</c> response header.</summary>
    protected void TriggerClientEvent(
        string eventName,
        object? detail = null,
        HxTriggerTiming timing = HxTriggerTiming.Receive)
        => HttpContext?.Response.HxTrigger(eventName, detail, timing);
}
