# How to implement one of these in your own XAF Blazor app

Every demo here is the same four-step pattern. Copy the pattern once, then pick the demo-specific
bits from the table below.

**Prerequisites:** an XAF Blazor Server app on DevExpress 26.1 (25.2 has the same `DxGridListEditor`
surface), and a DevExpress subscription for the NuGet packages. Nothing here needs XPO or EF Core
specifically; the demos use EF Core.

## The pattern

1. **A ViewController in the Blazor.Server project**, scoped to the ListView(s) you want to change.
   `DxGridListEditor` is Blazor-only, so the controller cannot live in the platform-agnostic module.
   `TargetViewId` takes a semicolon-separated list if one controller should serve several views.
2. **Grab the editor in `OnViewControlsCreated`.** That is the first point where `View.Editor` is a
   `DxGridListEditor` and its models exist. Nothing has rendered yet, so `editor.GridInstance` is
   still null here; use it only from event handlers later.
3. **Change the models, not the component.** XAF builds the `DxGrid` from these:
   - `editor.GridModel`, the `DxGridModel`: grid-level templates (`DetailRowTemplate`,
     `ToolbarTemplate`, ...), events (`CustomizeElement`, `CustomGroup`, `UnboundColumnData`,
     `ItemsDropped`), switches (`AllowDragRows`, `AutoCollapseDetailRow`, `VirtualScrollingMode`).
   - `editor.Columns.OfType<DxGridColumnWrapper>()` and each wrapper's `DxGridDataColumnModel`,
     per column: `CellDisplayTemplate`, `Width`, `FixedPosition`, `GroupIndex`, `SortMode`, ...
   - `editor.GridSummary.TotalSummary` / `.GroupSummary`: summary items (`DxGridSummaryItemWrapper`).
   - `editor.AddColumnModel(new DxGridDataColumnModel { ... })`: columns that are not properties.
4. **Templates are Razor components.** A template is `ctx => builder => ...`; keep the markup in a
   component under `Components/` and pass it the entity. Resolve the entity with
   `editor.GetObject(ctx.DataItem)`: `DataItem` is the entity in Client data-access mode but a
   `BlazorObjectRecord` in the server modes, and `GetObject` handles both.

```csharp
public class MyGridController : ObjectViewController<ListView, Order> {
    public MyGridController() {
        TargetViewId = "Order_ListView";           // or "Order_ListView;Order_ListView_Other"
    }

    protected override void OnViewControlsCreated() {
        base.OnViewControlsCreated();
        if(View.Editor is not DxGridListEditor editor) return;

        // grid-level
        editor.GridModel.DetailRowTemplate = ctx => builder => {
            builder.OpenComponent<OrderDetailRow>(0);
            builder.AddAttribute(1, nameof(OrderDetailRow.Order), (Order)editor.GetObject(ctx.DataItem));
            builder.CloseComponent();
        };

        // per column
        foreach(var column in editor.Columns.OfType<DxGridColumnWrapper>()) {
            if(column.PropertyName != nameof(Order.Status)) continue;
            column.DxGridDataColumnModel.CellDisplayTemplate = ctx => builder => {
                builder.OpenComponent<StatusBadge>(0);
                builder.AddAttribute(1, nameof(StatusBadge.Order), (Order)editor.GetObject(ctx.DataItem));
                builder.CloseComponent();
            };
        }
    }
}
```

Styling goes in your own stylesheet linked from `Pages/_Host.cshtml` (here `wwwroot/css/grid-demos.css`).
Data changes go through the controller's `ObjectSpace` + `CommitChanges()`; call `View.Refresh()` if the
grid does not pick a change up by itself.

## What to copy per demo

| Demo | Controller | Component(s) | Also |
|---|---|---|---|
| D1 nested grid | `MasterDetailController` | `OrderDetailRow.razor` | `.xg-detail*` CSS |
| D2 rich cells | `RichCellsController` | `OrderCell.razor` | `.xg-badge`, `.xg-avatar`, `.xg-bar*` CSS |
| D3 heat-map | `HeatmapController` | none | `.xg-heat-*`, `.xg-hot` CSS |
| D4 custom grouping | `GroupsController` | `GroupRowContent.razor` | |
| D5 custom summaries | `SummaryController` | `FooterCell.razor` | |
| D6 in-grid toolbar | `ToolbarController` | `DemoToolbar.razor`, `EmptyArea.razor` | |
| D7 unbound columns | `UnboundController` | none | |
| D8 drag to reorder | `ReorderController` | none | a sort-order property on the entity; clear the default column sort (see traps) |
| D9 layout presets | `PresetsController` | `PresetToolbar.razor` | `GridLayoutPreset` entity + DbContext registration |
| D10 wide grid | `WideColumnsController` | none | extra columns in the model (see `DemoViewsGeneratorUpdater`) |

Controllers are in `XafGrid.Blazor.Server/Controllers/Demos/`, components in
`XafGrid.Blazor.Server/Components/Grid/`. Each controller's summary comment names the `DxGridModel`
members it uses; `docs/findings.md` has the per-demo verdict and the dxdocs references.

## Traps

- **Grid-level `DataColumnCellDisplayTemplate` does nothing in XAF.** Every column already has its own
  `CellDisplayTemplate` (the property editor), and per-column beats grid-level. Override per column.
- **`CustomizeElement`: use `+=`.** XAF chains its own handler; assigning replaces it.
- **A ListView declared only in `Model.xafml` has no columns.** Either let the Model Editor persist it,
  or create it in code with a `ModelNodesGeneratorUpdater<ModelViewsNodesGenerator>`
  (`DemoViewsGeneratorUpdater.cs`). Model-diff nodes need `IsNewNode="True"` or the loader drops them.
- **Buttons inside a cell need `AddEventStopPropagationAttribute("onclick")`** on a wrapping element,
  or XAF's row click opens the DetailView.
- **Row drag-and-drop refuses drops while a column sort is active.** XAF sorts the `DefaultProperty`
  column by default; set its `SortIndex = -1` and sort the data through the model's `Sorting` node.
- **`AddColumnModel` appends columns sorted by `FieldName`**, not in call order.
- **`GetObject` also works in `CustomizeElement`:** `editor.GetObject(e.Grid.GetDataItem(e.VisibleIndex))`.
  Match columns there on `(e.Column as IGridDataColumn).FieldName`; `e.Column.Name` is unset.

## Testing it

Optional. `XafGrid.Tests.Playwright/XafTest.cs` is a reusable base for XAF Blazor: navigate to
`/{ViewId}`, wait for `#applicationLoadingPanel` to disappear, rows are `tr[data-visible-index]`,
theme via the `XAF_CurrentTheme` cookie. `AppHost.cs` starts the built exe on a fixed port against a
fresh SQLite file.
