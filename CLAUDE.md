# HtmxBlazor

Librairie de composants HTML interactifs : **Blazor SSR statique** pour le rendu, **htmx** pour l'interactivité, **zéro JavaScript écrit à la main** (pas de `hx-on`, pas de `<script>` inline — les seuls JS sont `htmx.min.js` et l'extension officielle `sse`, requise uniquement par `HxNotificationStream`).

## Référence obligatoire

**Avant d'ajouter ou modifier un composant, lire [`docs/SPECS.md`](docs/SPECS.md)** : architecture, contrats des classes de base, les 9 patterns d'interaction établis, la checklist en 13 points pour un nouveau composant, et les pièges connus. Tout nouveau composant doit suivre un pattern existant (ou en documenter un nouveau au §6 des specs).

## Structure

- `HtmxBlazor.Components/` — la librairie (RCL) : primitives dans `Htmx/`, composants dans `Components/<Famille>/` (un sous-dossier par famille : `Primitives/`, `Modal/`, `Table/`…), mapping d'endpoints dans `Endpoints/`, CSS par défaut dans `wwwroot/htmx-blazor.css`.
- `HtmxBlazor.Web/` — appli de démo ; chaque composant a sa section dans `Components/Pages/Demo.razor`, son fragment dans `Components/Fragments/<Famille>/` (sous-dossiers en miroir de la librairie, namespace unique `HtmxBlazor.Web.Components.Fragments` via `@namespace`) et son mapping dans `Endpoints/FragmentEndpoints.cs`.
- `HtmxBlazor.AppHost` / `HtmxBlazor.ServiceDefaults` — orchestration .NET Aspire.

## Rappels critiques

- Composants : préfixe `Hx`, `public` (jamais `internal` — la balise ne serait pas résolue), `@namespace HtmxBlazor.Components` en première ligne des `.razor`, hériter `HxComponentBase` ou `HxInteractiveComponentBase`.
- Statique SSR : pas de `@onclick`/`EventCallback`/`IJSRuntime` — toute interactivité passe par des attributs `hx-*`.
- Verbes non sûrs : toujours antiforgery (champ caché via `HxForm`, header via `HxButton`, validation via `MapHtmxPost/Put/Patch/Delete`).

## Commandes

```bash
dotnet build HtmxBlazor.Web/HtmxBlazor.Web.csproj   # build (la cible est net9.0 ; un SDK plus récent convient)
dotnet run --project HtmxBlazor.Web                 # lancer la démo, puis ouvrir /demo
```

Vérification d'un composant : build, curl des fragments avec `-H "HX-Request: true"` (fragment nu attendu, pas de document complet), rejet 400 des POST sans jeton, et test navigateur du clic (Playwright + Chromium) — détail au §7.12 des specs.
