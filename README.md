# Portfolio — Denny Kim's Design OS

> **Run it:** from the repo folder in Terminal —
> `dotnet run --project portfolio/Portfolio.csproj` → http://localhost:5290

A Blazor portfolio site built around a real storefront component library.
The portfolio itself is the showcase: AI-orchestrated development, mirroring
a production .NET + Blazor stack.

## Run it

Requires the .NET 10 SDK.

```bash
dotnet run --project portfolio/Portfolio.csproj
```

Then open http://localhost:5290 (workshop: http://localhost:5290/workshop)

## Structure

- `portfolio/` — the portfolio site (Home, Component workshop, Build log,
  Experiments, Archive, About). net9.0 + InteractiveServer, no NuGet packages.
- `storefront-sandbox/Storefront.Client/` — the 38 Blazor storefront components
  (vehicle picker, refine search, assembly results, product cards, cart, …).
  Compiled into the portfolio via linked files, not copied — edits here show up
  on the next build.

The components run against in-memory dummy data (`portfolio/Demo/`) with
service/model stubs (`portfolio/Stubs/`). No database, no backend, no real
company data.
