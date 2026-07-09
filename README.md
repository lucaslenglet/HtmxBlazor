# HtmxBlazor

Experiments combining **Blazor static SSR** (HTML rendering engine) with **[htmx](https://htmx.org)** (front-end interactivity engine) — with as little JavaScript as possible.

> 📖 **[docs/SPECS.md](docs/SPECS.md)** — spécifications techniques de la librairie : architecture, contrats des classes de base, patterns établis, et la checklist à suivre pour ajouter de nouveaux composants.

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
| `HxTabs` / `HxTab` | Server-driven tabs: each header GETs the fragment with `?tab=key`, the whole tab set is re-rendered (`outerHTML`). Only the active panel is rendered. |
| `HxModal` / `HxModalRoot` / `HxModalClose` | Modal fetched as a fragment into the root container; closing swaps an empty response over it (`app.MapHxModalClose()`), no JS. |
| `HxConfirm` | Confirmation modal: confirming POSTs the action (antiforgery header included) and swaps the response over the modal — an empty response closes it, out-of-band elements update the rest of the page. |
| `HxInfiniteScroll` | Infinite list: a sentinel fetches the next page when it becomes visible (`intersect once`) and is replaced by it. |
| `HxToast` / `HxToastRoot` | Notification prepended out-of-band (`hx-swap-oob`) into a global container; dismissed by the × or automatically through a delayed GET to the empty endpoint (`app.MapHxDismiss()`). |
| `HxDropdown` / `HxDropdownPanel` / `HxDropdownItem` | Dropdown panel fetched as a fragment; closes on backdrop click or when an endpoint raises the `hx-dropdown-close` event through `HX-Trigger`. |
| `HxTable` / `HxColumn` | Sortable, pageable table: the sort/page state travels in the URL and the whole table re-renders (`outerHTML` self-swap). The endpoint sorts and pages the data. |
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

See [docs/SPECS.md](docs/SPECS.md) for the full picture: request pipeline, base class contracts, antiforgery flows, the established interaction patterns (self-swap, deferred placeholder, server-triggered events, composite components, ...), known pitfalls, and the step-by-step checklist for adding a new component.

## Host requirements

```csharp
builder.Services.AddRazorComponents();
// ...
app.UseAntiforgery();
app.MapRazorComponents<App>();
app.MapFragments(); // your fragment endpoints
```

And include htmx plus the library's default stylesheet (tabs/modal) in the layout:

```html
<script src="lib/htmx/htmx.min.js" defer></script>
<link rel="stylesheet" href="@Assets["_content/HtmxBlazor.Components/htmx-blazor.css"]" />
```

### Tabs & modal in a nutshell

```razor
@* Tabs: this component is served both inline and as a fragment endpoint *@
<HxTabs Id="my-tabs" Url="/fragments/my-tabs" Active="@Active">
    <HxTab Key="one" Title="First">…</HxTab>
    <HxTab Key="two" Title="Second">…</HxTab>
</HxTabs>

@* Modal: a button fetches the modal fragment into the root *@
<HxButton Get="/fragments/my-modal" Target="@HxModalRoot.DefaultTarget">Open</HxButton>
<HxModalRoot />
```

```csharp
app.MapHtmxGet<MyTabs>("/fragments/my-tabs", ctx => new { Active = ctx.Request.Query["tab"].ToString() });
app.MapHtmxGet<MyModal>("/fragments/my-modal");
app.MapHxModalClose(); // empty endpoint used by every modal close control
app.MapHxDismiss();    // empty endpoint used by toasts and dropdowns
```

## Run the demo

```bash
dotnet run --project HtmxBlazor.Web
# then open /demo
```

The demo page showcases: a counter (POST + antiforgery header), active search (debounced input), lazy loading, polling, server-driven tabs, a modal, infinite scroll, out-of-band toasts, a confirm dialog updating a list out-of-band, a dropdown closed by a server-triggered event, a sortable paged table, and a form whose response triggers a client-side event through `HX-Trigger` that another element listens to.
