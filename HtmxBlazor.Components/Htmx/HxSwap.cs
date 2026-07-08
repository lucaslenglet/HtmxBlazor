namespace HtmxBlazor.Components.Htmx;

/// <summary>
/// How htmx swaps the response content into the DOM (<c>hx-swap</c>).
/// See https://htmx.org/attributes/hx-swap/
/// </summary>
public enum HxSwap
{
    /// <summary>Replace the inner html of the target element (htmx default).</summary>
    InnerHtml,

    /// <summary>Replace the entire target element with the response.</summary>
    OuterHtml,

    /// <summary>Replace the text content of the target element.</summary>
    TextContent,

    /// <summary>Insert the response before the target element.</summary>
    BeforeBegin,

    /// <summary>Insert the response before the first child of the target element.</summary>
    AfterBegin,

    /// <summary>Insert the response after the last child of the target element.</summary>
    BeforeEnd,

    /// <summary>Insert the response after the target element.</summary>
    AfterEnd,

    /// <summary>Delete the target element regardless of the response.</summary>
    Delete,

    /// <summary>Do not swap the response (out of band items are still processed).</summary>
    None,
}

public static class HxSwapExtensions
{
    /// <summary>The literal <c>hx-swap</c> attribute value.</summary>
    public static string ToAttributeValue(this HxSwap swap) => swap switch
    {
        HxSwap.InnerHtml => "innerHTML",
        HxSwap.OuterHtml => "outerHTML",
        HxSwap.TextContent => "textContent",
        HxSwap.BeforeBegin => "beforebegin",
        HxSwap.AfterBegin => "afterbegin",
        HxSwap.BeforeEnd => "beforeend",
        HxSwap.AfterEnd => "afterend",
        HxSwap.Delete => "delete",
        HxSwap.None => "none",
        _ => throw new ArgumentOutOfRangeException(nameof(swap), swap, null),
    };
}
