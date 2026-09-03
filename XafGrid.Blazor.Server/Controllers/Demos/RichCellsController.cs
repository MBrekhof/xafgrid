using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Blazor.Editors;
using XafGrid.Blazor.Server.Components.Grid;
using XafGrid.Module.BusinessObjects;

namespace XafGrid.Blazor.Server.Controllers.Demos;

/// <summary>
/// D2 — badge / stars+avatar / inline bar cells. XAF renders every column through a per-column
/// CellDisplayTemplate (its property editor), which beats DxGrid's grid-level
/// DataColumnCellDisplayTemplate — so the override has to be per column too.
/// </summary>
public class RichCellsController : ObjectViewController<ListView, Order> {
    public RichCellsController() {
        TargetViewId = "Order_ListView_Cells;Order_ListView_Showcase";
    }

    protected override void OnViewControlsCreated() {
        base.OnViewControlsCreated();
        if(View.Editor is not DxGridListEditor editor) return;

        foreach(var column in editor.Columns.OfType<DxGridColumnWrapper>()) {
            var kind = column.PropertyName switch {
                nameof(Order.Status) => OrderCell.CellKind.Status,
                nameof(Order.Customer) => OrderCell.CellKind.Customer,
                nameof(Order.Total) => OrderCell.CellKind.Total,
                _ => (OrderCell.CellKind?)null,
            };
            if(kind is null) continue;
            column.DxGridDataColumnModel.CellDisplayTemplate = ctx => builder => {
                builder.OpenComponent<OrderCell>(0);
                builder.AddAttribute(1, nameof(OrderCell.Order), (Order)editor.GetObject(ctx.DataItem));
                builder.AddAttribute(2, nameof(OrderCell.Kind), kind.Value);
                builder.CloseComponent();
            };
        }
    }
}
