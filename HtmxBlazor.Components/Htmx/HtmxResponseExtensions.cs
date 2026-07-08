using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;

namespace HtmxBlazor.Components.Htmx;

/// <summary>When htmx should dispatch an <c>HX-Trigger</c> event.</summary>
public enum HxTriggerTiming
{
    /// <summary>As soon as the response is received.</summary>
    Receive,

    /// <summary>After the settling step.</summary>
    AfterSettle,

    /// <summary>After the swap step.</summary>
    AfterSwap,
}

/// <summary>
/// Strongly typed helpers to set the htmx response headers.
/// All methods are no-ops on the DOM unless the request came from htmx.
/// </summary>
public static class HtmxResponseExtensions
{
    /// <summary>Triggers a client side event, optionally with a JSON-serialized detail payload.</summary>
    public static HttpResponse HxTrigger(
        this HttpResponse response,
        string eventName,
        object? detail = null,
        HxTriggerTiming timing = HxTriggerTiming.Receive)
    {
        var header = timing switch
        {
            HxTriggerTiming.AfterSettle => HtmxResponseHeaderNames.TriggerAfterSettle,
            HxTriggerTiming.AfterSwap => HtmxResponseHeaderNames.TriggerAfterSwap,
            _ => HtmxResponseHeaderNames.Trigger,
        };

        var current = response.Headers[header].ToString();

        if (detail is null && !current.StartsWith('{'))
        {
            // Simple form: comma separated list of event names.
            response.Headers[header] = string.IsNullOrEmpty(current)
                ? eventName
                : $"{current}, {eventName}";
            return response;
        }

        // JSON form: { "event": detail, ... } — upgrade any existing simple list.
        var events = new JsonObject();
        if (!string.IsNullOrEmpty(current))
        {
            if (current.StartsWith('{'))
            {
                events = JsonNode.Parse(current)!.AsObject();
            }
            else
            {
                foreach (var name in current.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                {
                    events[name] = null;
                }
            }
        }

        events[eventName] = detail is null ? null : JsonSerializer.SerializeToNode(detail);
        response.Headers[header] = events.ToJsonString();
        return response;
    }

    /// <summary>Client side redirect to a new location (full page load).</summary>
    public static HttpResponse HxRedirect(this HttpResponse response, string url)
    {
        response.Headers[HtmxResponseHeaderNames.Redirect] = url;
        return response;
    }

    /// <summary>Client side navigation to a new location without a full page reload.</summary>
    public static HttpResponse HxLocation(this HttpResponse response, string url)
    {
        response.Headers[HtmxResponseHeaderNames.Location] = url;
        return response;
    }

    /// <summary>Pushes a new URL into the browser history stack. Pass <c>null</c> to prevent the default push.</summary>
    public static HttpResponse HxPushUrl(this HttpResponse response, string? url)
    {
        response.Headers[HtmxResponseHeaderNames.PushUrl] = url ?? "false";
        return response;
    }

    /// <summary>Replaces the current URL in the browser location bar. Pass <c>null</c> to prevent the default replace.</summary>
    public static HttpResponse HxReplaceUrl(this HttpResponse response, string? url)
    {
        response.Headers[HtmxResponseHeaderNames.ReplaceUrl] = url ?? "false";
        return response;
    }

    /// <summary>Makes the client do a full refresh of the page.</summary>
    public static HttpResponse HxRefresh(this HttpResponse response)
    {
        response.Headers[HtmxResponseHeaderNames.Refresh] = "true";
        return response;
    }

    /// <summary>Overrides the target element of the swap (CSS selector).</summary>
    public static HttpResponse HxRetarget(this HttpResponse response, string cssSelector)
    {
        response.Headers[HtmxResponseHeaderNames.Retarget] = cssSelector;
        return response;
    }

    /// <summary>Overrides how the response is swapped in.</summary>
    public static HttpResponse HxReswap(this HttpResponse response, HxSwap swap, string? modifiers = null)
    {
        response.Headers[HtmxResponseHeaderNames.Reswap] = modifiers is null
            ? swap.ToAttributeValue()
            : $"{swap.ToAttributeValue()} {modifiers}";
        return response;
    }

    /// <summary>Overrides which part of the response is swapped in (CSS selector).</summary>
    public static HttpResponse HxReselect(this HttpResponse response, string cssSelector)
    {
        response.Headers[HtmxResponseHeaderNames.Reselect] = cssSelector;
        return response;
    }
}
