using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace HtmxBlazor.Components.Endpoints;

public static class HxModalEndpointExtensions
{
    /// <summary>
    /// Maps the empty endpoint used by <see cref="HxModal"/> close buttons: swapping
    /// the empty response over the modal (<c>outerHTML</c>) removes it from the DOM.
    /// </summary>
    public static IEndpointRouteBuilder MapHxModalClose(
        this IEndpointRouteBuilder endpoints,
        string pattern = HxModal.DefaultCloseRoute)
    {
        endpoints.MapGet(pattern, () => Results.Content(string.Empty, "text/html"));
        return endpoints;
    }
}
