using System.Text;

namespace HtmxBlazor.Components.Htmx;

/// <summary>
/// Fluent builder for <c>hx-trigger</c> attribute values.
/// See https://htmx.org/attributes/hx-trigger/
/// </summary>
/// <example>
/// <code>
/// HxTrigger.On("input").Changed().Delay(TimeSpan.FromMilliseconds(300))
/// // => "input changed delay:300ms"
///
/// HxTrigger.Load().Or(HxTrigger.Every(TimeSpan.FromSeconds(5)))
/// // => "load, every 5s"
/// </code>
/// </example>
public sealed class HxTrigger
{
    private readonly StringBuilder _spec;

    private HxTrigger(string spec) => _spec = new StringBuilder(spec);

    /// <summary>Triggers on a standard or custom DOM event, with an optional JS filter expression.</summary>
    public static HxTrigger On(string eventName, string? filter = null)
        => new(filter is null ? eventName : $"{eventName}[{filter}]");

    /// <summary>Triggers once, when the element is first loaded.</summary>
    public static HxTrigger Load() => new("load");

    /// <summary>Triggers once, when the element first scrolls into the viewport.</summary>
    public static HxTrigger Revealed() => new("revealed");

    /// <summary>Triggers when the element intersects the viewport (IntersectionObserver).</summary>
    public static HxTrigger Intersect(string? root = null, double? threshold = null)
    {
        var trigger = new HxTrigger("intersect");
        if (root is not null)
        {
            trigger._spec.Append(" root:").Append(root);
        }
        if (threshold is not null)
        {
            trigger._spec.Append(" threshold:").Append(threshold.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }
        return trigger;
    }

    /// <summary>Triggers on a fixed interval (polling).</summary>
    public static HxTrigger Every(TimeSpan interval) => new($"every {HxTiming.Format(interval)}");

    /// <summary>Only triggers once.</summary>
    public HxTrigger Once() => Append("once");

    /// <summary>Only triggers if the value of the element changed.</summary>
    public HxTrigger Changed() => Append("changed");

    /// <summary>Waits before issuing the request; the timer resets if the event fires again.</summary>
    public HxTrigger Delay(TimeSpan delay) => Append($"delay:{HxTiming.Format(delay)}");

    /// <summary>Ignores events fired during the throttle period after a request.</summary>
    public HxTrigger Throttle(TimeSpan throttle) => Append($"throttle:{HxTiming.Format(throttle)}");

    /// <summary>Listens for the event on another element (extended CSS selector, e.g. <c>body</c>, <c>closest form</c>).</summary>
    public HxTrigger From(string extendedSelector) => Append($"from:{extendedSelector}");

    /// <summary>Filters the event to a child of the listened element matching this selector.</summary>
    public HxTrigger Target(string cssSelector) => Append($"target:{cssSelector}");

    /// <summary>Stops the event from triggering ancestor htmx elements.</summary>
    public HxTrigger Consume() => Append("consume");

    /// <summary>How to queue events fired while a request is in flight (first, last, all, none).</summary>
    public HxTrigger Queue(string option) => Append($"queue:{option}");

    /// <summary>Combines this trigger with another one (comma separated list).</summary>
    public HxTrigger Or(HxTrigger other)
    {
        _spec.Append(", ").Append(other._spec);
        return this;
    }

    private HxTrigger Append(string modifier)
    {
        _spec.Append(' ').Append(modifier);
        return this;
    }

    public override string ToString() => _spec.ToString();

    public static implicit operator string(HxTrigger trigger) => trigger.ToString();
}
