namespace HtmxBlazor.Components.Htmx;

/// <summary>
/// Request headers sent by htmx on every AJAX request.
/// See https://htmx.org/docs/#request-headers
/// </summary>
public static class HtmxRequestHeaderNames
{
    public const string Request = "HX-Request";
    public const string Boosted = "HX-Boosted";
    public const string CurrentUrl = "HX-Current-URL";
    public const string HistoryRestoreRequest = "HX-History-Restore-Request";
    public const string Prompt = "HX-Prompt";
    public const string Target = "HX-Target";
    public const string Trigger = "HX-Trigger";
    public const string TriggerName = "HX-Trigger-Name";
}
