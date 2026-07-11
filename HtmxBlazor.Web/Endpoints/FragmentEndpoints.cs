using HtmxBlazor.Components;
using HtmxBlazor.Components.Endpoints;
using HtmxBlazor.Web.Components.Fragments;

namespace HtmxBlazor.Web.Endpoints;

public static class FragmentEndpoints
{
    public static WebApplication MapFragments(this WebApplication app)
    {
        app.MapHtmxPost<CounterFragment>(CounterFragment.Route, context => new
        {
            Count = int.TryParse(context.Request.Query["count"], out var count) ? count : 0,
        });

        app.MapHtmxGet<SearchFragment>(SearchFragment.Route, context => new
        {
            Query = context.Request.Query["q"].ToString(),
        });

        app.MapHtmxGet<ClockFragment>(ClockFragment.Route);

        app.MapHtmxGet<SlowFragment>(SlowFragment.Route);

        // The antiforgery filter has already read (and validated) the form,
        // so accessing Request.Form here is safe and non-blocking.
        app.MapHtmxPost<GreetingFragment>(GreetingFragment.Route, context => new
        {
            Name = context.Request.Form["name"].ToString(),
        });

        app.MapHtmxGet<SentCountFragment>(SentCountFragment.Route);

        app.MapHtmxGet<DemoTabs>(DemoTabs.Route, context => new
        {
            Active = context.Request.Query["tab"].ToString(),
        });

        app.MapHtmxGet<DemoModal>(DemoModal.Route);

        app.MapHtmxGet<DemoFeed>(DemoFeed.Route, context => new
        {
            Page = int.TryParse(context.Request.Query["page"], out var feedPage) ? feedPage : 1,
        });

        app.MapHtmxGet<NotifyFragment>(NotifyFragment.Route, context => new
        {
            Variant = Enum.TryParse<HxToastVariant>(context.Request.Query["variant"], ignoreCase: true, out var variant)
                ? variant
                : HxToastVariant.Success,
        });

        app.MapHtmxGet<DemoConfirm>(DemoConfirm.Route, context => new
        {
            Item = context.Request.Query["item"].ToString(),
        });

        app.MapHtmxPost<ItemDeletedFragment>(ItemDeletedFragment.Route, context => new
        {
            Item = context.Request.Query["item"].ToString(),
        });

        app.MapHtmxGet<DemoDropdown>(DemoDropdown.Route);

        app.MapHtmxGet<PickFragment>(PickFragment.Route, context => new
        {
            Choice = context.Request.Query["choice"].ToString(),
        });

        app.MapHtmxGet<DemoMultiDropdown>(DemoMultiDropdown.Route, context => new
        {
            Values = QueryValues(context),
            Open = bool.TryParse(context.Request.Query["open"], out var open) && open,
        });

        app.MapHtmxGet<ToppingsPanelFragment>(ToppingsPanelFragment.Route, context => new
        {
            Values = QueryValues(context),
        });

        app.MapHtmxGet<DemoTable>(DemoTable.Route, context => new
        {
            Sort = context.Request.Query["sort"].ToString() is { Length: > 0 } sort ? sort : null,
            Desc = bool.TryParse(context.Request.Query["desc"], out var desc) && desc,
            Page = int.TryParse(context.Request.Query["page"], out var tablePage) ? tablePage : 1,
        });

        app.MapHtmxGet<DemoAccordion>(DemoAccordion.Route, context => new
        {
            Open = context.Request.Query["open"].ToString() is { Length: > 0 } open ? open : null,
        });

        app.MapHtmxGet<SuggestFragment>(SuggestFragment.Route, context => new
        {
            Query = context.Request.Query["q"].ToString(),
        });

        app.MapHtmxGet<DemoAutocomplete>(DemoAutocomplete.Route, context => new
        {
            Value = context.Request.Query["value"].ToString(),
        });

        app.MapHtmxGet<MultiSuggestFragment>(MultiSuggestFragment.Route, context => new
        {
            Query = context.Request.Query["q"].ToString(),
            Values = QueryValues(context),
        });

        app.MapHtmxGet<DemoMultiAutocomplete>(DemoMultiAutocomplete.Route, context => new
        {
            Values = QueryValues(context),
        });

        // The antiforgery filter has already read the form (see GreetingFragment above).
        app.MapHtmxPost<DemoWizard>(DemoWizard.Route, context => new
        {
            Step = int.TryParse(context.Request.Query["step"], out var step) ? step : 1,
            Values = (IReadOnlyDictionary<string, string>)context.Request.Form
                .Where(field => !field.Key.StartsWith("__", StringComparison.Ordinal))
                .ToDictionary(field => field.Key, field => field.Value.LastOrDefault() ?? string.Empty),
        });

        app.MapHtmxDelete<RowDeletedFragment>(RowDeletedFragment.Route, context => new
        {
            Name = context.Request.Query["name"].ToString(),
        });

        app.MapHtmxPost<DemoRows>(DemoRows.ResetRoute, _ => new
        {
            Reset = true,
        });

        app.MapHxModalClose();

        app.MapHxDismiss();

        return app;
    }

    /// <summary>The multi-select values repeated in the query string (<c>?values=a&amp;values=b</c>).</summary>
    private static IReadOnlyList<string> QueryValues(HttpContext context)
        => context.Request.Query["values"]
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .ToArray();
}
