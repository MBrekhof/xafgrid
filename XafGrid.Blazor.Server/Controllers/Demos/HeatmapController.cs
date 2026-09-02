using DevExpress.Blazor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Blazor.Editors;
using XafGrid.Module.BusinessObjects;

namespace XafGrid.Blazor.Server.Controllers.Demos;

/// <summary>
/// D3 — row heat-map by order age, struck-through cancelled rows, highlighted big totals
/// (DxGridModel.CustomizeElement: CSS class + inline style per row/cell, beyond Conditional Appearance).
/// </summary>
public class HeatmapController : ObjectViewController<ListView, Order> {
    public HeatmapController() {
        TargetViewId = "Order_ListView_Heatmap";
    }

    protected override void OnViewControlsCreated() {
        base.OnViewControlsCreated();
        if(View.Editor is not DxGridListEditor editor) return;

        // += keeps XAF's own handler (it styles detail cells) in the chain
        editor.GridModel.CustomizeElement += (GridCustomizeElementEventArgs e) => {
            if(e.ElementType is not (GridElementType.DataRow or GridElementType.DataCell)) return;
            if(editor.GetObject(e.Grid.GetDataItem(e.VisibleIndex)) is not Order order) return;

            if(e.ElementType == GridElementType.DataRow) {
                var ageDays = (DateTime.Today - order.OrderDate.Date).TotalDays;
                e.CssClass += ageDays < 30 ? " xg-heat-0" : ageDays < 90 ? " xg-heat-1" : ageDays < 365 ? " xg-heat-2" : " xg-heat-3";
                if(order.Status == OrderStatus.Cancelled) e.Style += "opacity:.55;text-decoration:line-through;";
            }
            else if((e.Column as IGridDataColumn)?.FieldName == nameof(Order.Total) && order.Total > 5000m) {
                e.CssClass += " xg-hot";
            }
        };
    }
}
