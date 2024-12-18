namespace HtmxBlazor.Web.Endpoints;

public static class TicksEndpoints
{
    public static WebApplication MapTicks(this WebApplication application)
    {
        application.MapGet(GetTickts, GetTicks);

        return application;
    }

    public const string GetTickts = "/api/ticks";

    private static long GetTicks() => DateTime.Now.Ticks;
}
