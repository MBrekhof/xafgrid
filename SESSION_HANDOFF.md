# Session handoff — xafgrid

Last session: 2026-09-03. Task state lives on ContextBoard project `xafgrid` (board-only, no TODO.md).

## State

- P0–P3 done: harness + D1–D10 demos + Showcase (D1+D2 on one grid, README hero, CARD-1490), 12/12 Playwright tests green, evidence in `docs/screens/`,
  verdicts in `docs/findings.md`. Cards CARD-1472…1482 closed with their commit SHAs.
- Not done: D11 (filter-row/menu templates) and D12 (custom edit form) — deliberately postponed,
  same `DxGridModel` template properties; do them only if wanted.
- Repo: https://github.com/MBrekhof/xafgrid (public, MIT), branch `master`, account `MBrekhof`. HOW-TO-IMPLEMENT.md is the outside-reader guide — keep it in step when a demo pattern changes.

## Run / test

```
dotnet build XafGrid.sln
dotnet test XafGrid.Tests.Playwright        # launches bin/Debug/net8.0/XafGrid.Blazor.Server.exe on :5199, fresh SQLite
dotnet run --project XafGrid.Blazor.Server  # manual: http://localhost:5000, nav group "Grid Demos"
```

Set `XAFGRID_URL` to point the tests at an app you started yourself. Screenshots go to
`docs/screens/` and are committed.

## How a demo is wired

1. `XafGrid.Blazor.Server/DemoViewsGeneratorUpdater.cs` — adds the ListView node in code (xafml-only
   views get no columns) plus any model tweaks (extra columns, sorting).
2. `Model.xafml` — navigation item under "Grid Demos" (`IsNewNode="True"` is mandatory).
3. `Controllers/Demos/<Name>Controller.cs` — `ObjectViewController<ListView, T>` with `TargetViewId`,
   customizes `editor.GridModel` / `editor.Columns[].DxGridDataColumnModel` in `OnViewControlsCreated`.
4. Optional Razor component in `Components/Grid/`, CSS in `wwwroot/css/grid-demos.css`.
5. One `[Test]` in `XafGrid.Tests.Playwright/DemoTests.cs` + `ScreenshotAsync("dN-…")`.

Selectors and traps that cost time are all in `docs/findings.md` (splash wait, `tr[data-visible-index]`
rows, DxToolbar hidden clones → locate by role, drag needs a held pointer, sorted grids block drops).
