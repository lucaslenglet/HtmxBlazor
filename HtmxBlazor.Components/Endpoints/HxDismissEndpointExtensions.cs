using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace HtmxBlazor.Components.Endpoints;

public static class HxDismissEndpointExtensions
{
    /// <summary>Default route of the shared dismiss endpoint.</summary>
    public const string DefaultDismissRoute = "/hx/dismiss";

    /// <summary>
    /// Maps the shared empty endpoint used by components that remove or clear an
    /// element without JavaScript (<see cref="HxToast"/> dismissal, <see cref="HxDropdown"/>
    /// closing): swapping the empty response over an element (<c>outerHTML</c>) removes it,
    /// swapping it into an element (<c>innerHTML</c>) clears it.
    /// Deliberately 200 + empty body — htmx does not swap on a 204.
    /// </summary>
    public static IEndpointRouteBuilder MapHxDismiss(
        this IEndpointRouteBuilder endpoints,
        string pattern = DefaultDismissRoute)
    {
        endpoints.MapGet(pattern, () => Results.Content(string.Empty, "text/html"));
        return endpoints;
    }
}
