namespace HtmxBlazor.Components.Htmx;

internal static class HxTiming
{
    /// <summary>Formats a <see cref="TimeSpan"/> as an htmx timing declaration ("500ms", "2s").</summary>
    public static string Format(TimeSpan value)
    {
        var milliseconds = (long)value.TotalMilliseconds;
        return milliseconds % 1000 == 0
            ? $"{milliseconds / 1000}s"
            : $"{milliseconds}ms";
    }
}
