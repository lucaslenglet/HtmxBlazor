# HtmxBlazor.Components — Spécifications techniques

Document de référence pour comprendre le fonctionnement de la librairie et **ajouter de nouveaux composants** de façon cohérente.

## 1. Principes

1. **Blazor SSR statique = moteur de rendu HTML.** Pas de circuit SignalR, pas de WebAssembly, pas de `@rendermode`. Un composant est rendu une fois par requête, côté serveur, puis oublié.
2. **htmx = moteur d'interactivité.** Toute l'interactivité passe par des attributs `hx-*` déclaratifs. **Aucun JavaScript écrit à la main** (ni `hx-on`, ni `<script>` inline) : le seul JS de l'application est `htmx.min.js` — plus l'extension officielle `sse`, requise uniquement par `HxNotificationStream` (cf. P9).
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
│   ├── HxModalEndpointExtensions.cs           # MapHxModalClose()
│   └── HxDismissEndpointExtensions.cs         # MapHxDismiss() (endpoint vide partagé)
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
Utilisé par : compteur (démo), `HxTabs`, `HxTable` (tri + page dans l'URL, le serveur trie/pagine et le composant ne fait que rendre), `HxAccordion` (clé du panneau ouvert dans `?open=`), `HxWizard` (l'étape demandée dans l'URL, les valeurs déjà saisies re-postées à chaque action via des champs cachés — le bouton Retour POSTe aussi, avec `hx-include="closest form"` pour embarquer le jeton antiforgery et les champs sans déclencher la validation navigateur), sélection d'une suggestion `HxAutocomplete` (le fragment hôte se re-rend avec la valeur choisie). *C'est le pattern par défaut pour tout composant à état.*

### P2 — Cible séparée (`hx-target`)
Un élément déclencheur (input, bouton) vise un conteneur distinct par sélecteur CSS.
Utilisé par : recherche active, formulaire → `#greeting`, `HxAutocomplete` (input débounced → panneau de suggestions ; une requête vide doit renvoyer un corps vide pour vider le panneau).

### P3 — Placeholder différé
La page livre un placeholder ; un trigger `load` ou `revealed once` va chercher le vrai contenu et remplace le placeholder (`outerHTML`).
Utilisé par : `HxLazy`.
Variante **scroll infini** (`HxInfiniteScroll`) : la sentinelle en fin de liste (`intersect once` — pas `revealed`, cf. §8) va chercher la page suivante et est remplacée par elle : nouveaux items + nouvelle sentinelle. Le composant ne rend son wrapper que hors requête htmx (`IsHtmxRequest`, en tolérant boost et history restore) pour que les réponses de fragment soient nues.

### P4 — Polling
`hx-trigger="load, every Ns"` sur un conteneur qui se rafraîchit. Le serveur peut arrêter le polling en répondant **HTTP 286**.
Utilisé par : `HxPoll`.

### P5 — Événements serveur (`HX-Trigger`)
Un fragment signale un fait métier via `TriggerClientEvent("mon-event", detail)`. Ailleurs, des écouteurs `Trigger="@(HxTrigger.On("mon-event").From("body"))"` se rafraîchissent en réaction. Découplage total entre émetteur et écouteurs. (L'événement est dispatché sur l'élément qui a fait la requête et **bulle** jusqu'à `body` — d'où le `from:body`.)
Utilisé par : formulaire de la démo + badge compteur ; `HxDropdown` (le panneau écoute `hx-dropdown-close from:body` et se vide quand l'endpoint d'un item émet `HxDropdown.EventClose`) ; `HxAutocomplete` (même mécanisme avec `HxAutocomplete.EventClose`).

### P6 — Conteneur global + fermeture par swap vide
Un conteneur vide (`HxModalRoot`) reçoit des fragments à la demande. La **fermeture sans JS** : les contrôles de fermeture font un GET vers un endpoint qui renvoie une réponse vide en 200 (`MapHxModalClose`, ou `MapHxDismiss` — endpoint vide partagé), swappée en `outerHTML` sur `closest .hx-modal` → l'élément disparaît du DOM. La même réponse vide en `innerHTML` **vide** un conteneur au lieu de le supprimer.
Utilisé par : `HxModal`, `HxConfirm` (variante : le bouton de confirmation POSTe l'action réelle en ciblant `closest .hx-modal` en `outerHTML` — une réponse au corps vide ferme la modale, et la réponse peut mettre à jour le reste de la page via P5 ou P8), `HxDropdown` (ouverture par GET dans le panneau, fermeture par swap vide via le backdrop ou l'événement P5), `HxToast` (auto-dismiss : `hx-trigger="load delay:5s"` + GET vide en `outerHTML` sur lui-même), `HxAutocomplete` (fermeture du panneau de suggestions par backdrop ou événement P5), `HxDeleteRow` (variante : le DELETE de la ligne renvoie une réponse au corps principal vide swappée en `outerHTML` sur `closest tr` — la ligne disparaît — éventuellement accompagnée d'éléments oob, cf. P8).
⚠️ Répondre **200 + corps vide** (un 204 ne déclenche pas de swap).

### P7 — Composant composite (parent + items déclaratifs)
Quand le parent doit connaître ses enfants avant de rendre (onglets, colonnes, accordéon…) :
1. Le parent expose `ChildContent` et se rend en trois temps : `<CascadingValue Value="this" IsFixed="true">` → `@ChildContent` (les items s'enregistrent via `Parent.AddXxx(this)` dans leur `OnInitialized`, et **ne rendent rien** eux-mêmes) → `<HxDefer>` qui contient le vrai markup, rendu après l'enregistrement des items.
2. L'item est une classe `ComponentBase` pure avec `[CascadingParameter] internal Parent` ; il lève une exception si utilisé hors de son parent.
3. Dédoublonner à l'enregistrement (clé).

Utilisé par : `HxTabs`/`HxTab`, `HxTable`/`HxColumn` (générique : `@typeparam TItem` + `@attribute [CascadingTypeParameter(nameof(TItem))]` pour que les colonnes infèrent le type sans le répéter), `HxAccordion`/`HxAccordionItem`, `HxWizard`/`HxWizardStep` (chaque étape déclare ses champs via `Fields` pour que le parent ne re-rende pas en champ caché une valeur dont l'input est visible). Même pattern que `QuickGrid`.

### P8 — Mises à jour multi-zones (`hx-swap-oob`)
Une réponse de fragment peut mettre à jour d'autres zones que la cible principale : tout élément **au premier niveau de la réponse** portant `hx-swap-oob` est extrait avant le swap principal et swappé ailleurs. Deux formes :
- `hx-swap-oob="outerHTML"` : l'élément remplace celui du DOM qui porte le même `id` (ex. la liste re-rendue dans la réponse de suppression).
- `hx-swap-oob="afterbegin:#selecteur"` (positionnel) : ⚠️ htmx insère alors le **contenu** de l'élément oob, pas l'élément lui-même → prévoir un élément porteur autour du markup à insérer (cf. `HxToast`).

Combiné à P6, c'est la recette « action confirmée » : le POST de confirmation renvoie un corps principal vide (la modale se ferme) + la liste re-rendue en oob + un `HxToast` en oob.
Utilisé par : `HxToast`/`HxToastRoot`, démo de suppression (`HxConfirm`), démo `HxDeleteRow` (corps vide qui supprime la ligne + toast oob dans la même réponse).

### P9 — Flux serveur (SSE)
Pour pousser du contenu du serveur vers la page sans polling : l'extension htmx officielle **`sse`** ouvre un `EventSource` (`hx-ext="sse"` + `sse-connect="url"`) et swappe la charge utile HTML de chaque événement dans l'élément portant `sse-swap="nom-d-événement"`, selon son `hx-swap` (`afterbegin` pour un fil de notifications). L'endpoint répond en `Content-Type: text/event-stream`, écrit un fragment HTML par ligne `data:` (+ ligne vide), flushe après chaque événement et s'arrête sur `RequestAborted`.
⚠️ **C'est la seule entorse au « un seul JS »** : l'extension `htmx-ext-sse` est un script tiers de plus à charger (toujours zéro JS écrit à la main) — à signaler dans la doc de tout composant qui l'exige (checklist §7.1).
Utilisé par : `HxNotificationStream`.

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
- **`revealed` vs `intersect`** : `revealed` n'écoute que le scroll de la fenêtre ; dans un conteneur `overflow: auto`, utiliser `intersect` (IntersectionObserver, qui tient compte du clipping par les ancêtres scrollables).
- **`hx-swap-oob` positionnel** (`afterbegin:…`, `beforeend:…`) : htmx insère le **contenu** de l'élément oob, pas l'élément — envelopper le markup dans un porteur (cf. `HxToast`). Seuls `true`/`outerHTML` swappent l'élément lui-même (apparié par `id`). Et les éléments oob doivent être **au premier niveau** de la réponse.
- **SSE et tests navigateur** : une page qui contient un `HxNotificationStream` garde une connexion ouverte en permanence → tout `waitUntil: "networkidle"` (Playwright) ne se résout jamais. Attendre `load` à la place.
- **Bouton POST hors soumission de formulaire** (ex. le Retour du `HxWizard`) : un `type="button"` avec `hx-post` n'embarque rien par défaut — ajouter `hx-include="closest form"` pour envoyer les champs **et** le champ caché antiforgery. Bonus : la validation HTML native (`required`…) ne se déclenche pas, ce qui est le comportement attendu pour un retour en arrière.

## 9. Pistes de composants futurs

(Les composants anciennement listés ici — `HxInfiniteScroll`, `HxToast`, `HxConfirm`, `HxDropdown`, `HxTable`, puis `HxAccordion`, `HxAutocomplete`, `HxWizard`, `HxDeleteRow` et `HxNotificationStream` — sont implémentés ; voir les patterns §6 qu'ils illustrent.)

| Composant | Pattern pressenti |
|---|---|
| `HxEditInPlace` | P1 : un libellé cliquable GET sa version formulaire, la sauvegarde POSTe et re-rend le libellé. |
| `HxRating` | P1 : rangée d'étoiles, chaque étoile POSTe sa note et le composant se re-rend. |
| `HxTreeView` | P3 : nœuds repliés livrés avec la page, enfants chargés à la demande au dépliage. |
| `HxProgress` | P4 : barre de progression pollée, le serveur arrête le polling en 286 quand c'est terminé. |
| `HxCarousel` | P1 : l'index de la diapositive dans l'URL, précédent/suivant en self-swap. |
