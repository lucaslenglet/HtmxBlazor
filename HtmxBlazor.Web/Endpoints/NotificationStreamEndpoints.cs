namespace HtmxBlazor.Web.Endpoints;

public static class NotificationStreamEndpoints
{
    public const string Route = "/fragments/demo-notifications";

    private static readonly string[] Samples =
    [
        "A deploy just finished",
        "Someone starred the repo",
        "New comment on your PR",
        "Nightly build is green",
        "A new user signed up",
    ];

    /// <summary>
    /// Server-Sent Events endpoint feeding the <c>HxNotificationStream</c> demo:
    /// one <c>data:</c> line per event, each carrying a ready-to-swap HTML fragment.
    /// The loop ends when the client disconnects (page closed or navigated away).
    /// </summary>
    public static WebApplication MapNotificationStream(this WebApplication app)
    {
        app.MapGet(Route, async context =>
        {
            context.Response.Headers.ContentType = "text/event-stream";
            context.Response.Headers.CacheControl = "no-cache";

            try
            {
                for (var n = 1; !context.RequestAborted.IsCancellationRequested; n++)
                {
                    var html = $"""<p class="demo-notification"><strong>#{n}</strong> {Samples[(n - 1) % Samples.Length]} — {DateTime.Now:HH:mm:ss}</p>""";
                    await context.Response.WriteAsync($"data: {html}\n\n", context.RequestAborted);
                    await context.Response.Body.FlushAsync(context.RequestAborted);
                    await Task.Delay(TimeSpan.FromSeconds(2), context.RequestAborted);
                }
            }
            catch (OperationCanceledException)
            {
                // Client disconnected — expected way to end the stream.
            }
        });

        return app;
    }
}
