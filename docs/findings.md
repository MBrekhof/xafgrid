# Findings

Per-demo verdicts: what worked, what XAF's `DxGridListEditor` blocks, and the dxdocs reference.
Screenshots live in `docs/screens/`.

## P0 — harness (2026-09-02)

Verdict: **works**. `dotnet test XafGrid.Tests.Playwright` builds the app, launches the exe on
`http://localhost:5199` against a fresh SQLite file, seeds 1 500 orders and screenshots the Orders
grid (`screens/p0-orders.png`).

Lessons that every later test relies on:

- **XAF Blazor routes are `/{ViewId}`** — `GotoAsync(".../Order_ListView")` opens the view directly;
  no navigation clicking needed.
- **The app answers HTTP 200 before XAF is ready.** `_Host` renders immediately; XAF setup + schema
  update + seed happen on the first circuit. `Expect` default (5 s) is too short on a cold start —
  `SetDefaultExpectTimeout(60_000)`.
- **The splash overlays a fully rendered DOM.** `_Host.cshtml` renders a static
  `#applicationLoadingPanel` and `<app class="d-none">`; XAF's `loadingPanelObserver.ts` removes
  the panel when the circuit is up. Playwright's `ToBeVisible` passed on grid rows while the
  splash still covered them → wait for `#applicationLoadingPanel` to be hidden before asserting or
  screenshotting.
- **DX 26.1 data rows have no `dxbl-grid-data-row` class.** Rows are
  `.dxbl-grid tbody tr[data-visible-index]` (class `cursor-pointer xaf-prevent-contextmenu`);
  filler rows are `.dxbl-grid-empty-row`. The class the `xaf-playwright-testing` skill uses is
  from an older DX version.
- Template default `DatabaseVersionMismatch` handler throws unless a debugger is attached —
  replaced by `DatabaseUpdateMode.UpdateDatabaseAlways` + unconditional `e.Updater.Update()`
  (demo app, throw-away SQLite).
- `[Aggregated]` lives in `DevExpress.ExpressApp.DC`, not `DevExpress.Persistent.Base`.
