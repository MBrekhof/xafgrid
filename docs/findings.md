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

## How the demo ListViews are declared (2026-09-02)

- **A `<ListView>` added only in `Model.xafml` gets no `Columns` node** — the grid renders with the
  expand/selection columns and nothing else. The Model Editor avoids this by persisting the
  generated columns into the xafml. Views created in code via
  `ModelNodesGeneratorUpdater<ModelViewsNodesGenerator>` (`viewsNode.AddNode<IModelListView>(id)`
  + `ModelClass`) do get default columns (dxdocs 113315) → `DemoViewsGeneratorUpdater`.
- Nodes that don't exist in the generated model need `IsNewNode="True"` in a diff layer, or the
  loader silently drops them (that is what the Model Editor writes). Navigation items pointing
  at a missing view are filtered out, so the nav group vanishes too.
- `TargetViewId` on an `ObjectViewController<ListView, Order>` scopes each demo to its own view.

## D1 — master-detail row (`screens/d1-master-detail.png`)

Verdict: **works.** `GridModel.DetailRowTemplate = ctx => builder => …` with a Razor component
(`OrderDetailRow`) hosting a nested `DxGrid` over `Order.Lines`; `AutoCollapseDetailRow` keeps one
row open. Expand buttons appear automatically once a template is set.

- `ctx.DataItem` is the entity in Client mode and a `BlazorObjectRecord` in server modes;
  `DxGridListEditorBase.GetObject(dataItem)` is public and resolves both (spike S4 closed).
- Lazy loading works inside the template: `Order.Lines` and `line.Product` load through the view's
  object space.
- XAF 26.1 already has a built-in *preview row* (`ListEditorPreviewRowViewController`,
  `IModelListView.PreviewColumnName`): one column's property editor, always expanded. The extra
  here is on-demand expansion with arbitrary content.

## D2 — rich cells (`screens/d2-cells.png`)

Verdict: **works, per column only.** XAF renders every data column through its own
`DxGridDataColumnModel.CellDisplayTemplate` (the property editor's view component,
`DxGridListEditor.CreateDataColumnModel`). A per-column template beats the grid-level
`DataColumnCellDisplayTemplate`, so setting `GridModel.DataColumnCellDisplayTemplate` does nothing —
override `column.DxGridDataColumnModel.CellDisplayTemplate` on the `DxGridColumnWrapper` instead
(spike S5 closed). The edit template is untouched, so inline editing keeps XAF's editors.
`Status` → badge, `Customer` → avatar initials + stars, `Total` → inline bar (`OrderCell.razor`).

## D3 — heat-map (`screens/d3-heatmap.png`)

Verdict: **works.** `GridModel.CustomizeElement += e => …` (use `+=`: XAF chains its own handler for
detail cells). Row class by order age, inline `Style` for cancelled rows, cell class on `Total`.
`e.Column.Name` is unset — match on `(e.Column as IGridDataColumn).FieldName`. Row backgrounds need
`.dxbl-grid tr.cls > td { background-color }` to beat the theme's cell styles.
