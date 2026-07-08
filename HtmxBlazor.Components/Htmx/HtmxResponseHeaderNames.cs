namespace HtmxBlazor.Components.Htmx;

/// <summary>
/// Response headers understood by htmx.
/// See https://htmx.org/docs/#response-headers
/// </summary>
public static class HtmxResponseHeaderNames
{
    public const string Location = "HX-Location";
    public const string PushUrl = "HX-Push-Url";
    public const string Redirect = "HX-Redirect";
    public const string Refresh = "HX-Refresh";
    public const string ReplaceUrl = "HX-Replace-Url";
    public const string Reswap = "HX-Reswap";
    public const string Retarget = "HX-Retarget";
    public const string Reselect = "HX-Reselect";
    public const string Trigger = "HX-Trigger";
    public const string TriggerAfterSettle = "HX-Trigger-After-Settle";
    public const string TriggerAfterSwap = "HX-Trigger-After-Swap";
}
