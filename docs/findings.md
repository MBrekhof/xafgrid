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

## D4 — custom grouping (`screens/d4-groups.png`)

Verdict: **works.** On the column wrapper's `DxGridDataColumnModel`: `GroupIndex = 0`,
`GroupInterval = Custom`, `SortMode = Custom`; then `GridModel.CustomGroup` (`SameGroup` by bucket),
`CustomSort` (bucket order) and `CustomizeGroupValueDisplayText` (bucket label). Group summaries are
added through XAF's wrapper collection — `editor.GridSummary.GroupSummary.Add(new
DxGridSummaryItemWrapper(new DxGridSummaryItemModel { FieldName, SummaryType, ValueDisplayFormat }))`
— not through the `RenderFragment` on `DxGridModel.GroupSummary`. `DataColumnGroupRowTemplate` gets
`GridDataColumnGroupRowTemplateContext`; `context.Grid.GetGroupSummaryItems()` +
`GetGroupSummaryValue/FormattedValue(item, context.VisibleIndex)` give the numbers for the bar.
Virtual scrolling renders only the visible groups (test asserts the first bucket, not all of them).

## D5 — custom summaries (`screens/d5-summary.png`)

Verdict: **works.** Total summaries in code via `editor.GridSummary.TotalSummary.Add(...)` (Count,
Sum, Avg and a `GridSummaryItemType.Custom` item with a `Name`). `CustomSummary` runs
Start/Calculate/Finalize per item — `e.Grid.IsDataItemSelected(e.DataItem)` gives "sum of selected";
`CustomizeSummaryDisplayText` labels it. Selection changes don't recalc by themselves: handle
`View.SelectionChanged` and call `editor.GridInstance?.RefreshSummary()`. `ColumnFooterTemplate`
stacks the items (`context.SummaryItems`, `context.Grid.GetTotalSummaryDisplayText(item)`);
`FooterDisplayMode = Always`.

## D6 — in-grid toolbar (`screens/d6-toolbar.png`)

Verdict: **works.** `ToolbarTemplate` gets `GridToolbarTemplateContext.Grid` (the live `IGrid`), so a
`DxToolbar` can call `GroupBy`, `ExpandAllGroupRows`, `AutoFitColumnWidths`, `ShowFilterBuilder`,
`ClearFilter`, `ExportToXlsxAsync("orders", new GridXlExportOptions())` — the XLSX/CSV download works
inside XAF (built-in Export action only does PDF). `EmptyDataAreaTemplate` gets the grid too.
Playwright: DxToolbar renders a hidden `dxbl-virtual-el` clone of each item — locate items by role
(`GetByRole(Button, Name)`), not by text.

## D7 — unbound columns (`screens/d7-unbound.png`)

Verdict: **works.** `editor.AddColumnModel(new DxGridDataColumnModel { FieldName, Caption,
UnboundType, Width })` for code-computed columns (`GridModel.UnboundColumnData`, resolve the entity
with `editor.GetObject(e.DataItem)`), `UnboundExpression = "[Total] * 0.2"` for criteria-computed
ones, and a `CellDisplayTemplate` with a `DxButton` for an action column. The button must sit in an
element with `AddEventStopPropagationAttribute("onclick")`, otherwise XAF's row click opens the
DetailView. Editing through the view's `ObjectSpace` + `CommitChanges()` refreshes the row in place.
Added columns are appended **sorted by FieldName** (Actions, DaysOpen, LineCount, Margin), not in
call order — set `Caption`s accordingly or expect to reorder. Column-chooser survival (spike S2)
not checked.

## D8 — drag rows to reorder (`screens/d8-reorder.png`)

Verdict: **works.** `GridModel.AllowDragRows = true` (spike S1 closed: the property is declared on
`DxGridBaseModel`, which is why dxdocs' `DxGridModel` member list omits it; `SetAttribute` exists
as a general fallback), `AllowedDropTarget = Internal`, `DragHintTextTemplate`, and
`ItemsDropped = EventCallback.Factory.Create<GridItemsDroppedEventArgs>(this, …)`. The handler
renumbers `Product.SortOrder`, commits and calls `View.Refresh()`.

- **DxGrid refuses between-row drops while a column sort is active** — the drag starts (hint
  shows) but no drop indicator appears and `ItemsDropped` never fires. XAF's column generator puts
  `SortIndex 0` on the `DefaultProperty` column (Name), so clear it (`Columns["Name"].SortIndex =
  -1`) and order the *data* through the model `Sorting` node (`IModelSortProperty`) instead.
- Playwright: `DragToAsync` does not start DevExpress's pointer-based drag; hold the anchor
  (`td.dxbl-grid-row-drag-anchor-cell`) ~250 ms after `Mouse.Down`, then move in ≥10 steps.

## D9 — layout presets in the database (`screens/d9-presets.png`)

Verdict: **works.** `IGrid.SaveLayout()` → `JsonSerializer.Serialize(GridPersistentLayout)` into a
`GridLayoutPreset` entity (ViewId, Name, LayoutJson) through the view's `ObjectSpace`;
`LoadLayout(JsonSerializer.Deserialize<GridPersistentLayout>(json))` restores sort/columns. The
preset toolbar (`PresetToolbar.razor`) lives in `ToolbarTemplate` and owns the visible list.
Spike S6: XAF's own layout sync coexists — loading a preset also updates the model, harmless.
Gotchas: `DxTextBox` needs `BindValueMode.OnInput` for a button enabled on typing; `ClearSort()`
removes *every* sort including XAF's default Number sort → natural DB order.

## D10 — wide grid (`screens/d10-wide.png`)

Verdict: **works.** Extra nested-property columns (`Customer.Country`, … `Employee.Title`) added in
the generator updater (`Columns.AddNode<IModelColumn>(path)` + `PropertyName`), then per column on
`DxGridDataColumnModel`: `Width`, `MinWidth`, `FixedPosition` (Left for Number, Right for Total —
spike S3 closed), `FilterMenuButtonDisplayMode = Never` for nested columns, and a
`HeaderCaptionTemplate` with a tooltip `title`. `VirtualScrollingMode = RowsAndColumns` on top of
the model's `VirtualScrollingEnabled` virtualizes columns too. Fixed columns stay put while the
rest scrolls (test compares header bounding boxes).

## Not done

D11 (filter-row / filter-menu templates, custom search box) and D12 (custom edit form template)
are left out — lower value than the ten above; the same `DxGridModel` template properties apply.
