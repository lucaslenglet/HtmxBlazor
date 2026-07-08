using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace HtmxBlazor.Components.Endpoints;

/// <summary>
/// Maps Blazor components as htmx fragment endpoints: the component is rendered
/// with static SSR (no layout, no full document) and returned as an HTML fragment
/// ready to be swapped into the page by htmx.
/// </summary>
public static class HtmxEndpointRouteBuilderExtensions
{
    /// <summary>Maps a GET endpoint rendering <typeparamref name="TComponent"/> as an HTML fragment.</summary>
    /// <param name="parameters">
    /// Optional factory building the component parameters from the request,
    /// typically an anonymous object: <c>ctx => new { Query = ctx.Request.Query["q"].ToString() }</c>.
    /// </param>
    public static RouteHandlerBuilder MapHtmxGet<TComponent>(
        this IEndpointRouteBuilder endpoints,
        string pattern,
        Func<HttpContext, object?>? parameters = null)
        where TComponent : IComponent
        => endpoints.MapGet(pattern, (HttpContext context) => Render<TComponent>(context, parameters));

    /// <summary>
    /// Maps a POST endpoint rendering <typeparamref name="TComponent"/> as an HTML fragment.
    /// The antiforgery token is validated (form field or request header) unless
    /// <paramref name="validateAntiforgery"/> is <c>false</c>.
    /// </summary>
    public static RouteHandlerBuilder MapHtmxPost<TComponent>(
        this IEndpointRouteBuilder endpoints,
        string pattern,
        Func<HttpContext, object?>? parameters = null,
        bool validateAntiforgery = true)
        where TComponent : IComponent
        => endpoints.MapPost(pattern, (HttpContext context) => Render<TComponent>(context, parameters))
            .WithAntiforgeryValidation(validateAntiforgery);

    /// <summary>Maps a PUT endpoint rendering <typeparamref name="TComponent"/> as an HTML fragment.</summary>
    public static RouteHandlerBuilder MapHtmxPut<TComponent>(
        this IEndpointRouteBuilder endpoints,
        string pattern,
        Func<HttpContext, object?>? parameters = null,
        bool validateAntiforgery = true)
        where TComponent : IComponent
        => endpoints.MapPut(pattern, (HttpContext context) => Render<TComponent>(context, parameters))
            .WithAntiforgeryValidation(validateAntiforgery);

    /// <summary>Maps a PATCH endpoint rendering <typeparamref name="TComponent"/> as an HTML fragment.</summary>
    public static RouteHandlerBuilder MapHtmxPatch<TComponent>(
        this IEndpointRouteBuilder endpoints,
        string pattern,
        Func<HttpContext, object?>? parameters = null,
        bool validateAntiforgery = true)
        where TComponent : IComponent
        => endpoints.MapPatch(pattern, (HttpContext context) => Render<TComponent>(context, parameters))
            .WithAntiforgeryValidation(validateAntiforgery);

    /// <summary>Maps a DELETE endpoint rendering <typeparamref name="TComponent"/> as an HTML fragment.</summary>
    public static RouteHandlerBuilder MapHtmxDelete<TComponent>(
        this IEndpointRouteBuilder endpoints,
        string pattern,
        Func<HttpContext, object?>? parameters = null,
        bool validateAntiforgery = true)
        where TComponent : IComponent
        => endpoints.MapDelete(pattern, (HttpContext context) => Render<TComponent>(context, parameters))
            .WithAntiforgeryValidation(validateAntiforgery);

    private static RazorComponentResult<TComponent> Render<TComponent>(
        HttpContext context,
        Func<HttpContext, object?>? parameters)
        where TComponent : IComponent
    {
        var values = parameters?.Invoke(context);
        return values is null
            ? new RazorComponentResult<TComponent>()
            : new RazorComponentResult<TComponent>(values);
    }

    private static RouteHandlerBuilder WithAntiforgeryValidation(this RouteHandlerBuilder builder, bool validate)
    {
        if (!validate)
        {
            return builder;
        }

        return builder.AddEndpointFilter(async (context, next) =>
        {
            var antiforgery = context.HttpContext.RequestServices.GetRequiredService<IAntiforgery>();
            try
            {
                await antiforgery.ValidateRequestAsync(context.HttpContext);
            }
            catch (AntiforgeryValidationException)
            {
                return Results.BadRequest("Antiforgery token missing or invalid.");
            }

            return await next(context);
        });
    }
}
