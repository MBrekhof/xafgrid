# xafgrid — DxGrid extras beyond the standard XAF ListView

Date: 2026-09-02 · Status: draft for review

## Goal

One XAF Blazor Server app (DevExpress **26.1**, EF Core, SQLite, no security) whose ListViews each
demonstrate **one thing the DevExpress Blazor `DxGrid` can do that XAF's Model Editor cannot** —
done in code from a ViewController against `DxGridListEditor`. Every demo ships with a Playwright
(C#/NUnit) test that renders the view, asserts the behaviour and stores a screenshot. The repo is
also a findings log: what worked, what XAF's list editor blocks, with the dxdocs reference.

## What XAF already gives you (baseline — NOT demoed)

Verified against dxdocs 26.1 so we don't re-invent built-ins:

| Feature | XAF mechanism |
|---|---|
| Bands (grouped headers) | `BandsLayout` node in Model Editor → `DxGridBandColumn` (docs 113695 / 113694) |
| Conditional colours / enabled / visible | Conditional Appearance module |
| Column chooser, resize, reorder, Reset View Settings | Built in (113679) |
| Layout persistence | Application Model, one layout per view per user |
| Group / total summaries | `IModelColumn.SummaryType` |
| Inline edit modes (EditRow / EditCell / PopupEditForm) | `IModelListView.InlineEditMode` (113249) |
| Virtual scrolling (rows) | `IModelListViewBlazor.VirtualScrollingEnabled` |
| Context menus, PDF export, selection column, filter row/panel, search, focused row | Built in |
| Master–detail as list + detail form | `MasterDetailMode.ListViewAndDetailView` |

## The API surface we build on

All from `DevExpress.ExpressApp.Blazor.Editors.DxGridListEditor` (docs 402154, 404767):

- `GridModel` (`DxGridModel`) — mirrors every `DxGrid` parameter; set in `OnViewControlsCreated`
  **before** render. Templates are `RenderFragment<T>`; events are `Action<T>`/`EventCallback`.
- `Columns` → `DxGridColumnWrapper.DxGridDataColumnModel` — per-column parameters.
- `AddColumnModel(DxDataColumnBaseModel)` — adds an unbound column.
- `GridInstance` (`IGrid`) — the live component; only valid after render, never cache it
  (handle `GridComponentCaptured` each time).
- `GridSummary`, `GridSelectionColumnModel`, `GridCommandColumnModel`, `GridBandColumnModels`.

## Scaffold

```
dotnet new dx.xaf -n XafGrid -p Blazor -orm EFCore -db Sqlite -dbu Auto -m Default
```

- Projects: `XafGrid.Module`, `XafGrid.Blazor.Server`, plus `XafGrid.Tests.Playwright`
  (`Microsoft.Playwright.NUnit`, per the `xaf-tools:xaf-playwright-testing` skill).
- `-m Default` = Conditional Appearance + Validation — Conditional Appearance stays as the
  "standard way" to contrast with `CustomizeElement`.
- No `-s` → no security system, no login page.

### Demo domain (Northwind-lite, `XafGrid.Module/BusinessObjects`)

| Entity | Fields | Why |
|---|---|---|
| `Customer` | Name, Country, City, Rating (1–5), Since | grouping, stars, avatar initials |
| `Employee` | Name, Title | lookup column |
| `Product` | Name, Category, UnitPrice, Discontinued, **SortOrder** | drag-reorder target |
| `Order` | Number, Customer, Employee, OrderDate, ShippedDate?, Status (enum), Lines, Total (computed) | the main demo view |
| `OrderLine` | Order, Product, Quantity, UnitPrice, Discount, LineTotal | nested grid in detail row |
| `GridLayoutPreset` | ViewId, Name, LayoutJson | D9 named layouts |

Seeded deterministically in `ModuleUpdater` (fixed `Random` seed, name arrays — no Bogus):
50 customers, 8 employees, 30 products, **1 500 orders**, ~4 500 lines. Enough rows that
server mode, paging and virtual scrolling are visible; small enough that a Playwright run
deletes `xafgrid.db` and reseeds in seconds.

### How demos are exposed

One **extra ListView per demo** in `XafGrid.Blazor.Server/Model.xafml`
(`Order_ListView_MasterDetail`, `Order_ListView_Cells`, …) under a navigation group
**Grid Demos**. Each demo = one controller (`Controllers/Demos/<Name>Controller.cs`, targeting
that `TargetViewId`) + optional Razor component (`Components/Grid/`) + CSS
(`wwwroot/css/grid-demos.css`) + one Playwright test. Demos stay independent; the plain
`Order_ListView` remains untouched as the reference.

## Demos

Priority order. Each carries the `DxGridModel` members it exercises (all verified present on
`DxGridModel` in dxdocs 26.1 unless marked *spike*).

| # | ListView | Shows | Members |
|---|---|---|---|
| D1 | Order_ListView_MasterDetail | Expand a row → nested read-only grid of its lines + customer card | `DetailRowTemplate`, `DetailExpandButtonDisplayMode`, `AutoCollapseDetailRow`, `GridInstance.ExpandDetailRow` |
| D2 | Order_ListView_Cells | Rich cells: Status badge, Rating stars, Total inline bar, Customer avatar-initials | `DataColumnCellDisplayTemplate` (branch on `context.DataColumn.FieldName`) |
| D3 | Order_ListView_Heatmap | Row heat-map by order age, cell highlight over threshold, striped group rows — beyond Conditional Appearance | `CustomizeElement` (CSS class + style + attributes) |
| D4 | Order_ListView_Groups | Custom grouping (OrderDate → This week / This month / Older; Total → ranges), group row with count + sum + mini bar | `CustomGroup`, `DataColumnGroupRowTemplate`, `CustomizeGroupValueDisplayText`, `AutoExpandAllGroupRows`, `ShowGroupedColumns` |
| D5 | Order_ListView_Summary | Custom summary ("avg of selected", weighted avg), two-line footer, group footers | `CustomSummary`, `CustomizeSummaryDisplayText`, `ColumnFooterTemplate`, `ColumnGroupFooterTemplate`, `FooterDisplayMode`, `GroupFooterDisplayMode` |
| D6 | Order_ListView_Toolbar | Toolbar inside the grid: expand/collapse groups, auto-fit, **XLSX/CSV export** (XAF only ships PDF), filter builder, clear filter; empty-data call-to-action | `ToolbarTemplate`, `EmptyDataAreaTemplate`, `GridInstance.ExportToXlsxAsync / AutoFitColumnWidths / ExpandAllGroupRows / ShowFilterBuilder / ClearFilter` |
| D7 | Order_ListView_Unbound | Columns that are not entity properties: line count, days open, an action button column | `AddColumnModel`, `UnboundColumnData`, `DxGridDataColumnModel.UnboundType` (*spike S2*) |
| D8 | Product_ListView_Reorder | Drag rows to reorder; persisted to `Product.SortOrder` | `AllowedDropTarget`, `ItemsDropped`, `DragHintTextTemplate`, `AllowDragRows` (*spike S1*) |
| D9 | Order_ListView_Presets | Named layout presets (save as / load / delete) stored in `GridLayoutPreset` — XAF keeps only one layout per view | `GridInstance.SaveLayout/LoadLayout`, `LayoutAutoLoading`, `LayoutAutoSaving` (*spike S6*) |
| D10 | Order_ListView_Columns | Wide grid (30+ columns): fixed left/right columns, header icons + tooltips, column virtualization, per-column filter-button mode | `DxGridDataColumnModel.FixedPosition` (*spike S3*), `MinWidth`, `TextAlignment`, `FilterMenuButtonDisplayMode`, `ColumnHeaderCaptionTemplate`, `VirtualScrollingMode = RowsAndColumns` |
| D11 | Order_ListView_Filters | Filter-row cell replaced by a Status combo with icons and a date-range picker; trimmed filter menu; custom search box | `DataColumnFilterRowCellTemplate`, `CustomizeFilterMenu`, `CustomizeFilterRowEditor`, `SearchBoxTemplate`, `SearchTextParseMode`, `FilterBuilderTemplate` |
| D12 | Order_ListView_EditForm | Custom popup edit form layout, Enter moves down, new row at bottom | `EditFormTemplate`, `EnterKeyDirection`, `EditNewRowPosition`, `CustomizeDataRowEditor`, `CustomizeEditModel` |

D1–D3 are the visual payoff; D4–D7 the data-shaping set; D8–D10 the layout set; D11–D12 only
if still interesting after the rest.

### Spikes (unverified in dxdocs — try, expect possible failure, log the result)

- **S1** `AllowDragRows` is not in the documented `DxGridModel` property list (only
  `AllowedDropTarget`, `DropTargetMode`, `ItemsDropped`, `DragHintTextTemplate`). If missing,
  fall back to `ComponentInstance`/`SetAttribute` or read
  `C:\Program Files\DevExpress 26.1\Components\Sources\DevExpress.ExpressApp`.
- **S2** Does `AddColumnModel` accept `UnboundExpression` columns, and do they survive
  `ApplyModel()` / the column chooser?
- **S3** `FixedPosition` on `DxGridDataColumnModel` (exists on `DxGridDataColumn`; the model
  should mirror it).
- **S4** Server-mode data source (`DataAccessMode = Server`): `context.DataItem` in templates may
  be an `ObjectRecord`/proxy rather than the entity — resolve via `IObjectRecordSupport` /
  `ObjectSpace.GetObject` before touching navigation properties.
- **S5** XAF renders property editors as cell templates. Overriding
  `DataColumnCellDisplayTemplate` may break inline editing / protected-content placeholders.
  Keep D2 on a read-only ListView; test that inline edit still works on the plain view.
- **S6** `LoadLayout` vs XAF's own layout sync (`IDxGridLayoutChangedHolder` writes column
  index/width back to the model). Expect double-saving; acceptable for a demo, document it.

## Testing

- `XafGrid.Tests.Playwright` — NUnit, `Microsoft.Playwright.NUnit`, `PageTest` base class,
  screenshot-on-failure, DevExpress selectors (`.dxbl-grid`, `.dxbl-grid-group-row`, …) per the
  `xaf-tools:xaf-playwright-testing` skill.
- **Fixture**: `OneTimeSetUp` deletes `xafgrid.db`, launches the **built exe** on a fixed port
  (free-port check first), waits for HTTP 200, and kills the process tree in `OneTimeTearDown`.
- **One test per demo**: navigate to the nav item → wait for the grid → assert the behaviour
  (badge class present, detail row expanded, footer text, download event fired, row order
  after drag, layout regrouped after preset load, fixed column bounding box after horizontal
  scroll) → `TestResults/screens/<demo>.png`.
- **Themes**: D2/D3 are screenshotted in light and dark (XAF 26.1 theme switch) since colour is the
  point; the rest light only.
- Smoke test in P0 proves the harness before any demo exists.

## Phases

| Phase | Content | Done when |
|---|---|---|
| P0 | Scaffold, domain, seed, Playwright harness, smoke test (Orders grid renders 1 500 rows in server mode) | `dotnet test` green, screenshot in repo |
| P1 | D1, D2, D3 | each: test green + screenshot + findings entry |
| P2 | D4, D5, D6, D7 (+ spikes S2, S4, S5) | same |
| P3 | D8, D9, D10 (+ spikes S1, S3, S6) | same |
| P4 | D11, D12 — only if worthwhile | same |

Task state lives on ContextBoard (project `xafgrid`, board-only — no TODO.md): one card per
demo, closed with its commit SHA. `docs/findings.md` accumulates the per-demo verdicts.

## Repo layout

```
XafGrid.sln
XafGrid.Module/               BusinessObjects/, DatabaseUpdate/Updater.cs (seed), Model.DesignedDiffs.xafml
XafGrid.Blazor.Server/        Controllers/Demos/*.cs, Components/Grid/*.razor, wwwroot/css/grid-demos.css, Model.xafml
XafGrid.Tests.Playwright/     AppFixture.cs, Demos/*Tests.cs, TestResults/screens/
docs/superpowers/specs/       this spec
docs/findings.md              per-demo verdict + dxdocs refs
```

## Out of scope

WinForms, security/roles, Web API, deployment, TreeList/PivotGrid editors, bands (standard),
anything that needs a custom `ListEditor` subclass (that is a different experiment).
