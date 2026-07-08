using Microsoft.AspNetCore.Http;

namespace HtmxBlazor.Components.Htmx;

/// <summary>
/// Strongly typed access to the htmx request headers.
/// </summary>
public static class HtmxRequestExtensions
{
    /// <summary>Whether the request was issued by htmx (as opposed to a full page load).</summary>
    public static bool IsHtmx(this HttpRequest request)
        => request.Headers.ContainsKey(HtmxRequestHeaderNames.Request);

    /// <summary>Whether the request comes from an element using <c>hx-boost</c>.</summary>
    public static bool IsHtmxBoosted(this HttpRequest request)
        => string.Equals(request.Headers[HtmxRequestHeaderNames.Boosted], "true", StringComparison.OrdinalIgnoreCase);

    /// <summary>Whether the request is an htmx history restore request.</summary>
    public static bool IsHtmxHistoryRestore(this HttpRequest request)
        => string.Equals(request.Headers[HtmxRequestHeaderNames.HistoryRestoreRequest], "true", StringComparison.OrdinalIgnoreCase);

    /// <summary>The <c>id</c> of the target element, if any.</summary>
    public static string? HtmxTarget(this HttpRequest request)
        => NullIfEmpty(request.Headers[HtmxRequestHeaderNames.Target]);

    /// <summary>The <c>id</c> of the element that triggered the request, if any.</summary>
    public static string? HtmxTrigger(this HttpRequest request)
        => NullIfEmpty(request.Headers[HtmxRequestHeaderNames.Trigger]);

    /// <summary>The <c>name</c> of the element that triggered the request, if any.</summary>
    public static string? HtmxTriggerName(this HttpRequest request)
        => NullIfEmpty(request.Headers[HtmxRequestHeaderNames.TriggerName]);

    /// <summary>The user response to an <c>hx-prompt</c>, if any.</summary>
    public static string? HtmxPrompt(this HttpRequest request)
        => NullIfEmpty(request.Headers[HtmxRequestHeaderNames.Prompt]);

    /// <summary>The current browser URL when the request was issued, if any.</summary>
    public static string? HtmxCurrentUrl(this HttpRequest request)
        => NullIfEmpty(request.Headers[HtmxRequestHeaderNames.CurrentUrl]);

    private static string? NullIfEmpty(string? value)
        => string.IsNullOrEmpty(value) ? null : value;
}
