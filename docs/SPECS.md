# HtmxBlazor.Components — Spécifications techniques

Document de référence pour comprendre le fonctionnement de la librairie et **ajouter de nouveaux composants** de façon cohérente.

## 1. Principes

1. **Blazor SSR statique = moteur de rendu HTML.** Pas de circuit SignalR, pas de WebAssembly, pas de `@rendermode`. Un composant est rendu une fois par requête, côté serveur, puis oublié.
2. **htmx = moteur d'interactivité.** Toute l'interactivité passe par des attributs `hx-*` déclaratifs. **Aucun JavaScript écrit à la main** (ni `hx-on`, ni `<script>` inline) : le seul JS de l'application est `htmx.min.js`.
3. **L'état vit côté serveur ou dans les URLs.** Pas d'état caché dans le DOM, pas de classe togglée par JS. Ce que le client voit est exactement ce que le serveur a rendu en dernier.
4. **Un composant = rendu initial + fragment.** Le même `.razor` sert au rendu de la page complète et aux réponses partielles htmx. Zéro duplication de template.
5. **Dégradation gracieuse quand c'est possible** (ex. `HxLink` reste un `<a href>` fonctionnel sans JS).

## 2. Le pipeline

```mermaid
sequenceDiagram
    participant B as Navigateur (htmx)
    participant P as Page Blazor SSR<br/>(MapRazorComponents)
    participant F as Endpoint fragment<br/>(MapHtmxGet/Post&lt;T&gt;)

    B->>P: GET /demo
    P-->>B: page HTML complète (composants rendus inline)
    Note over B: htmx scanne les attributs hx-*
    B->>F: hx-get/hx-post (header HX-Request)
    F->>F: filtre antiforgery (verbes non sûrs)
    F->>F: RazorComponentResult<T> (rendu sans layout)
    F-->>B: fragment HTML + headers HX-* éventuels
    Note over B: swap dans le DOM (hx-target / hx-swap)<br/>+ événements HX-Trigger dispatchés
```

Deux voies pour servir un fragment :

| Voie | Quand l'utiliser |
|---|---|
| **Minimal API** : `app.MapHtmxGet<T>(route, ctx => new { … })` | Cas général. Paramètres explicites extraits de la requête, validation antiforgery intégrée sur les verbes non sûrs. |
| **Composant routable** : `@page "…"` + `@layout HxFragmentLayout` | Quand on veut le binding `[SupplyParameterFromQuery]` et le routeur Blazor. Le layout vide évite d'envelopper le fragment dans le document complet. |

## 3. Structure de la librairie

```
HtmxBlazor.Components/
├── Htmx/                            # primitives, aucune dépendance aux composants
│   ├── HtmxRequestHeaderNames.cs    # constantes des headers HX-* entrants
│   ├── HtmxResponseHeaderNames.cs   # constantes des headers HX-* sortants
│   ├── HtmxRequestExtensions.cs     # Request.IsHtmx(), HtmxTarget(), …
│   ├── HtmxResponseExtensions.cs    # Response.HxTrigger(), HxRedirect(), HxReswap(), …
│   ├── HxSwap.cs                    # enum hx-swap + ToAttributeValue()
│   ├── HxTrigger.cs                 # builder fluide hx-trigger
│   └── HxTiming.cs                  # TimeSpan → "500ms" / "2s"
├── Components/
│   ├── HxComponentBase.cs           # base : HttpContext, IsHtmxRequest, TriggerClientEvent
│   ├── HxInteractiveComponentBase.cs# base : paramètres hx-* typés + AllAttributes()
│   ├── HxDefer.cs                   # infra : rendu différé (pattern QuickGrid)
│   └── Hx*.razor / Hx*.cs           # les composants publics
├── Endpoints/
│   ├── HtmxEndpointRouteBuilderExtensions.cs  # MapHtmxGet/Post/Put/Patch/Delete<T>
│   └── HxModalEndpointExtensions.cs           # MapHxModalClose()
├── wwwroot/htmx-blazor.css          # styles par défaut (asset statique RCL)
└── _Imports.razor
```

**Dépendances** : uniquement `FrameworkReference Microsoft.AspNetCore.App`. Pas de package NuGet, pas de JS embarqué.

## 4. Contrats des classes de base

### `HxComponentBase`

À hériter par **tout composant de la librairie**.

| Membre | Rôle |
|---|---|
| `HttpContext` (CascadingParameter) | Contexte de la requête. Fourni par l'infra SSR (pages et `RazorComponentResult`). Nullable : toujours utiliser `?.`. |
| `AdditionalAttributes` (CaptureUnmatchedValues) | Tout attribut non reconnu est splatté sur l'élément racine du composant. |
| `IsHtmxRequest` | `true` si la requête vient de htmx (header `HX-Request`). Sert à distinguer rendu inline / fragment. |
| `TriggerClientEvent(name, detail?, timing?)` | Émet un événement client via le header `HX-Trigger` (detail sérialisé en JSON). |

### `HxInteractiveComponentBase : HxComponentBase`

À hériter par tout composant **qui émet des requêtes htmx** et veut exposer les attributs standards.

- Un paramètre typé par attribut htmx cœur : `Get`, `Post`, `Put`, `Patch`, `Delete`, `Trigger`, `Swap` (+`SwapModifiers`), `Target`, `Select`, `Indicator`, `DisabledElt`, `Vals`, `Include`, `Confirm`, `PushUrl`, `Sync`.
- `AllAttributes()` fusionne ces paramètres (non nuls uniquement) avec `AdditionalAttributes` — **les attributs de l'utilisateur gagnent** en cas de collision. À splatter sur l'élément racine : `<button @attributes="AllAttributes()">`.

### Règles de rendu Razor à connaître

- Un attribut dont la valeur est `null` **n'est pas rendu** → écrire `hx-get="@Url"` sans condition.
- Les attributs explicites écrits **après** un `@attributes` l'emportent sur le splat.
- Tag dynamique (`<@Tag>`) impossible en Razor → composant en C# pur avec `RenderTreeBuilder` (cf. `HxElement`).

## 5. Antiforgery (verbes non sûrs)

Deux mécanismes complémentaires, tous deux acceptés par `IAntiforgery.ValidateRequestAsync` :

| Contexte | Mécanisme | Implémentation |
|---|---|---|
| Formulaire (`HxForm`) | Champ caché `__RequestVerificationToken`, soumis par htmx avec le reste du form | `<AntiforgeryToken />` dans le composant |
| Élément isolé (`HxButton`) | Header `RequestVerificationToken` via `hx-headers` | `IAntiforgery.GetAndStoreTokens(HttpContext)` dans `OnParametersSet`, sérialisé en JSON |

Côté serveur, `MapHtmxPost/Put/Patch/Delete` ajoutent un endpoint filter qui appelle `ValidateRequestAsync` (400 si invalide), désactivable via `validateAntiforgery: false`.

⚠️ **Effet de bord utile** : le filtre lit le form de façon asynchrone avant le handler → `ctx.Request.Form` est ensuite accessible en synchrone dans la fabrique de paramètres.

## 6. Patterns établis

Réutiliser ces recettes pour tout nouveau composant.

### P1 — Auto-remplacement (`outerHTML` self-swap)
Le fragment porte un `id` (ou est ciblé par `closest`), contient lui-même les contrôles qui le re-rendent, et l'état voyage dans l'URL.
Utilisé par : compteur (démo), `HxTabs`. *C'est le pattern par défaut pour tout composant à état.*

### P2 — Cible séparée (`hx-target`)
Un élément déclencheur (input, bouton) vise un conteneur distinct par sélecteur CSS.
Utilisé par : recherche active, formulaire → `#greeting`.

### P3 — Placeholder différé
La page livre un placeholder ; un trigger `load` ou `revealed once` va chercher le vrai contenu et remplace le placeholder (`outerHTML`).
Utilisé par : `HxLazy`.

### P4 — Polling
`hx-trigger="load, every Ns"` sur un conteneur qui se rafraîchit. Le serveur peut arrêter le polling en répondant **HTTP 286**.
Utilisé par : `HxPoll`.

### P5 — Événements serveur (`HX-Trigger`)
Un fragment signale un fait métier via `TriggerClientEvent("mon-event", detail)`. Ailleurs, des écouteurs `Trigger="@(HxTrigger.On("mon-event").From("body"))"` se rafraîchissent en réaction. Découplage total entre émetteur et écouteurs.
Utilisé par : formulaire de la démo + badge compteur.

### P6 — Conteneur global + fermeture par swap vide
Un conteneur vide (`HxModalRoot`) reçoit des fragments à la demande. La **fermeture sans JS** : les contrôles de fermeture font un GET vers un endpoint qui renvoie une réponse vide en 200 (`MapHxModalClose`), swappée en `outerHTML` sur `closest .hx-modal` → l'élément disparaît du DOM.
Utilisé par : `HxModal`. ⚠️ Répondre **200 + corps vide** (un 204 ne déclenche pas de swap).

### P7 — Composant composite (parent + items déclaratifs)
Quand le parent doit connaître ses enfants avant de rendre (onglets, colonnes, accordéon…) :
1. Le parent expose `ChildContent` et se rend en trois temps : `<CascadingValue Value="this" IsFixed="true">` → `@ChildContent` (les items s'enregistrent via `Parent.AddXxx(this)` dans leur `OnInitialized`, et **ne rendent rien** eux-mêmes) → `<HxDefer>` qui contient le vrai markup, rendu après l'enregistrement des items.
2. L'item est une classe `ComponentBase` pure avec `[CascadingParameter] internal Parent` ; il lève une exception si utilisé hors de son parent.
3. Dédoublonner à l'enregistrement (clé).

Utilisé par : `HxTabs`/`HxTab`. Même pattern que `QuickGrid`.

## 7. Checklist : ajouter un nouveau composant

1. **Choisir le pattern** (§6) et vérifier qu'il ne requiert aucun JS. Si un JS minimal est inévitable, le signaler explicitement dans la doc du composant et proposer l'alternative sans JS.
2. **Fichier** dans `HtmxBlazor.Components/Components/`, en `.razor` sauf besoin de tag dynamique ou de logique de rendu (→ `.cs` + `RenderTreeBuilder`).
3. **Directives** : `@namespace HtmxBlazor.Components` en première ligne (le dossier ne fait pas foi) et `@inherits HxComponentBase` ou `HxInteractiveComponentBase`.
4. **Visibilité `public`** — le compilateur Razor ne résout pas les composants `internal` en balise.
5. **Nommage** : préfixe `Hx`. Paramètres : `Url` (endpoint fragment), `Id` quand un ciblage est nécessaire, constantes `public const` pour les valeurs par défaut partagées (`DefaultCloseRoute`, `DefaultTarget`…).
6. **Paramètres** : typés, `[EditorRequired]` quand indispensable, XML doc sur chaque paramètre. Splatter `AllAttributes()` (interactif) ou `AdditionalAttributes` (statique) sur la racine.
7. **CSS** : classes préfixées `hx-` dans `wwwroot/htmx-blazor.css`, minimalistes et surchargeables. Pas de style inline.
8. **Accessibilité** : rôles ARIA (`role="tablist"`, `aria-selected`, `aria-modal`…) dès la conception.
9. **Endpoint** : si le composant nécessite un endpoint générique, l'exposer en extension `MapHxXxx()` dans `Endpoints/`.
10. **Démo** : fragment `Demo*.razor` dans `HtmxBlazor.Web/Components/Fragments/` (avec `public const string Route`), mapping dans `FragmentEndpoints.cs`, section dans `Pages/Demo.razor`.
11. **Vérifier** :
    - `dotnet build` sans erreur ;
    - curl des fragments (`-H "HX-Request: true"`) : fragment nu, pas de document complet, headers `HX-*` attendus ;
    - rejet 400 des POST sans jeton antiforgery le cas échéant ;
    - test navigateur (Playwright + Chromium) du comportement au clic.
12. **Documenter** : ligne dans le tableau des composants du README ; nouveau pattern → l'ajouter au §6.

## 8. Pièges connus

- **Composant `internal`** → rendu comme balise HTML littérale, sans erreur de compilation. Toujours `public`.
- **`Request.Form` en synchrone** → exception Kestrel, sauf si le filtre antiforgery a déjà lu le form (§5). Pour un endpoint sans validation, lire via `ReadFormAsync`.
- **204 No Content** → htmx ne swappe pas. Pour « vider » un élément, renvoyer 200 + corps vide.
- **Statique SSR** : `OnAfterRender`, `IJSRuntime`, `EventCallback`/`@onclick` ne servent à rien — toute interactivité passe par htmx.
- **`[SupplyParameterFromQuery]`** ne fonctionne pas avec `RazorComponentResult` — passer les paramètres via la fabrique du `MapHtmx*`.
- **Assets RCL** : le CSS de la librairie est servi sous `_content/HtmxBlazor.Components/…` et doit être référencé via `@Assets[...]` (fingerprinting de `MapStaticAssets`).
- **Cache navigateur vs fragments** : si une même URL sert page complète et fragment, varier la réponse sur `HX-Request` (header `Vary`) — non nécessaire tant que les fragments ont des routes dédiées `/fragments/…`.

## 9. Pistes de composants futurs

| Composant | Pattern pressenti |
|---|---|
| `HxInfiniteScroll` | P3 : `revealed` sur le dernier élément de liste, swap `afterend`. |
| `HxToast` | P5 + swaps *out-of-band* (`hx-swap-oob`) vers un conteneur global de notifications. |
| `HxConfirm` | P6 : variante de modale rendant un formulaire de confirmation qui POSTe l'action réelle. |
| `HxDropdown` | P6 simplifié : ouverture par GET, fermeture par swap vide. |
| `HxTable` (tri/pagination) | P1 : l'état tri+page dans l'URL, la table entière se re-rend. |
