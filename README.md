# HtmxBlazor

Experiments combining **Blazor static SSR** (HTML rendering engine) with **[htmx](https://htmx.org)** (front-end interactivity engine) — with as little JavaScript as possible.

## How it works

Blazor is used purely as a server-side HTML templating engine (static SSR: no circuits, no WebAssembly, no SignalR). htmx, declared through plain HTML attributes, drives all the interactivity:

1. The browser loads a page fully rendered by Blazor SSR (`MapRazorComponents<App>`).
2. `hx-*` attributes on elements make htmx issue AJAX requests to **fragment endpoints**.
3. Fragment endpoints render a Blazor component *without any layout or document shell* and return the HTML fragment.
4. htmx swaps the fragment into the DOM (`hx-target` / `hx-swap`), and optionally reacts to response headers (`HX-Trigger`, `HX-Redirect`, ...).

The same `.razor` component can be rendered inline in the initial page **and** served as a fragment — no duplication.

## Projects

| Project | Purpose |
|---|---|
| `HtmxBlazor.Components` | The reusable component library (Razor Class Library, no JS). |
| `HtmxBlazor.Web` | Demo app — see the `/demo` page. |
| `HtmxBlazor.AppHost` / `HtmxBlazor.ServiceDefaults` | .NET Aspire orchestration. |

## The library: `HtmxBlazor.Components`

### htmx primitives (strongly typed)

- `HtmxRequestExtensions` — inspect htmx request headers: `Request.IsHtmx()`, `IsHtmxBoosted()`, `HtmxTarget()`, `HtmxTrigger()`, ...
- `HtmxResponseExtensions` — set htmx response headers fluently: `Response.HxTrigger("event", detail)`, `HxRedirect()`, `HxPushUrl()`, `HxRetarget()`, `HxReswap()`, `HxRefresh()`, ...
- `HxSwap` — enum for `hx-swap` values (`HxSwap.OuterHtml.ToAttributeValue()` → `"outerHTML"`).
- `HxTrigger` — fluent builder for `hx-trigger` specs:

```csharp
HxTrigger.On("input").Changed().Delay(TimeSpan.FromMilliseconds(300))
// => "input changed delay:300ms"
HxTrigger.Load().Or(HxTrigger.Every(TimeSpan.FromSeconds(5)))
// => "load, every 5s"
```

### Base classes

- `HxComponentBase` — cascading `HttpContext`, `IsHtmxRequest`, `TriggerClientEvent(...)`, attribute splatting.
- `HxInteractiveComponentBase` — adds every core htmx attribute as a typed parameter (`Get`, `Post`, `Trigger`, `Swap`, `Target`, `Indicator`, `Vals`, `Confirm`, `PushUrl`, ...).

### Components

| Component | What it does |
|---|---|
| `HxElement` | Generic htmx element with a configurable `Tag` — the escape hatch for anything. |
| `HxButton` | Action button; automatically sends the **antiforgery token** as a header on unsafe verbs. |
| `HxForm` | htmx-driven `<form>` with the antiforgery hidden field built in. |
| `HxLink` | Boosted navigation link (`hx-boost`), degrades gracefully to a normal `<a>`. |
| `HxLazy` | Deferred region: placeholder now, fragment fetched on `load` or on reveal. |
| `HxPoll` | Polls a fragment URL on an interval (stops on HTTP 286). |
| `HxIndicator` | Loading indicator shown while a request is in flight. |
| `HxFragmentLayout` | Empty layout for *routable* fragment components (`@layout HxFragmentLayout`). |

### Fragment endpoints

Map any component as an HTML fragment endpoint with `RazorComponentResult` under the hood:

```csharp
app.MapHtmxGet<SearchFragment>("/fragments/search",
    ctx => new { Query = ctx.Request.Query["q"].ToString() });

app.MapHtmxPost<CounterFragment>("/fragments/counter",
    ctx => new { Count = int.Parse(ctx.Request.Query["count"]) });
```

Unsafe verbs (`MapHtmxPost/Put/Patch/Delete`) validate the antiforgery token by default (form field or request header — matching what `HxForm` and `HxButton` send). The validation filter reads the form first, so `ctx.Request.Form` is safe to use in the parameter factory.

Alternative, also supported: make the fragment component itself routable (`@page "..."` + `@layout HxFragmentLayout`) and let `MapRazorComponents` serve it — handy when you want `[SupplyParameterFromQuery]` binding.

## Host requirements

```csharp
builder.Services.AddRazorComponents();
// ...
app.UseAntiforgery();
app.MapRazorComponents<App>();
app.MapFragments(); // your fragment endpoints
```

And include htmx in the layout (already done via libman in the demo):

```html
<script src="lib/htmx/htmx.min.js" defer></script>
```

## Run the demo

```bash
dotnet run --project HtmxBlazor.Web
# then open /demo
```

The demo page showcases: a counter (POST + antiforgery header), active search (debounced input), lazy loading, polling, and a form whose response triggers a client-side event through `HX-Trigger` that another element listens to.
