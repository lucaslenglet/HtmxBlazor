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

        app.MapHxModalClose();

        return app;
    }
}
